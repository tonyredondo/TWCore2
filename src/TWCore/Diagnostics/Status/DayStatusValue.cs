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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TWCore.Collections;
// ReSharper disable NotAccessedField.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable AccessToModifiedClosure

namespace TWCore.Diagnostics.Status
{
    /// <inheritdoc />
    /// <summary>
    /// Value status grouped by Day/Hour
    /// </summary>
    public class DayStatusValue<T> : IStatusItemProvider
    {
        private readonly LRU2QCollection<DateTime, List<T>[]> _values = new LRU2QCollection<DateTime, List<T>[]>(30);
        private readonly object _locker = new object();

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Name of the Status Item
        /// </summary>
        public string Name { get; set; }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Value status grouped by Day/Hour
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DayStatusValue(string name) => Name = name;
        #endregion

        #region Public Methods
        /// <summary>
        /// Register a value on the status
        /// </summary>
        /// <param name="value">Value to register</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Register(T value)
        {
            lock (_locker)
            {
                var now = Core.Now;
                var lists = _values.GetOrAdd(now.Date, _ =>
                {
                    var lst = new List<T>[24];
                    for (var i = 0; i < 24; i++)
                        lst[i] = new List<T>();
                    return lst;
                });
                lists[now.Hour].Add(value);
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
            lock (_locker)
            {
                var baseItem = new StatusItem
                {
                    Name = Name,
                    Values = { SortValues = false }
                };
                var daysValuePairs = _values.ToArray().OrderByDescending(i => i.Key).ToArray();
                var total = daysValuePairs.SelectMany(i => i.Value).Sum(i => i.Count);
                var daysSums = daysValuePairs.Select(i =>
                {
                    var totalCallsDay = i.Value.Sum(v => v.Count);
                    var percentCallsDay = (double)totalCallsDay * 100 / total;
                    return new DaysSum(i.Key, i.Value, totalCallsDay, percentCallsDay);
                }).ToArray();
                var now = Core.Now;
                foreach (var dsItem in daysSums)
                {
                    var dayItem = new StatusItem
                    {
                        Name = dsItem.Key.ToString("yyyy-MM-dd"),
                        Values = { SortValues = false }
                    };

                    dayItem.Values.Add("Total calls", dsItem.TotalCallsDay, dsItem.Key == now.Date);
                    dayItem.Values.Add("Percent of total calls", dsItem.PercentCallsDay, dsItem.Key == now.Date);


                    if (typeof(T) == typeof(double))
                        RenderDayHours<double>(dsItem, dayItem);
                    else if (typeof(T) == typeof(decimal))
                        RenderDayHours<decimal>(dsItem, dayItem);
                    else if (typeof(T) == typeof(int))
                        RenderDayHours<int>(dsItem, dayItem);
                    else if (typeof(T) == typeof(string))
                    {
                        for (var i = 0; i < dsItem.Value.Length; i++)
                        {
                            var values = dsItem.Value[i].Cast<string>().GroupBy(v => v)
                                .Select(v => new StatusItemValueItem(v.Key, v.Count(), StatusItemValueStatus.Unknown, dsItem.Key == now.Date && i == now.Hour)).ToArray();
                            dayItem.Values.Add($"Number of calls at {i}H", values);
                        }
                    }
                    else if (typeof(T) == typeof(float))
                        RenderDayHours<float>(dsItem, dayItem);
                    else if (typeof(T) == typeof(long))
                        RenderDayHours<long>(dsItem, dayItem);
                    else if (typeof(T) == typeof(short))
                        RenderDayHours<short>(dsItem, dayItem);
                    else if (typeof(T) == typeof(byte))
                        RenderDayHours<byte>(dsItem, dayItem);
                    else if (typeof(T) == typeof(ushort))
                        RenderDayHours<ushort>(dsItem, dayItem);

                    baseItem.Children.Add(dayItem);
                }
                baseItem.Values.AddOk("Total calls", total, true);
                return baseItem;
            }


        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RenderDayHours<TValue>(DaysSum sumItem, StatusItem dayItem) where TValue : IConvertible
        {
            var now = Core.Now;
            for (var i = 0; i < sumItem.Value.Length; i++)
            {
                var values = sumItem.Value[i].Cast<TValue>().ToArray();
                var calls = values.Length;
                var percentage = (double)calls * 100 / sumItem.TotalCallsDay;
                var minValue = calls > 0 ? values.Min() : default(TValue);
                var maxValue = calls > 0 ? values.Max() : default(TValue);
                (var averageValue, var stdValue) = values.GetAverageAndStdDev(v => v.ToDouble(null));

                var plot = sumItem.Key == now.Date && i == now.Hour;

                var callStatus = new StatusItemValueItem("Total Calls", calls, plot);
                var percentageStatus = new StatusItemValueItem("Percentage", percentage, plot);
                var lowestStatus = new StatusItemValueItem("Lowest Value", minValue, plot);
                var highestStatus = new StatusItemValueItem("Highest Value", maxValue, plot);
                var averageStatus = new StatusItemValueItem("Average", averageValue, plot);
                var stdStatus = new StatusItemValueItem("Standard Deviation", stdValue, plot);

                dayItem.Values.Add($"At {i}H:", callStatus, percentageStatus, lowestStatus, highestStatus,
                    averageStatus, stdStatus);
            }
        }
        #endregion

        #region Nested Type
        private struct DaysSum
        {
            public DateTime Key;
            public readonly List<T>[] Value;
            public readonly int TotalCallsDay;
            public readonly double PercentCallsDay;

            public DaysSum(DateTime key, List<T>[] value, int totalCallsDay, double percentCallsDay)
            {
                Key = key;
                Value = value;
                TotalCallsDay = totalCallsDay;
                PercentCallsDay = percentCallsDay;
            }
        }
        #endregion
    }
}
