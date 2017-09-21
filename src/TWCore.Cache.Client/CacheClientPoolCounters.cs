/*
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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Cache.Client
{
    /// <summary>
    /// Cache client Pool counters
    /// </summary>
    public class CacheClientPoolCounters
    {
        private readonly Queue<double>[] _times = new Queue<double>[13];

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
        public double ExistKeyAverageTime => GetAverage(0);
        /// <summary>
        /// Average time on milliseconds of Get execution
        /// </summary>
        public double GetAverageTime => GetAverage(1);
        /// <summary>
        /// Average time on milliseconds of GetByTag execution
        /// </summary>
        public double GetByTagAverageTime => GetAverage(2);
        /// <summary>
        /// Average time on milliseconds of GetCreationDate execution
        /// </summary>
        public double GetCreationDateAverageTime => GetAverage(3);
        /// <summary>
        /// Average time on milliseconds of GetExpirationDate execution
        /// </summary>
        public double GetExpirationDateAverageTime => GetAverage(4);
        /// <summary>
        /// Average time on milliseconds of GetKeys execution
        /// </summary>
        public double GetKeysAverageTime => GetAverage(5);
        /// <summary>
        /// Average time on milliseconds of GetMeta execution
        /// </summary>
        public double GetMetaAverageTime => GetAverage(6);
        /// <summary>
        /// Average time on milliseconds of GetMetaByTag execution
        /// </summary>
        public double GetMetaByTagAverageTime => GetAverage(7);
        /// <summary>
        /// Average time on milliseconds of GetOrSet execution
        /// </summary>
        public double GetOrSetAverageTime => GetAverage(8);
        /// <summary>
        /// Average time on milliseconds of Remove execution
        /// </summary>
        public double RemoveAverageTime => GetAverage(9);
        /// <summary>
        /// Average time on milliseconds of RemoveByTag execution
        /// </summary>
        public double RemoveByTagAverageTime => GetAverage(10);
        /// <summary>
        /// Average time on milliseconds of Set execution
        /// </summary>
        public double SetAverageTime => GetAverage(11);
        /// <summary>
        /// Average time on milliseconds of UpdateData execution
        /// </summary>
        public double UpdateDataAverageTime => GetAverage(12);
        #endregion

        #region .ctor
        /// <summary>
        /// Cache client Pool counters
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CacheClientPoolCounters()
        {
			for(var i = 0; i < _times.Length; i++)
				_times[i] = new Queue<double>();
			
            Core.Status.Attach(collection =>
            {
                collection.SortValues = false;

                collection.Add("Calls to exist key method", ExistKeyCalls, true);
                collection.Add("Calls to get method", GetCalls, true);
                collection.Add("Calls to get by tag method", GetByTagCalls, true);
                collection.Add("Calls to get creation date method", GetCreationDateCalls, true);
                collection.Add("Calls to get expiration date method", GetExpirationDateCalls, true);
                collection.Add("Calls to get keys method", GetKeysCalls, true);
                collection.Add("Calls to get meta method", GetMetaCalls, true);
                collection.Add("Calls to get meta by tag method", GetMetaByTagCalls, true);
                collection.Add("Calls to get or set method", GetOrSetCalls, true);
                collection.Add("Calls to remove method", RemoveCalls, true);
                collection.Add("Calls to remove by tag method", RemoveByTagCalls, true);
                collection.Add("Calls to set method", SetCalls, true);
                collection.Add("Calls to update data method", UpdateDataCalls, true);

                //
                collection.Add("Average time on milliseconds of ExistKey execution", ExistKeyAverageTime, true);
                collection.Add("Average time on milliseconds of Get execution", GetAverageTime, true);
                collection.Add("Average time on milliseconds of GetByTag execution", GetByTagAverageTime, true);
                collection.Add("Average time on milliseconds of GetCreationDate execution", GetCreationDateAverageTime, true);
                collection.Add("Average time on milliseconds of GetExpirationDate execution", GetExpirationDateAverageTime, true);
                collection.Add("Average time on milliseconds of GetKeys execution", GetKeysAverageTime, true);
                collection.Add("Average time on milliseconds of GetMeta execution", GetMetaAverageTime, true);
                collection.Add("Average time on milliseconds of GetMetaByTag execution", GetMetaByTagAverageTime, true);
                collection.Add("Average time on milliseconds of GetOrSet execution", GetOrSetAverageTime, true);
                collection.Add("Average time on milliseconds of Remove execution", RemoveAverageTime, true);
                collection.Add("Average time on milliseconds of RemoveByTag execution", RemoveByTagAverageTime, true);
                collection.Add("Average time on milliseconds of Set execution", SetAverageTime, true);
                collection.Add("Average time on milliseconds of UpdateData execution", UpdateDataAverageTime, true);
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
            IncrementQueue(0, ref executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGet(double executionTime)
        {
            Interlocked.Increment(ref GetCalls);
            IncrementQueue(1, ref executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetByTag(double executionTime)
        {
            Interlocked.Increment(ref GetByTagCalls);
            IncrementQueue(2, ref executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetCreationDate(double executionTime)
        {
            Interlocked.Increment(ref GetCreationDateCalls);
            IncrementQueue(3, ref executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetExpirationDate(double executionTime)
        {
            Interlocked.Increment(ref GetExpirationDateCalls);
            IncrementQueue(4, ref executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetKeys(double executionTime)
        {
            Interlocked.Increment(ref GetKeysCalls);
            IncrementQueue(5, ref executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetMeta(double executionTime)
        {
            Interlocked.Increment(ref GetMetaCalls);
            IncrementQueue(6, ref executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetMetaByTag(double executionTime)
        {
            Interlocked.Increment(ref GetMetaByTagCalls);
            IncrementQueue(7, ref executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetOrSet(double executionTime)
        {
            Interlocked.Increment(ref GetOrSetCalls);
            IncrementQueue(8, ref executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementRemove(double executionTime)
        {
            Interlocked.Increment(ref RemoveCalls);
            IncrementQueue(9, ref executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementRemoveByTag(double executionTime)
        {
            Interlocked.Increment(ref RemoveByTagCalls);
            IncrementQueue(10, ref executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementSet(double executionTime)
        {
            Interlocked.Increment(ref SetCalls);
            IncrementQueue(11, ref executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementUpdateData(double executionTime)
        {
            Interlocked.Increment(ref UpdateDataCalls);
            IncrementQueue(12, ref executionTime);
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IncrementQueue(int idx, ref double executionTime)
        {
			var queue = _times[idx];
			lock(queue) 
			{
				queue.Enqueue(executionTime);
				while (queue.Count > 100)
					queue.Dequeue();
			}
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double GetAverage(int idx)
        {
			var queue = _times[idx];
			lock(queue)
				return queue.Count > 0 ? queue.Average() : 0;
		}
    }
}
