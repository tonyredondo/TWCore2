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
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using TWCore.Diagnostics.Counters.Storages;
using TWCore.Settings;

namespace TWCore.Diagnostics.Counters
{
    /// <summary>
    /// Counters engine
    /// </summary>
    public class DefaultCountersEngine : ICountersEngine
    {
        private readonly ConcurrentDictionary<(string Category, string Name), ICounter> _counters = new ConcurrentDictionary<(string Category, string Name), ICounter>();
        private readonly BlockingCollection<ICounterReader> _counterReaders = new BlockingCollection<ICounterReader>();
        private readonly List<(ICounterReader, ICounterItem)> _counterReaderItemList = new List<(ICounterReader, ICounterItem)>();
        private readonly List<ICounterItem> _counterList = new List<ICounterItem>();
        private ICountersStorage[] _storages = null;
        private Timer _timer = null;
        private int _inProcess = 0;

        /// <summary>
        /// Gets the counter storage
        /// </summary>
        public ObservableCollection<ICountersStorage> Storages { get; set; }
        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        public CountersEngineSettings Settings { get; } = Core.GetSettings<CountersEngineSettings>();

        #region .ctor
        /// <summary>
        /// Counters engine
        /// </summary>
        public DefaultCountersEngine()
        {
            Storages = new ObservableCollection<ICountersStorage>();
            Storages.CollectionChanged += (sender, e) => 
            {
                _storages = Storages.ToArray();
            };
        }
        /// <summary>
        /// Counters engine
        /// </summary>
        /// <param name="storages">Counter storages</param>
        public DefaultCountersEngine(params ICountersStorage[] storages)
        {
            Storages = new ObservableCollection<ICountersStorage>();
            if (storages != null)
            {
                foreach (var storage in storages)
                    Storages.Add(storage);
                _storages = Storages.ToArray();
            }
            Storages.CollectionChanged += (sender, e) =>
           {
               _storages = Storages.ToArray();
           };
        }
        #endregion

        #region Private Methods
        private static void ThrowInvalidCounter() => throw new InvalidCounterException();
        #endregion

        #region Public Methods
        /// <summary>
        /// Start engine
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start()
        {
            if (_timer == null)
                _timer = new Timer(ProcessCounters);
            _timer.Change(Settings.FlushTimeoutInSeconds * 1000, Settings.FlushTimeoutInSeconds * 1000);
        }
        /// <summary>
        /// Stop engine
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        #endregion

