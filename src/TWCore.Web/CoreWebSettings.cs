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

using TWCore.Settings;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Web
{
    /// <inheritdoc />
    /// <summary>
    /// Core Web Settings
    /// </summary>
    public class CoreWebSettings : SettingsBase
    {
        /// <summary>
        /// Enable format mapping
        /// </summary>
        public bool EnableFormatMapping { get; set; } = true;
        /// <summary>
        /// Enable TWCore Serializers
        /// </summary>
        public bool EnableTWCoreSerializers { get; set; } = true;
        /// <summary>
        /// Enable JSon Strings values on enums
        /// </summary>
        public bool EnableJsonStringEnum { get; set; } = true;
        /// <summary>
        /// Enable TWCore Logger
        /// </summary>
        public bool EnableTWCoreLogger { get; set; } = true;
        /// <summary>
        /// Enable GZip Compressor
        /// </summary>
        public bool EnableGZipCompressor { get; set; } = true;
        /// <summary>
        /// Use custom xml serializer
        /// </summary>
        public bool UseCustomXmlSerializer { get; set; } = false;
    }
}
