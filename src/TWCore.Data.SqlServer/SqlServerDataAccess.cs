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
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using TWCore.Data.Schema;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override CatalogSchema OnGetSchema(DbConnection connection)
        {
            var schemas = connection.GetSchema();
            var tables = connection.GetSchema("Tables");
            var columns = connection.GetSchema("AllColumns");
            var foreignKeys = connection.GetSchema("ForeignKeys");
            var indexColumns = connection.GetSchema("IndexColumns");

            var tableRows = tables.Rows.Cast<DataRow>().ToArray();
            var catalog = new CatalogSchema();

            #region Create Tables
            foreach (DataRow column in columns.Rows)
            {
                var tableCatalog = (string)column["TABLE_CATALOG"];
                var tableSchema = (string)column["TABLE_SCHEMA"];
                var tableName = (string)column["TABLE_NAME"];
                var columnName = (string)column["COLUMN_NAME"];
                var ordinalPosition = (int)column["ORDINAL_POSITION"];
                var columnDefault = (string)(column["COLUMN_DEFAULT"] != DBNull.Value ? column["COLUMN_DEFAULT"] : null);
                var isNullable = (string)column["IS_NULLABLE"];
                var dataType = (string)column["DATA_TYPE"];
                var maxCharsLength = (int?)(column["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value ? column["CHARACTER_MAXIMUM_LENGTH"] : null);
                var maxBytesLength = (int?)(column["CHARACTER_OCTET_LENGTH"] != DBNull.Value ? column["CHARACTER_OCTET_LENGTH"] : null);
                var numericPrecision = (byte?)(column["NUMERIC_PRECISION"] != DBNull.Value ? column["NUMERIC_PRECISION"] : null);
                var numericPrecisionRadix = (short?)(column["NUMERIC_PRECISION_RADIX"] != DBNull.Value ? column["NUMERIC_PRECISION_RADIX"] : null);
                var numericScale = (int?)(column["NUMERIC_SCALE"] != DBNull.Value ? column["NUMERIC_SCALE"] : null);

                var tableRow = tableRows.FirstOrDefault(dRow => (string)dRow["TABLE_NAME"] == tableName);

                var table = catalog.Tables.FirstOrDefault(t => t.Name == tableName && t.Schema == tableSchema);
                if (table == null)
                {
                    table = new TableSchema { Name = tableName, Schema = tableSchema };
                    switch ((string)tableRow?["TABLE_TYPE"])
                    {
                        case "BASE TABLE":
                            table.Type = TableType.Table;
                            break;
                        case "VIEW":
                            table.Type = TableType.View;
                            break;
                    }
                    catalog.Tables.Add(table);
                }

                var tableColumn = table.Columns.FirstOrDefault(c => c.Name == columnName);
                if (tableColumn == null)
                {
                    tableColumn = new TableColumnSchema
                    {
                        Name = columnName,
                        Position = ordinalPosition,
                        IsNullable = isNullable == "YES",
                        NumericPrecision = numericPrecision,
                        NumericPrecisionRadix = numericPrecisionRadix
                    };

                    #region DataType Switch
                    switch (dataType)
                    {
                        case "uniqueidentifier":
                            tableColumn.DataType = (tableColumn.IsNullable ? "Guid?" : "Guid");
                            break;
                        case "char":
                        case "nchar":
                        case "varchar":
                        case "nvarchar":
                            tableColumn.DataType = "string";
                            tableColumn.MaxLength = maxCharsLength;
                            break;
                        case "varbinary":
                        case "binary":
                        case "rowversion":
                            tableColumn.DataType = "byte[]";
                            tableColumn.MaxLength = maxBytesLength;
                            break;
                        case "bigint":
                            tableColumn.DataType = (tableColumn.IsNullable ? "long?" : "long");
                            break;
                        case "int":
                            tableColumn.DataType = (tableColumn.IsNullable ? "int?" : "int");
                            break;
                        case "smallint":
                            tableColumn.DataType = (tableColumn.IsNullable ? "short?" : "short");
                            break;
                        case "tinyint":
                            tableColumn.DataType = (tableColumn.IsNullable ? "byte?" : "byte");
                            break;
                        case "bit":
                            tableColumn.DataType = (tableColumn.IsNullable ? "bool?" : "bool");
                            break;
                        case "real":
                            tableColumn.DataType = (tableColumn.IsNullable ? "float?" : "float");
                            break;
                        case "float":
                            tableColumn.DataType = (tableColumn.IsNullable ? "double?" : "double");
                            break;
                        case "smallmoney":
                        case "money":
                        case "numeric":
                        case "decimal":
                            tableColumn.DataType = (tableColumn.IsNullable ? "decimal?" : "decimal");
                            break;
                        case "date":
                        case "smalldatetime":
                        case "datetime":
                            tableColumn.DataType = (tableColumn.IsNullable ? "DateTime?" : "DateTime");
                            break;
                        case "geography":
                            tableColumn.DataType = "object";
                            break;
                        default:
                            break;
                    }
                    #endregion

                    table.Columns.Add(tableColumn);
                }
            }
            #endregion

            #region Create Indexes
            foreach (DataRow indexColumn in indexColumns.Rows)
            {
                var constraintCatalog = (string)indexColumn["constraint_catalog"];
                var constraintSchema = (string)indexColumn["constraint_schema"];
                var constraintName = (string)indexColumn["constraint_name"];
                var tableCatalog = (string)indexColumn["table_catalog"];
                var tableSchema = (string)indexColumn["table_schema"];
                var tableName = (string)indexColumn["table_name"];
                var columnName = (string)indexColumn["column_name"];
                var ordinalPosition = (int)indexColumn["ordinal_position"];
                var keyType = (byte)indexColumn["KeyType"];
                var indexName = (string)indexColumn["index_name"];


                var table = catalog.Tables.FirstOrDefault(t => t.Name == tableName && t.Schema == tableSchema);
                if (table == null) continue;

                var tIndex = table.Indexes.FirstOrDefault(i => i.Name == indexName);
                if (tIndex == null)
                {
                    tIndex = new TableIndexSchema { Name = indexName, ConstraintName = constraintName };
                    if (indexName.StartsWith("PK_"))
                    {
                        tIndex.Type = IndexType.PrimaryKey;
                    }
                    else if (indexName.StartsWith("UK_"))
                    {
                        tIndex.Type = IndexType.UniqueKey;
                    }
                    else if (indexName.StartsWith("IX_"))
                    {
                        tIndex.Type = IndexType.NonClusteredNonUniqueIndex;
                    }
                    else if (indexName.StartsWith("UX_"))
                    {
                        tIndex.Type = IndexType.UniqueIndex;
                    }
                    else if (indexName.StartsWith("AK_"))
                    {
                        tIndex.Type = IndexType.AlternateIndex;
                    }
                    else if (indexName.StartsWith("CLIX_"))
                    {
                        tIndex.Type = IndexType.ClusteredIndex;
                    }
                    else if (indexName.StartsWith("COVIX_"))
                    {
                        tIndex.Type = IndexType.CoveringIndex;
                    }
                    else if (indexName.StartsWith("UC_"))
                    {
                        tIndex.Type = IndexType.UniqueClusteredIndex;
                    }
                    table.Indexes.Add(tIndex);
                }

                var tColumn = new TableIndexColumnSchema
                {
                    ColumnName = columnName,
                    ColumnPosition = ordinalPosition,
                    KeyType = keyType
                };
                tIndex.Columns.Add(tColumn);


                var column = table.Columns.FirstOrDefault(cName => cName.Name == columnName);
                if (column != null && !column.IndexesName.Contains(indexName))
                    column.IndexesName.Add(indexName);
            }
            #endregion

            #region Create ForeignKeys
            foreach (DataRow foreignKey in foreignKeys.Rows)
            {
                var constraintCatalog = (string)foreignKey["CONSTRAINT_CATALOG"];
                var constraintSchema = (string)foreignKey["CONSTRAINT_SCHEMA"];
                var constraintName = (string)foreignKey["CONSTRAINT_NAME"];
                var tableCatalog = (string)foreignKey["TABLE_CATALOG"];
                var tableSchema = (string)foreignKey["TABLE_SCHEMA"];
                var tableName = (string)foreignKey["TABLE_NAME"];

                var table = catalog.Tables.FirstOrDefault(t => t.Name == tableName && t.Schema == tableSchema);
                if (table == null) continue;

                var fkTable = constraintName.Replace(tableName, string.Empty);
                fkTable = fkTable.Substring(4);

                table.ForeignKeys.Add(new ForeignKeySchema
                {
                    ConstraintName = constraintName,
                    ForeignTable = fkTable
                });
            }
            #endregion

            return catalog;
        }
    }
}
