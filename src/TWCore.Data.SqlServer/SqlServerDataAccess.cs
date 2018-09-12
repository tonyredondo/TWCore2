﻿/*
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TWCore.Data.Schema;
using TWCore.Data.Schema.Generator;
using TWCore.Diagnostics.Status;

namespace TWCore.Data.SqlServer
{
    /// <inheritdoc />
    /// <summary>
    /// Sql Server Data Access
    /// </summary>
    [StatusName("Sql Server Data Access")]
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

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Gets the database connection object
        /// </summary>
        /// <returns>A DbConnection object</returns>
        protected override DbConnection GetConnection() => new SqlConnection();
        /// <inheritdoc />
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
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Sql Server Data access
        /// </summary>
        public SqlServerDataAccess()
        {
            AccessType = DataAccessType.Query;
            ParametersBinder = new SqlServerParametersBinder(this);
            EntityValueConverter = new SqlServerEntityValueConverter(this);
        }
        /// <inheritdoc />
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
        #endregion

        #region GetSchema
        /// <summary>
        /// On GetSchema
        /// </summary>
        /// <param name="connection">Opened connection</param>
        /// <returns>Schema instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override CatalogSchema OnGetSchema(DbConnection connection)
        {
            var tables = connection.GetSchema("Tables");
            var columns = connection.GetSchema("AllColumns");
            var foreignKeys = connection.GetSchema("ForeignKeys");
            var indexColumns = connection.GetSchema("IndexColumns");

            var tableRows = tables.Rows.Cast<DataRow>().ToArray();
            var catalog = new CatalogSchema
            {
                ConnectionString = connection.ConnectionString,
                Name = (string)tableRows.FirstOrDefault()?["TABLE_CATALOG"],
                AssemblyQualifiedName = typeof(SqlServerDataAccess).AssemblyQualifiedName
            };

            #region Create Tables
            foreach (DataRow column in columns.Rows)
            {
                var tableSchema = (string)column["TABLE_SCHEMA"];
                var tableName = (string)column["TABLE_NAME"];
                var columnName = (string)column["COLUMN_NAME"];
                var ordinalPosition = (int)column["ORDINAL_POSITION"];
                var isNullable = (string)column["IS_NULLABLE"];
                var dataType = (string)column["DATA_TYPE"];
                var maxCharsLength = (int?)(column["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value ? column["CHARACTER_MAXIMUM_LENGTH"] : null);
                var maxBytesLength = (int?)(column["CHARACTER_OCTET_LENGTH"] != DBNull.Value ? column["CHARACTER_OCTET_LENGTH"] : null);
                var numericPrecision = (byte?)(column["NUMERIC_PRECISION"] != DBNull.Value ? column["NUMERIC_PRECISION"] : null);
                var numericPrecisionRadix = (short?)(column["NUMERIC_PRECISION_RADIX"] != DBNull.Value ? column["NUMERIC_PRECISION_RADIX"] : null);

                var tableRow = tableRows.FirstOrDefault((dRow, tName) => (string)dRow["TABLE_NAME"] == tName, tableName);

                var table = catalog.Tables.FirstOrDefault((t, mTableName, mTableSchema) => t.Name == mTableName && t.Schema == mTableSchema, tableName, tableSchema);
                if (table is null)
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

                var tableColumn = table.Columns.FirstOrDefault((c, cName) => c.Name == cName, columnName);
                if (tableColumn is null)
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
                    }
                    #endregion

                    table.Columns.Add(tableColumn);
                }
            }
            #endregion

            #region Create Indexes
            foreach (DataRow indexColumn in indexColumns.Rows)
            {
                var constraintName = (string)indexColumn["constraint_name"];
                var tableSchema = (string)indexColumn["table_schema"];
                var tableName = (string)indexColumn["table_name"];
                var columnName = (string)indexColumn["column_name"];
                var ordinalPosition = (int)indexColumn["ordinal_position"];
                var keyType = (byte)indexColumn["KeyType"];
                var indexName = (string)indexColumn["index_name"];


                var table = catalog.Tables.FirstOrDefault((t, mTableName, mTableSchema) => t.Name == mTableName && t.Schema == mTableSchema, tableName, tableSchema);
                if (table is null) continue;

                var tIndex = table.Indexes.FirstOrDefault((i, iName) => i.Name == iName, indexName);
                if (tIndex is null)
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


                var column = table.Columns.FirstOrDefault((cName, colName) => cName.Name == colName, columnName);
                if (column != null && !column.IndexesName.Contains(indexName))
                    column.IndexesName.Add(indexName);
            }
            #endregion

            #region Create ForeignKeys
            foreach (DataRow foreignKey in foreignKeys.Rows)
            {
                var constraintName = (string)foreignKey["CONSTRAINT_NAME"];
                var tableSchema = (string)foreignKey["TABLE_SCHEMA"];
                var tableName = (string)foreignKey["TABLE_NAME"];

                var table = catalog.Tables.FirstOrDefault((t, mTableName, mTableSchema) => t.Name == mTableName && t.Schema == mTableSchema, tableName, tableSchema);
                if (table is null) continue;

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
        #endregion

        #region IDataAccessDynamicGenerator
        /// <inheritdoc />
        /// <summary>
        /// Get the Select Base Sql from a GeneratorSelectionContainer instance
        /// </summary>
        /// <param name="container">Container object</param>
        /// <returns>Select base sql query</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string GetSelectFromContainer(GeneratorSelectionContainer container)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SELECT");
            var cols = new List<string>();
            foreach (var col in container.Columns)
                cols.Add($"   [{col.Table}].[{col.Column}] as '{col.Alias}'");
            sb.AppendLine(string.Join(", " + Environment.NewLine, cols.ToArray()));
            sb.AppendLine("FROM " + container.From);

            if (container.Joins.Count > 0)
            {
                var joins = new List<string>();
                foreach (var join in container.Joins)
                    joins.Add($"LEFT JOIN [{join.Table}] ON [{container.From}].[{join.FromColumn}] = [{join.Table}].[{join.TableColumn}]");
                sb.AppendLine(string.Join(Environment.NewLine, joins.ToArray()));
            }
            return sb.ToString();
        }
        /// <inheritdoc />
        /// <summary>
        /// Get the Where from Sql from GeneratorSelectionContainer instance
        /// </summary>
        /// <param name="container">Container object</param>
        /// <returns>The where list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override List<(string, string)> GetWhereFromContainer(GeneratorSelectionContainer container)
        {
            var lst = new List<(string, string)>();
            foreach (var w in container.Wheres)
            {
                var sb = new StringBuilder();
                sb.AppendLine("WHERE");
                var lFields = new List<string>();
                foreach (var f in w.Fields)
                    lFields.Add($"[{f.TableName}].[{f.FieldName}] = @{f.FieldName}");
                sb.AppendLine(string.Join(" AND" + Environment.NewLine, lFields.ToArray()));
                lst.Add((w.Name, sb.ToString()));
            }
            return lst;
        }
        /// <inheritdoc />
        /// <summary>
        /// Get the Insert sql from a GeneratorSelectionContainer instance
        /// </summary>
        /// <param name="container">Container object</param>
        /// <returns>Sql query</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string GetInsertFromContainer(GeneratorSelectionContainer container)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"INSERT INTO [{container.From}]");
            var lstCols = new List<string>();
            var lstVals = new List<string>();
            foreach (var tc in container.TableColumns)
            {
                lstCols.Add("[" + tc.Item1.Replace("*", string.Empty) + "]");
                lstVals.Add("@" + tc.Item2);
            }
            sb.AppendLine($"({ string.Join(", ", lstCols.ToArray()) })");
            sb.AppendLine("VALUES");
            sb.AppendLine($"({ string.Join(", ", lstVals.ToArray()) })");
            return sb.ToString();
        }
        /// <inheritdoc />
        /// <summary>
        /// Get the Update sql from a GeneratorSelectionContainer instance
        /// </summary>
        /// <param name="container">Container object</param>
        /// <returns>Sql query</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string GetUpdateFromContainer(GeneratorSelectionContainer container)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"UPDATE [{container.From}]");

            var lstSets = container.TableColumns.Where(t => !t.Item1.StartsWith("*")).Select(t => $"\t{t.Item1} = @{t.Item2}").ToArray();
            var lstWhere = container.TableColumns.Where(t => t.Item1.StartsWith("*")).Select(t => $"\t{t.Item1.Substring(1)} = @{t.Item2}").ToArray();

            sb.AppendLine("SET");
            sb.AppendLine(string.Join("," + Environment.NewLine, lstSets));
            sb.AppendLine("WHERE");
            sb.AppendLine(string.Join("," + Environment.NewLine, lstWhere));

            return sb.ToString();
        }
        /// <inheritdoc />
        /// <summary>
        /// Get the Delete sql from a GeneratorSelectionContainer instance
        /// </summary>
        /// <param name="container">Container object</param>
        /// <returns>Sql query</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string GetDeleteFromContainer(GeneratorSelectionContainer container)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DELETE FROM [{container.From}]");
            var lstWhere = container.TableColumns.Where(t => t.Item1.StartsWith("*")).Select(t => $"\t{t.Item1.Substring(1)} = @{t.Item2}").ToArray();
            sb.AppendLine("WHERE");
            sb.AppendLine(string.Join("," + Environment.NewLine, lstWhere));
            return sb.ToString();
        }
        #endregion
    }
}
