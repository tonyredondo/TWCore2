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
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;

namespace TWCore.Diagnostics.Log.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// Writes the Logs items using the System.Diagnostics.Trace method
    /// </summary>
    [StatusName("Trace Log")]
    public class TraceLogStorage : ILogStorage
    {
        private static readonly ConcurrentStack<StringBuilder> StringBuilderPool = new ConcurrentStack<StringBuilder>();

        /// <inheritdoc />
        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task WriteAsync(ILogItem item)
        {
            if (!StringBuilderPool.TryPop(out var strBuffer))
                strBuffer = new StringBuilder();
            
            strBuffer.Append(item.Timestamp.GetTimeSpanFormat());
            strBuffer.AppendFormat(string.Format("{0, 11}: ", item.Level));

            if (!string.IsNullOrEmpty(item.GroupName))
                strBuffer.Append(item.GroupName + " - ");

            if (!string.IsNullOrEmpty(item.TypeName))
                strBuffer.AppendFormat(string.Format("<{0}> ", item.TypeName));

            if (!string.IsNullOrEmpty(item.Code))
                strBuffer.Append("[" + item.Code + "] ");

            strBuffer.AppendLine(item.Message);

            if (item.Exception != null)
            {
                strBuffer.AppendLine("Exceptions:\r\n");
                GetExceptionDescription(item.Exception, strBuffer);
            }
            System.Diagnostics.Trace.WriteLine(strBuffer.ToString());
            strBuffer.Clear();
            StringBuilderPool.Push(strBuffer);
            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetExceptionDescription(SerializableException itemEx, StringBuilder sbuilder)
        {
            while (true)
            {
                sbuilder.AppendFormat("\tType: {0}\r\n\tMessage: {1}\r\n\tStack: {2}\r\n\r\n", itemEx.ExceptionType, itemEx.Message.Replace("\r", "\\r").Replace("\n", "\\n"), itemEx.StackTrace);
                if (itemEx.InnerException is null) break;
                itemEx = itemEx.InnerException;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task WriteEmptyLineAsync()
        {
            System.Diagnostics.Trace.WriteLine(string.Empty);
            return Task.CompletedTask;
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose the current object resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
        }
    }
}
