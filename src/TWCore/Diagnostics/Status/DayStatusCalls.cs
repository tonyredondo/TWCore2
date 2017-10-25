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
using System.Linq;
using System.Runtime.CompilerServices;
using TWCore.Collections;
// ReSharper disable NotAccessedField.Local
// ReSharper disable MemberCanBePrivate.Local

namespace TWCore.Diagnostics.Status
{
    /// <inheritdoc />
    /// <summary>
    /// Calls status grouped by Day/Hour
    /// </summary>
    public class DayStatusCalls : IStatusItemProvider
    {
        private readonly LRU2QCollection<DateTime, int[]> _values = new LRU2QCollection<DateTime, int[]>(30);

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Name of the Status Item
        /// </summary>
        public string Name { get; set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Calls status grouped by Day/Hour
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DayStatusCalls(string name) => Name = name;
        #endregion

        #region Public Methods
        /// <summary>
        /// Register a call on the status
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Register()
        {
            lock (this)
            {
                var now = Core.Now;
                var dayArray = _values.GetOrAdd(now.Date, _ => new int[24]);
                dayArray[now.Hour]++;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Get's the Status Item instance
        /// </summary>
        /// <returns>StatusItem Instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatusItem GetStatusItem()
        {
            lock (this)
            {
                var baseItem = new StatusItem
                {
                    Name = Name,
                    Values = { SortValues = false }
                };
                var now = Core.Now;

                var daysValuePairs = _values.ToArray().OrderByDescending(i => i.Key).ToArray();
                var total = daysValuePairs.SelectMany(i => i.Value).Sum();
                var daysSums = daysValuePairs.Select(i =>
                {
                    var totalDay = i.Value.Sum();
                    var percentDay = (double)totalDay * 100 / total;
                    return new DaysSum(i.Key, i.Value, totalDay, percentDay);
                }).ToArray();

                foreach (var item in daysSums)
                {
                    var dayItem = new StatusItem
                    {
                        Name = item.Key.ToString("yyyy-MM-dd"),
                        Values = { SortValues = false }
                    };
                    dayItem.Values.Add("Total calls", item.TotalDay, item.Key == now.Date);
                    dayItem.Values.Add("Percent of total calls", item.PercentDay, item.Key == now.Date);
                    for (var i = 0; i < item.Value.Length; i++)
                    {
                        dayItem.Values.Add($"Number of calls at {i}H", item.Value[i], item.Key == now.Date && i == now.Hour);
                    }
                    baseItem.Childrens.Add(dayItem);
                }
                baseItem.Values.AddGood("Total calls", total, true);
                (var average, var standardDeviation) = daysSums.GetAverageAndStdDev(tuple => (double)tuple.TotalDay);
                baseItem.Values.AddGood("Average of calls", average, true);
                baseItem.Values.AddGood("Standard Deviation of calls", standardDeviation, true);
                return baseItem;
            }
        }
        #endregion

        #region Nested Type
        private struct DaysSum
        {
            public DateTime Key;
            public readonly int[] Value;
            public readonly int TotalDay;
            public readonly double PercentDay;

            public DaysSum(DateTime key, int[] value, int totalDay, double percentDay)
            {
                Key = key;
                Value = value;
                TotalDay = totalDay;
                PercentDay = percentDay;
            }
        }
        #endregion
    }
}
