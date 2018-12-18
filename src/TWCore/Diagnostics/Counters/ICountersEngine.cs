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
using System.Collections.ObjectModel;
using TWCore.Diagnostics.Counters.Storages;

namespace TWCore.Diagnostics.Counters
{
	/// <summary>
	/// Counter engine interface
	/// </summary>
	public interface ICountersEngine : IDisposable
	{
        /// <summary>
        /// Gets the counter storage
        /// </summary>
        ObservableCollection<ICountersStorage> Storages { get; set; }
        /// <summary>
        /// Start engine
        /// </summary>
        void Start();
        /// <summary>
        /// Stop engine
        /// </summary>
        void Stop();
        /// <summary>
        /// Gets an integer counter
        /// </summary>
        /// <returns>The integer counter</returns>
        /// <param name="category">Counter category</param>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        IntegerCounter GetIntegerCounter(string category, string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User);
		/// <summary>
		/// Gets an integer counter
		/// </summary>
		/// <returns>The integer counter</returns>
		/// <param name="name">Counter name</param>
		/// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
		IntegerCounter GetIntegerCounter(string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User);
		/// <summary>
		/// Gets an double counter
		/// </summary>
		/// <returns>The double counter</returns>
		/// <param name="category">Counter category</param>
		/// <param name="name">Counter name</param>
		/// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
		DoubleCounter GetDoubleCounter(string category, string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User);
		/// <summary>
		/// Gets an double counter
		/// </summary>
		/// <returns>The double counter</returns>
		/// <param name="name">Counter name</param>
		/// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
		DoubleCounter GetDoubleCounter(string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User);
		/// <summary>
		/// Gets an decimal counter
		/// </summary>
		/// <returns>The decimal counter</returns>
		/// <param name="category">Counter category</param>
		/// <param name="name">Counter name</param>
		/// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
		DecimalCounter GetDecimalCounter(string category, string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User);
		/// <summary>
		/// Gets an decimal counter
		/// </summary>
		/// <returns>The decimal counter</returns>
		/// <param name="name">Counter name</param>
		/// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
		DecimalCounter GetDecimalCounter(string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User);
	}
}