using System;
using System.Runtime.InteropServices;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable CheckNamespace

namespace DasMulli.Win32.ServiceUtils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ScAction:IEquatable<ScAction>
    {
        private ScActionType _Type;
        private uint _Delay;
        
        public ScActionType Type
        {
            get => _Type;
            set => _Type = value;
        }

        public TimeSpan Delay
        {
            get => TimeSpan.FromMilliseconds(_Delay);
            set => _Delay = (uint)Math.Round(value.TotalMilliseconds);
        }

        public bool Equals(ScAction other)
        {
            return _Type == other._Type && _Delay == other._Delay;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ScAction oObj && Equals(oObj);
        }

        public override int GetHashCode()
        {
            return HashCode
                .Of(Delay)
                .And(Type);
        }
    }
}