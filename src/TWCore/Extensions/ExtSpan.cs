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
using System.IO;
using System.Runtime.CompilerServices;
// ReSharper disable CheckNamespace

namespace TWCore
{
    /// <summary>
    /// Helper to SubArray common operations
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Get the Span from the MemoryStream buffer instance.
        /// </summary>
        /// <param name="mStream">MemoryStream instance</param>
        /// <returns>Span instance from the buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> AsSpan(this MemoryStream mStream)
            => mStream != null ? new Span<byte>(mStream.GetBuffer(), 0, (int)mStream.Length) : Span<byte>.Empty;
        /// <summary>
        /// Get the ReadOnlySpan from the MemoryStream buffer instance.
        /// </summary>
        /// <param name="mStream">MemoryStream instance</param>
        /// <returns>ReadOnlySpan instance from the buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> AsReadOnlySpan(this MemoryStream mStream)
            => mStream != null ? new ReadOnlySpan<byte>(mStream.GetBuffer(), 0, (int)mStream.Length) : ReadOnlySpan<byte>.Empty;
        /// <summary>
        /// Get the Memory from the MemoryStream buffer instance.
        /// </summary>
        /// <param name="mStream">MemoryStream instance</param>
        /// <returns>Memory instance from the buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> AsMemory(this MemoryStream mStream)
            => mStream != null ? new Memory<byte>(mStream.GetBuffer(), 0, (int)mStream.Length) : Memory<byte>.Empty;
        /// <summary>
        /// Get the ReadOnlyMemory from the MemoryStream buffer instance.
        /// </summary>
        /// <param name="mStream">MemoryStream instance</param>
        /// <returns>ReadOnlyMemory instance from the buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<byte> AsReadOnlyMemory(this MemoryStream mStream)
            => mStream != null ? new ReadOnlyMemory<byte>(mStream.GetBuffer(), 0, (int)mStream.Length) : ReadOnlyMemory<byte>.Empty;
        /// <summary>
        /// Split a char memory using a separator
        /// </summary>
        /// <param name="memory">Source memory</param>
        /// <param name="separator">Char separator</param>
        /// <param name="options">StringSplit options</param>
        /// <returns>List with the split result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<ReadOnlyMemory<char>> Split(this ReadOnlyMemory<char> memory, char separator, StringSplitOptions options = StringSplitOptions.None)
        {
            var result = new List<ReadOnlyMemory<char>>();
            while (memory.Length > 0)
            {
                var idx = memory.Span.IndexOf(separator);
                if (idx == -1)
                    break;
                var value = memory.Slice(0, idx);
                if (options == StringSplitOptions.None || value.Length > 0)
                    result.Add(value);
                memory = memory.Slice(idx + 1);
            }
            if (options == StringSplitOptions.None || memory.Length > 0)
                result.Add(memory);
            return result;
        }
        /// <summary>
        /// Split a char memory using a separator
        /// </summary>
        /// <param name="memory">Source memory</param>
        /// <param name="separator">Char separator</param>
        /// <param name="options">StringSplit options</param>
        /// <returns>List with the split result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<ReadOnlyMemory<char>> Split(this ReadOnlyMemory<char> memory, ReadOnlySpan<char> separator, StringSplitOptions options = StringSplitOptions.None)
        {
            var result = new List<ReadOnlyMemory<char>>();
            while (memory.Length > 0)
            {
                var idx = memory.Span.IndexOf(separator);
                if (idx == -1)
                    break;
                var value = memory.Slice(0, idx);
                if (options == StringSplitOptions.None || value.Length > 0)
                    result.Add(value);
                memory = memory.Slice(idx + separator.Length);
            }
            if (options == StringSplitOptions.None || memory.Length > 0)
                result.Add(memory);
            return result;
        }
        /// <summary>
        /// Split a char memory using a separator
        /// </summary>
        /// <param name="memory">Source memory</param>
        /// <param name="separator">Char separator</param>
        /// <param name="options">StringSplit options</param>
        /// <returns>List with the split result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<string> SplitAsString(this ReadOnlyMemory<char> memory, char separator, StringSplitOptions options = StringSplitOptions.None)
            => SplitAsString(memory.Span, separator, options);
        /// <summary>
        /// Split a char memory using a separator
        /// </summary>
        /// <param name="memory">Source memory</param>
        /// <param name="separator">Char separator</param>
        /// <param name="options">StringSplit options</param>
        /// <returns>List with the split result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<string> SplitAsString(this ReadOnlyMemory<char> memory, ReadOnlySpan<char> separator, StringSplitOptions options = StringSplitOptions.None)
            => SplitAsString(memory.Span, separator, options);
        /// <summary>
        /// Split a char span using a separator
        /// </summary>
        /// <param name="span">Source span</param>
        /// <param name="separator">Char separator</param>
        /// <param name="options">StringSplit options</param>
        /// <returns>List with the split result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<string> SplitAsString(this ReadOnlySpan<char> span, char separator, StringSplitOptions options = StringSplitOptions.None)
        {
            var result = new List<string>();
            while (span.Length > 0)
            {
                var idx = span.IndexOf(separator);
                if (idx == -1)
                    break;
                var value = span.Slice(0, idx);
                if (options == StringSplitOptions.None || value.Length > 0)
                    result.Add(value.ToString());
                span = span.Slice(idx + 1);
            }
            if (options == StringSplitOptions.None || span.Length > 0)
                result.Add(span.ToString());
            return result;
        }
        /// <summary>
        /// Split a char span using a separator
        /// </summary>
        /// <param name="span">Source span</param>
        /// <param name="separator">Char separator</param>
        /// <param name="options">StringSplit options</param>
        /// <returns>List with the split result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<string> SplitAsString(this ReadOnlySpan<char> span, ReadOnlySpan<char> separator, StringSplitOptions options = StringSplitOptions.None)
        {
            var result = new List<string>();
            while (span.Length > 0)
            {
                var idx = span.IndexOf(separator);
                if (idx == -1)
                    break;
                var value = span.Slice(0, idx);
                if (options == StringSplitOptions.None || value.Length > 0)
                    result.Add(value.ToString());
                span = span.Slice(idx + separator.Length);
            }
            if (options == StringSplitOptions.None || span.Length > 0)
                result.Add(span.ToString());
            return result;
        }
    }
}
