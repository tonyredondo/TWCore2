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
        private static readonly bool HasConsole;
        /// <summary>
        /// Use Color Schema on Console
        /// </summary>
        public static bool UseColor { get; set; } = true;

        #region .ctor Static
        static ConsoleLogStorage()
        {
            HasConsole = ServiceContainer.HasConsole;
            DefaultColor = HasConsole ? Console.ForegroundColor : ConsoleColor.Gray;
        }
        #endregion
        
        private class NonBlockingConsole
        {
            private static BlockingCollection<(string Message, ConsoleColor Color)> _queue = new BlockingCollection<(string Message, ConsoleColor Color)>();

            static NonBlockingConsole()
            {
                var thread = new Thread(ConsoleThread)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Lowest,
                    Name = "Console Thread"
                };
                thread.Start();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void WriteLine(string line, ConsoleColor color) 
                => _queue.Add((line, color));

            private static void ConsoleThread()
            {
                var lastColor = DefaultColor;
                while (true)
                {
                    (var message, var color) = _queue.Take();
                    if (color != lastColor)
                    {
                        Console.ForegroundColor = color;
                        lastColor = color;
                    }
                    Console.Write(message);
                }
            }
        }
        
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task WriteAsync(ILogItem item)
        {
            if (!HasConsole) return Task.CompletedTask;
            string message;
            var color = DefaultColor;
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
                message = StringBuffer.ToString();
                StringBuffer.Clear();
            }
            if (UseColor)
            {
                switch (item.Level)
                {
                    case LogLevel.Error:
                        color = ConsoleColor.Red;
                        break;
                    case LogLevel.Warning:
                        color = ConsoleColor.Yellow;
                        break;
                    case LogLevel.InfoBasic:
                    case LogLevel.InfoMedium:
                    case LogLevel.InfoDetail:
                        color = ConsoleColor.Cyan;
                        break;
                    case LogLevel.Debug:
                        color = ConsoleColor.DarkCyan;
                        break;
                    case LogLevel.LibDebug:
                        color = ConsoleColor.DarkCyan;
                        break;
                    case LogLevel.Stats:
                        color = ConsoleColor.DarkGreen;
                        break;
                    case LogLevel.Verbose:
                        color = ConsoleColor.Gray;
                        break;
                    case LogLevel.LibVerbose:
                        color = ConsoleColor.DarkGray;
                        break;
                }
            }
            NonBlockingConsole.WriteLine(message, color);
            return Task.CompletedTask;
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
