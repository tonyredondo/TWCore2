// ReSharper disable CheckNamespace

// ReSharper disable UnusedParameter.Global
namespace DasMulli.Win32.ServiceUtils
{
    /// <summary>
    /// Service stopped callback delegate
    /// </summary>
    public delegate void ServiceStoppedCallback();
    
    /// <summary>
    /// Win32 Service interface
    /// </summary>
    public interface IWin32Service
    {
        #region Properties
        /// <summary>
        /// Gets if the service can pause and continue
        /// </summary>
        bool CanPauseAndContinue { get; }
        /// <summary>
        /// Service name
        /// </summary>
        string ServiceName { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Starts the service
        /// </summary>
        /// <param name="startupArguments">Startup arguments</param>
        /// <param name="serviceStoppedCallback">Service stopped callback delegate instance</param>
        void Start(string[] startupArguments, ServiceStoppedCallback serviceStoppedCallback);
        /// <summary>
        /// Stops the service
        /// </summary>
        void Stop();
        /// <summary>
        /// Pause the service
        /// </summary>
        void OnPause();
        /// <summary>
        /// Unpause the service
        /// </summary>
        void OnContinue();
        #endregion
    }
}