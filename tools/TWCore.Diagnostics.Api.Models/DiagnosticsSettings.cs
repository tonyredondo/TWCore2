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

// ReSharper disable UnusedMember.Global

using System;
using TWCore.Settings;

namespace TWCore.Diagnostics
{
    /// <summary>
    /// Diagnostics settings
    /// </summary>
    public class DiagnosticsSettings : SettingsBase
    {
        /// <summary>
        /// Gets or Sets if the traces should be stored to disk
        /// </summary>
        public bool StoreTracesToDisk { get; set; } = false;
        /// <summary>
        /// Traces folder path
        /// </summary>
        public string TracesFolderPath { get; set; } = Environment.CurrentDirectory;
        /// <summary>
        /// Binary data
        /// </summary>
        public bool WriteInBinary { get; set; } = false;
        /// <summary>
        /// Xml data
        /// </summary>
        public bool WriteInXml { get; set; } = true;
        /// <summary>
        /// Json data
        /// </summary>
        public bool WriteInJson { get; set; } = true;
        /// <summary>
        /// Force write binary on app
        /// </summary>
        [SettingsArray(';')]
        public string[] ForceBinaryOnApp { get; set; } = new string[0];
        /// <summary>
        /// Force write xml on app
        /// </summary>
        [SettingsArray(';')]
        public string[] ForceXmlOnApp { get; set; } = new string[0];
        /// <summary>
        /// Force write json on app
        /// </summary>
        [SettingsArray(';')]
        public string[] ForceJsonOnApp { get; set; } = new string[0];
        /// <summary>
        /// Parallel messaging process
        /// </summary>
        public int ParallelMessagingProcess { get; set; } = 5;
        /// <summary>
        /// Process timer in seconds
        /// </summary>
        public int ProcessTimerInSeconds { get; set; } = 10;
    }
}