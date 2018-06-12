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
using TWCore.Diagnostics.Trace.Storages;
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Trace
{
    /// <inheritdoc />
    /// <summary>
    /// Interface for Trace engine
    /// </summary>
    public interface ITraceEngine : IDisposable
    {
        /// <summary>
        /// Trace storages items
        /// </summary>
        TraceStorageCollection Storage { get; }
        /// <summary>
        /// Gets or sets the trace item factory
        /// </summary>
        CreateTraceItemDelegate ItemFactory { get; set; }
        /// <summary>
        /// Write a trace item into the trace storages
        /// </summary>
        /// <param name="item">Trace item</param>
        void Write(TraceItem item);

        /// <summary>
        /// Write a trace item into the trace storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="traceName">Trace name</param>
        /// <param name="traceObject">Trace object</param>
        /// <param name="idstags">Identifiers tags</param>
        void Write(string groupName, string traceName, object traceObject, params Guid[] idstags);
        /// <summary>
        /// Write a trace item into the trace storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="traceName">Trace name</param>
        /// <param name="traceObject">Trace object</param>
        void Write(string groupName, string traceName, object traceObject);
        /// <summary>
        /// Write a trace item into the trace storages
        /// </summary>
        /// <param name="traceName">Trace name</param>
        /// <param name="traceObject">Trace object</param>
        void Write(string traceName, object traceObject);
        /// <summary>
        /// Write a trace item into the trace storages
        /// </summary>
        /// <param name="traceObject">Trace object</param>
        void Write(object traceObject);


        /// <summary>
        /// Write a trace item into the trace storages if is in Debug Mode
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="traceName">Trace name</param>
        /// <param name="traceObject">Trace object</param>
        /// <param name="idstags">Identifiers tags</param>
        void WriteDebug(string groupName, string traceName, object traceObject, params Guid[] idstags);
        /// <summary>
        /// Write a trace item into the trace storages if is in Debug Mode
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="traceName">Trace name</param>
        /// <param name="traceObject">Trace object</param>
        void WriteDebug(string groupName, string traceName, object traceObject);
        /// <summary>
        /// Write a trace item into the trace storages if is in Debug Mode
        /// </summary>
        /// <param name="traceName">Trace name</param>
        /// <param name="traceObject">Trace object</param>
        void WriteDebug(string traceName, object traceObject);
        /// <summary>
        /// Write a trace item into the trace storages if is in Debug Mode
        /// </summary>
        /// <param name="traceObject">Trace object</param>
        void WriteDebug(object traceObject);

        /// <summary>
        /// Enable or Disable the Trace engine
        /// </summary>
        bool Enabled { get; set; }
    }
}
