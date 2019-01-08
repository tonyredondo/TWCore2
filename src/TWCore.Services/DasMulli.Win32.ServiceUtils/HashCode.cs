using System.Collections.Generic;
using System.Linq;

// ReSharper disable CheckNamespace

namespace DasMulli.Win32.ServiceUtils
{
    /// <summary>
    /// Simplifies the work of hashing.
    /// </summary>
    public readonly struct HashCode
    {
        private readonly int _value;

        #region .ctor
        private HashCode(int value)
        {
            _value = value;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Converts a hashcode struct to the int value
        /// </summary>
        /// <param name="hashCode">Hashcode instance</param>
        public static implicit operator int(HashCode hashCode)
        {
            return hashCode._value;
        }
        /// <summary>
        /// Gets the HashCode of an item
        /// </summary>
        /// <typeparam name="T">Type of the item</typeparam>
        /// <param name="item">Item instance</param>
        /// <returns>Hashcode value</returns>
        public static HashCode Of<T>(T item)
        {
            return new HashCode(GetHashCode(item));
        }
        /// <summary>
        /// Combine hashcode with the hashcode of other item
        /// </summary>
        /// <typeparam name="T">Type of the item</typeparam>
        /// <param name="item">Item instance</param>
        /// <returns>Hashcode value resulted of the combination of both hashcodes</returns>
        public HashCode And<T>(T item)
        {
            return new HashCode(CombineHashCodes(_value, GetHashCode(item)));
        }
        /// <summary>
        /// Combine hashcode with the hashcode of all items on the ienumerable
        /// </summary>
        /// <typeparam name="T">Type of the item</typeparam>
        /// <param name="items">Items ienumerable instance</param>
        /// <returns>Hashcode value resulted of the combination of both hashcodes</returns>
        public HashCode AndEach<T>(IEnumerable<T> items)
        {
            var hashCode = items.Select(GetHashCode).Aggregate(CombineHashCodes);
            return new HashCode(CombineHashCodes(_value, hashCode));
        }
        #endregion

        #region Private Methods
        private static int CombineHashCodes(int h1, int h2)
        {
            unchecked
            {
                // Code copied from System.Tuple so it must be the best way to combine hash codes or at least a good one.
                return ((h1 << 5) + h1) ^ h2;
            }
        }
        private static int GetHashCode<T>(T item)
        {
            return item?.GetHashCode() ?? 0;
        }
        #endregion
    }
}