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

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Services;
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable SwitchStatementMissingSomeCases

namespace TWCore.Diagnostics.Log.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// Writes the Logs items using the Console.Writeline method
    /// </summary>
    [StatusName("Console Log")]
    public class ConsoleLogStorage : ILogStorage
    {
        private static readonly ConsoleColor DefaultColor;
        private static readonly ConcurrentStack<StringBuilder> StringBuilderPool;
        private static readonly bool HasConsole;
        private readonly NonBlockingConsole nonBlockingConsole = new NonBlockingConsole();

        #region Properties
        /// <summary>
        /// Use Color Schema on Console
        /// </summary>
        public static bool UseColor { get; set; } = true;
        #endregion

        #region .ctor Static
        static ConsoleLogStorage()
        {
            HasConsole = ServiceContainer.HasConsole;
            if (HasConsole)
            {
                Console.ResetColor();
                DefaultColor = Console.ForegroundColor;
            }
            else
            {
                DefaultColor = ConsoleColor.Gray;
            }
            StringBuilderPool = new ConcurrentStack<StringBuilder>();
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetExceptionDescription(SerializableException itemEx, StringBuilder sbuilder)
        {
            while (true)
            {
                if (itemEx.Data == null || itemEx.Data.Count == 0)
                    sbuilder.AppendFormat("\tType: {0}\r\n\tMessage: {1}\r\n\tStack: {2}\r\n\r\n", itemEx.ExceptionType, itemEx.Message.Replace("\r", "\\r").Replace("\n", "\\n"), itemEx.StackTrace);
                else
                {
                    sbuilder.AppendFormat("\tType: {0}\r\n\tMessage: {1}\r\n\tData:\r\n",
                        itemEx.ExceptionType,
                        itemEx.Message.Replace("\r", "\\r").Replace("\n", "\\n"));

                    foreach (var dataItem in itemEx.Data)
                        sbuilder.AppendFormat("\t\t{0}: {1}\r\n", dataItem.Key, dataItem.Value);

                    sbuilder.AppendFormat("\tStack: {0}\r\n\r\n",
                        itemEx.StackTrace);
                }
                if (itemEx.InnerException is null)
                    break;
                itemEx = itemEx.InnerException;
            }
        }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task WriteAsync(ILogItem item)
        {
            if (!HasConsole) return Task.CompletedTask;
            if (!StringBuilderPool.TryPop(out var strBuffer))
                strBuffer = new StringBuilder();

            strBuffer.Append(item.Timestamp.GetTimeSpanFormat());
            strBuffer.AppendFormat("{0, 11}: ", item.Level);

            if (!string.IsNullOrEmpty(item.GroupName))
                strBuffer.Append(item.GroupName + " | ");

            if (!string.IsNullOrEmpty(item.TypeName))
                strBuffer.AppendFormat("<{0}> ", item.TypeName);

            if (!string.IsNullOrEmpty(item.Code))
                strBuffer.Append("[" + item.Code + "] ");

            strBuffer.AppendLine(item.Message);
            if (item.Exception != null)
            {
                strBuffer.AppendLine("Exceptions:\r\n");
                GetExceptionDescription(item.Exception, strBuffer);
            }
            var message = strBuffer.ToString();
            var color = DefaultColor;
            strBuffer.Clear();
            StringBuilderPool.Push(strBuffer);

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
            nonBlockingConsole.Write(message, color);
            return Task.CompletedTask;
        }
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task WriteEmptyLineAsync()
        {
            nonBlockingConsole.Write("\r\n", DefaultColor);
            return Task.CompletedTask;
        }
        /// <summary>
        /// Writes a group metadata item to the storage
        /// </summary>
        /// <param name="item">Group metadata item</param>
        /// <returns>Task process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task WriteAsync(IGroupMetadata item)
        {
            if (!HasConsole) return Task.CompletedTask;
            if (item == null || string.IsNullOrWhiteSpace(item.GroupName)) return Task.CompletedTask;

            if (!StringBuilderPool.TryPop(out var strBuffer))
                strBuffer = new StringBuilder();

            strBuffer.Append(item.Timestamp.GetTimeSpanFormat());
            strBuffer.AppendFormat("{0, 11}: ", "GroupInfo");
            strBuffer.Append(item.GroupName + " | ");
            if (item.Items != null)
            {
                strBuffer.Append("[");
                var count = item.Items.Length;
                for (var i = 0; i < count; i++)
                {
                    var keyValue = item.Items[i];
                    strBuffer.AppendFormat("{0} = {1}", keyValue.Key, keyValue.Value);
                    if (i < count - 1)
                        strBuffer.Append(", ");
                }
                strBuffer.Append("] ");
            }
            strBuffer.AppendLine();

            var message = strBuffer.ToString();
            var color = DefaultColor;
            strBuffer.Clear();
            StringBuilderPool.Push(strBuffer);

            if (UseColor)
                color = ConsoleColor.Cyan;
            nonBlockingConsole.Write(message, color);
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
            nonBlockingConsole.Dispose();
        }
        #endregion

        #region Nested Types

        private class NonBlockingConsole
        {
            private BlockingCollection<(string Message, ConsoleColor Color)> _queue = new BlockingCollection<(string Message, ConsoleColor Color)>();
            private CancellationTokenSource _tokenSource = new CancellationTokenSource();
            private ManualResetEventSlim _disposeEvent = new ManualResetEventSlim();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NonBlockingConsole()
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
            public void Write(string line, ConsoleColor color)
            {
                if (!_tokenSource.IsCancellationRequested)
                    _queue.Add((line, color));
            }

            private void ConsoleThread()
            {
                var lastColor = DefaultColor;
                var token = _tokenSource.Token;
                while (true)
                {
                    string message;
                    ConsoleColor color;
                    if (!token.IsCancellationRequested)
                    {
                        try
                        {
                            (message, color) = _queue.Take(token);
                        }
                        catch (OperationCanceledException)
                        {
                            continue;
                        }
                    }
                    else if (_queue.TryTake(out var mc))
                    {
                        message = mc.Message;
                        color = mc.Color;
                    }
                    else
                        break;
                    if (color != lastColor)
                    {
                        Console.ForegroundColor = color;
                        lastColor = color;
                    }
                    Console.Write(message);
                }
                _disposeEvent.Set();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _tokenSource.Cancel();
                _disposeEvent.Wait();
            }
        }

        #endregion
    }
}
