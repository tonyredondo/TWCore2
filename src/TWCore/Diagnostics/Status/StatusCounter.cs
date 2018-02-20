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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TWCore.Collections;
// ReSharper disable NotAccessedField.Local
// ReSharper disable MemberCanBePrivate.Local

namespace TWCore.Diagnostics.Status
{
    /// <inheritdoc />
    /// <summary>
    /// Default Status Counter class.
    /// </summary>
    public class StatusCounter : IDisposable
    {
        private readonly LRU2QCollection<string, NumberItem> _counterValues = new LRU2QCollection<string, NumberItem>(100);
        private bool _disposed;
        private int _index;
        
        /// <summary>
        /// Status Counter Name
        /// </summary>
        public string Name { get; private set; } = "Status Counters";

        #region .ctor
        /// <summary>
        /// Default Status Counter class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatusCounter()
        {
            Core.Status.Attach(GetStatusItem);
        }
        /// <summary>
        /// Default Status Counter class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatusCounter(string name)
        {
            Name = name;
            Core.Status.Attach(GetStatusItem);
        }
        ~StatusCounter()
        {
            Dispose();
        }
        public void Dispose()
        {
            Core.Status.DeAttachObject(this);
            _counterValues.Clear();
            _disposed = true;
        }
        #endregion

        /// <summary>
        /// Register a value to a counter
        /// </summary>
        /// <param name="counterName">Counter name</param>
        /// <param name="value">Actual value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Register(string counterName, double value)
        {
            var numberItem = _counterValues.GetOrAdd(counterName, _ => new NumberItem(_index++));
            numberItem.Register(value);
        }
        /// <summary>
        /// Gets the Status Item with all calculations
        /// </summary>
        /// <returns>StatusItem</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StatusItem GetStatusItem()
        {
            if (_disposed) return null;
            var counterValues = _counterValues.ToArray().OrderBy(k => k.Value.Index).ToArray();
            if (counterValues.Length == 0) return null;
            var status = new StatusItem { Name = Name };
            foreach (var counter in counterValues)
                status.Children.Add(counter.Value.GetStatusItem(counter.Key));
            return status;
        }

        #region Nested Classes

