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
using System.Text;

namespace TWCore.Diagnostics.Log.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// Writes the Logs items using the System.Diagnostics.Trace method
    /// </summary>
    public class TraceLogStorage : ILogStorage
    {
        private readonly StringBuilder _stringBuffer = new StringBuilder(128);

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
                _stringBuffer.Append(item.Timestamp.GetTimeSpanFormat());
                _stringBuffer.AppendFormat(string.Format("{0, 11}: ", item.Level));

                if (!string.IsNullOrEmpty(item.GroupName))
                    _stringBuffer.Append(item.GroupName + " - ");

                if (item.LineNumber > 0)
                    _stringBuffer.AppendFormat(string.Format("<{0};{1:000}> ", item.TypeName, item.LineNumber));
                else if (!string.IsNullOrEmpty(item.TypeName))
                    _stringBuffer.AppendFormat(string.Format("<{0}> ", item.TypeName));

                if (!string.IsNullOrEmpty(item.Code))
                    _stringBuffer.Append("[" + item.Code + "] ");

                _stringBuffer.AppendLine(item.Message);

                if (item.Exception != null)
                {
                    _stringBuffer.AppendLine("Exceptions:\r\n");
                    GetExceptionDescription(item.Exception, _stringBuffer);
                }

                System.Diagnostics.Trace.WriteLine(_stringBuffer.ToString());
                _stringBuffer.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetExceptionDescription(SerializableException itemEx, StringBuilder sbuilder)
        {
            while (true)
            {
                sbuilder.AppendFormat("\tType: {0}\r\n\tMessage: {1}\r\n\tStack: {2}\r\n\r\n", itemEx.ExceptionType, itemEx.Message.Replace("\r", "\\r").Replace("\n", "\\n"), itemEx.StackTrace);
                if (itemEx.InnerException == null) break;
                itemEx = itemEx.InnerException;
            }
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