        #region ICountersEngine Methods
        /// <summary>
        /// Gets an integer counter
        /// </summary>
        /// <returns>The integer counter</returns>
        /// <param name="category">Counter category</param>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        /// <param name="kind">Counter kind</param>
        /// <param name="unit">Counter unit</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntegerCounter GetIntegerCounter(string category, string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User, CounterKind kind = CounterKind.Application, CounterUnit unit = CounterUnit.Unknown)
        {
            var counter = _counters.GetOrAdd((category, name), _ =>
            {
                var item = new IntegerCounter(category, name, type, level, kind, unit);
                _counterReaders.Add(item);
                return item;
            });
            if (counter is IntegerCounter iCounter)
                return iCounter;
            ThrowInvalidCounter();
            return null;
        }
        /// <summary>
        /// Gets an integer counter
        /// </summary>
        /// <returns>The integer counter</returns>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        /// <param name="kind">Counter kind</param>
        /// <param name="unit">Counter unit</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntegerCounter GetIntegerCounter(string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User, CounterKind kind = CounterKind.Application, CounterUnit unit = CounterUnit.Unknown)
        {
            var counter = _counters.GetOrAdd((string.Empty, name), _ =>
            {
                var item = new IntegerCounter(string.Empty, name, type, level, kind, unit);
                _counterReaders.Add(item);
                return item;
            });
            if (counter is IntegerCounter iCounter)
                return iCounter;
            ThrowInvalidCounter();
            return null;
        }
        /// <summary>
        /// Gets an double counter
        /// </summary>
        /// <returns>The double counter</returns>
        /// <param name="category">Counter category</param>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        /// <param name="kind">Counter kind</param>
        /// <param name="unit">Counter unit</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DoubleCounter GetDoubleCounter(string category, string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User, CounterKind kind = CounterKind.Application, CounterUnit unit = CounterUnit.Unknown)
        {
            var counter = _counters.GetOrAdd((category, name), _ =>
            {
                var item = new DoubleCounter(category, name, type, level, kind, unit);
                _counterReaders.Add(item);
                return item;
            });
            if (counter is DoubleCounter iCounter)
                return iCounter;
            ThrowInvalidCounter();
            return null;
        }
        /// <summary>
        /// Gets an double counter
        /// </summary>
        /// <returns>The double counter</returns>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        /// <param name="kind">Counter kind</param>
        /// <param name="unit">Counter unit</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DoubleCounter GetDoubleCounter(string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User, CounterKind kind = CounterKind.Application, CounterUnit unit = CounterUnit.Unknown)
        {
            var counter = _counters.GetOrAdd((string.Empty, name), _ =>
            {
                var item = new DoubleCounter(string.Empty, name, type, level, kind, unit);
                _counterReaders.Add(item);
                return item;
            });
            if (counter is DoubleCounter iCounter)
                return iCounter;
            ThrowInvalidCounter();
            return null;
        }
        /// <summary>
        /// Gets an decimal counter
        /// </summary>
        /// <returns>The decimal counter</returns>
        /// <param name="category">Counter category</param>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        /// <param name="kind">Counter kind</param>
        /// <param name="unit">Counter unit</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DecimalCounter GetDecimalCounter(string category, string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User, CounterKind kind = CounterKind.Application, CounterUnit unit = CounterUnit.Unknown)
        {
            var counter = _counters.GetOrAdd((category, name), _ =>
            {
                var item = new DecimalCounter(category, name, type, level, kind, unit);
                _counterReaders.Add(item);
                return item;
            });
            if (counter is DecimalCounter iCounter)
                return iCounter;
            ThrowInvalidCounter();
            return null;
        }
        /// <summary>
        /// Gets an decimal counter
        /// </summary>
        /// <returns>The decimal counter</returns>
        /// <param name="name">Counter name</param>
        /// <param name="type">Counter type</param>
        /// <param name="level">Counter level</param>
        /// <param name="kind">Counter kind</param>
        /// <param name="unit">Counter unit</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DecimalCounter GetDecimalCounter(string name, CounterType type = CounterType.Cumulative, CounterLevel level = CounterLevel.User, CounterKind kind = CounterKind.Application, CounterUnit unit = CounterUnit.Unknown)
        {
            var counter = _counters.GetOrAdd((string.Empty, name), _ =>
            {
                var item = new DecimalCounter(string.Empty, name, type, level, kind, unit);
                _counterReaders.Add(item);
                return item;
            });
            if (counter is DecimalCounter iCounter)
                return iCounter;
            ThrowInvalidCounter();
            return null;
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessCounters(object state)
        {
            var storages = _storages;
            if (storages == null) return;
            if (storages.Length == 0) return;
            if (Interlocked.CompareExchange(ref _inProcess, 1, 0) == 1) return;
            try
            {
                foreach(var i in _counterReaders)
                {
                    var value = i.Take(Settings.MaximumBatchPerCounter);
                    if (!(value is null))
                    {
                        _counterList.Add(value);
                        _counterReaderItemList.Add((i, value));
                    }
                }
                if (_counterList.Count > 0)
                {
                    foreach (var storage in storages)
                        storage.Store(_counterList);

                    foreach (var item in _counterReaderItemList)
                        item.Item1.ReturnToPool(item.Item2);
                }
                _counterList.Clear();
                _counterReaderItemList.Clear();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            Interlocked.Exchange(ref _inProcess, 0);
        }
        #endregion

        #region Disposable
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Stop();
            _counters.Clear();
            _counterReaders.Dispose();
            _storages = null;
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
            public int FlushTimeoutInSeconds { get; set; } = 30;
            /// <summary>
            /// Gets or sets the maximum counter values batch per counter
            /// </summary>
            public int MaximumBatchPerCounter { get; set; } = 10000;
        }
    }
}