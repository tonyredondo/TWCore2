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

using Microsoft.Extensions.Logging;
using System;
// ReSharper disable InconsistentNaming

namespace TWCore.Web.Logger
{
    /// <inheritdoc />
    /// <summary>
    /// Logger for TWCore Log
    /// </summary>
    [IgnoreStackFrameLog]
    public class TWCoreLogger : ILogger
    {
        private readonly string _name;

        #region .ctor
        /// <summary>
        /// Logger for TWCore Log
        /// </summary>
        /// <param name="name">Category Name</param>
        public TWCoreLogger(string name)
        {
            _name = name;
        }
        #endregion

        #region ILogger
        /// <inheritdoc />
        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>An IDisposable that ends the logical operation scope on dispose.</returns>
        public IDisposable BeginScope<TState>(TState state) => null;
        /// <inheritdoc />
        /// <summary>
        /// Checks if the given logLevel is enabled.
        /// </summary>
        /// <param name="logLevel">level to be checked.</param>
        /// <returns>true if enabled.</returns>
        public bool IsEnabled(LogLevel logLevel) => true;
        /// <inheritdoc />
        /// <summary>
        /// Writes a log entry.
        /// </summary>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">Id of the event.</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a string message of the state and exception.</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Diagnostics.Log.LogLevel cLogLevel;
            switch(logLevel)
            {
                case LogLevel.Critical:
                    cLogLevel = Diagnostics.Log.LogLevel.Error;
                    break;
                case LogLevel.Debug:
                    cLogLevel = Diagnostics.Log.LogLevel.Debug;
                    break;
                case LogLevel.Error:
                    cLogLevel = Diagnostics.Log.LogLevel.Error;
                    break;
                case LogLevel.Information:
                    cLogLevel = Diagnostics.Log.LogLevel.InfoBasic;
                    break;
                case LogLevel.Trace:
                    cLogLevel = Diagnostics.Log.LogLevel.InfoDetail;
                    break;
                case LogLevel.Warning:
                    cLogLevel = Diagnostics.Log.LogLevel.Warning;
                    break;
                case LogLevel.None:
                    return;
                default:
                    cLogLevel = Diagnostics.Log.LogLevel.Verbose;
                    break;
            }
            var type = _name ?? string.Empty;
            var dotIdx = type?.LastIndexOf(".") ?? -1;
            if (dotIdx > -1)
                type = type.Substring(dotIdx + 1);
            
            Core.Log.Write(cLogLevel, eventId.Id.ToString(), formatter(state, exception), "AspNetCore", exception, string.Empty, type);
        }
        #endregion
    }
}
