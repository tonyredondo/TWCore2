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
using System.Threading;
using System.Threading.Tasks;

namespace TWCore.IO
{
    /// <summary>
    /// File IO helpers
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Copy File in Async mode
        /// </summary>
        /// <param name="sourceFile">Source file path</param>
        /// <param name="destinationFile">Destination file path</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task CopyFileAsync(string sourceFile, string destinationFile)
            => CopyFileAsync(sourceFile, destinationFile, false, CancellationToken.None);
        /// <summary>
        /// Copy File in Async mode
        /// </summary>
        /// <param name="sourceFile">Source file path</param>
        /// <param name="destinationFile">Destination file path</param>
        /// <param name="overwrite">True if the destination file should be overwritten</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task CopyFileAsync(string sourceFile, string destinationFile, bool overwrite)
            => CopyFileAsync(sourceFile, destinationFile, overwrite, CancellationToken.None);
        /// <summary>
        /// Copy File in Async mode
        /// </summary>
        /// <param name="sourceFile">Source file path</param>
        /// <param name="destinationFile">Destination file path</param>
        /// <param name="cancellationToken">CancellationToken instance</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task CopyFileAsync(string sourceFile, string destinationFile, CancellationToken cancellationToken)
            => CopyFileAsync(sourceFile, destinationFile, false, cancellationToken);
        /// <summary>
        /// Copy File in Async mode
        /// </summary>
        /// <param name="sourceFile">Source file path</param>
        /// <param name="destinationFile">Destination file path</param>
        /// <param name="overwrite">True if the destination file should be overwritten</param>
        /// <param name="cancellationToken">CancellationToken instance</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task CopyFileAsync(string sourceFile, string destinationFile, bool overwrite, CancellationToken cancellationToken)
        {
            const FileOptions fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            const int bufferSize = 16384;
            using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize, fileOptions))
            {
                using (var destinationStream = new FileStream(destinationFile, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite, bufferSize, fileOptions))
                {
                    await sourceStream.CopyToAsync(destinationStream, bufferSize, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}