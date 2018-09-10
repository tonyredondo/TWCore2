// ReSharper disable CheckNamespace

namespace DasMulli.Win32.ServiceUtils
{
    /// <summary>
    /// Service state
    /// </summary>
    public enum ServiceState : uint
    {
        /// <summary>
        /// Stopped
        /// </summary>
        Stopped = 0x00000001,
        /// <summary>
        /// Start Pending
        /// </summary>
        StartPending = 0x00000002,
        /// <summary>
        /// Stop Pending
        /// </summary>
        StopPending = 0x00000003,
        /// <summary>
        /// Running
        /// </summary>
        Running = 0x00000004,
        /// <summary>
        /// Continue Pending
        /// </summary>
        ContinuePending = 0x00000005,
        /// <summary>
        /// Pause Pending
        /// </summary>
        PausePending = 0x00000006,
        /// <summary>
        /// Paused
        /// </summary>
        Paused = 0x00000007
    }
}