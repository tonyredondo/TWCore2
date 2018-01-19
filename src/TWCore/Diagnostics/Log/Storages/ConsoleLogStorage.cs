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
        public void Write(ILogItem item)
        {
            if (!ServiceContainer.HasConsole) return;
            lock(PadLock) 
            {
                if (UseColor)
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

                Console.Write(item.Timestamp.GetTimeSpanFormat());
                Console.Write("{0, 10}: ",item.Level);

                if (!string.IsNullOrEmpty(item.GroupName))
                    Console.Write(item.GroupName + " | ");

                if (item.LineNumber > 0)
                    Console.Write("<{0};{1:000}> ", string.IsNullOrEmpty(item.TypeName) ? string.Empty : item.TypeName, item.LineNumber);
                else if (!string.IsNullOrEmpty(item.TypeName))
                    Console.Write("<{0}> ", item.TypeName);

                if (!string.IsNullOrEmpty(item.Code))
                    Console.Write("[" + item.Code + "] ");

                Console.WriteLine(item.Message);
                if (item.Exception != null)
                {
                    Console.WriteLine("Exceptions:\r\n");
                    Console.WriteLine(GetExceptionDescription(item.Exception));
                }
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
            lock (PadLock)
                Console.WriteLine();
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
