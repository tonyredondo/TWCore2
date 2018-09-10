// ReSharper disable CheckNamespace

namespace DasMulli.Win32.ServiceUtils
{
    /// <summary>
    /// Error severity
    /// </summary>
    public enum ErrorSeverity : uint
    {
        /// <summary>
        /// Ignore
        /// </summary>
        Ignore = 0,
        /// <summary>
        /// Normal
        /// </summary>
        Normal = 1,
        /// <summary>
        /// Severe
        /// </summary>
        Severe = 2,
        /// <summary>
        /// Critical
        /// </summary>
        Crititcal = 3
    }
}