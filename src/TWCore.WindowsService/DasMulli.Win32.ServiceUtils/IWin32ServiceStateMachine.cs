namespace DasMulli.Win32.ServiceUtils
{
    public interface IWin32ServiceStateMachine
    {
        void OnStart(string[] startupArguments, ServiceStatusReportCallback statusReportCallback);
        void OnCommand(ServiceControlCommand command, uint commandSpecificEventType);
    }
}