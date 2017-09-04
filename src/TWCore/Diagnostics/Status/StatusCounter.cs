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
using System.Threading;
using System.Threading.Tasks;

namespace TWCore.Diagnostics.Status
{
    /// <summary>
    /// Default Status Counter class.
    /// </summary>
    public class StatusCounter
    {
        readonly ConcurrentDictionary<string, NumberItem> CounterValues = new ConcurrentDictionary<string, NumberItem>();

        /// <summary>
        /// Status Counter Name
        /// </summary>
        public string Name { get; set; } = "Status Counters";

        /// <summary>
        /// Default Status Counter class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatusCounter()
        {
            Core.Status.Attach(GetStatusItem);
        }

        /// <summary>
        /// Register a value to a counter
        /// </summary>
        /// <param name="counterName">Counter name</param>
        /// <param name="value">Actual value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Register(string counterName, double value)
        {
            var _value = CounterValues.GetOrAdd(counterName, _ => new NumberItem());
            _value.Register(value);
        }
        /// <summary>
        /// Gets the Status Item with all calculations
        /// </summary>
        /// <returns>StatusItem</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatusItem GetStatusItem()
        {
            var status = new StatusItem { Name = Name };
            var counterValues = CounterValues.ToArray().OrderBy(k => k.Key);
            foreach (var counter in counterValues)
                status.Childrens.Add(counter.Value.GetStatusItem(counter.Key));
            return status;
        }

        #region Nested Classes

        private class NumberItem
        {
            private ItemValue<double> _lastestValue;
            private readonly ItemInterval<double> _lastMinuteValues = new ItemInterval<double>(TimeSpan.FromMinutes(1), val => (decimal)val.Value);
            private readonly ItemInterval<double> _last10MinutesValues = new ItemInterval<double>(TimeSpan.FromMinutes(10), val => (decimal)val.Value);
            private readonly ItemInterval<double> _last20MinutesValues = new ItemInterval<double>(TimeSpan.FromMinutes(20), val => (decimal)val.Value);
            private readonly ItemInterval<double> _last30MinutesValues = new ItemInterval<double>(TimeSpan.FromMinutes(30), val => (decimal)val.Value);
            private readonly ItemInterval<double> _oneHourValues = new ItemInterval<double>(TimeSpan.FromHours(1), val => (decimal)val.Value);
            private readonly ItemInterval<double> _sixHourValues = new ItemInterval<double>(TimeSpan.FromHours(6), val => (decimal)val.Value);

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

                cStatus.Values.Add("Lastest Value", _lastestValue?.Value);
                cStatus.Values.Add("Lastest Value Date", _lastestValue?.ValueDate);

                _lastMinuteValues.Calculate();
                var c1 = new StatusItem { Name = "Last Minute" };
                c1.Values.Add("Calls", _lastMinuteValues.CallsQuantity);
                c1.Values.Add("Lowest Value", _lastMinuteValues.LowestValue?.Value, StatusItemValueStatus.Green);
                c1.Values.Add("Lowest Value Date", _lastMinuteValues.LowestValue?.ValueDate);
                c1.Values.Add("Highest Value", _lastMinuteValues.HighestValue?.Value, StatusItemValueStatus.Red);
                c1.Values.Add("Highest Value Date", _lastMinuteValues.HighestValue?.ValueDate);
                c1.Values.Add("Average Value", _lastMinuteValues.AverageValue);
                c1.Values.Add("Standard Deviation", _lastMinuteValues.StandardDeviation);
                foreach (var percentil in _lastMinuteValues.Percentils)
                    c1.Values.Add($"Percentil {percentil.Percentil * 100}%", $"[{percentil.Min} - {percentil.Max}]");
                cStatus.Childrens.Add(c1);

                _last10MinutesValues.Calculate();
                var c2 = new StatusItem { Name = "Last 10 Minutes" };
                c2.Values.Add("Calls", _last10MinutesValues.CallsQuantity);
                c2.Values.Add("Lowest Value", _last10MinutesValues.LowestValue?.Value, StatusItemValueStatus.Green);
                c2.Values.Add("Lowest Value Date", _last10MinutesValues.LowestValue?.ValueDate);
                c2.Values.Add("Highest Value", _last10MinutesValues.HighestValue?.Value, StatusItemValueStatus.Red);
                c2.Values.Add("Highest Value Date", _last10MinutesValues.HighestValue?.ValueDate);
                c2.Values.Add("Average Value", _last10MinutesValues.AverageValue);
                c2.Values.Add("Standard Deviation", _last10MinutesValues.StandardDeviation);
                foreach (var percentil in _last10MinutesValues.Percentils)
                    c2.Values.Add($"Percentil {percentil.Percentil * 100}%", $"[{percentil.Min} - {percentil.Max}]");
                cStatus.Childrens.Add(c2);

