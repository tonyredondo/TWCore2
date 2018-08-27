﻿/*
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
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

using System.Runtime.CompilerServices;

namespace TWCore
{
    /// <summary>
    /// Readable Bytes
    /// </summary>
    public struct ReadableBytes
    {
        public long Value { get; set; }
        public string Text { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() 
            => Text;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => Value.GetHashCode() + Text?.GetHashCode() ?? 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is ReadableBytes rBytes && Value == rBytes.Value && Text == rBytes.Text;
    }

    /// <summary>
    /// Extensions for numbers data types
    /// </summary>
    public static partial class Extensions
    {
        #region ReadableBytes
        /// <summary>
        /// Format the length to a human readable style format.
        /// </summary>
        /// <param name="bytes">Bytes length to format</param>
        /// <returns>Human readeable style format</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadableBytes ToReadableBytes(this long bytes)
        {
            const int scale = 1024;
            var orders = new[] { "GB", "MB", "KB", "Bytes" };
            var max = (long)System.Math.Pow(scale, orders.Length - 1);
            foreach (var order in orders)
            {
                if (bytes > max)
                    return new ReadableBytes { Text = string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order), Value = bytes };
                max /= scale;
            }
            return new ReadableBytes { Text = "0 Bytes", Value = bytes };
        }
        /// <summary>
        /// Format the length to a human readeable style format.
        /// </summary>
        /// <param name="bytes">Bytes length to format</param>
        /// <returns>Human readeable style format</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadableBytes ToReadableBytes(this double bytes) => ToReadableBytes((long)bytes);
        /// <summary>
        /// Format the length to a human readeable style format.
        /// </summary>
        /// <param name="bytes">Bytes length to format</param>
        /// <returns>Human readeable style format</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadableBytes ToReadableBytes(this float bytes) => ToReadableBytes((long)bytes);
        /// <summary>
        /// Format the length to a human readeable style format.
        /// </summary>
        /// <param name="bytes">Bytes length to format</param>
        /// <returns>Human readeable style format</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadableBytes ToReadableBytes(this int bytes) => ToReadableBytes((long)bytes);
        #endregion

        #region Converts
        /// <summary>
        /// Converts degrees to radians
        /// </summary>
        /// <param name="degrees">Degrees value</param>
        /// <returns>Radians value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToRad(this double degrees) => degrees * (System.Math.PI / 180);
        /// <summary>
        /// Converts radians to degrees
        /// </summary>
        /// <param name="radians">Radians value</param>
        /// <returns>Degrees value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToDegrees(this double radians) => radians * 180 / System.Math.PI;
        /// <summary>
        /// Converts radians to bearing
        /// </summary>
        /// <param name="radians">Radians value</param>
        /// <returns>Bearing value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToBearing(this double radians) => (ToDegrees(radians) + 360) % 360;
        #endregion

        #region ToMegabytes
        /// <summary>
        /// Gets the Megabyte size of a length
        /// </summary>
        /// <param name="bytes">Bytes length to format</param>
        /// <returns>The length in megabytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToMegabytes(this long bytes)
            => ((double)bytes / 1024) / 1024;
        /// <summary>
        /// Gets the Megabyte size of a length
        /// </summary>
        /// <param name="bytes">Bytes length to format</param>
        /// <returns>The length in megabytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToMegabytes(this double bytes) => ToMegabytes((long)bytes);
        /// <summary>
        /// Gets the Megabyte size of a length
        /// </summary>
        /// <param name="bytes">Bytes length to format</param>
        /// <returns>The length in megabytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToMegabytes(this float bytes) => ToMegabytes((long)bytes);
        /// <summary>
        /// Gets the Megabyte size of a length
        /// </summary>
        /// <param name="bytes">Bytes length to format</param>
        /// <returns>The length in megabytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToMegabytes(this int bytes) => ToMegabytes((long)bytes);
        #endregion
    }
}
