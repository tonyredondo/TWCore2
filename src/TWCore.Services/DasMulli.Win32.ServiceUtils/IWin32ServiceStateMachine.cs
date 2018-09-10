// ReSharper disable CheckNamespace

// ReSharper disable UnusedParameter.Global
namespace DasMulli.Win32.ServiceUtils
{
    /// <summary>
    /// Win32 Service StateMachine interface
    /// </summary>
    public interface IWin32ServiceStateMachine
    {
        /// <summary>
        /// On Start command
        /// </summary>
        /// <param name="startupArguments">Start arguments</param>
        /// <param name="statusReportCallback">Status report callback</param>
        void OnStart(string[] startupArguments, ServiceStatusReportCallback statusReportCallback);
        /// <summary>
        /// On Command
        /// </summary>
        /// <param name="command">Service control command</param>
        /// <param name="commandSpecificEventType">Command specific event type</param>
        void OnCommand(ServiceControlCommand command, uint commandSpecificEventType);
    }
}