                _last20MinutesValues.Calculate();
                var c3 = new StatusItem { Name = "Last 20 Minutes" };
                c3.Values.Add("Calls", _last20MinutesValues.CallsQuantity);
                c3.Values.Add("Lowest Value", _last20MinutesValues.LowestValue?.Value, StatusItemValueStatus.Green);
                c3.Values.Add("Lowest Value Date", _last20MinutesValues.LowestValue?.ValueDate);
                c3.Values.Add("Highest Value", _last20MinutesValues.HighestValue?.Value, StatusItemValueStatus.Red);
                c3.Values.Add("Highest Value Date", _last20MinutesValues.HighestValue?.ValueDate);
                c3.Values.Add("Average Value", _last20MinutesValues.AverageValue);
                c3.Values.Add("Standard Deviation", _last20MinutesValues.StandardDeviation);
                foreach (var percentil in _last20MinutesValues.Percentils)
                    c3.Values.Add($"Percentil {percentil.Percentil * 100}%", $"[{percentil.Min} - {percentil.Max}]");
                cStatus.Childrens.Add(c3);

                _last30MinutesValues.Calculate();
                var c4 = new StatusItem { Name = "Last 30 Minutes" };
                c4.Values.Add("Calls", _last30MinutesValues.CallsQuantity);
                c4.Values.Add("Lowest Value", _last30MinutesValues.LowestValue?.Value, StatusItemValueStatus.Green);
                c4.Values.Add("Lowest Value Date", _last30MinutesValues.LowestValue?.ValueDate);
                c4.Values.Add("Highest Value", _last30MinutesValues.HighestValue?.Value, StatusItemValueStatus.Red);
                c4.Values.Add("Highest Value Date", _last30MinutesValues.HighestValue?.ValueDate);
                c4.Values.Add("Average Value", _last30MinutesValues.AverageValue);
                c4.Values.Add("Standard Deviation", _last30MinutesValues.StandardDeviation);
                foreach (var percentil in _last30MinutesValues.Percentils)
                    c4.Values.Add($"Percentil {percentil.Percentil * 100}%", $"[{percentil.Min} - {percentil.Max}]");
                cStatus.Childrens.Add(c4);

                _oneHourValues.Calculate();
                var c5 = new StatusItem { Name = "Last Hour" };
                c5.Values.Add("Calls", _oneHourValues.CallsQuantity);
                c5.Values.Add("Lowest Value", _oneHourValues.LowestValue?.Value, StatusItemValueStatus.Green);
                c5.Values.Add("Lowest Value Date", _oneHourValues.LowestValue?.ValueDate);
                c5.Values.Add("Highest Value", _oneHourValues.HighestValue?.Value, StatusItemValueStatus.Red);
                c5.Values.Add("Highest Value Date", _oneHourValues.HighestValue?.ValueDate);
                c5.Values.Add("Average Value", _oneHourValues.AverageValue);
                c5.Values.Add("Minutes Standard Deviation", _oneHourValues.StandardDeviation);
                foreach (var percentil in _oneHourValues.Percentils)
                    c5.Values.Add($"Percentil {percentil.Percentil * 100}%", $"[{percentil.Min} - {percentil.Max}]");
                cStatus.Childrens.Add(c5);

                _sixHourValues.Calculate();
                var c6 = new StatusItem { Name = "Last 6 Hours" };
                c6.Values.Add("Calls", _sixHourValues.CallsQuantity);
                c6.Values.Add("Lowest Value", _sixHourValues.LowestValue?.Value, StatusItemValueStatus.Green);
                c6.Values.Add("Lowest Value Date", _sixHourValues.LowestValue?.ValueDate);
                c6.Values.Add("Highest Value", _sixHourValues.HighestValue?.Value, StatusItemValueStatus.Red);
                c6.Values.Add("Highest Value Date", _sixHourValues.HighestValue?.ValueDate);
                c6.Values.Add("Average Value", _sixHourValues.AverageValue);
                c6.Values.Add("Standard Deviation", _sixHourValues.StandardDeviation);
                foreach (var percentil in _sixHourValues.Percentils)
                    c6.Values.Add($"Percentil {percentil.Percentil * 100}%", $"[{percentil.Min} - {percentil.Max}]");
                cStatus.Childrens.Add(c6);

