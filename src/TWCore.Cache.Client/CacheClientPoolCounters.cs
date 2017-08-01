﻿/*
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

using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TWCore.Cache.Client
{
    /// <summary>
    /// Cache client Pool counters
    /// </summary>
    public class CacheClientPoolCounters
    {
        ConcurrentDictionary<string, ConcurrentQueue<double>> Times = new ConcurrentDictionary<string, ConcurrentQueue<double>>();

        #region Calls methods
        /// <summary>
        /// Calls to exist key method
        /// </summary>
        public long ExistKeyCalls;
        /// <summary>
        /// Calls to get method
        /// </summary>
        public long GetCalls;
        /// <summary>
        /// Calls to get by tag method
        /// </summary>
        public long GetByTagCalls;
        /// <summary>
        /// Calls to get creation date method
        /// </summary>
        public long GetCreationDateCalls;
        /// <summary>
        /// Calls to get expiration date method
        /// </summary>
        public long GetExpirationDateCalls;
        /// <summary>
        /// Calls to get keys method
        /// </summary>
        public long GetKeysCalls;
        /// <summary>
        /// Calls to get meta method
        /// </summary>
        public long GetMetaCalls;
        /// <summary>
        /// Calls to get meta by tag method
        /// </summary>
        public long GetMetaByTagCalls;
        /// <summary>
        /// Calls to get or set method
        /// </summary>
        public long GetOrSetCalls;
        /// <summary>
        /// Calls to remove method
        /// </summary>
        public long RemoveCalls;
        /// <summary>
        /// Calls to remove by tag method
        /// </summary>
        public long RemoveByTagCalls;
        /// <summary>
        /// Calls to set method
        /// </summary>
        public long SetCalls;
        /// <summary>
        /// Calls to update data method
        /// </summary>
        public long UpdateDataCalls;
        #endregion

        #region Average time methods
        /// <summary>
        /// Average time on milliseconds of ExistKey execution
        /// </summary>
        public double ExistKeyAverageTime => GetAverage(nameof(ExistKeyCalls));
        /// <summary>
        /// Average time on milliseconds of Get execution
        /// </summary>
        public double GetAverageTime => GetAverage(nameof(GetCalls));
        /// <summary>
        /// Average time on milliseconds of GetByTag execution
        /// </summary>
        public double GetByTagAverageTime => GetAverage(nameof(GetByTagCalls));
        /// <summary>
        /// Average time on milliseconds of GetCreationDate execution
        /// </summary>
        public double GetCreationDateAverageTime => GetAverage(nameof(GetCreationDateCalls));
        /// <summary>
        /// Average time on milliseconds of GetExpirationDate execution
        /// </summary>
        public double GetExpirationDateAverageTime => GetAverage(nameof(GetExpirationDateCalls));
        /// <summary>
        /// Average time on milliseconds of GetKeys execution
        /// </summary>
        public double GetKeysAverageTime => GetAverage(nameof(GetKeysCalls));
        /// <summary>
        /// Average time on milliseconds of GetMeta execution
        /// </summary>
        public double GetMetaAverageTime => GetAverage(nameof(GetMetaCalls));
        /// <summary>
        /// Average time on milliseconds of GetMetaByTag execution
        /// </summary>
        public double GetMetaByTagAverageTime => GetAverage(nameof(GetMetaByTagCalls));
        /// <summary>
        /// Average time on milliseconds of GetOrSet execution
        /// </summary>
        public double GetOrSetAverageTime => GetAverage(nameof(GetOrSetCalls));
        /// <summary>
        /// Average time on milliseconds of Remove execution
        /// </summary>
        public double RemoveAverageTime => GetAverage(nameof(RemoveCalls));
        /// <summary>
        /// Average time on milliseconds of RemoveByTag execution
        /// </summary>
        public double RemoveByTagAverageTime => GetAverage(nameof(RemoveByTagCalls));
        /// <summary>
        /// Average time on milliseconds of Set execution
        /// </summary>
        public double SetAverageTime => GetAverage(nameof(SetCalls));
        /// <summary>
        /// Average time on milliseconds of UpdateData execution
        /// </summary>
        public double UpdateDataAverageTime => GetAverage(nameof(UpdateDataCalls));
        #endregion

        #region .ctor
        /// <summary>
        /// Cache client Pool counters
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CacheClientPoolCounters()
        {
            Core.Status.Attach(collection =>
            {
                collection.SortValues = false;

                collection.Add("Calls to exist key method", ExistKeyCalls);
                collection.Add("Calls to get method", GetCalls);
                collection.Add("Calls to get by tag method", GetByTagCalls);
                collection.Add("Calls to get creation date method", GetCreationDateCalls);
                collection.Add("Calls to get expiration date method", GetExpirationDateCalls);
                collection.Add("Calls to get keys method", GetKeysCalls);
                collection.Add("Calls to get meta method", GetMetaCalls);
                collection.Add("Calls to get meta by tag method", GetMetaByTagCalls);
                collection.Add("Calls to get or set method", GetOrSetCalls);
                collection.Add("Calls to remove method", RemoveCalls);
                collection.Add("Calls to remove by tag method", RemoveByTagCalls);
                collection.Add("Calls to set method", SetCalls);
                collection.Add("Calls to update data method", UpdateDataCalls);

                //
                collection.Add("Average time on milliseconds of ExistKey execution", ExistKeyAverageTime);
                collection.Add("Average time on milliseconds of Get execution", GetAverageTime);
                collection.Add("Average time on milliseconds of GetByTag execution", GetByTagAverageTime);
                collection.Add("Average time on milliseconds of GetCreationDate execution", GetCreationDateAverageTime);
                collection.Add("Average time on milliseconds of GetExpirationDate execution", GetExpirationDateAverageTime);
                collection.Add("Average time on milliseconds of GetKeys execution", GetKeysAverageTime);
                collection.Add("Average time on milliseconds of GetMeta execution", GetMetaAverageTime);
                collection.Add("Average time on milliseconds of GetMetaByTag execution", GetMetaByTagAverageTime);
                collection.Add("Average time on milliseconds of GetOrSet execution", GetOrSetAverageTime);
                collection.Add("Average time on milliseconds of Remove execution", RemoveAverageTime);
                collection.Add("Average time on milliseconds of RemoveByTag execution", RemoveByTagAverageTime);
                collection.Add("Average time on milliseconds of Set execution", SetAverageTime);
                collection.Add("Average time on milliseconds of UpdateData execution", UpdateDataAverageTime);
            });
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementExistKey(double executionTime)
        {
            Interlocked.Increment(ref ExistKeyCalls);
            IncrementQueue(nameof(ExistKeyCalls), executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGet(double executionTime)
        {
            Interlocked.Increment(ref GetCalls);
            IncrementQueue(nameof(GetCalls), executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetByTag(double executionTime)
        {
            Interlocked.Increment(ref GetByTagCalls);
            IncrementQueue(nameof(GetByTagCalls), executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetCreationDate(double executionTime)
        {
            Interlocked.Increment(ref GetCreationDateCalls);
            IncrementQueue(nameof(GetCreationDateCalls), executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetExpirationDate(double executionTime)
        {
            Interlocked.Increment(ref GetExpirationDateCalls);
            IncrementQueue(nameof(GetExpirationDateCalls), executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetKeys(double executionTime)
        {
            Interlocked.Increment(ref GetKeysCalls);
            IncrementQueue(nameof(GetKeysCalls), executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetMeta(double executionTime)
        {
            Interlocked.Increment(ref GetMetaCalls);
            IncrementQueue(nameof(GetMetaCalls), executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetMetaByTag(double executionTime)
        {
            Interlocked.Increment(ref GetMetaByTagCalls);
            IncrementQueue(nameof(GetMetaByTagCalls), executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetOrSet(double executionTime)
        {
            Interlocked.Increment(ref GetOrSetCalls);
            IncrementQueue(nameof(GetOrSetCalls), executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementRemove(double executionTime)
        {
            Interlocked.Increment(ref RemoveCalls);
            IncrementQueue(nameof(RemoveCalls), executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementRemoveByTag(double executionTime)
        {
            Interlocked.Increment(ref RemoveByTagCalls);
            IncrementQueue(nameof(RemoveByTagCalls), executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementSet(double executionTime)
        {
            Interlocked.Increment(ref SetCalls);
            IncrementQueue(nameof(SetCalls), executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementUpdateData(double executionTime)
        {
            Interlocked.Increment(ref UpdateDataCalls);
            IncrementQueue(nameof(UpdateDataCalls), executionTime);
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IncrementQueue(string name, double executionTime)
        {
            var queue = Times.GetOrAdd(name, key => new ConcurrentQueue<double>());
            queue.Enqueue(executionTime);
            while (queue.Count > 50)
                queue.TryDequeue(out double res);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        double GetAverage(string name)
        {
            var queue = Times.GetOrAdd(name, key => new ConcurrentQueue<double>());
            if (queue.Count > 0)
                return queue.Average();
            return 0;
        }
    }
}
