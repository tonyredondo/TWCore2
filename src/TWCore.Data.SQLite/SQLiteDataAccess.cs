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

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TWCore.Data.Schema.Generator;
// ReSharper disable InconsistentNaming

namespace TWCore.Data.SQLite
{
    /// <summary>
    /// SQLite Data Access
    /// </summary>
    public class SQLiteDataAccess : DataAccessBase
    {
        /// <summary>
        /// Gets the database connection object
        /// </summary>
        /// <returns>A DbConnection object</returns>
        protected override DbConnection GetConnection() => new SqliteConnection();
        /// <summary>
        /// Gets the database command object
        /// </summary>
        /// <returns>A DbCommand object</returns>
        protected override DbCommand GetCommand() => new SqliteCommand();

        #region .ctor
        /// <summary>
        /// Sql Server Data access
        /// </summary>
        public SQLiteDataAccess()
        {
            AccessType = DataAccessType.Query;
        }
        /// <summary>
        /// Sql Server Data access
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <param name="accessType">Data access type</param>
        // ReSharper disable once UnusedParameter.Local
        public SQLiteDataAccess(string connectionString, DataAccessType accessType = DataAccessType.Query)
        {
            ConnectionString = connectionString;
            AccessType = DataAccessType.Query;
        }
        #endregion

        #region IDataAccessDynamicGenerator
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string GetSelectFromContainer(GeneratorSelectionContainer container)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SELECT");
            var cols = new List<string>();
            foreach (var col in container.Columns)
                cols.Add($"   {col.Table}.{col.Column} AS \"{col.Alias}\"");
            sb.AppendLine(string.Join(", " + Environment.NewLine, cols.ToArray()));
            sb.AppendLine("FROM " + container.From);

            if (container.Joins.Count > 0)
            {
                var joins = new List<string>();
                foreach (var join in container.Joins)
                    joins.Add($"LEFT JOIN {join.Table} ON {container.From}.{join.FromColumn} = {join.Table}.{join.TableColumn}");
                sb.AppendLine(string.Join(Environment.NewLine, joins.ToArray()));
            }
            return sb.ToString();
        }
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
                    lFields.Add($"{f.TableName}.{f.FieldName} = @{f.FieldName}");
                sb.AppendLine(string.Join(" AND" + Environment.NewLine, lFields.ToArray()));
                lst.Add((w.Name, sb.ToString()));
            }
            return lst;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string GetInsertFromContainer(GeneratorSelectionContainer container)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"INSERT INTO {container.From}");
            var lstCols = new List<string>();
            var lstVals = new List<string>();
            foreach (var tc in container.TableColumns)
            {
                lstCols.Add(tc.Item1.Replace("*", string.Empty));
                lstVals.Add("@" + tc.Item2);
            }
            sb.AppendLine($"({ string.Join(", ", lstCols.ToArray()) })");
            sb.AppendLine("VALUES");
            sb.AppendLine($"({ string.Join(", ", lstVals.ToArray()) })");
            return sb.ToString();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string GetUpdateFromContainer(GeneratorSelectionContainer container)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"UPDATE {container.From}");

            var lstSets = container.TableColumns.Where(t => !t.Item1.StartsWith("*")).Select(t => $"\t{t.Item1} = @{t.Item2}").ToArray();
            var lstWhere = container.TableColumns.Where(t => t.Item1.StartsWith("*")).Select(t => $"\t{t.Item1.Substring(1)} = @{t.Item2}").ToArray();

            sb.AppendLine("SET");
            sb.AppendLine(string.Join("," + Environment.NewLine, lstSets));
            sb.AppendLine("WHERE");
            sb.AppendLine(string.Join("," + Environment.NewLine, lstWhere));

            return sb.ToString();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string GetDeleteFromContainer(GeneratorSelectionContainer container)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DELETE FROM {container.From}");
            var lstWhere = container.TableColumns.Where(t => t.Item1.StartsWith("*")).Select(t => $"\t{t.Item1.Substring(1)} = @{t.Item2}").ToArray();
            sb.AppendLine("WHERE");
            sb.AppendLine(string.Join("," + Environment.NewLine, lstWhere));
            return sb.ToString();
        }
        #endregion
    }
}
