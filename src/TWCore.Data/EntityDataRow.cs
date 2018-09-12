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

namespace TWCore.Data
{
    /// <summary>
    /// Entity Data Access Data Row
    /// </summary>
    /// <typeparam name="T">Type of the entity</typeparam>
    public class EntityDataRow<T>
    {
        private volatile bool _bound;
        private object[] _rowValues;
        private EntityBinder _binder;
        private FillDataDelegate<T> _fillMethod;
        private T _entity;


        #region Properties
        /// <summary>
        /// Gets the entity after calling the fillmethod
        /// </summary>
        public T Entity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {

                if (_bound || _rowValues is null)
                    return _entity;
                lock (this)
                {
                    if (_bound) return _entity;
                    var values = _rowValues;
                    _entity = _fillMethod(_binder, values);
                    _rowValues = null;
                    _fillMethod = null;
                    _binder = null;
                    _bound = true;
                    return _entity;
                }
            }
        }
        #endregion

        /// <summary>
        /// Entity Data Access Data Row
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityDataRow(object[] rowValues, EntityBinder binder, FillDataDelegate<T> fillMethod)
        {
            _entity = default(T);
            _rowValues = rowValues;
            _binder = binder;
            _fillMethod = fillMethod;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetRowValues(object[] rowValues)
        {
            _rowValues = rowValues;
        }
    }
}
