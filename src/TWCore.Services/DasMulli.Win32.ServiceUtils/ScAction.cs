using System;
using System.Runtime.InteropServices;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable CheckNamespace

namespace DasMulli.Win32.ServiceUtils
{
    /// <summary>
    /// SC action
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ScAction:IEquatable<ScAction>
    {
        private ScActionType _Type;
        private uint _Delay;

        #region Properties
        /// <summary>
        /// Sc action type
        /// </summary>
        public ScActionType Type
        {
            get => _Type;
            set => _Type = value;
        }
        /// <summary>
        /// Delay
        /// </summary>
        public TimeSpan Delay
        {
            get => TimeSpan.FromMilliseconds(_Delay);
            set => _Delay = (uint)Math.Round(value.TotalMilliseconds);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks if the ScAction is equals to other instance
        /// </summary>
        /// <param name="other">Other ScAction instance</param>
        /// <returns>True if both actions are equals; otherwise, false.</returns>
        public bool Equals(ScAction other)
        {
            return _Type == other._Type && _Delay == other._Delay;
        }

        /// <summary>
        /// Checks if the ScAction is equals to other instance
        /// </summary>
        /// <param name="obj">Other object instance</param>
        /// <returns>True if both actions are equals; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ScAction oObj && Equals(oObj);
        }

        /// <summary>
        /// Gets the hash code of the action
        /// </summary>
        /// <returns>hashcode</returns>
        public override int GetHashCode()
        {
            return HashCode.Of(Delay).And(Type);
        }
        #endregion
    }
}