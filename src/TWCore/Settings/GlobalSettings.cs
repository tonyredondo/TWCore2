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

using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Settings
{
    /// <summary>
    /// Global settings configuration
    /// </summary>
    [XmlRoot("Settings"), DataContract]
    public class GlobalSettings
    {
        /// <summary>
        /// Global default settings for all applications
        /// </summary>
        [XmlElement("Global"), DataMember]
        public SettingsSet Global { get; set; } = new SettingsSet();
        /// <summary>
        /// Collection of applications settings
        /// </summary>
        [XmlElement("AppSettings"), DataMember]
        public NameCollection<AppSettings> Applications { get; set; } = new NameCollection<AppSettings>();

        /// <summary>
        /// Get all items by environment and application name
        /// </summary>
        /// <param name="environmentName">Environment name</param>
        /// <param name="machineName">Machine name</param>
        /// <param name="applicationName">Application name</param>
        /// <returns>KeyValueCollection with all items for the environment and application</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValueCollection GetItems(string environmentName, string machineName, string applicationName)
        {
            var globals = Global?.GetItems(environmentName, machineName);
            if (globals != null)
                globals.ThrowExceptionOnDuplicateKeys = false;
            if (Applications is null) return globals;

            var results = new KeyValueCollection
            {
                ThrowExceptionOnDuplicateKeys = false
            };

            var appsNames = applicationName?.SplitAndTrim(",") ?? new string[0];
            foreach(var appName in appsNames)
            {
                if (Applications.TryGetByPartialKey(appName, out var appSetting))
                {
                    var items = appSetting.GetItems(environmentName, machineName);
                    items?.Each(i => results.Add(i));
                }
                else
                    Core.Log.Warning("The Settings for ApplicationName = {0} cannot be found.", applicationName);
            }
            globals?.Each(i => results.Add(i));
            return results;
        }
    }
}
