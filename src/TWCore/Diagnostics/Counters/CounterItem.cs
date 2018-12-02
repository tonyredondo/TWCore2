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

namespace TWCore.Diagnostics.Counters
{
	/// <summary>
	/// Counter item
	/// </summary>
	public class CounterItem<T>
	{
		/// <summary>
		/// Gets or sets the category
		/// </summary>
		/// <value>The counter category</value>
		public string Category { get; set; }
		/// <summary>
		/// Gets or sets the name
		/// </summary>
		/// <value>The counter name</value>
		public string Name { get; set; }
		/// <summary>
		/// Gets or sets the type
		/// </summary>
		/// <value>The counter type</value>
		public CounterType Type { get; set; }
		/// <summary>
		/// Gets or sets the values
		/// </summary>
		/// <value>The counter values</value>
		public CounterItemValue<T>[] Values { get; set; }
	}
	/// <summary>
	/// Counter item value
	/// </summary>
	public class CounterItemValue<T>
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
		public CounterItemValue() { }
		/// <summary>
		/// Counter item
		/// </summary>
		/// <param name="timestamp">Counter value timestamp</param>
		/// <param name="value">Counter value</param>
		public CounterItemValue(DateTime timestamp, T value)
		{
			Timestamp = timestamp;
			Value = value;
		}
	}
}