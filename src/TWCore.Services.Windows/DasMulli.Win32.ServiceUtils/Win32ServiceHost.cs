using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

// ReSharper disable CheckNamespace

namespace DasMulli.Win32.ServiceUtils
{
    // This implementation is roughly based on https://msdn.microsoft.com/en-us/library/bb540475(v=vs.85).aspx
    public sealed class Win32ServiceHost
    {
        private readonly string _serviceName;
        private readonly IWin32ServiceStateMachine _stateMachine;
        private readonly INativeInterop _nativeInterop;
        private readonly ServiceMainFunction _serviceMainFunctionDelegate;
        private readonly ServiceControlHandler _serviceControlHandlerDelegate;
        private ServiceStatus _serviceStatus = new ServiceStatus(ServiceType.Win32OwnProcess, ServiceState.StartPending, ServiceAcceptedControlCommandsFlags.None,
            win32ExitCode: 0, serviceSpecificExitCode: 0, checkPoint: 0, waitHint: 0);
        private ServiceStatusHandle _serviceStatusHandle;
        private readonly TaskCompletionSource<int> _stopTaskCompletionSource = new TaskCompletionSource<int>();

        public Win32ServiceHost(IWin32Service service)
            : this(service, Win32Interop.Wrapper)
        {
        }

        internal Win32ServiceHost(IWin32Service service, INativeInterop nativeInterop)
        {
            _serviceName = service?.ServiceName ?? throw new ArgumentNullException(nameof(service));
            _stateMachine = new SimpleServiceStateMachine(service);
            _nativeInterop = nativeInterop ?? throw new ArgumentNullException(nameof(nativeInterop));
            _serviceMainFunctionDelegate = ServiceMainFunction;
            _serviceControlHandlerDelegate = HandleServiceControlCommand;
        }

        public Win32ServiceHost(string serviceName, IWin32ServiceStateMachine stateMachine)
            : this(serviceName, stateMachine, Win32Interop.Wrapper)
        {
        }

        internal Win32ServiceHost(string serviceName, IWin32ServiceStateMachine stateMachine, INativeInterop nativeInterop)
        {
            _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _nativeInterop = nativeInterop ?? throw new ArgumentNullException(nameof(nativeInterop));
            _serviceMainFunctionDelegate = ServiceMainFunction;
            _serviceControlHandlerDelegate = HandleServiceControlCommand;
        }

        public Task<int> RunAsync()
        {
            var serviceTable = new ServiceTableEntry[2]; // second one is null/null to indicate termination
            serviceTable[0].serviceName = _serviceName;
            serviceTable[0].serviceMainFunction = Marshal.GetFunctionPointerForDelegate(_serviceMainFunctionDelegate);

            try
            {
                // StartServiceCtrlDispatcherW call returns when ServiceMainFunction exits
                if (!_nativeInterop.StartServiceCtrlDispatcherW(serviceTable))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            catch (DllNotFoundException dllException)
            {
                throw new PlatformNotSupportedException(nameof(Win32ServiceHost) + " is only supported on Windows with service management API set.",
                    dllException);
            }

            return _stopTaskCompletionSource.Task;
        }

        public int Run()
        {
            return RunAsync().Result;
        }

        private void ServiceMainFunction(int numArgs, IntPtr argPtrPtr)
        {
            var startupArguments = ParseArguments(numArgs, argPtrPtr);

            _serviceStatusHandle = _nativeInterop.RegisterServiceCtrlHandlerExW(_serviceName, _serviceControlHandlerDelegate, IntPtr.Zero);

            if (_serviceStatusHandle.IsInvalid)
            {
                _stopTaskCompletionSource.SetException(new Win32Exception(Marshal.GetLastWin32Error()));
                return;
            }

            ReportServiceStatus(ServiceState.StartPending, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: 0, waitHint: 60000);

            try
            {
                _stateMachine.OnStart(startupArguments, ReportServiceStatus);
            }
            catch
            {
                ReportServiceStatus(ServiceState.Stopped, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: -1, waitHint: 0);
            }
        }

        private uint _checkpointCounter = 1;

        private void ReportServiceStatus(ServiceState state, ServiceAcceptedControlCommandsFlags acceptedControlCommands, int win32ExitCode, uint waitHint)
        {
            if (_serviceStatus.State == ServiceState.Stopped)
            {
                // we refuse to leave or alter the final state
                return;
            }

            _serviceStatus.State = state;
            _serviceStatus.Win32ExitCode = win32ExitCode;
            _serviceStatus.WaitHint = waitHint;

            _serviceStatus.AcceptedControlCommands = state == ServiceState.Stopped
                ? ServiceAcceptedControlCommandsFlags.None // since we enforce "Stopped" as final state, no longer accept control messages
                : acceptedControlCommands;

            _serviceStatus.CheckPoint = state == ServiceState.Running || state == ServiceState.Stopped || state == ServiceState.Paused
                ? 0 // MSDN: This value is not valid and should be zero when the service does not have a start, stop, pause, or continue operation pending.
                : _checkpointCounter++;

            _nativeInterop.SetServiceStatus(_serviceStatusHandle, ref _serviceStatus);

            if (state == ServiceState.Stopped)
            {
                _stopTaskCompletionSource.TrySetResult(win32ExitCode);
            }
        }

        private void HandleServiceControlCommand(ServiceControlCommand command, uint eventType, IntPtr eventData, IntPtr eventContext)
        {
            try
            {
                _stateMachine.OnCommand(command, eventType);
            }
            catch
            {
                ReportServiceStatus(ServiceState.Stopped, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: -1, waitHint: 0);
            }
        }

        private static string[] ParseArguments(int numArgs, IntPtr argPtrPtr)
        {
            if (numArgs <= 0)
            {
                return Array.Empty<string>();
            }
            // skip first parameter becuase it is the name of the service
            var args = new string[numArgs - 1];
            for (var i = 0; i < numArgs - 1; i++)
            {
                argPtrPtr = IntPtr.Add(argPtrPtr, IntPtr.Size);
                var argPtr = Marshal.PtrToStructure<IntPtr>(argPtrPtr);
                args[i] = Marshal.PtrToStringUni(argPtr);
            }
            return args;
        }
    }
}