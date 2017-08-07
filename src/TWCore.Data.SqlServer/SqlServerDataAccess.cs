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

using System.Data.Common;
using System.Data.SqlClient;

namespace TWCore.Data.SqlServer
{
    /// <summary>
    /// Sql Server Data Access
    /// </summary>
    public class SqlServerDataAccess : DataAccessBase
    {
        #region Static Default Values
        /// <summary>
        /// Default Value for replace the DateTime.MinValue to SqlDateTime.MinValue and DateTime.MaxValue to SqlDateTime.MaxValue in parameters binding and data selection
        /// </summary>
        public static bool DefaultReplaceDateTimeMinMaxValues { get; set; } = true;
        /// <summary>
        /// Default Value for use structured data type on DataTable instances
        /// </summary>
        public static bool DefaultUseStructuredDataType { get; set; } = false;
        #endregion

        /// <summary>
        /// Gets the database connection object
        /// </summary>
        /// <returns>A DbConnection object</returns>
        protected override DbConnection GetConnection() => new SqlConnection();
        /// <summary>
        /// Gets the database command object
        /// </summary>
        /// <returns>A DbCommand object</returns>
        protected override DbCommand GetCommand() => new SqlCommand();
        /// <summary>
        /// Replace the DateTime.MinValue to SqlDateTime.MinValue and DateTime.MaxValue to SqlDateTime.MaxValue in parameters binding and data selection
        /// </summary>
        public bool ReplaceDateTimeMinMaxValues { get; set; } = DefaultReplaceDateTimeMinMaxValues;
        /// <summary>
        /// Use structured data type on DataTable instances
        /// </summary>
        public bool UseStructuredDataType { get; set; } = DefaultUseStructuredDataType;

        /// <summary>
        /// Sql Server Data access
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <param name="accessType">Data access type</param>
        public SqlServerDataAccess(string connectionString, DataAccessType accessType = DataAccessType.StoredProcedure)
        {
            ConnectionString = connectionString;
            AccessType = accessType;
            ParametersBinder = new SqlServerParametersBinder(this);
            EntityValueConverter = new SqlServerEntityValueConverter(this);
        }
    }
}
