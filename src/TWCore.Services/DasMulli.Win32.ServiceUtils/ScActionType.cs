// ReSharper disable CheckNamespace

namespace DasMulli.Win32.ServiceUtils
{
    /// <summary>
    /// SC Action Type
    /// </summary>
    public enum ScActionType
    {
        /// <summary>
        /// None
        /// </summary>
        ScActionNone = 0,
        /// <summary>
        /// Restart
        /// </summary>
        ScActionRestart = 1,
        /// <summary>
        /// Reboot
        /// </summary>
        ScActionReboot = 2,
        /// <summary>
        /// Run Command
        /// </summary>
        ScActionRunCommand = 3
    }
}