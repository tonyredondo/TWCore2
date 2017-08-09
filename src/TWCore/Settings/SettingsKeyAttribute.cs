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
using System.Runtime.CompilerServices;

namespace TWCore.Settings
{
    /// <summary>
    /// Property settings key attribute for Settings parser
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SettingsKeyAttribute : Attribute
    {
        /// <summary>
        /// Settings key
        /// </summary>
        public string SettingsKey { get; private set; }
        /// <summary>
        /// Get if the container name should be added to the settings key
        /// </summary>
        public bool UseContainerName { get; private set; } = true;

        /// <summary>
        /// Property settings key attribute for Settings parser adding the container name to the key.
        /// </summary>
        /// <param name="key">Settings key</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SettingsKeyAttribute(string key)
        {
            SettingsKey = key;
        }
        /// <summary>
        /// Property settings key attribute for Settings parser
        /// </summary>
        /// <param name="key">Settings key</param>
        /// <param name="useContainerName">True if the container name should be added to the settings key; otherwise, false.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SettingsKeyAttribute(string key, bool useContainerName)
        {
            SettingsKey = key;
            UseContainerName = true;
        }
    }
}
