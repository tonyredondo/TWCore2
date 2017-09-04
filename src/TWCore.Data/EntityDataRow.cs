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

using System.Runtime.CompilerServices;

namespace TWCore.Data
{
    /// <summary>
    /// Entity Data Access Data Row
    /// </summary>
    /// <typeparam name="T">Type of the entity</typeparam>
    public class EntityDataRow<T>
    {
        volatile bool _bound;
        T entity;

        #region Properties
        /// <summary>
        /// Database row values
        /// </summary>
        public object[] RowValues;
        /// <summary>
        /// Entity binder instance
        /// </summary>
        public EntityBinder Binder;
        /// <summary>
        /// Fill method function
        /// </summary>
        public FillDataDelegate<T> FillMethod;
        /// <summary>
        /// Gets the entity after calling the fillmethod
        /// </summary>
        public T Entity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!_bound)
                {
                    entity = FillMethod(Binder, RowValues);
                    _bound = true;
                }
                return entity;
            }
        }
        #endregion

        /// <summary>
        /// Entity Data Access Data Row
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityDataRow(object[] rowValues, EntityBinder binder, FillDataDelegate<T> fillMethod)
        {
            RowValues = rowValues;
            Binder = binder;
            FillMethod = fillMethod;
        }
    }
}
