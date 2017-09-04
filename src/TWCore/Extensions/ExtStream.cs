/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

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

using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TWCore
{
    /// <summary>
    /// Extension for a Stream
    /// </summary>
    public static partial class Extensions
    {
        #region Read
        /// <summary>
        /// Reads to the end of the stream using a StreamReader implementation.
        /// </summary>
        /// <param name="stream">Stream source</param>
        /// <param name="encoding">Encoding to use to read the stream. UTF-8 if is null</param>
        /// <returns>String on the stream</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TextReadToEnd(this Stream stream, Encoding encoding = null)
        {
            using (var sr = new StreamReader(stream, encoding ?? Encoding.UTF8))
                return sr.ReadToEnd();
        }
        /// <summary>
        /// Reads to the end of the stream using a StreamReader implementation.
        /// </summary>
        /// <param name="stream">Stream source</param>
        /// <param name="encoding">Encoding to use to read the stream. UTF-8 if is null</param>
        /// <returns>String on the stream</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<string> TextReadToEndAsync(this Stream stream, Encoding encoding = null)
        {
            using (var sr = new StreamReader(stream, encoding ?? Encoding.UTF8))
                return await sr.ReadToEndAsync().ConfigureAwait(false);
        }
        /// <summary>
        /// Reads the data from a stream a returns the bytes array
        /// </summary>
        /// <param name="stream">Stream source</param>
        /// <returns>Bytes array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SubArray<byte> ReadBytes(this Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToSubArray();
            }
        }
        /// <summary>
        /// Reads the data from a stream a returns the bytes array
        /// </summary>
        /// <param name="stream">Stream source</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <returns>Bytes array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SubArray<byte> ReadBytes(this Stream stream, int bufferSize)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms, bufferSize);
                return ms.ToSubArray();
            }
        }
        /// <summary>
        /// Reads the data from a stream a returns the bytes array
        /// </summary>
        /// <param name="stream">Stream source</param>
        /// <returns>Bytes array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<SubArray<byte>> ReadBytesAsync(this Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms).ConfigureAwait(false);
                return ms.ToSubArray();
            }
        }
        /// <summary>
        /// Reads the data from a stream a returns the bytes array
        /// </summary>
        /// <param name="stream">Stream source</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <returns>Bytes array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<SubArray<byte>> ReadBytesAsync(this Stream stream, int bufferSize)
        {
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms, bufferSize).ConfigureAwait(false);
                return ms.ToArray();
            }
        }
        /// <summary>
        /// Reads the data from a stream a returns the bytes array
        /// </summary>
        /// <param name="stream">Stream source</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>Bytes array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<SubArray<byte>> ReadBytesAsync(this Stream stream, int bufferSize, int timeout)
        {
            var cts = new CancellationTokenSource();
            using (var ms = new MemoryStream())
            {
                cts.CancelAfter(timeout);
                await stream.CopyToAsync(ms, bufferSize, cts.Token).ConfigureAwait(false);
                return ms.ToSubArray();
            }
        }
        /// <summary>
        /// Reads the data from a stream a returns the bytes array
        /// </summary>
        /// <param name="stream">Stream source</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Bytes array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<SubArray<byte>> ReadBytesAsync(this Stream stream, int bufferSize, CancellationToken cancellationToken)
        {
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms, bufferSize, cancellationToken).ConfigureAwait(false);
                return ms.ToSubArray();
            }
        }
        /// <summary>
        /// Read a string line from a stream
        /// </summary>
        /// <param name="stream">Stream source</param>
        /// <param name="encoding">Encoding to convert bytes to string. Default value is UTF-8</param>
        /// <returns>String line</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadLine(this Stream stream, Encoding encoding = null)
        {
            using (var sr = new StreamReader(stream, encoding ?? Encoding.UTF8))
                return sr.ReadLine();
        }
        /// <summary>
        /// Read a string line from a stream
        /// </summary>
        /// <param name="stream">Stream source</param>
        /// <param name="encoding">Encoding to convert bytes to string. Default value is UTF-8</param>
        /// <returns>String line</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<string> ReadLineAsync(this Stream stream, Encoding encoding = null)
        {
            using (var sr = new StreamReader(stream, encoding ?? Encoding.UTF8))
                return await sr.ReadLineAsync().ConfigureAwait(false);
        }
        #endregion

        #region Write
        /// <summary>
        /// Writes a string value to the stream
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <param name="value">String value to write to the stream</param>
        /// <param name="encoding">Encoding used to write the string. Default value is UTF-8</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteText(this Stream stream, string value, Encoding encoding = null)
        {
            using (var sw = new StreamWriter(stream, encoding ?? Encoding.UTF8, 4096, true))
                sw.Write(value);
        }
        /// <summary>
        /// Writes a string value to the stream
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <param name="value">String value to write to the stream</param>
        /// <param name="encoding">Encoding used to write the string. Default value is UTF-8</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WriteTextAsync(this Stream stream, string value, Encoding encoding = null)
        {
            using (var sw = new StreamWriter(stream, encoding ?? Encoding.UTF8, 4096, true))
                await sw.WriteAsync(value).ConfigureAwait(false);
        }
        /// <summary>
        /// Writes a string line to the stream
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <param name="line">String line to write to the stream</param>
        /// <param name="encoding">Encoding used to write the string. Default value is UTF-8</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLine(this Stream stream, string line, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            var bytes = encoding.GetBytes(line + "\r\n");
            stream.Write(bytes, 0, bytes.Length);
        }
        /// <summary>
        /// Writes a string line to the stream
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <param name="line">String line to write to the stream</param>
        /// <param name="encoding">Encoding used to write the string. Default value is UTF-8</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WriteLineAsync(this Stream stream, string line, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            var bytes = encoding.GetBytes(line + "\r\n");
            await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }
        /// <summary>
        /// Writes a byte array to the stream
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <param name="value">Byte array to write to the stream</param>
        /// <param name="flush">true if a flush must be applied after write the string value. Default value is true</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream stream, byte[] value, bool flush = true)
        {
            stream.Write(value, 0, value.Length);
            if (flush)
                stream.Flush();
        }
        /// <summary>
        /// Writes a byte array to the stream
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <param name="value">Byte array to write to the stream</param>
        /// <param name="flush">true if a flush must be applied after write the string value. Default value is true</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream stream, SubArray<byte> value, bool flush = true)
        {
            stream.Write(value.Array, value.Offset, value.Count);
            if (flush)
                stream.Flush();
        }
        /// <summary>
        /// Writes a byte array to the stream
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <param name="value">Byte array to write to the stream</param>
        /// <param name="flush">true if a flush must be applied after write the string value. Default value is true</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WriteBytesAsync(this Stream stream, byte[] value, bool flush = true)
        {
            await stream.WriteAsync(value, 0, value.Length).ConfigureAwait(false);
            if (flush)
                await stream.FlushAsync().ConfigureAwait(false);
        }
        /// <summary>
        /// Writes a byte array to the stream
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <param name="value">Byte array to write to the stream</param>
        /// <param name="flush">true if a flush must be applied after write the string value. Default value is true</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WriteBytesAsync(this Stream stream, SubArray<byte> value, bool flush = true)
        {
            await stream.WriteAsync(value.Array, value.Offset, value.Count).ConfigureAwait(false);
            if (flush)
                await stream.FlushAsync().ConfigureAwait(false);
        }
        /// <summary>
        /// Write the string content to a stream destination using buffered reading/writing
        /// </summary>
        /// <param name="source">Stream source</param>
        /// <param name="destination">Stream destination</param>
        /// <param name="length">Length of bytes to write. 0 = not defined.</param>
        /// <param name="bufferSize">Size of the buffer array</param>
        /// <param name="timeOutToReadBytes">Timeout to read bytes</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteToStream(this Stream source, Stream destination, int length = 0, int bufferSize = 4096, int timeOutToReadBytes = 0)
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
            int toRead = length;
            bool nolength = (toRead <= 0);
            while (toRead > 0 || nolength)
            {
                if (timeOutToReadBytes > 0)
                {
                    var rTask = source.ReadAsync(buffer, 0, buffer.Length);
                    bytesRead = rTask.Wait(timeOutToReadBytes) ? rTask.Result : -1;
                }
                else
                    bytesRead = source.Read(buffer, 0, buffer.Length);
                if (bytesRead < 1)
                    break;
                toRead -= bytesRead;
                destination.Write(buffer, 0, bytesRead);
            }
        }
        /// <summary>
        /// Write the string content to a stream destination using buffered reading/writing
        /// </summary>
        /// <param name="source">Stream source</param>
        /// <param name="destination">Stream destination</param>
        /// <param name="length">Length of bytes to write. 0 = not defined.</param>
        /// <param name="bufferSize">Size of the buffer array</param>
        /// <param name="timeOutToReadBytes">Timeout to read bytes</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WriteToStreamAsync(this Stream source, Stream destination, int length = 0, int bufferSize = 4096, int timeOutToReadBytes = 0)
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
            int toRead = length;
            bool nolength = (toRead <= 0);
            while (toRead > 0 || nolength)
            {
                if (timeOutToReadBytes > 0)
                {
                    var rTask = source.ReadAsync(buffer, 0, buffer.Length);
                    bytesRead = rTask.Wait(timeOutToReadBytes) ? rTask.Result : -1;
                }
                else
                    bytesRead = await source.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                if (bytesRead < 1)
                    break;
                toRead -= bytesRead;
                await destination.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
            }
        }
        #endregion
    }
}
