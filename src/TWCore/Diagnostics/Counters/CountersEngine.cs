﻿/*
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


using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using TWCore.Diagnostics.Counters.Storages;

namespace TWCore.Diagnostics.Counters
{

    /// <summary>
    /// Counters engine
    /// </summary>
    public class CountersEngine : ICountersEngine
    {
        private ConcurrentDictionary<(string Category, string Name), ICounter> _counters = new ConcurrentDictionary<(string Category, string Name), ICounter>();
        private BlockingCollection<ICounterReader> _counterReaders = new BlockingCollection<ICounterReader>();

        /// <summary>
        /// Gets the storages
        /// </summary>
        /// <value>The storages collection</value>
        public BlockingCollection<ICountersStorage> Storages { get; }

        #region .ctor
        /// <summary>
        /// Counters engine
        /// </summary>
        public CountersEngine()
        {
            Storages = new BlockingCollection<ICountersStorage>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets an integer counter
        /// </summary>
        /// <returns>The integer counter</returns>
        /// <param name="category">Counter category</param>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        public IntegerCounter GetIntegerCounter(string category, string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User)
        {
            var counter = _counters.GetOrAdd((category, name), _ =>
            {
                var item = new IntegerCounter(category, name, type, level);
                _counterReaders.Add(item);
                return item;
            });
            if (counter is IntegerCounter iCounter)
                return iCounter;
            throw new InvalidCounterException();
        }
        /// <summary>
        /// Gets an integer counter
        /// </summary>
        /// <returns>The integer counter</returns>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        public IntegerCounter GetIntegerCounter(string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User)
        {
            var counter = _counters.GetOrAdd((string.Empty, name), _ =>
            {
                var item = new IntegerCounter(string.Empty, name, type, level);
                _counterReaders.Add(item);
                return item;
            });
            if (counter is IntegerCounter iCounter)
                return iCounter;
            throw new InvalidCounterException();
        }
        /// <summary>
        /// Gets an double counter
        /// </summary>
        /// <returns>The double counter</returns>
        /// <param name="category">Counter category</param>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        public DoubleCounter GetDoubleCounter(string category, string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User)
        {
            var counter = _counters.GetOrAdd((category, name), _ =>
            {
                var item = new DoubleCounter(category, name, type, level);
                _counterReaders.Add(item);
                return item;
            });
            if (counter is DoubleCounter iCounter)
                return iCounter;
            throw new InvalidCounterException();

        }
        /// <summary>
        /// Gets an double counter
        /// </summary>
        /// <returns>The double counter</returns>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        public DoubleCounter GetDoubleCounter(string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User)
        {
            var counter = _counters.GetOrAdd((string.Empty, name), _ =>
            {
                var item = new DoubleCounter(string.Empty, name, type, level);
                _counterReaders.Add(item);
                return item;
            });
            if (counter is DoubleCounter iCounter)
                return iCounter;
            throw new InvalidCounterException();
        }
        /// <summary>
        /// Gets an decimal counter
        /// </summary>
        /// <returns>The decimal counter</returns>
        /// <param name="category">Counter category</param>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        public DecimalCounter GetDecimalCounter(string category, string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User)
        {
            var counter = _counters.GetOrAdd((category, name), _ =>
            {
                var item = new DecimalCounter(category, name, type, level);
                _counterReaders.Add(item);
                return item;
            });
            if (counter is DecimalCounter iCounter)
                return iCounter;
            throw new InvalidCounterException();
        }
        /// <summary>
        /// Gets an decimal counter
        /// </summary>
        /// <returns>The decimal counter</returns>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        public DecimalCounter GetDecimalCounter(string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User)
        {
            var counter = _counters.GetOrAdd((string.Empty, name), _ =>
            {
                var item = new DecimalCounter(string.Empty, name, type, level);
                _counterReaders.Add(item);
                return item;
            });
            if (counter is DecimalCounter iCounter)
                return iCounter;
            throw new InvalidCounterException();
        }
        #endregion

        #region Private Methods
        private void ProcessCounters()
        {
            if (Storages == null) return;
            foreach (var storage in Storages)
                storage.Store(_counterReaders);
        }
        #endregion
    }
}