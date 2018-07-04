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
        public bool EnableFormatMapping { get; set; } = true;
        public bool EnableTWCoreSerializers { get; set; } = true;
        public bool EnableJsonStringEnum { get; set; } = true;
        public bool EnableTWCoreLogger { get; set; } = true;
        public bool EnableGZipCompressor { get; set; } = true;
        public bool UseCustomXmlSerializer { get; set; } = false;
    }
}
