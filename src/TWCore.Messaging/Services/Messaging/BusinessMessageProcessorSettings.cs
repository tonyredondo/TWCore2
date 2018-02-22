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

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CheckNamespace

namespace TWCore.Services.Messaging
{
    /// <inheritdoc />
    /// <summary>
    /// Business message processor settings
    /// </summary>
    public class BusinessMessageProcessorSettings : SettingsBase
    {
        /// <summary>
        /// Business item initial count in percent of the total maximum items.
        /// </summary>
        public float InitialBusinessesPercent { get; set; } = 0.3F;
    }
}
