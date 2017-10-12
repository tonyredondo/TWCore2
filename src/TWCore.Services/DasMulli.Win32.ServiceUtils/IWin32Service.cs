// ReSharper disable CheckNamespace

// ReSharper disable UnusedParameter.Global
namespace DasMulli.Win32.ServiceUtils
{
    public delegate void ServiceStoppedCallback();
    public interface IWin32Service
    {
        bool CanPauseAndContinue { get; }
        string ServiceName { get; }
        void Start(string[] startupArguments, ServiceStoppedCallback serviceStoppedCallback);
        void Stop();
        void OnPause();
        void OnContinue();
    }
}