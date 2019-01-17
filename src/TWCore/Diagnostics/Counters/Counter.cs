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
using System.Runtime.CompilerServices;

namespace TWCore.Diagnostics.Counters
{
    /// <summary>
    /// Abstract Counter
    /// </summary>
    /// <typeparam name="T">Type of counter</typeparam>
    public abstract class Counter<T>: ICounter<T>, ICounterReader
        where T: IConvertible
    {
        private BlockingCollection<CounterItemValue<T>> _counterValues = new BlockingCollection<CounterItemValue<T>>();
        private volatile CounterItemValue<T> _lastValue = null;
        private readonly object _valueLock = new object();

        #region Properties
        /// <summary>
        /// Gets the counter category
        /// </summary>
        public string Category { get; private set; }
        /// <summary>
        /// Gets the counter name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the counter type
        /// </summary>
        public CounterType Type { get; private set; }
        /// <summary>
        /// Gets the counter level
        /// </summary>
        public CounterLevel Level { get; private set; }
        /// <summary>
        /// Gets the counter kind
        /// </summary>
        public CounterKind Kind { get; private set; }
        /// <summary>
        /// Gets the counter value unit
        /// </summary>
        public CounterUnit Unit { get; private set; }
        #endregion


        #region .ctor
        /// <summary>
        /// Abstract Counter
        /// </summary>
        /// <param name="category">Counter category</param>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        /// <param name="kind">Counter kind</param>
        /// <param name="unit">Counter unit</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Counter(string category, string name, CounterType type, CounterLevel level, CounterKind kind, CounterUnit unit)
        {
            Category = category;
            Name = name;
            Type = type;
            Level = level;
            Kind = kind;
            Unit = unit;
        }
        #endregion

        /// <summary>
        /// Add the specified value
        /// </summary>
        /// <param name="value">Value to be added to the counter</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T value)
        {
            //Now with a truncate to a second
            var now = Core.Now.TruncateTo(TimeSpan.FromSeconds(1));
            var lastValue = _lastValue;
            if (lastValue?.Timestamp == now)
            {
                var nValue = MergeValues(lastValue.Value, value);
                lock (_valueLock)
                {
                    if (_lastValue != null)
                    {
                        _lastValue.Value = nValue;
                        return;
                    }
                }
            }
            var newItem = CounterItemValue<T>.Retrieve(now, value);
            _lastValue = newItem;
            _counterValues.Add(newItem);
        }
        /// <summary>
        /// Takes a maximum number of values from the counter
        /// </summary>
        /// <returns>The counter value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ICounterItem ICounterReader.Take(int items)
        {
            if (_counterValues.Count == 0) return null;
            var lstItems = new List<CounterItemValue<T>>();
            var itemIdx = 0;
            while (itemIdx < items && _counterValues.TryTake(out var item))
            {
                if (item == _lastValue)
                {
                    lock (_valueLock)
                    {
                        _lastValue = null;
                    }
                }
                lstItems.Add(item);
                itemIdx++;
            }
            return CounterItem<T>.Retrieve(Category, Name, Type, Level, Kind, Unit, lstItems);
        }
        /// <summary>
        /// Return the ICounterItem to the pool
        /// </summary>
        /// <param name="item">Item value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReturnToPool(ICounterItem item)
        {
            if (item is CounterItem<T> value)
            {
                foreach(var lVal in value.Values)
                    CounterItemValue<T>.Store(lVal);
                CounterItem<T>.Store(value);
            }
        }

        /// <summary>
        /// Merge values
        /// </summary>
        /// <returns>The value merged</returns>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract T MergeValues(T oldValue, T newValue);
    }


    /// <summary>
    /// Integer Counter
    /// </summary>
    public sealed class IntegerCounter : Counter<int>
    {
        /// <summary>
        /// Integer Counter
        /// </summary>
        /// <param name="category">Counter category</param>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        /// <param name="kind">Counter kind</param>
        /// <param name="unit">Counter unit</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IntegerCounter(string category, string name, CounterType type, CounterLevel level, CounterKind kind, CounterUnit unit) : base(category, name, type, level, kind, unit)
        {
        }
        /// <summary>
        /// Increment value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Increment()
        {
            base.Add(1);
        }
        /// <summary>
        /// Decrement value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Decrement()
        {
            base.Add(-1);
        }
        /// <summary>
        /// Merge values
        /// </summary>
        /// <returns>The value merged</returns>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override int MergeValues(int oldValue, int newValue)
        {
            if (Type == CounterType.Average)
                return (oldValue + newValue) / 2;
            return oldValue + newValue;
        }
    }
    /// <summary>
    /// Double Counter
    /// </summary>
    public sealed class DoubleCounter : Counter<double>
    {
        /// <summary>
        /// Double Counter
        /// </summary>
        /// <param name="category">Counter category</param>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        /// <param name="kind">Counter kind</param>
        /// <param name="unit">Counter unit</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal DoubleCounter(string category, string name, CounterType type, CounterLevel level, CounterKind kind, CounterUnit unit) : base(category, name, type, level, kind, unit)
        {
        }
        /// <summary>
        /// Merge values
        /// </summary>
        /// <returns>The value merged</returns>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override double MergeValues(double oldValue, double newValue)
        {
            if (Type == CounterType.Average)
                return (oldValue + newValue) / 2;
            return oldValue + newValue;
        }
    }
    /// <summary>
    /// Decimal Counter
    /// </summary>
    public sealed class DecimalCounter : Counter<decimal>
    {
        /// <summary>
        /// Decimal Counter
        /// </summary>
        /// <param name="category">Counter category</param>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        /// <param name="kind">Counter kind</param>
        /// <param name="unit">Counter unit</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal DecimalCounter(string category, string name, CounterType type, CounterLevel level, CounterKind kind, CounterUnit unit) : base(category, name, type, level, kind, unit)
        {
        }
        /// <summary>
        /// Merge values
        /// </summary>
        /// <returns>The value merged</returns>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override decimal MergeValues(decimal oldValue, decimal newValue)
        {
            if (Type == CounterType.Average)
                return (oldValue + newValue) / 2;
            return oldValue + newValue;
        }
    }

}