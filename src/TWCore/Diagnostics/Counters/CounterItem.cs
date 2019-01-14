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
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TWCore.Diagnostics.Counters
{
    /// <summary>
    /// Counter item
    /// </summary>
    public sealed class CounterItem<T> : ICounterItem
	{
        /// <summary>
        /// Gets or sets the counter environment
        /// </summary>
        public string Environment { get; set; }
        /// <summary>
        /// Gets or sets the counter application name
        /// </summary>
        public string Application { get; set; }
		/// <summary>
		/// Gets or sets the counter category
		/// </summary>
		/// <value>The counter category</value>
		public string Category { get; set; }
		/// <summary>
		/// Gets or sets the counter name
		/// </summary>
		/// <value>The counter name</value>
		public string Name { get; set; }
		/// <summary>
		/// Gets or sets the counter type
		/// </summary>
		/// <value>The counter type</value>
		public CounterType Type { get; set; }
        /// <summary>
        /// Gets or sets the counter level
        /// </summary>
        /// <value>The counter level</value>
        public CounterLevel Level { get; set; }
        /// <summary>
        /// Gets the counter kind
        /// </summary>
        public CounterKind Kind { get; set; }
        /// <summary>
        /// Gets or sets the counter unit
        /// </summary>
        public CounterUnit Unit { get; set; }
        /// <summary>
        /// Type of value
        /// </summary>
        public Type TypeOfValue => typeof(T);
        /// <summary>
        /// Values Count
        /// </summary>
        public int Count => Values.Count;
        /// <summary>
        /// Gets or sets the values
        /// </summary>
        /// <value>The counter values</value>
        public List<CounterItemValue<T>> Values { get; set; }

        /// <summary>
        /// Counter item
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CounterItem() { }


        /// <summary>
        /// Counter item
        /// </summary>
        /// <param name="category">Counter category</param>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        /// <param name="kind">Counter kind</param>
        /// <param name="unit">Counter unit</param>
        /// <param name="values">Counter values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CounterItem<T> Retrieve(string category, string name, CounterType type, CounterLevel level, CounterKind kind, CounterUnit unit, List<CounterItemValue<T>> values)
        {
            var item = ReferencePool<CounterItem<T>>.Shared.New();
            item.Environment = Core.EnvironmentName;
            item.Application = Core.ApplicationName;
            item.Category = category;
            item.Name = name;
            item.Type = type;
            item.Level = level;
            item.Kind = kind;
            item.Unit = unit;
            item.Values = values;
            return item;
        }
        /// <summary>
        /// Store the specified item.
        /// </summary>
        /// <param name="item">Item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(CounterItem<T> item)
        {
            item.Environment = null;
            item.Application = null;
            item.Category = null;
            item.Name = null;
            item.Type = CounterType.Cumulative;
            item.Level = CounterLevel.User;
            item.Kind = CounterKind.Unknown;
            item.Unit = CounterUnit.Unknown;
            item.Values = null;
            ReferencePool<CounterItem<T>>.Shared.Store(item);
        }
    }
	/// <summary>
	/// Counter item value
	/// </summary>
	public sealed class CounterItemValue<T>
	{
		/// <summary>
		/// Counter value timestamp
		/// </summary>
		public DateTime Timestamp { get; set; }
		/// <summary>
		/// Counter value
		/// </summary>
		public T Value { get; set; }

        /// <summary>
        /// Counter item
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CounterItemValue() { }

        /// <summary>
        /// Retrieve Counter item
        /// </summary>
        /// <param name="timestamp">Counter value timestamp</param>
        /// <param name="value">Counter value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CounterItemValue<T> Retrieve(DateTime timestamp, T value)
        {
            var item = ReferencePool<CounterItemValue<T>>.Shared.New();
            item.Timestamp = timestamp;
            item.Value = value;
            return item;
        }
        /// <summary>
        /// Store the specified item.
        /// </summary>
        /// <param name="item">Item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(CounterItemValue<T> item)
        {
            item.Timestamp = DateTime.MinValue;
            item.Value = default;
            ReferencePool<CounterItemValue<T>>.Shared.Store(item);
        }
    }
}