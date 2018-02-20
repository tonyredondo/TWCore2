/*
Copyright 2015-2018 Daniel Adrian Redondo Suarez

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TWCore
{
    /// <summary>
    /// Equality comparer based on a key selector using a delegate.
    /// </summary>
    public static class KeySelectorEqualityComparer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEqualityComparer<T> Create<T, TKey>(Func<T, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
            => new KeySelectorEqualityComparer<T, TKey>(keySelector, keyComparer);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEqualityComparer<T> Create<T, TKey>(Func<T, TKey> keySelector)
            => new KeySelectorEqualityComparer<T, TKey>(keySelector);
    }

    /// <inheritdoc />
    /// <summary>
    /// Equality comparer based on a key selector using a delegate.
    /// </summary>
    /// <typeparam name="T">Type of the Object</typeparam>
    /// <typeparam name="TKey">Type of the Object key</typeparam>
    public sealed class KeySelectorEqualityComparer<T, TKey> : IEqualityComparer<T>
    {
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly Func<T, TKey> _keySelector;

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Equality comparer based on a key selector using a delegate.
        /// </summary>
        /// <param name="keySelector">Delegate to select the key of the object</param>
        public KeySelectorEqualityComparer(Func<T, TKey> keySelector) : this(keySelector, EqualityComparer<TKey>.Default) { }
        /// <inheritdoc />
        /// <summary>
        /// Equality comparer based on a key selector using a delegate.
        /// </summary>
        /// <param name="keySelector">Delegate to select the key of the object</param>
        /// <param name="keyComparer">EqualityComparer instance for the key type</param>
        public KeySelectorEqualityComparer(Func<T, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
        {
            _keySelector = keySelector;
            _keyComparer = keyComparer;
        }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">Object instance to compare</param>
        /// <param name="y">Object instance to compare</param>
        /// <returns>true if both objects have the same TKey; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(T x, T y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x == null || y == null)
                return false;
            var kX = _keySelector(x);
            var kY = _keySelector(y);
            return _keyComparer.Equals(kX, kY);
        }
        /// <inheritdoc />
        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>Object hash value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(T obj)
        {
            if (obj == null) return -1;
            return _keyComparer.GetHashCode(_keySelector(obj));
        }
        #endregion
    }
}
