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

using TWCore.Data.Schema.Generator;

namespace TWCore.Data
{
    /// <summary>
    /// IDataAccess Dynamic Generator Interface
    /// </summary>
    public interface IDataAccessDynamicGenerator
    {
        /// <summary>
        /// Get a dynamic query from a Dal created using the DalGenerator
        /// </summary>
        /// <param name="queryName">Dynamic query key</param>
        /// <returns>Query</returns>
        string GetDynamicQuery(string queryName);
        /// <summary>
        /// Get the Select Base Sql from a GeneratorSelectionContainer instance
        /// </summary>
        /// <param name="container">Container object</param>
        /// <returns>Select base sql query</returns>
        string GetSelectFromContainer(GeneratorSelectionContainer container);
    }
}
