using System;

// ReSharper disable CheckNamespace

namespace DasMulli.Win32.ServiceUtils
{
    /// <summary>
    /// Control Commands Flags
    /// </summary>
    [Flags]
    public enum ServiceAcceptedControlCommandsFlags : uint
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Stop
        /// </summary>
        Stop = 0x00000001,
        /// <summary>
        /// Pause and Continue
        /// </summary>
        PauseContinue = 0x00000002,
        /// <summary>
        /// Shutdown
        /// </summary>
        Shutdown = 0x00000004,
        /// <summary>
        /// Param Change
        /// </summary>
        ParamChange = 0x00000008,
        /// <summary>
        /// NetBind Change
        /// </summary>
        NetBindChange = 0x00000010,
        /// <summary>
        /// Pre shutdown
        /// </summary>
        PreShutdown = 0x00000100,
        /// <summary>
        /// Hardware Profile change
        /// </summary>
        HardwareProfileChange = 0x00000020,
        /// <summary>
        /// Power Event
        /// </summary>
        PowerEvent = 0x00000040,
        /// <summary>
        /// Session change
        /// </summary>
        SessionChange = 0x00000080
    }
}