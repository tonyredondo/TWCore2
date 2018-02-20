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

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
// ReSharper disable InconsistentNaming

namespace TWCore.Web.Logger
{
    /// <inheritdoc />
    /// <summary>
    /// Logger provider for TWCoreLogger
    /// </summary>
    [IgnoreStackFrameLog]
    public class TWCoreLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, TWCoreLogger> _loggers = new ConcurrentDictionary<string, TWCoreLogger>();

        /// <inheritdoc />
        /// <summary>
        /// Creates a new Microsoft.Extensions.Logging.ILogger instance.
        /// </summary>
        /// <param name="categoryName">The category name for messages produced by the logger.</param>
        /// <returns>Microsoft.Extensions.Logging.ILogger instance</returns>
        public ILogger CreateLogger(string categoryName)
            => _loggers.GetOrAdd(categoryName, name => new TWCoreLogger(name));

        public void Dispose() 
            => _loggers.Clear();
    }
}
