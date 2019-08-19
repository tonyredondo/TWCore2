// ReSharper disable ParameterHidesMember
// ReSharper disable CheckNamespace

namespace DasMulli.Win32.ServiceUtils
{
    /// <summary>
    /// Simple service state machine
    /// </summary>
    public sealed class SimpleServiceStateMachine : IWin32ServiceStateMachine
    {
        private readonly IWin32Service _serviceImplementation;
        private ServiceStatusReportCallback _statusReportCallback;

        #region .ctor
        /// <summary>
        /// Simple service state machine
        /// </summary>
        /// <param name="serviceImplementation">Win32 Service instance</param>
        public SimpleServiceStateMachine(IWin32Service serviceImplementation)
        {
            _serviceImplementation = serviceImplementation;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// On Start command
        /// </summary>
        /// <param name="startupArguments">Start arguments</param>
        /// <param name="statusReportCallback">Status report callback</param>
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
        /// <summary>
        /// On Command
        /// </summary>
        /// <param name="command">Service control command</param>
        /// <param name="commandSpecificEventType">Command specific event type</param>
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
        #endregion

        #region Private Methods
        private void HandleServiceImplementationStoppedOnItsOwn()
        {
            _statusReportCallback(ServiceState.Stopped, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: 0, waitHint: 0);
        }
        #endregion
    }
}