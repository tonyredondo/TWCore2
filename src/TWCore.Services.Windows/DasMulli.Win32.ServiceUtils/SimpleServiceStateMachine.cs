// ReSharper disable ParameterHidesMember
// ReSharper disable CheckNamespace

using System;
using System.Diagnostics;

namespace DasMulli.Win32.ServiceUtils
{
    public sealed class SimpleServiceStateMachine : IWin32ServiceStateMachine
    {
        private readonly IWin32Service _serviceImplementation;
        private ServiceStatusReportCallback _statusReportCallback;

        public SimpleServiceStateMachine(IWin32Service serviceImplementation)
        {
            _serviceImplementation = serviceImplementation;
        }

        public void OnStart(string[] startupArguments, ServiceStatusReportCallback statusReportCallback)
        {
            _statusReportCallback = statusReportCallback;

            try
            {
                _serviceImplementation.Start(startupArguments, HandleServiceImplementationStoppedOnItsOwn);

                if (_serviceImplementation.CanPauseAndContinue)
                    statusReportCallback(ServiceState.Running, ServiceAcceptedControlCommandsFlags.Stop | ServiceAcceptedControlCommandsFlags.PauseContinue, win32ExitCode: 0, waitHint: 0);
                else
                    statusReportCallback(ServiceState.Running, ServiceAcceptedControlCommandsFlags.Stop, win32ExitCode: 0, waitHint: 0);
            }
            catch
            {
                statusReportCallback(ServiceState.Stopped, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: -1, waitHint: 0);
            }
        }

        public void OnCommand(ServiceControlCommand command, uint commandSpecificEventType)
        {
            switch (command)
            {
                case ServiceControlCommand.Stop:
                {
                    _statusReportCallback(ServiceState.StopPending, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: 0, waitHint: 60000);
                    var win32ExitCode = 0;
                    try
                    {
                        _serviceImplementation.Stop();
                    }
                    catch
                    {
                        win32ExitCode = -1;
                    }
                    finally
                    {
                        _statusReportCallback(ServiceState.Stopped, ServiceAcceptedControlCommandsFlags.None,
                            win32ExitCode, waitHint: 0);
                    }
                    break;
                }
                case ServiceControlCommand.Pause:
                {
                    _statusReportCallback(ServiceState.PausePending, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: 0, waitHint: 20000);
                    var win32ExitCode = 0;
                    try
                    {
                        _serviceImplementation.OnPause();
                    }
                    catch
                    {
                        win32ExitCode = -1;
                    }
                    _statusReportCallback(ServiceState.Paused, ServiceAcceptedControlCommandsFlags.PauseContinue | ServiceAcceptedControlCommandsFlags.Stop, win32ExitCode, waitHint: 0);
                    break;
                }
                case ServiceControlCommand.Continue:
                {
                    _statusReportCallback(ServiceState.ContinuePending, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: 0, waitHint: 20000);
                    var win32ExitCode = 0;
                    try
                    {
                        _serviceImplementation.OnContinue();
                    }
                    catch
                    {
                        win32ExitCode = -1;
                    }
                    if (_serviceImplementation.CanPauseAndContinue)
                        _statusReportCallback(ServiceState.Running, ServiceAcceptedControlCommandsFlags.Stop | ServiceAcceptedControlCommandsFlags.PauseContinue, win32ExitCode, waitHint: 0);
                    else
                        _statusReportCallback(ServiceState.Running, ServiceAcceptedControlCommandsFlags.Stop, win32ExitCode, waitHint: 0);
                        break;
                }
            }
        }

        private void HandleServiceImplementationStoppedOnItsOwn()
        {
            _statusReportCallback(ServiceState.Stopped, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: 0, waitHint: 0);
        }
    }
}