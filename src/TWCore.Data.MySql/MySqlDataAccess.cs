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

using MySql.Data.MySqlClient;
using System.Data.Common;

namespace TWCore.Data.MySql
{
    public class MySqlDataAccess : DataAccessBase
    {
        /// <summary>
        /// Gets the database connection object
        /// </summary>
        /// <returns>A DbConnection object</returns>
        protected override DbConnection GetConnection() => new MySqlConnection();
        /// <summary>
        /// Gets the database command object
        /// </summary>
        /// <returns>A DbCommand object</returns>
        protected override DbCommand GetCommand() => new MySqlCommand();
        /// <summary>
        /// MySql Server Data access
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <param name="accessType">Data access type</param>
        public MySqlDataAccess(string connectionString, DataAccessType accessType = DataAccessType.Query)
        {
            ConnectionString = connectionString;
            AccessType = accessType;
            EntityValueConverter = new MySqlEntityValueConverter(this);
        }
    }
}
