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


using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using TWCore.Diagnostics.Counters.Storages;
using TWCore.Settings;

namespace TWCore.Diagnostics.Counters
{

    /// <summary>
    /// Counters engine
    /// </summary>
    public class CountersEngine : ICountersEngine
    {
        private readonly ConcurrentDictionary<(string Category, string Name), ICounter> _counters = new ConcurrentDictionary<(string Category, string Name), ICounter>();
        private readonly BlockingCollection<ICounterReader> _counterReaders = new BlockingCollection<ICounterReader>();

        /// <summary>
        /// Gets the counter storage
        /// </summary>
        public ICountersStorage Storage { get; set; }
        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        public CountersEngineSettings Settings { get; } = Core.GetSettings<CountersEngineSettings>();

        #region .ctor
        /// <summary>
        /// Counters engine
        /// </summary>
        public CountersEngine()
        {
        }
        /// <summary>
        /// Counters engine
        /// </summary>
        /// <param name="storage">Counter storage</param>
        public CountersEngine(ICountersStorage storage)
        {
            Storage = storage;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessCounters()
        {
            var counters = _counterReaders.Select(i => i.Take(1000));
            Storage.Store(counters);
        }
        #endregion

        /// <summary>
        /// Counters engine sttings
        /// </summary>
        public class CountersEngineSettings: SettingsBase
        {
            /// <summary>
            /// Gets or sets the flush timeout in seconds
            /// </summary>
            public int FlushTimeoutInSeconds { get; set; } = 10;
            /// <summary>
            /// Gets or sets the maximum counter values batch per counter
            /// </summary>
            public int MaximumBatchPerCounter { get; set; } = 1000;
        }
    }
}