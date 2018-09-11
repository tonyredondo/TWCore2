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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TWCore.Diagnostics.Log;

namespace TWCore.Data.Schema.Generator
{
    /// <summary>
    /// Dal Generator
    /// </summary>
    public class DalGenerator
    {
        private readonly CatalogSchema _schema;
        private readonly string _namespace;
        private IDataAccessDynamicGenerator _dataAccessGenerator;

        #region Properties
        /// <summary>
        /// Gets the EntityName Delegate
        /// </summary>
        public Func<string, string> GetEntityNameDelegate { get; set; } = tableName =>
        {
            tableName = tableName.Replace("_", " ");
            tableName = tableName.CapitalizeEachWords();
            tableName = tableName.Replace("-", "_");
            return tableName.RemoveSpaces();
        };
        /// <summary>
        /// Gets the StoredProcedure name Delegate
        /// </summary>
        public Func<string, string, string> GetStoredProcedureNameDelegate { get; set; } = (table, name) => "sp" + table + name;
        /// <summary>
        /// Enable create entities
        /// </summary>
        public bool EnableCreateEntities { get; set; } = true;
        /// <summary>
        /// Enable create interfaces
        /// </summary>
        public bool EnableCreateInterfaces { get; set; } = true;
        /// <summary>
        /// Enable create solution
        /// </summary>
        public bool EnableCreateSolution { get; set; } = true;
        /// <summary>
        /// Enable create dal
        /// </summary>
        public bool EnableCreateDal { get; set; } = true;
        /// <summary>
        /// Generator type
        /// </summary>
        public DalGeneratorType GeneratorType { get; set; } = DalGeneratorType.Embedded;
        #endregion

