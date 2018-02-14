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

namespace TWCore.Diagnostics.Log
{
    /// <summary>
    /// Log item interface
    /// </summary>
    public interface ILogItem
    {
        /// <summary>
        /// Item unique identifier
        /// </summary>
        Guid Id { get; }
        /// <summary>
        /// Environment name
        /// </summary>
        string EnvironmentName { get; }
        /// <summary>
        /// Machine name
        /// </summary>
        string MachineName { get; }
        /// <summary>
        /// Application name
        /// </summary>
        string ApplicationName { get; }
        /// <summary>
        /// Process name
        /// </summary>
        string ProcessName { get; }
        /// <summary>
        /// Process Id
        /// </summary>
        int ProcessId { get; }
        /// <summary>
        /// Assembly name
        /// </summary>
        string AssemblyName { get; }
        /// <summary>
        /// Type name
        /// </summary>
        string TypeName { get; }
        /// <summary>
        /// Line number
        /// </summary>
        int LineNumber { get; }
        /// <summary>
        /// Nivel de log
        /// </summary>
        LogLevel Level { get; }
        /// <summary>
        /// Message code
        /// </summary>
        string Code { get; }
        /// <summary>
        /// Message
        /// </summary>
        string Message { get; }
        /// <summary>
        /// Item timestamp
        /// </summary>
        DateTime Timestamp { get; }
        /// <summary>
        /// Message group name
        /// </summary>
        string GroupName { get; }
        /// <summary>
        /// If is an error log item, the exception object instance
        /// </summary>
        SerializableException Exception { get; }
    }
}
