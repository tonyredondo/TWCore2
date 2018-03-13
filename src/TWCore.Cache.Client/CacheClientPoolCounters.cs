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

using System.Runtime.CompilerServices;
using System.Threading;
using TWCore.Diagnostics.Status;

// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Cache.Client
{
    /// <summary>
    /// Cache client Pool counters
    /// </summary>
    public class CacheClientPoolCounters
    {
        private readonly double?[] _times = new double?[15];

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
        /// <summary>
        /// Calls to copy method
        /// </summary>
        public long CopyCalls;
        /// <summary>
        /// Calls to set multi method
        /// </summary>
        public long SetMultiCalls;
        #endregion

        #region Average time methods
        /// <summary>
        /// Average time on milliseconds of ExistKey execution
        /// </summary>
        public double ExistKeyAverageTime => _times[0] ?? 0;
        /// <summary>
        /// Average time on milliseconds of Get execution
        /// </summary>
        public double GetAverageTime => _times[1] ?? 0;
        /// <summary>
        /// Average time on milliseconds of GetByTag execution
        /// </summary>
        public double GetByTagAverageTime => _times[2] ?? 0;
        /// <summary>
        /// Average time on milliseconds of GetCreationDate execution
        /// </summary>
        public double GetCreationDateAverageTime => _times[3] ?? 0;
        /// <summary>
        /// Average time on milliseconds of GetExpirationDate execution
        /// </summary>
        public double GetExpirationDateAverageTime => _times[4] ?? 0;
        /// <summary>
        /// Average time on milliseconds of GetKeys execution
        /// </summary>
        public double GetKeysAverageTime => _times[5] ?? 0;
        /// <summary>
        /// Average time on milliseconds of GetMeta execution
        /// </summary>
        public double GetMetaAverageTime => _times[6] ?? 0;
        /// <summary>
        /// Average time on milliseconds of GetMetaByTag execution
        /// </summary>
        public double GetMetaByTagAverageTime => _times[7] ?? 0;
        /// <summary>
        /// Average time on milliseconds of GetOrSet execution
        /// </summary>
        public double GetOrSetAverageTime => _times[8] ?? 0;
        /// <summary>
        /// Average time on milliseconds of Remove execution
        /// </summary>
        public double RemoveAverageTime => _times[9] ?? 0;
        /// <summary>
        /// Average time on milliseconds of RemoveByTag execution
        /// </summary>
        public double RemoveByTagAverageTime => _times[10] ?? 0;
        /// <summary>
        /// Average time on milliseconds of Set execution
        /// </summary>
        public double SetAverageTime => _times[11] ?? 0;
        /// <summary>
        /// Average time on milliseconds of UpdateData execution
        /// </summary>
        public double UpdateDataAverageTime => _times[12] ?? 0;
        /// <summary>
        /// Average time on milliseconds of Copy execution
        /// </summary>
        public double CopyAverageTime => _times[13] ?? 0;
        /// <summary>
        /// Average time on milliseconds of Set multi execution
        /// </summary>
        public double SetMultiAverageTime => _times[14] ?? 0;
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

                collection.Add("ExistKey Method", new StatusItemValueItem("Calls", ExistKeyCalls, true), new StatusItemValueItem("Average time (ms)", ExistKeyAverageTime, true));
                collection.Add("Get Method", new StatusItemValueItem("Calls", GetCalls, true), new StatusItemValueItem("Average time (ms)", GetAverageTime, true));
                collection.Add("GetByTag Method", new StatusItemValueItem("Calls", GetByTagCalls, true), new StatusItemValueItem("Average time (ms)", GetByTagAverageTime, true));
                collection.Add("GetCreationDate Method", new StatusItemValueItem("Calls", GetCreationDateCalls, true), new StatusItemValueItem("Average time (ms)", GetCreationDateAverageTime, true));
                collection.Add("GetExpirationDate Method", new StatusItemValueItem("Calls", GetExpirationDateCalls, true), new StatusItemValueItem("Average time (ms)", GetExpirationDateAverageTime, true));
                collection.Add("GetKeys Method", new StatusItemValueItem("Calls", GetKeysCalls, true), new StatusItemValueItem("Average time (ms)", GetKeysAverageTime, true));
                collection.Add("GetMeta Method", new StatusItemValueItem("Calls", GetMetaCalls, true), new StatusItemValueItem("Average time (ms)", GetMetaAverageTime, true));
                collection.Add("GetMetaByTag Method", new StatusItemValueItem("Calls", GetMetaByTagCalls, true), new StatusItemValueItem("Average time (ms)", GetMetaByTagAverageTime, true));
                collection.Add("GetOrSet Method", new StatusItemValueItem("Calls", GetOrSetCalls, true), new StatusItemValueItem("Average time (ms)", GetOrSetAverageTime, true));
                collection.Add("Remove Method", new StatusItemValueItem("Calls", RemoveCalls, true), new StatusItemValueItem("Average time (ms)", RemoveAverageTime, true));
                collection.Add("RemoveByTag Method", new StatusItemValueItem("Calls", RemoveByTagCalls, true), new StatusItemValueItem("Average time (ms)", RemoveByTagAverageTime, true));
                collection.Add("Set Method", new StatusItemValueItem("Calls", SetCalls, true), new StatusItemValueItem("Average time (ms)", SetAverageTime, true));
                collection.Add("Update Method", new StatusItemValueItem("Calls", UpdateDataCalls, true), new StatusItemValueItem("Average time (ms)", UpdateDataAverageTime, true));
                collection.Add("Copy Method", new StatusItemValueItem("Calls", CopyCalls, true), new StatusItemValueItem("Average time (ms)", CopyAverageTime, true));
                collection.Add("Set Multi Method", new StatusItemValueItem("Calls", SetMultiCalls, true), new StatusItemValueItem("Average time (ms)", SetMultiAverageTime, true));
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
            IncrementQueue(0, executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGet(double executionTime)
        {
            Interlocked.Increment(ref GetCalls);
            IncrementQueue(1, executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetByTag(double executionTime)
        {
            Interlocked.Increment(ref GetByTagCalls);
            IncrementQueue(2, executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetCreationDate(double executionTime)
        {
            Interlocked.Increment(ref GetCreationDateCalls);
            IncrementQueue(3, executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetExpirationDate(double executionTime)
        {
            Interlocked.Increment(ref GetExpirationDateCalls);
            IncrementQueue(4, executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetKeys(double executionTime)
        {
            Interlocked.Increment(ref GetKeysCalls);
            IncrementQueue(5, executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetMeta(double executionTime)
        {
            Interlocked.Increment(ref GetMetaCalls);
            IncrementQueue(6, executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetMetaByTag(double executionTime)
        {
            Interlocked.Increment(ref GetMetaByTagCalls);
            IncrementQueue(7, executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetOrSet(double executionTime)
        {
            Interlocked.Increment(ref GetOrSetCalls);
            IncrementQueue(8, executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementRemove(double executionTime)
        {
            Interlocked.Increment(ref RemoveCalls);
            IncrementQueue(9, executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementRemoveByTag(double executionTime)
        {
            Interlocked.Increment(ref RemoveByTagCalls);
            IncrementQueue(10, executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementSet(double executionTime)
        {
            Interlocked.Increment(ref SetCalls);
            IncrementQueue(11, executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementUpdateData(double executionTime)
        {
            Interlocked.Increment(ref UpdateDataCalls);
            IncrementQueue(12, executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementCopy(double executionTime)
        {
            Interlocked.Increment(ref CopyCalls);
            IncrementQueue(13, executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementSetMulti(double executionTime)
        {
            Interlocked.Increment(ref SetMultiCalls);
            IncrementQueue(14, executionTime);
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IncrementQueue(int idx, double executionTime)
        {
            _times[idx] = _times[idx].HasValue ? (_times[idx] * 0.8) + (executionTime * 0.2) : executionTime;
        }
    }
}
