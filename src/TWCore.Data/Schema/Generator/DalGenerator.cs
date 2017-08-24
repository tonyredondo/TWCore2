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
using System.IO;
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

        /// <summary>
        /// Gets the EntityName Delegate
        /// </summary>
        public Func<string, string> GetEntityNameDelegate { get; set; } = new Func<string, string>(tableName =>
        {
            tableName = tableName.Replace("_", " ");
            tableName = tableName.CapitalizeEachWords();
            tableName = tableName.Replace("-", "_");
            return tableName.RemoveSpaces();
        });

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

        public void Create(string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string fName, fContent;

            (fName, fContent) = CreateProject();
            WriteToDisk(Path.Combine(directory, fName), fContent);

            (fName, fContent) = CreateDatabaseEntity();
            WriteToDisk(Path.Combine(directory, fName), fContent);

            foreach(var table in _schema.Tables)
            {
                (fName, fContent) = CreateEntity(table.Name);
                WriteToDisk(Path.Combine(directory, fName), fContent);
            }
        }


        (string, string) CreateProject()
        {
            string projFile = DalGeneratorConsts.formatProj;
            projFile = projFile.Replace("($ASSEMBLYNAME$)", _schema.Assembly);
            return (_schema.Name + Path.DirectorySeparatorChar + "Data." + _schema.Name + ".csproj", projFile);
        }
        (string, string) CreateDatabaseEntity()
        {
            string header = DalGeneratorConsts.formatHeader;
            header += "using " + _schema.Assembly + ";\r\n";
            string databaseEntities = DalGeneratorConsts.formatDatabaseEntities;
            databaseEntities = databaseEntities.Replace("($NAMESPACE$)", _namespace);
            databaseEntities = databaseEntities.Replace("($DATABASENAME$)", _schema.Name);
            databaseEntities = databaseEntities.Replace("($CONNECTIONSTRING$)", _schema.ConnectionString);
            databaseEntities = databaseEntities.Replace("($PROVIDER$)", _schema.Provider);

            return ( _schema.Name + Path.DirectorySeparatorChar + _schema.Name + ".cs", header + databaseEntities);
        }
        (string, string) CreateEntity(string tableName)
        {
            var table = _schema.Tables.FirstOrDefault(t => t.Name == tableName);
            if (table == null) return (null, null);

            string header = DalGeneratorConsts.formatHeader;
            string entityWrapper = DalGeneratorConsts.formatEntityWrapper;
            string columnFormat = DalGeneratorConsts.formatEntityColumn;

            var entityColumns = new List<string>();
            foreach (var column in table.Columns)
            {
                var strColumn = columnFormat;
                bool added = false;

                //We have to check first if the column has a FK
                foreach (var fk in table.ForeignKeys)
                {
                    var fkTable = _schema.Tables.FirstOrDefault(t => t.Name == fk.ForeignTable);
                    if (fkTable != null)
                    {
                        var fkColumn = fkTable.Columns.FirstOrDefault(c => c.Name == column.Name);
                        if (fkColumn != null)
                        {
                            var isPK = fkColumn.IndexesName.Any(i => i.StartsWith("PK"));
                            if (isPK)
                            {
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
                        }
                    }
                }

                if (!added)
                {
                    //We try to find other entity to match the Id (without FK)
                    if (column.Name != "Id" && column.Name.EndsWith("Id") && !column.IndexesName.Any(i => i.StartsWith("PK")))
                    {
                        foreach (var t in _schema.Tables)
                        {
                            var iPk = t.Indexes.FirstOrDefault(i => i.Type == IndexType.PrimaryKey);
                            if (iPk?.Columns?.Count == 1)
                            {
                                if (iPk.Columns[0].ColumnName == column.Name)
                                {
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
                }

                if (!added)
                {
                    strColumn = strColumn.Replace("($COLUMNTYPE$)", column.DataType);
                    strColumn = strColumn.Replace("($COLUMNNAME$)", GetName(column.Name));
                    entityColumns.Add(strColumn);
                }
            }



            entityWrapper = entityWrapper.Replace("($NAMESPACE$)", _namespace);
            entityWrapper = entityWrapper.Replace("($DATABASENAME$)", _schema.Name);
            entityWrapper = entityWrapper.Replace("($TABLENAME$)", GetEntityNameDelegate(table.Name));
            entityWrapper = entityWrapper.Replace("($COLUMNS$)", string.Join(string.Empty, entityColumns.ToArray()));


            var filePath = Path.Combine(_schema.Name, "Entities");
            filePath = Path.Combine(filePath, GetEntityNameDelegate(table.Name) + ".cs");
            return (filePath, header + entityWrapper);
        }

        void WriteToDisk(string fileName, string content)
        {
            var dname = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(dname))
                Directory.CreateDirectory(dname);
            if (!File.Exists(fileName))
                File.WriteAllText(fileName, content);
        }
        
        string GetName(string name)
        {
            name = name.Replace("-", "_");
            name = name.Replace(" ", "_");
            name = name.Replace("__", "_");
            return name;
        }
    }
}
