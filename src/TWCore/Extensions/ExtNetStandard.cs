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

// ReSharper disable CheckNamespace

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;

namespace TWCore
{
#if NETSTANDARD2_0
    /// <summary>
    /// Extension for NetStandard Compatibility
    /// </summary>
    public static class ExtNetStandard
    {
        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentDictionary{TKey,TValue}"/>
        /// if the key does not already exist.
        /// </summary>
        /// <param name="item">Item to extend</param>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <param name="factoryArgument">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="valueFactory"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.OverflowException">The dictionary contains too many
        /// elements.</exception>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory
        /// if the key was not in the dictionary.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue GetOrAdd<TKey, TValue, TArg>(this ConcurrentDictionary<TKey, TValue> item, TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument)
        {
            return item.GetOrAdd(key, mKey => valueFactory(key, factoryArgument));
        }
        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified char 
        /// in the current System.String object. A parameter specifies the type of search 
        /// to use for the specified char.
        /// </summary>
        /// <param name="str">Item to extend</param>
        /// <param name="value">The char to seek.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
        /// <returns>The index position of the value parameter if that char is found, or -1 if it is not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(this string str, char value, StringComparison comparisonType)
        {
            return str.IndexOf(value.ToString(), comparisonType);
        }
        /// <summary>
        /// Try to write the value to the span
        /// </summary>
        /// <param name="destination">Span destination</param>
        /// <param name="value">Value to write</param>
        /// <returns>True if the value was copied to the destination; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(Span<byte> destination, bool value)
        {
            return BitConverter.GetBytes(value).AsSpan().TryCopyTo(destination);
        }
        /// <summary>
        /// Try to write the value to the span
        /// </summary>
        /// <param name="destination">Span destination</param>
        /// <param name="value">Value to write</param>
        /// <returns>True if the value was copied to the destination; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(Span<byte> destination, char value)
        {
            return BitConverter.GetBytes(value).AsSpan().TryCopyTo(destination);
        }
        /// <summary>
        /// Try to write the value to the span
        /// </summary>
        /// <param name="destination">Span destination</param>
        /// <param name="value">Value to write</param>
        /// <returns>True if the value was copied to the destination; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(Span<byte> destination, double value)
        {
            return BitConverter.GetBytes(value).AsSpan().TryCopyTo(destination);
        }
        /// <summary>
        /// Try to write the value to the span
        /// </summary>
        /// <param name="destination">Span destination</param>
        /// <param name="value">Value to write</param>
        /// <returns>True if the value was copied to the destination; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(Span<byte> destination, short value)
        {
            return BitConverter.GetBytes(value).AsSpan().TryCopyTo(destination);
        }
        /// <summary>
        /// Try to write the value to the span
        /// </summary>
        /// <param name="destination">Span destination</param>
        /// <param name="value">Value to write</param>
        /// <returns>True if the value was copied to the destination; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(Span<byte> destination, int value)
        {
            return BitConverter.GetBytes(value).AsSpan().TryCopyTo(destination);
        }
        /// <summary>
        /// Try to write the value to the span
        /// </summary>
        /// <param name="destination">Span destination</param>
        /// <param name="value">Value to write</param>
        /// <returns>True if the value was copied to the destination; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(Span<byte> destination, long value)
        {
            return BitConverter.GetBytes(value).AsSpan().TryCopyTo(destination);
        }
        /// <summary>
        /// Try to write the value to the span
        /// </summary>
        /// <param name="destination">Span destination</param>
        /// <param name="value">Value to write</param>
        /// <returns>True if the value was copied to the destination; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(Span<byte> destination, float value)
        {
            return BitConverter.GetBytes(value).AsSpan().TryCopyTo(destination);
        }
        /// <summary>
        /// Try to write the value to the span
        /// </summary>
        /// <param name="destination">Span destination</param>
        /// <param name="value">Value to write</param>
        /// <returns>True if the value was copied to the destination; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(Span<byte> destination, ushort value)
        {
            return BitConverter.GetBytes(value).AsSpan().TryCopyTo(destination);
        }
        /// <summary>
        /// Try to write the value to the span
        /// </summary>
        /// <param name="destination">Span destination</param>
        /// <param name="value">Value to write</param>
        /// <returns>True if the value was copied to the destination; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(Span<byte> destination, uint value)
        {
            return BitConverter.GetBytes(value).AsSpan().TryCopyTo(destination);
        }
        /// <summary>
        /// Try to write the value to the span
        /// </summary>
        /// <param name="destination">Span destination</param>
        /// <param name="value">Value to write</param>
        /// <returns>True if the value was copied to the destination; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(Span<byte> destination, ulong value)
        {
            return BitConverter.GetBytes(value).AsSpan().TryCopyTo(destination);
        }
        /// <summary>
        /// Write ReadOnlySpan buffer to the stream
        /// </summary>
        /// <param name="stream">Stream instance</param>
        /// <param name="buffer">Buffer to write</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, ReadOnlySpan<byte> buffer)
        {
            var bytes = buffer.ToArray();
            stream.Write(bytes, 0, bytes.Length);
        }
    }
#endif
}
