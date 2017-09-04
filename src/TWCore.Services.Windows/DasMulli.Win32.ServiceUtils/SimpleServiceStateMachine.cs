// ReSharper disable ParameterHidesMember

namespace DasMulli.Win32.ServiceUtils
{
    public sealed class SimpleServiceStateMachine : IWin32ServiceStateMachine
    {
        private readonly IWin32Service serviceImplementation;
        private ServiceStatusReportCallback statusReportCallback;

        public SimpleServiceStateMachine(IWin32Service serviceImplementation)
        {
            this.serviceImplementation = serviceImplementation;
        }

        public void OnStart(string[] startupArguments, ServiceStatusReportCallback statusReportCallback)
        {
            this.statusReportCallback = statusReportCallback;

            try
            {
                serviceImplementation.Start(startupArguments, HandleServiceImplementationStoppedOnItsOwn);

                if (serviceImplementation.CanPauseAndContinue)
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
                    statusReportCallback(ServiceState.StopPending, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: 0, waitHint: 60000);
                    var win32ExitCode = 0;
                    try
                    {
                        serviceImplementation.Stop();
                    }
                    catch
                    {
                        win32ExitCode = -1;
                    }
                    statusReportCallback(ServiceState.Stopped, ServiceAcceptedControlCommandsFlags.None, win32ExitCode, waitHint: 0);
                    break;
                }
                case ServiceControlCommand.Pause:
                {
                    statusReportCallback(ServiceState.PausePending, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: 0, waitHint: 10000);
                    var win32ExitCode = 0;
                    try
                    {
                        serviceImplementation.OnPause();
                    }
                    catch
                    {
                        win32ExitCode = -1;
                    }
                    statusReportCallback(ServiceState.Paused, ServiceAcceptedControlCommandsFlags.None, win32ExitCode, waitHint: 0);
                    break;
                }
                case ServiceControlCommand.Continue:
                {
                    statusReportCallback(ServiceState.ContinuePending, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: 0, waitHint: 10000);
                    var win32ExitCode = 0;
                    try
                    {
                        serviceImplementation.OnContinue();
                    }
                    catch
                    {
                        win32ExitCode = -1;
                    }
                    if (serviceImplementation.CanPauseAndContinue)
                        statusReportCallback(ServiceState.Running, ServiceAcceptedControlCommandsFlags.Stop | ServiceAcceptedControlCommandsFlags.PauseContinue, win32ExitCode, waitHint: 0);
                    else
                        statusReportCallback(ServiceState.Running, ServiceAcceptedControlCommandsFlags.Stop, win32ExitCode, waitHint: 0);
                        break;
                }
            }
        }

        private void HandleServiceImplementationStoppedOnItsOwn()
        {
            statusReportCallback(ServiceState.Stopped, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: 0, waitHint: 0);
        }
    }
}