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
using TWCore.Diagnostics.Counters;
using TWCore.Diagnostics.Status;

// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Cache.Client
{
    /// <summary>
    /// Cache client Pool counters
    /// </summary>
    [StatusName("Counters")]
    public class CacheClientPoolCounters
    {
        const string Category = "Cache Client";

        private DoubleCounter _existKeyTime;
        private DoubleCounter _getTime;
        private DoubleCounter _getByTagTime;
        private DoubleCounter _getCreationDateTime;
        private DoubleCounter _getExpirationDateTime;
        private DoubleCounter _getKeysTime;
        private DoubleCounter _getMetaTime;
        private DoubleCounter _getMetaByTagTime;
        private DoubleCounter _getOrSetTime;
        private DoubleCounter _removeTime;
        private DoubleCounter _removeByTagTime;
        private DoubleCounter _setTime;
        private DoubleCounter _updateDataTime;
        private DoubleCounter _copyTime;
        private DoubleCounter _setMultiTime;
        private DoubleCounter _executeExtensionTime;

        #region .ctor
        /// <summary>
        /// Cache client Pool counters
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CacheClientPoolCounters(string name)
        {
            _existKeyTime = Core.Counters.GetDoubleCounter(Category, name + @"\Exists Time", CounterType.Average, CounterLevel.Framework);
            _getTime = Core.Counters.GetDoubleCounter(Category, name + @"\Get Time", CounterType.Average, CounterLevel.Framework);
            _getByTagTime = Core.Counters.GetDoubleCounter(Category, name + @"\Get By Tag Time", CounterType.Average, CounterLevel.Framework);
            _getCreationDateTime = Core.Counters.GetDoubleCounter(Category, name + @"\Get CreationDate Time", CounterType.Average, CounterLevel.Framework);
            _getExpirationDateTime = Core.Counters.GetDoubleCounter(Category, name + @"\Get ExpirationDate Time", CounterType.Average, CounterLevel.Framework);
            _getKeysTime = Core.Counters.GetDoubleCounter(Category, name + @"\Get Keys Time", CounterType.Average, CounterLevel.Framework);
            _getMetaTime = Core.Counters.GetDoubleCounter(Category, name + @"\Get Meta Time", CounterType.Average, CounterLevel.Framework);
            _getMetaByTagTime = Core.Counters.GetDoubleCounter(Category, name + @"\Get Meta By Tag Time", CounterType.Average, CounterLevel.Framework);
            _getOrSetTime = Core.Counters.GetDoubleCounter(Category, name + @"\Get Or Sets Time", CounterType.Average, CounterLevel.Framework);
            _removeTime = Core.Counters.GetDoubleCounter(Category, name + @"\Remove Time", CounterType.Average, CounterLevel.Framework);
            _removeByTagTime = Core.Counters.GetDoubleCounter(Category, name + @"\Remove By Tag Time", CounterType.Average, CounterLevel.Framework);
            _setTime = Core.Counters.GetDoubleCounter(Category, name + @"\Set Time", CounterType.Average, CounterLevel.Framework);
            _updateDataTime = Core.Counters.GetDoubleCounter(Category, name + @"\Update Data Time", CounterType.Average, CounterLevel.Framework);
            _copyTime = Core.Counters.GetDoubleCounter(Category, name + @"\Copy Time", CounterType.Average, CounterLevel.Framework);
            _setMultiTime = Core.Counters.GetDoubleCounter(Category, name + @"\Set Multi Time", CounterType.Average, CounterLevel.Framework);
            _executeExtensionTime = Core.Counters.GetDoubleCounter(Category, name + @"\Execute Extension Time", CounterType.Average, CounterLevel.Framework);

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
            _existKeyTime.Add(executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGet(double executionTime)
        {
            _getTime.Add(executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetByTag(double executionTime)
        {
            _getByTagTime.Add(executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetCreationDate(double executionTime)
        {
            _getCreationDateTime.Add(executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetExpirationDate(double executionTime)
        {
            _getExpirationDateTime.Add(executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetKeys(double executionTime)
        {
            _getKeysTime.Add(executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetMeta(double executionTime)
        {
            _getMetaTime.Add(executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetMetaByTag(double executionTime)
        {
            _getMetaByTagTime.Add(executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGetOrSet(double executionTime)
        {
            _getOrSetTime.Add(executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementRemove(double executionTime)
        {
            _removeTime.Add(executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementRemoveByTag(double executionTime)
        {
            _removeByTagTime.Add(executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementSet(double executionTime)
        {
            _setTime.Add(executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementUpdateData(double executionTime)
        {
            _updateDataTime.Add(executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementCopy(double executionTime)
        {
            _copyTime.Add(executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementSetMulti(double executionTime)
        {
            _setMultiTime.Add(executionTime);
        }
        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="executionTime">Execution Time</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementExecuteExtension(double executionTime)
        {
            _executeExtensionTime.Add(executionTime);
        }
        #endregion

    }
}