                return cStatus;
            }
        }

        class ItemInterval<T>
        {
            IComparer<T> _comparer;
            TimeSpan _sinceSlideTime;
            ConcurrentQueue<ItemValue<T>> _queue;
            Func<ItemValue<T>, decimal> _funcToAverage;
            CancellationTokenSource tokenSource;
            Task tsk;
            bool dirty;

            public double CallsQuantity;
            public DateTime LastDate;
            public ItemValue<T> LowestValue;
            public ItemValue<T> HighestValue;
            public decimal? AverageValue;
            public double? StandardDeviation;
            public List<ItemPercentil> Percentils;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ItemInterval(TimeSpan sinceSlideTime, Func<ItemValue<T>, decimal> funcToAverage) : this(sinceSlideTime, funcToAverage, Comparer<T>.Default) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ItemInterval(TimeSpan sinceSlideTime, Func<ItemValue<T>, decimal> funcToAverage, IComparer<T> comparer)
            {
                _sinceSlideTime = sinceSlideTime;
                _comparer = comparer;
                _funcToAverage = funcToAverage;
                _queue = new ConcurrentQueue<ItemValue<T>>();
                tokenSource = new CancellationTokenSource();
                Percentils = new List<ItemPercentil> {
                    new ItemPercentil(0.7),
                    new ItemPercentil(0.8),
                    new ItemPercentil(0.9),
                    new ItemPercentil(0.95),
                    new ItemPercentil(0.99),
                };
                var token = tokenSource.Token;
                tsk = Task.Run(async () =>
                {
                    try
                    {
                        while (!token.IsCancellationRequested)
                        {
                            Calculate();
                            await Task.Delay(5000, token);
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                }, token);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            ~ItemInterval()
            {
                tokenSource.Cancel();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Register(T value)
            {
                LastDate = Core.Now;
                var itemValue = new ItemValue<T>(value);
                _queue.Enqueue(itemValue);
                dirty = true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Calculate()
            {
                var now = Core.Now;
                while (true)
                {
                    if (!_queue.TryPeek(out var _value))
                        break;
                    if (now - _value.ValueDate >= _sinceSlideTime)
                    {
                        _queue.TryDequeue(out _value);
                        dirty = true;
                    }
                    else
                        break;
                }
                if (dirty)
                {
                    AverageValue = _queue.Count > 0 ? Math.Round(_queue.Average(_funcToAverage), 2) : (decimal?)null;
                    StandardDeviation = _queue.Count > 0 ? Math.Round(_queue.GetStdDev(i => (double)_funcToAverage(i)), 2) : (double?)null;
                    var qArray = _queue.ToArray();
                    CallsQuantity = qArray.Length;
                    var sorted = qArray.Select((i, idx) => Tuple.Create(_funcToAverage(i), i)).OrderBy(i => i.Item1).ToArray();
                    LowestValue = sorted.FirstOrDefault()?.Item2;
                    HighestValue = sorted.LastOrDefault()?.Item2;
                    if (Percentils?.Count > 0 && sorted.Length > 0)
                    {
                        var posIndexes = sorted.Length - 1;
                        foreach (var percentil in Percentils)
                        {
                            var idx = percentil.Percentil * posIndexes;
                            var minIdx = posIndexes - (int)idx;
                            var maxIdx = (int)Math.Ceiling(idx);
                            percentil.Max = sorted[maxIdx].Item1;
                            percentil.Min = sorted[minIdx].Item1;
                        }
                    }
                    dirty = false;
                }
            }
        }
        class ItemValue<T>
        {
            public T Value;
            public DateTime ValueDate;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ItemValue(T value)
            {
                Value = value;
                ValueDate = Core.Now;
            }
        }
        class ItemPercentil
        {
            public double Percentil;
            public decimal Min;
            public decimal Max;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ItemPercentil(double percentil)
            {
                Percentil = percentil;
            }
        }

        #endregion
    }
}
