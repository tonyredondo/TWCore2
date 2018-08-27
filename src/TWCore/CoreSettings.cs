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
using TWCore.Settings;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore
{
    /// <inheritdoc />
    /// <summary>
    /// Global Core Settings
    /// </summary>
    [SettingsContainer("Core")]
    public class CoreSettings : SettingsBase
    {
        /// <summary>
        /// Debug Mode
        /// </summary>
        public bool DebugMode { get; set; }

        /// <summary>
        /// Settings reload time in minutes
        /// </summary>
        [SettingsKey("SettingsReloadTimeInMinutes")]
        public int SettingsReloadTimeInMinutes { get; set; } = 0;


        //Discovery Settings
        /// <summary>
        /// Enable Discovery
        /// </summary>
        [SettingsKey("Discovery.Enabled")]
        public bool EnableDiscovery { get; set; } = true;
        /// <summary>
        /// Discovery Port number
        /// </summary>
        [SettingsKey("Discovery.Port")]
        public int DiscoveryPort { get; private set; } = 64128;
        /// <summary>
        /// Discovery Multicast Ip Address
        /// </summary>
        [SettingsKey("Discovery.MulticastIp")]
        public string DiscoveryMulticastIp { get; private set; } = "230.23.12.83";
        /// <summary>
        /// Discovery SerializerMimeType
        /// </summary>
        [SettingsKey("Discovery.SerializerMimeType")]
        public string DiscoverySerializerMimeType { get; private set; }
        /// <summary>
        /// Discovery Disable Receive thread
        /// </summary>
        [SettingsKey("Discovery.DisableReceive")]
        public bool DiscoveryDisableReceive { get; private set; } = false;


        //Worker Settings
        /// <summary>
        /// Default Worker WaitTimeout
        /// </summary>
        [SettingsKey("Worker.WaitTimeout")]
        public int WorkerWaitTimeout { get; set; } = 10000;

        //Log Settings
        /// <summary>
        /// Gets or sets the maximum log level to write
        /// </summary>
        [SettingsKey("Log.MaxLogLevel")]
        public int LogMaxLogLevel { get; set; } = 1023;
        /// <summary>
        /// Gets or sets the maximum items quantity on the default log engine
        /// </summary>
        [SettingsKey("Log.MaximumItemsInQueue")]
        public int LogMaximumItemsInQueue { get; set; } = 2500;
        /// <summary>
        /// Enable or disable the default log engine
        /// </summary>
        [SettingsKey("Log.Enabled")]
        public bool LogEnabled { get; set; } = true;


        //Status Settings
        /// <summary>
        /// Enable or disable the default status engine
        /// </summary>
        [SettingsKey("Status.Enabled")]
        public bool StatusEnabled { get; set; } = true;


        //Trace Settings
        /// <summary>
        /// Enable or disable the default trace engine
        /// </summary>
        [SettingsKey("Trace.Enabled")]
        public bool TraceEnabled { get; set; } = true;
        /// <summary>
        /// Trace object serializer
        /// </summary>
        [SettingsKey("Trace.Serializer")]
        public string TraceSerializer { get; set; }
        /// <summary>
        /// Trace object serializer compressor
        /// </summary>
        [SettingsKey("Trace.Compressor")]
        public string TraceCompressor { get; set; }
        /// <summary>
        /// Trace serializer type
        /// </summary>
        public Type TraceSerializerType => Core.GetType(TraceSerializer) ?? typeof(Serialization.XmlTextSerializer);
        /// <summary>
        /// Trace compressor type
        /// </summary>
        public Type TraceCompressorType => Core.GetType(TraceCompressor);
        /// <summary>
        /// Trace queue limit
        /// </summary>
        public int TraceQueueLimit { get; set; } = 100;

        //Injector
        /// <summary>
        /// Use only assemblies loaded, false if uses all assemblies in the folder
        /// </summary>
        [SettingsKey("Injector.UseOnlyLoadedAssemblies")]
        public bool InjectorUseOnlyLoadedAssemblies { get; set; } = true;

        /// <summary>
        /// Timeout in minutes to set the LargeObjectHeap (LOH) Compact in the Garbage Collector. 
        /// Default = 60
        /// </summary>
        [SettingsKey("LargeObjectHeapCompactTimeoutInMinutes")]
        public int LargeObjectHeapCompactTimeoutInMinutes { get; set; } = 60;
    }
}
