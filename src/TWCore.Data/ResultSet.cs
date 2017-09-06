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

namespace TWCore.Data
{
    /// <inheritdoc />
    /// <summary>
    /// Result set 
    /// </summary>
    /// <typeparam name="T">Entity type for the result set</typeparam>
    public class ResultSet<T> : IResultSet
    {
        private static readonly FillDataDelegate<T> DefaultFillMethod = ((e, o) => e.Bind<T>(o));

        #region Fields
        private readonly List<EntityDataRow<T>> _dataRows = new List<EntityDataRow<T>>();
        #endregion

        #region Properties
        /// <summary>
        /// Result values
        /// </summary>
        public IEnumerable<T> Result => _dataRows.Select(row => row.Entity);
        /// <summary>
        /// Entity binder
        /// </summary>
        public EntityBinder Binder { get; private set; }
        /// <summary>
        /// Fill method delegate
        /// </summary>
        public FillDataDelegate<T> FillMethod { get; set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Result set 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ResultSet(FillDataDelegate<T> fillMethod)
        {
            FillMethod = fillMethod;
        }
        #endregion

        #region Methods
        /// <inheritdoc />
        /// <summary>
        /// Prepare the result set
        /// </summary>
        /// <param name="valueConverter">Value converter instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PrepareSet(IEntityValueConverter valueConverter)
        {
            Binder = new EntityBinder(valueConverter);
            EntityBinder.PrepareEntity(typeof(T));
            FillMethod = FillMethod ?? DefaultFillMethod;
        }
        /// <inheritdoc />
        /// <summary>
        /// Add new row
        /// </summary>
        /// <param name="columns">Column values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRow(object[] columns) 
            => _dataRows.Add(new EntityDataRow<T>(columns, Binder, FillMethod));
        /// <inheritdoc />
        /// <summary>
        /// Sets the column names to the binder
        /// </summary>
        /// <param name="columns">Column names and index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetColumnsOnBinder(Dictionary<string, int> columns)
            => Binder.ColumnIndex = columns;
        #endregion
    }
}
