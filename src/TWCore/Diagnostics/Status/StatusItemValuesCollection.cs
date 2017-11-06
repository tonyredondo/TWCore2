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
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace TWCore.Diagnostics.Status
{
    /// <inheritdoc />
    /// <summary>
    /// Collection of values for a Status Item
    /// </summary>
    public class StatusItemValuesCollection : List<StatusItemValue>
    {
        /// <summary>
        /// Sort values
        /// </summary>
        public bool SortValues { get; set; } = true;

        public StatusItemValuesCollection()
        {
        }
        public StatusItemValuesCollection(IEnumerable<StatusItemValue> col) : base(col)
        {
        }
        
        /// <summary>
        /// Adds a new item value
        /// </summary>
        /// <param name="key">Key of the value</param>
        /// <param name="value">Value</param>
        /// <param name="status">Value status</param>
        /// <param name="plotEnabled">Enabled to plot</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(string key, object value, StatusItemValueStatus status = StatusItemValueStatus.Unknown, bool plotEnabled = false)
            => Add(new StatusItemValue(key, value, status, plotEnabled));
        /// <summary>
        /// Adds a new item value
        /// </summary>
        /// <param name="key">Key of the value</param>
        /// <param name="values">Values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(string key, params StatusItemValueItem[] values)
            => Add(new StatusItemValue(key, values));
        /// <summary>
        /// Adds a new item value
        /// </summary>
        /// <param name="key">Key of the value</param>
        /// <param name="value">Value</param>
        /// <param name="plotEnabled">Enabled to plot</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(string key, object value, bool plotEnabled)
            => Add(new StatusItemValue(key, value, StatusItemValueStatus.Unknown, plotEnabled));
        /// <summary>
        /// Adds a Ok item value
        /// </summary>
        /// <param name="key">Key of the value</param>
        /// <param name="value">Value</param>
        /// <param name="plotEnabled">Enabled to plot</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddOk(string key, object value, bool plotEnabled = false)
            => Add(key, value, StatusItemValueStatus.Ok, plotEnabled);
        /// <summary>
        /// Adds a warning item value
        /// </summary>
        /// <param name="key">Key of the value</param>
        /// <param name="value">Value</param>
        /// <param name="plotEnabled">Enabled to plot</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddWarning(string key, object value, bool plotEnabled = false)
            => Add(key, value, StatusItemValueStatus.Warning, plotEnabled);
        /// <summary>
        /// Adds a red item value
        /// </summary>
        /// <param name="key">Key of the value</param>
        /// <param name="value">Value</param>
        /// <param name="plotEnabled">Enabled to plot</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddError(string key, object value, bool plotEnabled = false)
            => Add(key, value, StatusItemValueStatus.Error, plotEnabled);

    }
}
