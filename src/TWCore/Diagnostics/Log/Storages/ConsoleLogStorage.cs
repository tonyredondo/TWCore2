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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Services;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable SwitchStatementMissingSomeCases

namespace TWCore.Diagnostics.Log.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// Writes the Logs items using the Console.Writeline method
    /// </summary>
    public class ConsoleLogStorage : ILogStorage
    {
        private static readonly object PadLock = new object();
        private static readonly ConsoleColor DefaultColor;
        private static readonly StringBuilder StringBuffer = new StringBuilder();
        private static LogLevel _lastLogLevel;

        /// <summary>
        /// Use Color Schema on Console
        /// </summary>
        public static bool UseColor { get; set; } = true;

        #region .ctor Static
        static ConsoleLogStorage()
        {
            DefaultColor = ServiceContainer.HasConsole ? Console.ForegroundColor : ConsoleColor.Gray;
        }
        #endregion
        
        
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task WriteAsync(ILogItem item)
        {
            if (!ServiceContainer.HasConsole) return Task.CompletedTask;
            string buffer;
            lock(PadLock)
            {
                StringBuffer.Append(item.Timestamp.GetTimeSpanFormat());
                StringBuffer.AppendFormat("{0, 11}: ", item.Level);

                if (!string.IsNullOrEmpty(item.GroupName))
                    StringBuffer.Append(item.GroupName + " | ");

                if (item.LineNumber > 0)
                    StringBuffer.AppendFormat("<{0};{1:000}> ", string.IsNullOrEmpty(item.TypeName) ? string.Empty : item.TypeName, item.LineNumber);
                else if (!string.IsNullOrEmpty(item.TypeName))
                    StringBuffer.AppendFormat("<{0}> ", item.TypeName);

                if (!string.IsNullOrEmpty(item.Code))
                    StringBuffer.Append("[" + item.Code + "] ");

                StringBuffer.AppendLine(item.Message);
                if (item.Exception != null)
                {
                    StringBuffer.AppendLine("Exceptions:\r\n");
                    GetExceptionDescription(item.Exception, StringBuffer);
                }
                buffer = StringBuffer.ToString();
                StringBuffer.Clear();

                if (item.Level != _lastLogLevel && UseColor)
                {
                    switch (item.Level)
                    {
                        case LogLevel.Error:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case LogLevel.Warning:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        case LogLevel.InfoBasic:
                        case LogLevel.InfoMedium:
                        case LogLevel.InfoDetail:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            break;
                        case LogLevel.Debug:
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            break;
                        case LogLevel.LibDebug:
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            break;
                        case LogLevel.Stats:
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            break;
                        case LogLevel.Verbose:
                            Console.ForegroundColor = ConsoleColor.Gray;
                            break;
                        case LogLevel.LibVerbose:
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            break;
                        default:
                            Console.ForegroundColor = DefaultColor;
                            break;
                    }
                }
            }

            var cTask = Console.Out.WriteAsync(buffer);
            return cTask.ContinueWith(_ => _lastLogLevel = item.Level, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetExceptionDescription(SerializableException itemEx, StringBuilder sbuilder)
        {
            while (true)
            {
                sbuilder.AppendFormat("\tType: {0}\r\n\tMessage: {1}\r\n\tStack: {2}\r\n\r\n", itemEx.ExceptionType, itemEx.Message.Replace("\r", "\\r").Replace("\n", "\\n"), itemEx.StackTrace);
                if (itemEx.InnerException == null)
                    break;
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
            lock (PadLock)
                Console.WriteLine();
            return Task.CompletedTask;
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose the current object resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (ServiceContainer.HasConsole && UseColor)
                Console.ForegroundColor = DefaultColor;
        }
    }
}