        #region .ctor
        /// <summary>
        /// Dal Generator
        /// </summary>
        /// <param name="schema">Catalog schema</param>
        /// <param name="namespace">Namespace name</param>
        public DalGenerator(CatalogSchema schema, string @namespace)
        {
            _schema = schema;
            _namespace = @namespace;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Create Dal
        /// </summary>
        /// <param name="directory">Folder path</param>
        /// <param name="generator">IDataAccessDynamicGenerator instance to create the sql queries of the DAL</param>
        public void Create(string directory, IDataAccessDynamicGenerator generator = null)
        {
            try
            {
                if (generator != null)
                    _dataAccessGenerator = generator;
                else
                {
                    var dagenType = Core.GetType(_schema.AssemblyQualifiedName);
                    if (dagenType != null)
                        _dataAccessGenerator = Activator.CreateInstance(dagenType) as IDataAccessDynamicGenerator;
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(LogLevel.Warning, ex);
            }
            if (_dataAccessGenerator == null)
                GeneratorType = DalGeneratorType.StoredProcedure;
            CreateEntities(directory);
            CreateInterfaces(directory);
            CreateDal(directory);
        }
        #endregion

        #region Private Methods
        private void CreateEntities(string directory)
        {
            if (!EnableCreateEntities) return;

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string fName, fContent;

            (fName, fContent) = CreateAbstractionProject();
            WriteToDisk(Path.Combine(directory, fName), fContent);

            foreach (var table in _schema.Tables)
            {
                (fName, fContent) = CreateEntity(table.Name);
                WriteToDisk(Path.Combine(directory, fName), fContent);
            }
        }
        private void CreateInterfaces(string directory)
        {
            if (!EnableCreateInterfaces) return;

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string fName, fContent;

            (fName, fContent) = CreateAbstractionProject();
            WriteToDisk(Path.Combine(directory, fName), fContent);

            foreach (var table in _schema.Tables)
            {
                (fName, fContent) = CreateInterface(table.Name);
                WriteToDisk(Path.Combine(directory, fName), fContent);
            }
        }
        private void CreateDal(string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string fName, fContent;

            if (EnableCreateSolution)
            {
                (fName, fContent) = CreateSolution();
                WriteToDisk(Path.Combine(directory, fName), fContent);
            }

            (fName, fContent) = CreateDalProject();
            WriteToDisk(Path.Combine(directory, fName), fContent);

            (fName, fContent) = CreateDatabaseEntity();
            WriteToDisk(Path.Combine(directory, fName), fContent);

            foreach (var table in _schema.Tables)
            {
                (fName, fContent) = CreateClass(table.Name);
                WriteToDisk(Path.Combine(directory, fName), fContent);
            }
        }


        #region Abstractions
        private (string, string) CreateAbstractionProject()
        {
            var dag = _dataAccessGenerator.GetType();
            var projFile = DalGeneratorConsts.FormatAbstractionsProject;
            projFile = projFile.Replace("($DATAASSEMBLYNAME$)", dag.Assembly.GetName().Name);
            projFile = projFile.Replace("($VERSION$)", Core.FrameworkVersion);
            var filePath = Path.Combine(_schema.Name, "Abstractions", _namespace + "." + _schema.Name + ".Abstractions.csproj");
            return (filePath, projFile);
        }
        private (string, string) CreateEntity(string tableName)
        {
            var table = _schema.Tables.FirstOrDefault((t, tName) => t.Name == tName, tableName);
            if (table == null) return (null, null);

            const string header = DalGeneratorConsts.FormatEntityHeader;
            const string entityWrapper = DalGeneratorConsts.FormatEntityWrapper;
            const string columnFormat = DalGeneratorConsts.FormatEntityColumn;

            var entityColumns = new List<string>();
            foreach (var column in table.Columns)
            {
                var strColumn = columnFormat;
                var added = false;

                if (!column.IndexesName.Any(i => i.StartsWith("PK")))
                {
                    //We have to check first if the column has a FK
                    foreach (var fk in table.ForeignKeys)
                    {
                        var fkTable = _schema.Tables.FirstOrDefault((t, fTable) => t.Name == fTable, fk.ForeignTable);
                        var fkColumn = fkTable?.Columns.FirstOrDefault((c, cName) => c.Name == cName, column.Name);
                        if (fkColumn == null) continue;
                        var isPk = fkColumn.IndexesName.Any(i => i.StartsWith("PK"));
                        if (!isPk) continue;
                        
                        strColumn = strColumn.Replace("($COLUMNTYPE$)", "Ent" + GetEntityNameDelegate(fkTable.Name));
                        var name = column.Name;
                        if (name.EndsWith("Id"))
                            name = name.SubstringToLast("Id") + "Item";
                        else
                            name = fkTable.Name;
                        strColumn = strColumn.Replace("($COLUMNNAME$)", GetName(name));

                        if (!entityColumns.Contains(strColumn))
                            entityColumns.Add(strColumn);
                        added = true;
                        break;
                    }

                    if (!added)
                    {
                        //We try to find other entity to match the Id (without FK)
                        if (column.Name != "Id" && column.Name.EndsWith("Id"))
                        {
                            foreach (var t in _schema.Tables)
                            {
                                var iPk = t.Indexes.FirstOrDefault(i => i.Type == IndexType.PrimaryKey);
                                if (iPk?.Columns?.Count != 1) continue;
                                if (iPk.Columns[0].ColumnName != column.Name) continue;
                                    
                                strColumn = strColumn.Replace("($COLUMNTYPE$)", "Ent" + GetEntityNameDelegate(t.Name));
                                var name = column.Name.SubstringToLast("Id") + "Item";
                                strColumn = strColumn.Replace("($COLUMNNAME$)", GetName(name));
                                strColumn += "          // TODO: This property should have a ForeignKey in DB table.";
                                entityColumns.Add(strColumn);
                                added = true;
                                break;
                            }
                        }
                    }
                }
                
                if (added) continue;
                strColumn = strColumn.Replace("($COLUMNTYPE$)", column.DataType);
                strColumn = strColumn.Replace("($COLUMNNAME$)", GetName(column.Name));
                entityColumns.Add(strColumn);
            }

            var body = header + entityWrapper;
            body = body.Replace("($NAMESPACE$)", _namespace);
            body = body.Replace("($DATABASENAME$)", _schema.Name);
            body = body.Replace("($TABLENAME$)", GetEntityNameDelegate(table.Name));
            body = body.Replace("($COLUMNS$)", string.Join(string.Empty, entityColumns.ToArray()));

            var filePath = Path.Combine(_schema.Name, "Abstractions", "Entities");
            filePath = Path.Combine(filePath, "Ent" + GetEntityNameDelegate(table.Name) + ".cs");
            return (filePath, body);
        }

        private (string, string) CreateInterface(string tableName)
        {
            var table = _schema.Tables.FirstOrDefault((t, tName) => t.Name == tName, tableName);
            if (table == null) return (null, null);

            const string header = DalGeneratorConsts.FormatDalInterfaceHeader;
            const string interfaceWrapper = DalGeneratorConsts.FormatDalInterfaceWrapper;
            const string interfaceMethod = DalGeneratorConsts.FormatDalInterfaceMethod;

            var entityTableName = GetEntityNameDelegate(table.Name);
            var entityName = "Ent" + entityTableName;

            var methods = new List<string>();
            var methods2 = new List<string> { Environment.NewLine };

            methods.Add(interfaceMethod.Replace("($RETURNTYPE$)", $"IEnumerable<{entityName}>").Replace("($METHODNAME$)", "GetAll").Replace("($METHODPARAMETERS$)", ""));
            methods2.Add(interfaceMethod.Replace("($RETURNTYPE$)", $"Task<IEnumerable<{entityName}>>").Replace("($METHODNAME$)", "GetAllAsync").Replace("($METHODPARAMETERS$)", ""));

            foreach (var index in table.Indexes)
            {
                var columnNames = new List<string>();
                var names = new List<string>();
                var parameters = new List<string>();
                foreach (var col in index.Columns.OrderBy(c => c.ColumnPosition))
                {
                    var column = table.Columns.FirstOrDefault((c, cName) => c.Name == cName, col.ColumnName);
                    columnNames.Add(col.ColumnName);
                    names.Add(GetName(col.ColumnName));
                    parameters.Add(column.DataType + " @" + GetName(col.ColumnName.Substring(0, 1).ToLowerInvariant() + col.ColumnName.Substring(1)));
                }

                //NORMAL
                if (index.Type == IndexType.PrimaryKey || index.Type == IndexType.UniqueKey || index.Type == IndexType.UniqueIndex || index.Type == IndexType.UniqueClusteredIndex)
                {
                    methods.Add(interfaceMethod.Replace("($RETURNTYPE$)", entityName).Replace("($METHODNAME$)", "GetBy" + string.Join("", names.ToArray())).Replace("($METHODPARAMETERS$)", string.Join(", ", parameters.ToArray())));
                }
                else
                {
                    methods.Add(interfaceMethod.Replace("($RETURNTYPE$)", $"IEnumerable<{entityName}>").Replace("($METHODNAME$)", "GetAllBy" + string.Join("", names.ToArray())).Replace("($METHODPARAMETERS$)", string.Join(", ", parameters.ToArray())));
                }

                //ASYNC
                if (index.Type == IndexType.PrimaryKey || index.Type == IndexType.UniqueKey || index.Type == IndexType.UniqueIndex || index.Type == IndexType.UniqueClusteredIndex)
                {
                    methods2.Add(interfaceMethod.Replace("($RETURNTYPE$)", $"Task<{entityName}>").Replace("($METHODNAME$)", "GetBy" + string.Join("", names.ToArray()) + "Async").Replace("($METHODPARAMETERS$)", string.Join(", ", parameters.ToArray())));
                }
                else
                {
                    methods2.Add(interfaceMethod.Replace("($RETURNTYPE$)", $"Task<IEnumerable<{entityName}>>").Replace("($METHODNAME$)", "GetAllBy" + string.Join("", names.ToArray()) + "Async").Replace("($METHODPARAMETERS$)", string.Join(", ", parameters.ToArray())));
                }

                if (index.Type == IndexType.PrimaryKey)
                {
                    methods.Add(interfaceMethod.Replace("($RETURNTYPE$)", "int").Replace("($METHODNAME$)", "Delete").Replace("($METHODPARAMETERS$)", string.Join(", ", parameters.ToArray())));
                    methods2.Add(interfaceMethod.Replace("($RETURNTYPE$)", "Task<int>").Replace("($METHODNAME$)", "DeleteAsync").Replace("($METHODPARAMETERS$)", string.Join(", ", parameters.ToArray())));
                }
            }

            methods.Add(interfaceMethod.Replace("($RETURNTYPE$)", "int").Replace("($METHODNAME$)", "Insert").Replace("($METHODPARAMETERS$)", entityName + " value"));
            methods.Add(interfaceMethod.Replace("($RETURNTYPE$)", "int").Replace("($METHODNAME$)", "Update").Replace("($METHODPARAMETERS$)", entityName + " value"));
            methods.Add(interfaceMethod.Replace("($RETURNTYPE$)", "int").Replace("($METHODNAME$)", "Delete").Replace("($METHODPARAMETERS$)", entityName + " value"));
            methods2.Add(interfaceMethod.Replace("($RETURNTYPE$)", "Task<int>").Replace("($METHODNAME$)", "InsertAsync").Replace("($METHODPARAMETERS$)", entityName + " value"));
            methods2.Add(interfaceMethod.Replace("($RETURNTYPE$)", "Task<int>").Replace("($METHODNAME$)", "UpdateAsync").Replace("($METHODPARAMETERS$)", entityName + " value"));
            methods2.Add(interfaceMethod.Replace("($RETURNTYPE$)", "Task<int>").Replace("($METHODNAME$)", "DeleteAsync").Replace("($METHODPARAMETERS$)", entityName + " value"));


            var body = header + interfaceWrapper;
            body = body.Replace("($NAMESPACE$)", _namespace);
            body = body.Replace("($DATABASENAME$)", _schema.Name);
            body = body.Replace("($TABLENAME$)", entityTableName);
            body = body.Replace("($METHODS$)", string.Join(string.Empty, methods.Concat(methods2).ToArray()));

            var filePath = Path.Combine(_schema.Name, "Abstractions", "Interfaces");
            filePath = Path.Combine(filePath, "IDal" + entityTableName + ".cs");
            return (filePath, body);
        }
        #endregion

        #region Dal
        private (string, string) CreateSolution()
        {
            var prov = _dataAccessGenerator.GetType().Name.Replace("DataAccess", string.Empty);
            var projFile = DalGeneratorConsts.FormatSolution;
            projFile = projFile.Replace("($NAMESPACE$)", _namespace);
            projFile = projFile.Replace("($CATALOGNAME$)", _schema.Name);
            projFile = projFile.Replace("($PROVIDERNAME$)", prov);
            var filePath = Path.Combine(_schema.Name, _namespace + "." + _schema.Name + ".sln");
            return (filePath, projFile);
        }
        private (string, string) CreateDalProject()
        {
            var dag = _dataAccessGenerator.GetType();
            var prov = dag.Name.Replace("DataAccess", string.Empty);
            var projFile = DalGeneratorConsts.FormatDalProject;
            projFile = projFile.Replace("($DATAASSEMBLYNAME$)", dag.Assembly.GetName().Name);
            projFile = projFile.Replace("($VERSION$)", Core.FrameworkVersion);
            projFile = projFile.Replace("($NAMESPACE$)", _namespace);
            projFile = projFile.Replace("($CATALOGNAME$)", _schema.Name);
            var filePath = Path.Combine(_schema.Name, "Dal." + prov, _namespace + "." + _schema.Name + "." + prov + ".csproj");
            return (filePath, projFile);
        }
        private (string, string) CreateDatabaseEntity()
        {
            var dag = _dataAccessGenerator.GetType();
            var prov = dag.Name.Replace("DataAccess", string.Empty);
            var header = DalGeneratorConsts.FormatEntityHeader;
            header += "using " + dag.Assembly.GetName().Name + ";\r\n";
            var databaseEntities = DalGeneratorConsts.FormatDatabaseEntities;
            databaseEntities = databaseEntities.Replace("($NAMESPACE$)", _namespace);
            databaseEntities = databaseEntities.Replace("($PROVIDERNAME$)", prov);
            databaseEntities = databaseEntities.Replace("($DATABASENAME$)", _schema.Name);
            databaseEntities = databaseEntities.Replace("($CONNECTIONSTRING$)", (_dataAccessGenerator as DataAccessBase)?.ConnectionString ?? _schema.ConnectionString);
            databaseEntities = databaseEntities.Replace("($PROVIDER$)", dag.Name);
            databaseEntities = databaseEntities.Replace("($QUERYTYPE$)", GeneratorType == DalGeneratorType.StoredProcedure ? "StoredProcedure" : "Query");
            var filePath = Path.Combine(_schema.Name, "Dal." + prov, _schema.Name + ".cs");
            return (filePath, header + databaseEntities);
        }
        private (string, string) CreateClass(string tableName)
        {
            var table = _schema.Tables.FirstOrDefault((t, tName) => t.Name == tName, tableName);
            if (table == null) return (null, null);

            const string header = DalGeneratorConsts.FormatDalHeader;
            const string dalWrapper = DalGeneratorConsts.FormatDalWrapper;
            const string dalSelectMethod = DalGeneratorConsts.FormatDalSelectMethod;
            const string dalExecuteMethod = DalGeneratorConsts.FormatDalExecuteMethod;
            const string dalDeleteExecuteMethod = DalGeneratorConsts.FormatDalDeleteExecuteMethod;

            var entityTableName = GetEntityNameDelegate(table.Name);
            var entityName = "Ent" + entityTableName;

            var methods = new List<string>();
            var methods2 = new List<string> { Environment.NewLine };

            methods.Add(dalSelectMethod
                .Replace("($RETURNTYPE$)", $"IEnumerable<{entityName}>")
                .Replace("($METHODNAME$)", "GetAll")
                .Replace("($METHODPARAMETERS$)", "")
                .Replace("($DATASELECT$)", "Data.SelectElements")
                .Replace("($DATARETURN$)", entityName)
                .Replace("($DATASQL$)", GeneratorType == DalGeneratorType.Embedded ? "SelectSql" : $"\"{GetStoredProcedureNameDelegate(entityTableName, "GetAll")}\"")
                .Replace("($DATAPARAMETERS$)", "")
                );
            methods2.Add(dalSelectMethod
                .Replace("($RETURNTYPE$)", $"Task<IEnumerable<{entityName}>>")
                .Replace("($METHODNAME$)", "GetAllAsync")
                .Replace("($METHODPARAMETERS$)", "")
                .Replace("($DATASELECT$)", "DataAsync.SelectElementsAsync")
                .Replace("($DATARETURN$)", entityName)
                .Replace("($DATASQL$)", GeneratorType == DalGeneratorType.Embedded ? "SelectSql" : $"\"{GetStoredProcedureNameDelegate(entityTableName, "GetAll")}\"")
                .Replace("($DATAPARAMETERS$)", "")
                );

            foreach (var index in table.Indexes)
            {
                var columnNames = new List<string>();
                var names = new List<string>();
                var parameters = new List<string>();
                var objParameters = new List<string>();
                foreach (var col in index.Columns.OrderBy(c => c.ColumnPosition))
                {
                    var column = table.Columns.FirstOrDefault((c, cName) => c.Name == cName, col.ColumnName);
                    columnNames.Add(col.ColumnName);
                    names.Add(GetName(col.ColumnName));
                    var pName = " @" + GetName(col.ColumnName.Substring(0, 1).ToLowerInvariant() + col.ColumnName.Substring(1));
                    parameters.Add(column.DataType + pName);
                    objParameters.Add($"{GetName(col.ColumnName)} = {pName}");
                }
                var mName = string.Join("", names.ToArray());
                var mParameters = string.Join(", ", parameters.ToArray());
                var oParameters = "new { " + string.Join(", ", objParameters.ToArray()) + " }";
                if (index.Type == IndexType.PrimaryKey || index.Type == IndexType.UniqueKey || index.Type == IndexType.UniqueIndex || index.Type == IndexType.UniqueClusteredIndex)
                {
                    methods.Add(dalSelectMethod
                        .Replace("($RETURNTYPE$)", entityName)
                        .Replace("($METHODNAME$)", "GetBy" + mName)
                        .Replace("($METHODPARAMETERS$)", mParameters)
                        .Replace("($DATASELECT$)", "Data.SelectElement")
                        .Replace("($DATARETURN$)", entityName)
                        .Replace("($DATASQL$)", GeneratorType == DalGeneratorType.Embedded ? "SelectSql + By" + mName : $"\"{GetStoredProcedureNameDelegate(entityTableName, "GetBy" + mName)}\"")
                        .Replace("($DATAPARAMETERS$)", ", " + oParameters)
                        );
                    methods2.Add(dalSelectMethod
                        .Replace("($RETURNTYPE$)", $"Task<{entityName}>")
                        .Replace("($METHODNAME$)", "GetBy" + mName + "Async")
                        .Replace("($METHODPARAMETERS$)", mParameters)
                        .Replace("($DATASELECT$)", "DataAsync.SelectElementAsync")
                        .Replace("($DATARETURN$)", entityName)
                        .Replace("($DATASQL$)", GeneratorType == DalGeneratorType.Embedded ? "SelectSql + By" + mName : $"\"{GetStoredProcedureNameDelegate(entityTableName, "GetBy" + mName)}\"")
                        .Replace("($DATAPARAMETERS$)", ", " + oParameters)
                        );
                }
                else
                {
                    methods.Add(dalSelectMethod
                        .Replace("($RETURNTYPE$)", $"IEnumerable<{entityName}>")
                        .Replace("($METHODNAME$)", "GetAllBy" + mName)
                        .Replace("($METHODPARAMETERS$)", mParameters)
                        .Replace("($DATASELECT$)", "Data.SelectElements")
                        .Replace("($DATARETURN$)", entityName)
                        .Replace("($DATASQL$)", GeneratorType == DalGeneratorType.Embedded ? "SelectSql + By" + mName : $"\"{GetStoredProcedureNameDelegate(entityTableName, "GetAllBy" + mName)}\"")
                        .Replace("($DATAPARAMETERS$)", ", " + oParameters)
                        );
                    methods2.Add(dalSelectMethod
                        .Replace("($RETURNTYPE$)", $"Task<IEnumerable<{entityName}>>")
                        .Replace("($METHODNAME$)", "GetAllBy" + mName + "Async")
                        .Replace("($METHODPARAMETERS$)", mParameters)
                        .Replace("($DATASELECT$)", "DataAsync.SelectElementsAsync")
                        .Replace("($DATARETURN$)", entityName)
                        .Replace("($DATASQL$)", GeneratorType == DalGeneratorType.Embedded ? "SelectSql + By" + mName : $"\"{GetStoredProcedureNameDelegate(entityTableName, "GetAllBy" + mName)}\"")
                        .Replace("($DATAPARAMETERS$)", ", " + oParameters)
                        );
                }


                if (index.Type == IndexType.PrimaryKey)
                {
                    methods.Add(dalDeleteExecuteMethod
                        .Replace("($RETURNTYPE$)", "int")
                        .Replace("($METHODNAME$)", "Delete")
                        .Replace("($METHODPARAMETERS$)", mParameters)
                        .Replace("($DATATYPE$)", entityName)
                        .Replace("($ASYNC$)", "")
                        .Replace("($DATASQL$)", GeneratorType == DalGeneratorType.Embedded ? "DeleteSQL" : $"\"{GetStoredProcedureNameDelegate(entityTableName, "Delete")}\"")
                        .Replace("($DATAPARAMETERS$)", ", " + oParameters)
                        );

                    methods2.Add(dalDeleteExecuteMethod
                        .Replace("($RETURNTYPE$)", "Task<int>")
                        .Replace("($METHODNAME$)", "DeleteAsync")
                        .Replace("($METHODPARAMETERS$)", mParameters)
                        .Replace("($DATATYPE$)", entityName)
                        .Replace("($ASYNC$)", "Async")
                        .Replace("($DATASQL$)", GeneratorType == DalGeneratorType.Embedded ? "DeleteSQL" : $"\"{GetStoredProcedureNameDelegate(entityTableName, "Delete")}\"")
                        .Replace("($DATAPARAMETERS$)", ", " + oParameters)
                        );

                }
            }

            methods.Add(dalExecuteMethod
                .Replace("($RETURNTYPE$)", "int")
                .Replace("($METHODNAME$)", "Insert")
                .Replace("($METHODNAME2$)", "Insert")
                .Replace("($DATATYPE$)", entityName)
                .Replace("($ASYNC$)", "")
                .Replace("($DATASQL$)", GeneratorType == DalGeneratorType.Embedded ? "InsertSQL" : $"\"{GetStoredProcedureNameDelegate(entityTableName, "Insert")}\"")
                );

            methods.Add(dalExecuteMethod
                .Replace("($RETURNTYPE$)", "int")
                .Replace("($METHODNAME$)", "Update")
                .Replace("($METHODNAME2$)", "Update")
                .Replace("($DATATYPE$)", entityName)
                .Replace("($ASYNC$)", "")
                .Replace("($DATASQL$)", GeneratorType == DalGeneratorType.Embedded ? "UpdateSQL" : $"\"{GetStoredProcedureNameDelegate(entityTableName, "Update")}\"")
                );
            methods.Add(dalExecuteMethod
                .Replace("($RETURNTYPE$)", "int")
                .Replace("($METHODNAME$)", "Delete")
                .Replace("($METHODNAME2$)", "Delete")
                .Replace("($DATATYPE$)", entityName)
                .Replace("($ASYNC$)", "")
                .Replace("($DATASQL$)", GeneratorType == DalGeneratorType.Embedded ? "DeleteSQL" : $"\"{GetStoredProcedureNameDelegate(entityTableName, "Delete")}\"")
                );


            methods2.Add(dalExecuteMethod
                .Replace("($RETURNTYPE$)", "Task<int>")
                .Replace("($METHODNAME$)", "InsertAsync")
                .Replace("($METHODNAME2$)", "Insert")
                .Replace("($DATATYPE$)", entityName)
                .Replace("($ASYNC$)", "Async")
                .Replace("($DATASQL$)", GeneratorType == DalGeneratorType.Embedded ? "InsertSQL" : $"\"{GetStoredProcedureNameDelegate(entityTableName, "Insert")}\"")
                );

            methods2.Add(dalExecuteMethod
                .Replace("($RETURNTYPE$)", "Task<int>")
                .Replace("($METHODNAME$)", "UpdateAsync")
                .Replace("($METHODNAME2$)", "Update")
                .Replace("($DATATYPE$)", entityName)
                .Replace("($ASYNC$)", "Async")
                .Replace("($DATASQL$)", GeneratorType == DalGeneratorType.Embedded ? "UpdateSQL" : $"\"{GetStoredProcedureNameDelegate(entityTableName, "Update")}\"")
                );
            methods2.Add(dalExecuteMethod
                .Replace("($RETURNTYPE$)", "Task<int>")
                .Replace("($METHODNAME$)", "DeleteAsync")
                .Replace("($METHODNAME2$)", "Delete")
                .Replace("($DATATYPE$)", entityName)
                .Replace("($ASYNC$)", "Async")
                .Replace("($DATASQL$)", GeneratorType == DalGeneratorType.Embedded ? "DeleteSQL" : $"\"{GetStoredProcedureNameDelegate(entityTableName, "Delete")}\"")
                );

            var body = header + dalWrapper;

            //FillEntity
            var fillEntities = GetFillMethodSentences(table);

            //PrepareEntity
            var prepareEntities = GetPrepareEntitySentences(table);

            //SelectBaseSQL
            var otherSqls = string.Empty;
            if (GeneratorType == DalGeneratorType.Embedded)
            {
                var container = GetSelectColumns(tableName);
                var sbSql = _dataAccessGenerator?.GetSelectFromContainer(container).Replace("\"", "\"\"");
                otherSqls = $"\t\tconst string SelectSql = @\"{Environment.NewLine}{sbSql}\";{Environment.NewLine}";
                var wheresList = _dataAccessGenerator?.GetWhereFromContainer(container);
                if (wheresList != null)
                {
                    foreach (var w in wheresList)
                        otherSqls +=
                            $"\t\tconst string {GetName(w.Item1)} = @\"{Environment.NewLine}{w.Item2.Replace("\"", "\"\"")}\";{Environment.NewLine}";
                }
                var insertSql = _dataAccessGenerator?.GetInsertFromContainer(container).Replace("\"", "\"\"");
                otherSqls += $"\t\tconst string InsertSQL = @\"{Environment.NewLine}{insertSql}\";{Environment.NewLine}";

                var updateSql = _dataAccessGenerator?.GetUpdateFromContainer(container).Replace("\"", "\"\"");
                otherSqls += $"\t\tconst string UpdateSQL = @\"{Environment.NewLine}{updateSql}\";{Environment.NewLine}";

                var deleteSql = _dataAccessGenerator?.GetDeleteFromContainer(container).Replace("\"", "\"\"");
                otherSqls += $"\t\tconst string DeleteSQL = @\"{Environment.NewLine}{deleteSql}\";{Environment.NewLine}";
            }
            var prov = _dataAccessGenerator?.GetType().Name.Replace("DataAccess", string.Empty);

            body = body.Replace("($OTHERSQLS$)", otherSqls);
            body = body.Replace("($FILLENTITY$)", string.Join("", fillEntities.ToArray()));
            body = body.Replace("($PREPAREENTITY$)", string.Join("", prepareEntities.ToArray()));
            body = body.Replace("($NAMESPACE$)", _namespace);
            body = body.Replace("($PROVIDERNAME$)", prov);
            body = body.Replace("($DATABASENAME$)", _schema.Name);
            body = body.Replace("($TABLENAME$)", entityTableName);
            body = body.Replace("($DATATYPE$)", entityName);
            body = body.Replace("($DATATYPE2$)", "ent" + entityTableName);
            body = body.Replace("($METHODS$)", string.Join(string.Empty, methods.Concat(methods2).ToArray()));

            var filePath = Path.Combine(_schema.Name, "Dal." + prov);
            filePath = Path.Combine(filePath, "Dal" + entityTableName + ".cs");
            return (filePath, body);
        }

