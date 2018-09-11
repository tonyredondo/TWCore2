// ReSharper disable CheckNamespace

namespace DasMulli.Win32.ServiceUtils
{
    /// <summary>
    /// Service Control Command
    /// </summary>
    public enum ServiceControlCommand : uint
    {
        /// <summary>
        /// Stop
        /// </summary>
        Stop = 0x00000001,
        /// <summary>
        /// Pause
        /// </summary>
        Pause = 0x00000002,
        /// <summary>
        /// Continue
        /// </summary>
        Continue = 0x00000003,
        /// <summary>
        /// Interrogate
        /// </summary>
        Interrogate = 0x00000004,
        /// <summary>
        /// Shutdown
        /// </summary>
        Shutdown = 0x00000005,
        /// <summary>
        /// Paramchange
        /// </summary>
        Paramchange = 0x00000006,
        /// <summary>
        /// NetBind Add
        /// </summary>
        NetBindAdd = 0x00000007,
        /// <summary>
        /// NetBind Removed
        /// </summary>
        NetBindRemoved = 0x00000008,
        /// <summary>
        /// NetBind Enable
        /// </summary>
        NetBindEnable = 0x00000009,
        /// <summary>
        /// NetBind Disable
        /// </summary>
        NetBindDisable = 0x0000000A,
        /// <summary>
        /// Device Event
        /// </summary>
        DeviceEvent = 0x0000000B,
        /// <summary>
        /// Hardware Profile Change
        /// </summary>
        HardwareProfileChange = 0x0000000C,
        /// <summary>
        /// Power Event
        /// </summary>
        PowerEvent = 0x0000000D,
        /// <summary>
        /// Sesion Change
        /// </summary>
        SessionChange = 0x0000000E
    }
}