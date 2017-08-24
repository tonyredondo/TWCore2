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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TWCore.Data.Schema.Generator
{
    /// <summary>
    /// Dal Generator
    /// </summary>
    public class DalGenerator
    {
        CatalogSchema _schema;
        string _namespace;

        #region .ctor
        /// <summary>
        /// Dal Generator
        /// </summary>
        /// <param name="schema">Catalog schema</param>
        public DalGenerator(CatalogSchema schema, string @namespace)
        {
            _schema = schema;
            _namespace = @namespace;
        }
        #endregion

        public string CreateDatabaseEntity()
        {
            string header = DalGeneratorConsts.formatHeader;
            string databaseEntities = DalGeneratorConsts.formatDatabaseEntities;
            databaseEntities = databaseEntities.Replace("($NAMESPACE$)", _namespace);
            databaseEntities = databaseEntities.Replace("($DATABASENAME$)", _schema.Name);
            databaseEntities = databaseEntities.Replace("($CONNECTIONSTRING$)", _schema.ConnectionString);
            databaseEntities = databaseEntities.Replace("($PROVIDER$)", _schema.Provider);
            return header + databaseEntities;
        }

        public string CreateEntity(string tableName)
        {
            var table = _schema.Tables.FirstOrDefault(t => t.Name == tableName);
            if (table == null) return null;

            string header = DalGeneratorConsts.formatHeader;
            string entityWrapper = DalGeneratorConsts.formatEntityWrapper;
            string columnFormat = DalGeneratorConsts.formatEntityColumn;

            var entityColumns = new StringBuilder();
            foreach(var column in table.Columns)
            {
                var strColumn = columnFormat;

                //We have to check first if the column has a FK
                strColumn = strColumn.Replace("($COLUMNTYPE$)", column.DataType);
                strColumn = strColumn.Replace("($COLUMNNAME$)", column.Name);

                entityColumns.Append(strColumn);
            }

            entityWrapper = entityWrapper.Replace("($NAMESPACE$)", _namespace);
            entityWrapper = entityWrapper.Replace("($DATABASENAME$)", _schema.Name);
            entityWrapper = entityWrapper.Replace("($TABLENAME$)", table.Name);
            entityWrapper = entityWrapper.Replace("($COLUMNS$)", entityColumns.ToString());

            return header + entityWrapper;
        }
    }
}
