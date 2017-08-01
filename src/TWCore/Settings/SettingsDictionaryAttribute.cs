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
    /// Dictionary attribute attribute for MAppSettings parser
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SettingsDictionaryAttribute : Attribute
    {
        /// <summary>
        /// Item separator
        /// </summary>
        public char ItemSeparator { get; private set; }
        /// <summary>
        /// KeyValue separator
        /// </summary>
        public char KeyValueSeparator { get; private set; }

        /// <summary>
        /// Dictionary attribute attribute for MAppSettings parser
        /// </summary>
        /// <param name="itemSeparator">Item separator</param>
        /// <param name="keyValueSeparator">KeyValue separator</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SettingsDictionaryAttribute(char itemSeparator, char keyValueSeparator)
        {
            ItemSeparator = itemSeparator;
            KeyValueSeparator = keyValueSeparator;
        }
    }
}
