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

namespace TWCore.Diagnostics.Log
{
    /// <summary>
    /// Creates a new log item
    /// </summary>
    /// <param name="level">Item log level</param>
    /// <param name="code">Item code</param>
    /// <param name="message">Item message</param>
    /// <param name="groupName">Item group name</param>
    /// <param name="ex">Item exception</param>
    /// <returns>LogItem instance</returns>
    public delegate ILogItem CreateLogItemDelegate(LogLevel level, string code, string message, string groupName, Exception ex, string assemblyName, string typeName);

    /// <summary>
    /// Create new log engine delegate
    /// </summary>
    /// <returns>New Log engine</returns>
    public delegate ILogEngine CreateLogEngineDelegate();
}
