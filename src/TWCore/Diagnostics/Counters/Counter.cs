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
    {
        private BlockingCollection<CounterItemValue<T>> _counterValues = new BlockingCollection<CounterItemValue<T>>();

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
        #endregion


        #region .ctor
        /// <summary>
        /// Abstract Counter
        /// </summary>
        /// <param name="category">Counter category</param>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Counter(string category, string name, CounterType type, CounterLevel level)
        {
            Category = category;
            Name = name;
            Type = type;
            Level = level;
        }
        #endregion

        /// <summary>
        /// Add the specified value
        /// </summary>
        /// <param name="value">Value to be added to the counter</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T value)
        {
            _counterValues.Add(new CounterItemValue<T>(Core.Now, value));
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
                lstItems.Add(item);
                itemIdx++;
            }
            return new CounterItem<T>(Category, Name, Type, Level, lstItems);
        }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IntegerCounter(string category, string name, CounterType type, CounterLevel level) : base(category, name, type, level)
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal DoubleCounter(string category, string name, CounterType type, CounterLevel level) : base(category, name, type, level)
        {
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal DecimalCounter(string category, string name, CounterType type, CounterLevel level) : base(category, name, type, level)
        {
        }
    }

}