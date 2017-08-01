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
            var status = new StatusItem { Name = this.Name };
            var counterValues = CounterValues.ToArray().OrderBy(k => k.Key);
            foreach (var counter in counterValues)
                status.Childrens.Add(counter.Value.GetStatusItem(counter.Key));
            return status;
        }

        #region Nested Classes

        class NumberItem
        {
            public ItemValue<double> LastestValue = null;
            public ItemInterval<double> LastMinuteValues = new ItemInterval<double>(TimeSpan.FromMinutes(1), val => (decimal)val.Value);
            public ItemInterval<double> Last10MinutesValues = new ItemInterval<double>(TimeSpan.FromMinutes(10), val => (decimal)val.Value);
            public ItemInterval<double> Last20MinutesValues = new ItemInterval<double>(TimeSpan.FromMinutes(20), val => (decimal)val.Value);
            public ItemInterval<double> Last30MinutesValues = new ItemInterval<double>(TimeSpan.FromMinutes(30), val => (decimal)val.Value);
            public ItemInterval<double> OneHourValues = new ItemInterval<double>(TimeSpan.FromHours(1), val => (decimal)val.Value);
            public ItemInterval<double> SixHourValues = new ItemInterval<double>(TimeSpan.FromHours(6), val => (decimal)val.Value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Register(double value)
            {
                LastestValue = new ItemValue<double>(value);
                LastMinuteValues.Register(value);
                Last10MinutesValues.Register(value);
                Last20MinutesValues.Register(value);
                Last30MinutesValues.Register(value);
                OneHourValues.Register(value);
                SixHourValues.Register(value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public StatusItem GetStatusItem(string name)
            {
                var cStatus = new StatusItem { Name = name };

                cStatus.Values.Add("Lastest Value", LastestValue?.Value);
                cStatus.Values.Add("Lastest Value Date", LastestValue?.ValueDate);

                LastMinuteValues.Calculate();
                var c1 = new StatusItem { Name = "Last Minute" };
                c1.Values.Add("Calls", LastMinuteValues.CallsQuantity);
                c1.Values.Add("Lowest Value", LastMinuteValues.LowestValue?.Value, StatusItemValueStatus.Green);
                c1.Values.Add("Lowest Value Date", LastMinuteValues.LowestValue?.ValueDate);
                c1.Values.Add("Highest Value", LastMinuteValues.HighestValue?.Value, StatusItemValueStatus.Red);
                c1.Values.Add("Highest Value Date", LastMinuteValues.HighestValue?.ValueDate);
                c1.Values.Add("Average Value", LastMinuteValues.AverageValue);
                c1.Values.Add("Standard Deviation", LastMinuteValues.StandardDeviation);
                foreach (var percentil in LastMinuteValues.Percentils)
                    c1.Values.Add($"Percentil {percentil.Percentil * 100}%", $"[{percentil.Min} - {percentil.Max}]");
                cStatus.Childrens.Add(c1);

                Last10MinutesValues.Calculate();
                var c2 = new StatusItem { Name = "Last 10 Minutes" };
                c2.Values.Add("Calls", Last10MinutesValues.CallsQuantity);
                c2.Values.Add("Lowest Value", Last10MinutesValues.LowestValue?.Value, StatusItemValueStatus.Green);
                c2.Values.Add("Lowest Value Date", Last10MinutesValues.LowestValue?.ValueDate);
                c2.Values.Add("Highest Value", Last10MinutesValues.HighestValue?.Value, StatusItemValueStatus.Red);
                c2.Values.Add("Highest Value Date", Last10MinutesValues.HighestValue?.ValueDate);
                c2.Values.Add("Average Value", Last10MinutesValues.AverageValue);
                c2.Values.Add("Standard Deviation", Last10MinutesValues.StandardDeviation);
                foreach (var percentil in Last10MinutesValues.Percentils)
                    c2.Values.Add($"Percentil {percentil.Percentil * 100}%", $"[{percentil.Min} - {percentil.Max}]");
                cStatus.Childrens.Add(c2);

                Last20MinutesValues.Calculate();
                var c3 = new StatusItem { Name = "Last 20 Minutes" };
                c3.Values.Add("Calls", Last20MinutesValues.CallsQuantity);
                c3.Values.Add("Lowest Value", Last20MinutesValues.LowestValue?.Value, StatusItemValueStatus.Green);
                c3.Values.Add("Lowest Value Date", Last20MinutesValues.LowestValue?.ValueDate);
                c3.Values.Add("Highest Value", Last20MinutesValues.HighestValue?.Value, StatusItemValueStatus.Red);
                c3.Values.Add("Highest Value Date", Last20MinutesValues.HighestValue?.ValueDate);
                c3.Values.Add("Average Value", Last20MinutesValues.AverageValue);
                c3.Values.Add("Standard Deviation", Last20MinutesValues.StandardDeviation);
                foreach (var percentil in Last20MinutesValues.Percentils)
                    c3.Values.Add($"Percentil {percentil.Percentil * 100}%", $"[{percentil.Min} - {percentil.Max}]");
                cStatus.Childrens.Add(c3);

                Last30MinutesValues.Calculate();
                var c4 = new StatusItem { Name = "Last 30 Minutes" };
                c4.Values.Add("Calls", Last30MinutesValues.CallsQuantity);
                c4.Values.Add("Lowest Value", Last30MinutesValues.LowestValue?.Value, StatusItemValueStatus.Green);
                c4.Values.Add("Lowest Value Date", Last30MinutesValues.LowestValue?.ValueDate);
                c4.Values.Add("Highest Value", Last30MinutesValues.HighestValue?.Value, StatusItemValueStatus.Red);
                c4.Values.Add("Highest Value Date", Last30MinutesValues.HighestValue?.ValueDate);
                c4.Values.Add("Average Value", Last30MinutesValues.AverageValue);
                c4.Values.Add("Standard Deviation", Last30MinutesValues.StandardDeviation);
                foreach (var percentil in Last30MinutesValues.Percentils)
                    c4.Values.Add($"Percentil {percentil.Percentil * 100}%", $"[{percentil.Min} - {percentil.Max}]");
                cStatus.Childrens.Add(c4);

                OneHourValues.Calculate();
                var c5 = new StatusItem { Name = "Last Hour" };
                c5.Values.Add("Calls", OneHourValues.CallsQuantity);
                c5.Values.Add("Lowest Value", OneHourValues.LowestValue?.Value, StatusItemValueStatus.Green);
                c5.Values.Add("Lowest Value Date", OneHourValues.LowestValue?.ValueDate);
                c5.Values.Add("Highest Value", OneHourValues.HighestValue?.Value, StatusItemValueStatus.Red);
                c5.Values.Add("Highest Value Date", OneHourValues.HighestValue?.ValueDate);
                c5.Values.Add("Average Value", OneHourValues.AverageValue);
                c5.Values.Add("Minutes Standard Deviation", OneHourValues.StandardDeviation);
                foreach (var percentil in OneHourValues.Percentils)
                    c5.Values.Add($"Percentil {percentil.Percentil * 100}%", $"[{percentil.Min} - {percentil.Max}]");
                cStatus.Childrens.Add(c5);

                SixHourValues.Calculate();
                var c6 = new StatusItem { Name = "Last 6 Hours" };
                c6.Values.Add("Calls", SixHourValues.CallsQuantity);
                c6.Values.Add("Lowest Value", SixHourValues.LowestValue?.Value, StatusItemValueStatus.Green);
                c6.Values.Add("Lowest Value Date", SixHourValues.LowestValue?.ValueDate);
                c6.Values.Add("Highest Value", SixHourValues.HighestValue?.Value, StatusItemValueStatus.Red);
                c6.Values.Add("Highest Value Date", SixHourValues.HighestValue?.ValueDate);
                c6.Values.Add("Average Value", SixHourValues.AverageValue);
                c6.Values.Add("Standard Deviation", SixHourValues.StandardDeviation);
                foreach (var percentil in SixHourValues.Percentils)
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
            bool dirty = false;

            public double CallsQuantity = 0;
            public DateTime LastDate;
            public ItemValue<T> LowestValue = null;
            public ItemValue<T> HighestValue = null;
            public decimal? AverageValue = null;
            public double? StandardDeviation = null;
            public List<ItemPercentil> Percentils = null;

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
