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

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TWCore.Diagnostics.Status
{
    /// <summary>
    /// Collection of values for a Status Item
    /// </summary>
    public class StatusItemValuesCollection : List<StatusItemValue>
    {
        /// <summary>
        /// Parent status Item
        /// </summary>
        public StatusItem Parent { get; set; }
        /// <summary>
        /// Adds a new item value
        /// </summary>
        /// <param name="key">Key of the value</param>
        /// <param name="value">Value</param>
        /// <param name="status">Value status</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(string key, object value, StatusItemValueStatus status = StatusItemValueStatus.Unknown)
            => base.Add(new StatusItemValue(key, value?.ToString(), status));
        /// <summary>
        /// Adds a good item value
        /// </summary>
        /// <param name="key">Key of the value</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddGood(string key, object value)
            => Add(key, value?.ToString(), StatusItemValueStatus.Green);
        /// <summary>
        /// Adds a bad item value
        /// </summary>
        /// <param name="key">Key of the value</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBad(string key, object value)
            => Add(key, value?.ToString(), StatusItemValueStatus.Yellow);
        /// <summary>
        /// Adds a ugly item value
        /// </summary>
        /// <param name="key">Key of the value</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddUgly(string key, object value)
            => Add(key, value?.ToString(), StatusItemValueStatus.Red);

        /// <summary>
        /// Sort values
        /// </summary>
        public bool SortValues { get; set; } = true;
    }
}
