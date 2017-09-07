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

using System;
using System.Runtime.CompilerServices;

namespace TWCore.Diagnostics.Log.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// Writes the Logs items using the System.Diagnostics.Trace method
    /// </summary>
    public class TraceLogStorage : ILogStorage
    {
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ILogItem item)
        {
            lock(Console.Out) 
            {
                System.Diagnostics.Trace.Write(item.Timestamp.GetTimeSpanFormat());
                System.Diagnostics.Trace.Write(string.Format(" ({0:000}) ", item.ThreadId));
                System.Diagnostics.Trace.Write(string.Format( "{0, 10}: ", item.Level));

                if (!string.IsNullOrEmpty(item.GroupName))
                    System.Diagnostics.Trace.Write(item.GroupName + " ");

                if (item.LineNumber > 0)
                    System.Diagnostics.Trace.Write(string.Format("<{0};{1:000}> ", item.TypeName, item.LineNumber));
                else if (!string.IsNullOrEmpty(item.TypeName))
                    System.Diagnostics.Trace.Write(string.Format("<{0}> ", item.TypeName));

                if (!string.IsNullOrEmpty(item.Code))
                    System.Diagnostics.Trace.Write("[" + item.Code + "] ");

                System.Diagnostics.Trace.WriteLine(item.Message);
                if (item.Exception == null) return;
                System.Diagnostics.Trace.WriteLine("Exceptions:\r\n");
                System.Diagnostics.Trace.WriteLine(GetExceptionDescription(item.Exception));
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetExceptionDescription(SerializableException itemEx)
        {
            var desc = string.Format("\tType: {0}\r\n\tMessage: {1}\r\n\tStack: {2}\r\n", itemEx.ExceptionType, itemEx.Message.Replace("\r", "\\r").Replace("\n", "\\n"), itemEx.StackTrace);
            if (itemEx.InnerException != null)
                desc += GetExceptionDescription(itemEx.InnerException);
            return desc;
        }
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEmptyLine()
        {
            System.Diagnostics.Trace.WriteLine(string.Empty);
        }
        /// <summary>
        /// Dispose the current object resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
        }
    }
}
