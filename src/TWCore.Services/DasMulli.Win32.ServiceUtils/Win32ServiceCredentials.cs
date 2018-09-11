using System;
// ReSharper disable ArgumentsStyleNamedExpression

// ReSharper disable CheckNamespace

namespace DasMulli.Win32.ServiceUtils
{
    /// <summary>
    /// Win32 Service Credentials
    /// </summary>
    public struct Win32ServiceCredentials : IEquatable<Win32ServiceCredentials>
    {
        /// <summary>
        /// Local system credentials
        /// </summary>
        public static Win32ServiceCredentials LocalSystem = new Win32ServiceCredentials(userName: null, password: null);
        /// <summary>
        /// Local service credentials
        /// </summary>
        public static Win32ServiceCredentials LocalService = new Win32ServiceCredentials(@"NT AUTHORITY\LocalService", password: null);
        /// <summary>
        /// Network service credentials
        /// </summary>
        public static Win32ServiceCredentials NetworkService = new Win32ServiceCredentials(@"NT AUTHORITY\NetworkService", password: null);

        #region Properties
        /// <summary>
        /// Username
        /// </summary>
        public string UserName { get; }
        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; }
        #endregion

        #region .ctor
        /// <summary>
        /// Win32 Service Credentials
        /// </summary>
        /// <param name="userName">Username</param>
        /// <param name="password">Password</param>
        public Win32ServiceCredentials(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
        #endregion

        /// <summary>
        /// Checks if the Win32ServiceCredentials is equals to other instance
        /// </summary>
        /// <param name="other">Other Win32ServiceCredentials instance</param>
        /// <returns>True if both actions are equals; otherwise, false.</returns>
        public bool Equals(Win32ServiceCredentials other)
        {
            return string.Equals(UserName, other.UserName) && string.Equals(Password, other.Password);
        }

        /// <summary>
        /// Checks if the Win32ServiceCredentials is equals to other instance
        /// </summary>
        /// <param name="obj">Other object instance</param>
        /// <returns>True if both actions are equals; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(objA: null, objB: obj))
            {
                return false;
            }
            return obj is Win32ServiceCredentials wsCred && Equals(wsCred);
        }

        /// <summary>
        /// Gets the hash code of the action
        /// </summary>
        /// <returns>hashcode</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((UserName?.GetHashCode() ?? 0)*397) ^ (Password?.GetHashCode() ?? 0);
            }
        }

        /// <summary>
        /// Get if two instances are equals
        /// </summary>
        /// <param name="left">First Win32ServiceCredentials instance</param>
        /// <param name="right">Second Win32ServiceCredentials instance</param>
        /// <returns>True if both instances are equals; otherwise, false.</returns>
        public static bool operator ==(Win32ServiceCredentials left, Win32ServiceCredentials right)
        {
            return left.Equals(right);
        }
        /// <summary>
        /// Get if two instances aren't equals
        /// </summary>
        /// <param name="left">First Win32ServiceCredentials instance</param>
        /// <param name="right">Second Win32ServiceCredentials instance</param>
        /// <returns>True if both instances are not equals; otherwise, false.</returns>
        public static bool operator !=(Win32ServiceCredentials left, Win32ServiceCredentials right)
        {
            return !left.Equals(right);
        }
    }
}