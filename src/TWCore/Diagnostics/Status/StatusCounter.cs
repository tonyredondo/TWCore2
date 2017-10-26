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
            private readonly ItemInterval<double> _lastMinuteValues = new ItemInterval<double>(TimeSpan.FromMinutes(1), val => val.Value);
            private readonly ItemInterval<double> _last10MinutesValues = new ItemInterval<double>(TimeSpan.FromMinutes(10), val => val.Value);
            private readonly ItemInterval<double> _last20MinutesValues = new ItemInterval<double>(TimeSpan.FromMinutes(20), val => val.Value);
            private readonly ItemInterval<double> _last30MinutesValues = new ItemInterval<double>(TimeSpan.FromMinutes(30), val => val.Value);
            private readonly ItemInterval<double> _oneHourValues = new ItemInterval<double>(TimeSpan.FromHours(1), val => val.Value);
            private readonly ItemInterval<double> _sixHourValues = new ItemInterval<double>(TimeSpan.FromHours(6), val => val.Value);

            public NumberItem(int index)
            {
                Index = index;
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Register(double value)
            {
                _lastestValue = new ItemValue<double>(value);
                _lastMinuteValues.Register(value);
                _last10MinutesValues.Register(value);
                _last20MinutesValues.Register(value);
                _last30MinutesValues.Register(value);
                _oneHourValues.Register(value);
                _sixHourValues.Register(value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public StatusItem GetStatusItem(string name)
            {
                var cStatus = new StatusItem { Name = name };

                cStatus.Values.Add("Lastest Value", _lastestValue?.Value, true);
                cStatus.Values.Add("Lastest Value Date", _lastestValue?.ValueDate);

                _lastMinuteValues.Calculate();
                var c1 = new StatusItem { Name = "Last Minute" };
                c1.Values.Add("Calls", _lastMinuteValues.CallsQuantity, true);
                c1.Values.Add("Lowest Value", _lastMinuteValues.LowestValue?.Value, StatusItemValueStatus.Ok);
                c1.Values.Add("Lowest Value Date", _lastMinuteValues.LowestValue?.ValueDate);
                c1.Values.Add("Highest Value", _lastMinuteValues.HighestValue?.Value, StatusItemValueStatus.Error);
                c1.Values.Add("Highest Value Date", _lastMinuteValues.HighestValue?.ValueDate);
                c1.Values.Add("Average Value", _lastMinuteValues.AverageValue, true);
                c1.Values.Add("Standard Deviation", _lastMinuteValues.StandardDeviation, true);
                foreach (var percentile in _lastMinuteValues.Percentiles)
                    c1.Values.Add($"Percentile {percentile.Percentil * 100}%", new StatusItemValueItem("Min", percentile.Min), new StatusItemValueItem("Max", percentile.Max));
                cStatus.Children.Add(c1);

                _last10MinutesValues.Calculate();
                var c2 = new StatusItem { Name = "Last 10 Minutes" };
                c2.Values.Add("Calls", _last10MinutesValues.CallsQuantity, true);
                c2.Values.Add("Lowest Value", _last10MinutesValues.LowestValue?.Value, StatusItemValueStatus.Ok);
                c2.Values.Add("Lowest Value Date", _last10MinutesValues.LowestValue?.ValueDate);
                c2.Values.Add("Highest Value", _last10MinutesValues.HighestValue?.Value, StatusItemValueStatus.Error);
                c2.Values.Add("Highest Value Date", _last10MinutesValues.HighestValue?.ValueDate);
                c2.Values.Add("Average Value", _last10MinutesValues.AverageValue, true);
                c2.Values.Add("Standard Deviation", _last10MinutesValues.StandardDeviation, true);
                foreach (var percentile in _last10MinutesValues.Percentiles)
                    c2.Values.Add($"Percentile {percentile.Percentil * 100}%", new StatusItemValueItem("Min", percentile.Min), new StatusItemValueItem("Max", percentile.Max));
                cStatus.Children.Add(c2);

                _last20MinutesValues.Calculate();
                var c3 = new StatusItem { Name = "Last 20 Minutes" };
                c3.Values.Add("Calls", _last20MinutesValues.CallsQuantity, true);
                c3.Values.Add("Lowest Value", _last20MinutesValues.LowestValue?.Value, StatusItemValueStatus.Ok);
                c3.Values.Add("Lowest Value Date", _last20MinutesValues.LowestValue?.ValueDate);
                c3.Values.Add("Highest Value", _last20MinutesValues.HighestValue?.Value, StatusItemValueStatus.Error);
                c3.Values.Add("Highest Value Date", _last20MinutesValues.HighestValue?.ValueDate);
                c3.Values.Add("Average Value", _last20MinutesValues.AverageValue, true);
                c3.Values.Add("Standard Deviation", _last20MinutesValues.StandardDeviation, true);
                foreach (var percentile in _last20MinutesValues.Percentiles)
                    c3.Values.Add($"Percentile {percentile.Percentil * 100}%", new StatusItemValueItem("Min", percentile.Min), new StatusItemValueItem("Max", percentile.Max));
                cStatus.Children.Add(c3);

                _last30MinutesValues.Calculate();
                var c4 = new StatusItem { Name = "Last 30 Minutes" };
                c4.Values.Add("Calls", _last30MinutesValues.CallsQuantity, true);
                c4.Values.Add("Lowest Value", _last30MinutesValues.LowestValue?.Value, StatusItemValueStatus.Ok);
                c4.Values.Add("Lowest Value Date", _last30MinutesValues.LowestValue?.ValueDate);
                c4.Values.Add("Highest Value", _last30MinutesValues.HighestValue?.Value, StatusItemValueStatus.Error);
                c4.Values.Add("Highest Value Date", _last30MinutesValues.HighestValue?.ValueDate);
                c4.Values.Add("Average Value", _last30MinutesValues.AverageValue, true);
                c4.Values.Add("Standard Deviation", _last30MinutesValues.StandardDeviation, true);
                foreach (var percentile in _last30MinutesValues.Percentiles)
                    c4.Values.Add($"Percentile {percentile.Percentil * 100}%", new StatusItemValueItem("Min", percentile.Min), new StatusItemValueItem("Max", percentile.Max));
                cStatus.Children.Add(c4);

                _oneHourValues.Calculate();
                var c5 = new StatusItem { Name = "Last Hour" };
                c5.Values.Add("Calls", _oneHourValues.CallsQuantity, true);
                c5.Values.Add("Lowest Value", _oneHourValues.LowestValue?.Value, StatusItemValueStatus.Ok);
                c5.Values.Add("Lowest Value Date", _oneHourValues.LowestValue?.ValueDate);
                c5.Values.Add("Highest Value", _oneHourValues.HighestValue?.Value, StatusItemValueStatus.Error);
                c5.Values.Add("Highest Value Date", _oneHourValues.HighestValue?.ValueDate);
                c5.Values.Add("Average Value", _oneHourValues.AverageValue, true);
                c5.Values.Add("Minutes Standard Deviation", _oneHourValues.StandardDeviation, true);
                foreach (var percentile in _oneHourValues.Percentiles)
                    c5.Values.Add($"Percentile {percentile.Percentil * 100}%", new StatusItemValueItem("Min", percentile.Min), new StatusItemValueItem("Max", percentile.Max));
                cStatus.Children.Add(c5);

                _sixHourValues.Calculate();
                var c6 = new StatusItem { Name = "Last 6 Hours" };
                c6.Values.Add("Calls", _sixHourValues.CallsQuantity, true);
                c6.Values.Add("Lowest Value", _sixHourValues.LowestValue?.Value, StatusItemValueStatus.Ok);
                c6.Values.Add("Lowest Value Date", _sixHourValues.LowestValue?.ValueDate);
                c6.Values.Add("Highest Value", _sixHourValues.HighestValue?.Value, StatusItemValueStatus.Error);
                c6.Values.Add("Highest Value Date", _sixHourValues.HighestValue?.ValueDate);
                c6.Values.Add("Average Value", _sixHourValues.AverageValue, true);
                c6.Values.Add("Standard Deviation", _sixHourValues.StandardDeviation, true);
                foreach (var percentile in _sixHourValues.Percentiles)
                    c6.Values.Add($"Percentile {percentile.Percentil * 100}%", new StatusItemValueItem("Min", percentile.Min), new StatusItemValueItem("Max", percentile.Max));
                cStatus.Children.Add(c6);

                return cStatus;
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
                    new ItemPercentile(0.95),
                    new ItemPercentile(0.99)
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
                CallsQuantity = qArray.Length;
                var sorted = qArray.Select((i, idx) => Tuple.Create(_funcToAverage(i), i)).OrderBy(i => i.Item1).ToArray();
                LowestValue = sorted.FirstOrDefault()?.Item2;
                HighestValue = sorted.LastOrDefault()?.Item2;
                if (Percentiles == null) return;
                if (Percentiles.Count > 0 && sorted.Length > 0)
                {
                    var posIndexes = sorted.Length - 1;
                    foreach (var percentile in Percentiles)
                    {
                        var idx = percentile.Percentil * posIndexes;
                        var minIdx = posIndexes - (int)idx;
                        var maxIdx = (int)Math.Ceiling(idx);
                        percentile.Max = sorted[maxIdx].Item1;
                        percentile.Min = sorted[minIdx].Item1;
                    }
                }
                _dirty = false;
            }

        }
        private class ItemValue<T>
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
