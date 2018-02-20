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
using TWCore.Data.Schema.Generator;

namespace TWCore.Data
{
    /// <summary>
    /// IDataAccess Dynamic Generator Interface
    /// </summary>
    public interface IDataAccessDynamicGenerator
    {
        /// <summary>
        /// Get the Select Base Sql from a GeneratorSelectionContainer instance
        /// </summary>
        /// <param name="container">Container object</param>
        /// <returns>Select base sql query</returns>
        string GetSelectFromContainer(GeneratorSelectionContainer container);
		/// <summary>
		/// Get the Sql wheres from the GeneratorSelectionContainer instance.
		/// </summary>
		/// <param name="container">Container object</param>
		/// <returns>List of index name and where statement</returns>
		List<(string, string)> GetWhereFromContainer(GeneratorSelectionContainer container);
        /// <summary>
        /// Get the Insert sql from a GeneratorSelectionContainer instance
        /// </summary>
        /// <param name="container">Container object</param>
        /// <returns>Sql query</returns>
        string GetInsertFromContainer(GeneratorSelectionContainer container);
        /// <summary>
        /// Get the Update sql from a GeneratorSelectionContainer instance
        /// </summary>
        /// <param name="container">Container object</param>
        /// <returns>Sql query</returns>
        string GetUpdateFromContainer(GeneratorSelectionContainer container);
        /// <summary>
        /// Get the Delete sql from a GeneratorSelectionContainer instance
        /// </summary>
        /// <param name="container">Container object</param>
        /// <returns>Sql query</returns>
        string GetDeleteFromContainer(GeneratorSelectionContainer container);
    }
}
