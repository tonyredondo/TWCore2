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
    /// Counter interface
    /// </summary>
    public interface ICounter
	{
		/// <summary>
		/// Gets the counter category
		/// </summary>
		string Category { get; }
		/// <summary>
		/// Gets the counter name
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Gets the counter type
		/// </summary>
		CounterType Type { get; }
        /// <summary>
        /// Gets the counter level
        /// </summary>
        CounterLevel Level { get; }
	}
	/// <summary>
	/// Counter interface
	/// </summary>
	public interface ICounter<T> : ICounter
	{
		/// <summary>
		/// Add the specified value
		/// </summary>
		/// <param name="value">Value to be added to the counter</param>
		void Add(T value);
	}
}