        private class NumberItem
        {
            public int Index { get; }
            private ItemValue<double> _lastestValue;
            private readonly ItemInterval<double> _last10MinutesValues = new ItemInterval<double>(TimeSpan.FromMinutes(10), val => val.Value);
            private readonly ItemInterval<double> _last30MinutesValues = new ItemInterval<double>(TimeSpan.FromMinutes(30), val => val.Value);
            private readonly ItemInterval<double> _oneHourValues = new ItemInterval<double>(TimeSpan.FromHours(1), val => val.Value);
            private readonly ItemInterval<double> _24HoursValues = new ItemInterval<double>(TimeSpan.FromHours(24), val => val.Value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NumberItem(int index)
            {
                Index = index;
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Register(double value)
            {
                _lastestValue = new ItemValue<double>(value);
                _last10MinutesValues.Register(value);
                _last30MinutesValues.Register(value);
                _oneHourValues.Register(value);
                _24HoursValues.Register(value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public StatusItem GetStatusItem(string name)
            {
                var cStatus = new StatusItem { Name = name };
                cStatus.Values.Add("Lastest Value", _lastestValue.Value, true);
                cStatus.Values.Add("Lastest Value Date", _lastestValue.ValueDate);
                cStatus.Children.Add(GetStatusItem(_last10MinutesValues, "Last 10 Minutes"));
                cStatus.Children.Add(GetStatusItem(_last30MinutesValues, "Last 30 Minutes"));
                cStatus.Children.Add(GetStatusItem(_oneHourValues, "Last Hour"));
                cStatus.Children.Add(GetStatusItem(_24HoursValues, "Last 24 Hours"));
                return cStatus;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static StatusItem GetStatusItem(ItemInterval<double> interval, string name)
            {
                interval.Calculate();
                var item = new StatusItem { Name = name };
                item.Values.Add("Calls", interval.CallsQuantity, true);
                item.Values.Add("Value",
                    new StatusItemValueItem("Lowest", interval.LowestValue.Value, StatusItemValueStatus.Ok, true),
                    new StatusItemValueItem("Lowest Date", interval.LowestValue.ValueDate),
                    new StatusItemValueItem("Highest", interval.HighestValue.Value, StatusItemValueStatus.Ok, true),
                    new StatusItemValueItem("Highest Date", interval.HighestValue.ValueDate),
                    new StatusItemValueItem("Average", interval.AverageValue, true),
                    new StatusItemValueItem("Standard Deviation", interval.StandardDeviation, true)
                );
                foreach (var percentile in interval.Percentiles)
                    item.Values.Add($"Percentile {percentile.Percentil * 100}%", 
                        new StatusItemValueItem("Calls", percentile.Calls), 
                        new StatusItemValueItem("Call Diff", percentile.CallDiff), 
                        new StatusItemValueItem("Min", percentile.Min), 
                        new StatusItemValueItem("Max", percentile.Max)
                        );
                return item;
            }
        }

        private class ItemInterval<T>
        {
            private readonly TimeSpan _sinceSlideTime;
            private readonly ConcurrentQueue<ItemValue<T>> _queue;
            private readonly Func<ItemValue<T>, double> _funcToAverage;
            private bool _dirty;

            public double CallsQuantity;
            public DateTime LastDate;
            public ItemValue<T> LowestValue;
            public ItemValue<T> HighestValue;
            public double? AverageValue;
            public double? StandardDeviation;
            public readonly List<ItemPercentile> Percentiles;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ItemInterval(TimeSpan sinceSlideTime, Func<ItemValue<T>, double> funcToAverage)
            {
                _sinceSlideTime = sinceSlideTime;
                _funcToAverage = funcToAverage;
                _queue = new ConcurrentQueue<ItemValue<T>>();
                Percentiles = new List<ItemPercentile> {
                    new ItemPercentile(0.8),
                    new ItemPercentile(0.9),
                    new ItemPercentile(0.98)
                };
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Register(T value)
            {
                LastDate = Core.Now;
                var itemValue = new ItemValue<T>(value);
                _queue.Enqueue(itemValue);
                _dirty = true;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Calculate()
            {
                var now = Core.Now;
                while (true)
                {
                    if (!_queue.TryPeek(out var value))
                        break;
                    if (now - value.ValueDate >= _sinceSlideTime)
                    {
                        _queue.TryDequeue(out value);
                        _dirty = true;
                    }
                    else
                        break;
                }
                if (!_dirty) return;
                if (_queue.Count > 0)
                    (AverageValue, StandardDeviation) = _queue.GetAverageAndStdDev(_funcToAverage);
                else
                {
                    AverageValue = null;
                    StandardDeviation = null;
                }
                var qArray = _queue.ToArray();
                if (qArray.Length > 0)
                {
                    var sorted = qArray.Select((i, idx) => (_funcToAverage(i), i)).OrderBy(i => i.Item1).ToArray();
                    CallsQuantity = sorted.Length;
                    LowestValue = sorted.First().Item2;
                    HighestValue = sorted.Last().Item2;
                    if (Percentiles == null || Percentiles.Count == 0) return;
                    var posIndexes = sorted.Length - 1;
                    foreach (var percentile in Percentiles)
                    {
                        var idx = percentile.Percentil * posIndexes;
                        var minIdx = posIndexes - (int) idx;
                        var maxIdx = (int) Math.Ceiling(idx);
                        percentile.Calls = (maxIdx - minIdx) + 1;
                        percentile.CallDiff = sorted.Length - percentile.Calls;
                        percentile.Max = sorted[maxIdx].Item1;
                        percentile.Min = sorted[minIdx].Item1;
                    }
                }
                _dirty = false;
            }

        }
        private struct ItemValue<T>
        {
            public readonly T Value;
            public readonly DateTime ValueDate;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ItemValue(T value)
            {
                Value = value;
                ValueDate = Core.Now;
            }
        }
        private class ItemPercentile
        {
            public readonly double Percentil;
            public int Calls;
            public int CallDiff;
            public double Min;
            public double Max;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ItemPercentile(double percentil)
            {
                Percentil = percentil;
            }
        }
        #endregion

    }
}