        private List<string> GetFillMethodSentences(TableSchema table)
        {
            var fillEntities = new List<string>();
            foreach (var column in table.Columns)
            {
                var added = false;

                if (!column.IndexesName.Any(i => i.StartsWith("PK", StringComparison.OrdinalIgnoreCase)))
                {
                    //We have to check first if the column has a FK
                    foreach (var fk in table.ForeignKeys)
                    {
                        var fkTable = _schema.Tables.FirstOrDefault((t, fTable) => t.Name == fTable, fk.ForeignTable);
                        var fkColumn = fkTable?.Columns.FirstOrDefault((c, cName) => c.Name == cName, column.Name);
                        if (fkColumn == null) continue;
                        var isPk = fkColumn.IndexesName.Any(i => i.StartsWith("PK", StringComparison.OrdinalIgnoreCase));
                        if (!isPk) continue;
                            
                        var name = column.Name;
                        if (name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                            name = name.SubstringToLast("Id") + "Item";
                        else
                            name = fkTable.Name;
                        name = GetName(name);
                        var tName = GetEntityNameDelegate(fkTable.Name);
                        var type = "Ent" + tName;

                        var fill = $"            ($DATATYPE2$).{name} = binder.Bind<{type}>(rowValues, \"{fkTable.Name}.%\");\r\n";
                        if (!fillEntities.Contains(fill))
                            fillEntities.Add(fill);

                        added = true;
                        break;
                    }

                    if (!added)
                    {
                        //We try to find other entity to match the Id (without FK)
                        if (column.Name != "Id" && column.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var t in _schema.Tables)
                            {
                                var iPk = t.Indexes.FirstOrDefault(i => i.Type == IndexType.PrimaryKey);
                                if (iPk?.Columns?.Count != 1) continue;
                                if (iPk.Columns[0].ColumnName != column.Name) continue;
                                    
                                var name = column.Name.SubstringToLast("Id") + "Item";
                                name = GetName(name);
                                var tName = GetEntityNameDelegate(t.Name);
                                var type = "Ent" + tName;
                                var fill = $"            ($DATATYPE2$).{name} = binder.Bind<{type}>(rowValues, \"{t.Name}.%\");\r\n";
                                if (!fillEntities.Contains(fill))
                                    fillEntities.Add(fill);
                                break;
                            }
                        }
                    }
                }

            }

            return fillEntities;
        }
        private List<string> GetPrepareEntitySentences(TableSchema table)
        {
            var prepareEntities = new List<string>();
            foreach (var column in table.Columns)
            {
                var added = false;

                if (!column.IndexesName.Any(i => i.StartsWith("PK", StringComparison.OrdinalIgnoreCase)))
                {
                    //We have to check first if the column has a FK
                    foreach (var fk in table.ForeignKeys)
                    {
                        var fkTable = _schema.Tables.FirstOrDefault((t, fTable) => t.Name == fTable, fk.ForeignTable);
                        var fkColumn = fkTable?.Columns.FirstOrDefault((c, cName) => c.Name == cName, column.Name);
                        if (fkColumn == null) continue;
                        var isPk = fkColumn.IndexesName.Any(i => i.StartsWith("PK", StringComparison.OrdinalIgnoreCase));
                        if (!isPk) continue;
                                
                        var name = column.Name;
                        if (name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                            name = name.SubstringToLast("Id") + "Item";
                        else
                            name = fkTable.Name;
                        name = GetName(name);

                        var fill = "            param[\"@" + column.Name + "\"] = value." + name + "." + GetName(column.Name) + ";\r\n";
                        prepareEntities.Add(fill);

                        added = true;
                        break;
                    }

                    if (!added)
                    {
                        //We try to find other entity to match the Id (without FK)
                        if (column.Name != "Id" && column.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var t in _schema.Tables)
                            {
                                var iPk = t.Indexes.FirstOrDefault(i => i.Type == IndexType.PrimaryKey);
                                if (iPk?.Columns?.Count != 1) continue;
                                if (iPk.Columns[0].ColumnName != column.Name) continue;
                                    
                                var name = column.Name.SubstringToLast("Id") + "Item";
                                name = GetName(name);
                                var fill = "            param[\"@" + column.Name + "\"] = value." + name + "." + GetName(column.Name) + ";\r\n";
                                prepareEntities.Add(fill);
                                added = true;
                                break;
                            }
                        }
                    }
                }

                if (!added)
                {
                    var fill = "            param[\"@" + column.Name + "\"] = value." + GetName(column.Name) + ";\r\n";
                    prepareEntities.Add(fill);
                }
            }
            return prepareEntities;
        }
        private GeneratorSelectionContainer GetSelectColumns(string tableName)
        {
            var container = new GeneratorSelectionContainer();
            var table = _schema.Tables.FirstOrDefault((t, tName) => t.Name == tName, tableName);
            if (table == null) return container;
            container.From = table.Name;

            foreach (var column in table.Columns.OrderBy(c => c.Position))
            {
                var added = false;

                if (!column.IndexesName.Any(i => i.StartsWith("PK", StringComparison.OrdinalIgnoreCase)))
                {
                    container.TableColumns.Add((column.Name, GetName(column.Name)));

                    foreach (var fk in table.ForeignKeys)
                    {
                        var fkTable = _schema.Tables.FirstOrDefault((t, fTable) => t.Name == fTable, fk.ForeignTable);
                        var fkColumn = fkTable?.Columns.FirstOrDefault((c, cName) => c.Name == cName, column.Name);
                        if (fkColumn == null) continue;
                        var isPk = fkColumn.IndexesName.Any(i => i.StartsWith("PK", StringComparison.OrdinalIgnoreCase));
                        if (!isPk) continue;
                                
                        foreach (var foreignColumn in fkTable.Columns)
                            container.Columns.Add(new GeneratorSelectionColumn
                            {
                                Table = fkTable.Name,
                                Column = foreignColumn.Name,
                                Alias = $"{fkTable.Name}.{GetName(foreignColumn.Name)}"
                            });

                        container.Joins.Add(new GeneratorSelectionJoin
                        {
                            Table = fkTable.Name,
                            TableColumn = fkColumn.Name,
                            FromColumn = column.Name
                        });

                        added = true;
                        break;
                    }

                    if (!added)
                    {
                        //We try to find other entity to match the Id (without FK)
                        if (column.Name != "Id" && column.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var t in _schema.Tables)
                            {
                                var iPk = t.Indexes.FirstOrDefault(i => i.Type == IndexType.PrimaryKey);
                                if (iPk?.Columns?.Count != 1) continue;
                                if (iPk.Columns[0].ColumnName != column.Name) continue;
                                
                                foreach (var foreignColumn in t.Columns)
                                    container.Columns.Add(new GeneratorSelectionColumn
                                    {
                                        Table = t.Name,
                                        Column = foreignColumn.Name,
                                        Alias = $"{t.Name}.{GetName(foreignColumn.Name)}"
                                    });
                                container.Joins.Add(new GeneratorSelectionJoin
                                {
                                    Table = t.Name,
                                    TableColumn = iPk.Columns[0].ColumnName,
                                    FromColumn = column.Name
                                });
                                added = true;
                                break;
                            }
                        }
                    }
                }
                else
                    container.TableColumns.Add(("*" + column.Name, GetName(column.Name)));

                if (!added)
                {
                    container.Columns.Add(new GeneratorSelectionColumn
                    {
                        Table = table.Name,
                        Column = column.Name,
                        Alias = GetName(column.Name)
                    });
                }
            }

            foreach (var index in table.Indexes)
            {
                var names = new List<string>();
                var whereIdx = new GeneratorWhereIndex();
                foreach (var column in index.Columns.OrderBy(c => c.ColumnPosition))
                {
                    var tColumn = container.Columns.FirstOrDefault((c, cName) => c.Column == cName, column.ColumnName);
                    if (tColumn == null) continue;
                    names.Add(tColumn.Column);
                    whereIdx.Fields.Add(new GeneratorWhereField
                    {
                        FieldName = tColumn.Column,
                        TableName = tColumn.Table
                    });
                }
                var mName = string.Join("", names.ToArray());
                whereIdx.Name = "By" + mName;

                if (container.Wheres.All((w, wName) => w.Name != wName, whereIdx.Name))
                    container.Wheres.Add(whereIdx);
            }
            return container;
        }
        #endregion


        private static void WriteToDisk(string fileName, string content)
        {
            var dname = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(dname))
                Directory.CreateDirectory(dname);
            if (!File.Exists(fileName))
                File.WriteAllText(fileName, content);
        }
        private static string GetName(string name)
        {
            name = name.Replace("-", "_");
            name = name.Replace(" ", "_");
            name = name.Replace("__", "_");
            return name;
        }
        #endregion
    }
}
