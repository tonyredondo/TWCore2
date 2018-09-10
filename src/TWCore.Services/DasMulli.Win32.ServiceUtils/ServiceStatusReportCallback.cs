// ReSharper disable CheckNamespace

namespace DasMulli.Win32.ServiceUtils
{
    /// <summary>
    /// Service status report callback
    /// </summary>
    /// <param name="state">Service state</param>
    /// <param name="acceptedControlCommands">Accepted control commands</param>
    /// <param name="win32ExitCode">Win32 exit code</param>
    /// <param name="waitHint">Wait hint</param>
    public delegate void ServiceStatusReportCallback(ServiceState state, ServiceAcceptedControlCommandsFlags acceptedControlCommands, int win32ExitCode, uint waitHint);
}