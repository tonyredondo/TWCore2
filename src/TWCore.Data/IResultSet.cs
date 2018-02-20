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

using System.Collections.Generic;

namespace TWCore.Data
{
    /// <summary>
    /// Result set definition
    /// </summary>
    public interface IResultSet
    {
        /// <summary>
        /// Add new row
        /// </summary>
        /// <param name="columns">Column values</param>
        void AddRow(object[] columns);
        /// <summary>
        /// Prepare the result set
        /// </summary>
        /// <param name="valueConverter">Value converter instance</param>
        void PrepareSet(IEntityValueConverter valueConverter);
        /// <summary>
        /// Sets the column names to the binder
        /// </summary>
        /// <param name="columns">Column names and index</param>
        void SetColumnsOnBinder(Dictionary<string, int> columns);
    }
}