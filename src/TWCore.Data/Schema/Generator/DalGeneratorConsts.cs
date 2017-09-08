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


namespace TWCore.Data.Schema.Generator
{
    /// <summary>
    /// DalGenerator Consts
    /// </summary>
    internal static class DalGeneratorConsts
    {

        public const string FormatAbstractionsProject = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""TWCore"" Version=""($VERSION$)"" />
    <PackageReference Include=""TWCore.Data"" Version=""($VERSION$)"" />
  </ItemGroup>
</Project>
";
        public const string FormatDalProject = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""TWCore"" Version=""($VERSION$)"" />
    <PackageReference Include=""TWCore.Data"" Version=""($VERSION$)"" />
    <PackageReference Include=""($DATAASSEMBLYNAME$)"" Version=""($VERSION$)"" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include=""..\Abstractions\($NAMESPACE$).($CATALOGNAME$).Abstractions.csproj"" />
  </ItemGroup>
</Project>
";

        public const string FormatSolution = @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 15
VisualStudioVersion = 15.0.26823.1
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{9A19103F-16F7-4668-BE54-9A1E7A4F7556}"") = ""($NAMESPACE$).($CATALOGNAME$).Abstractions"", ""Abstractions\($NAMESPACE$).($CATALOGNAME$).Abstractions.csproj"", ""{D82C9EBF-4922-4722-9BC4-74D0DE8E8718}""
EndProject
Project(""{9A19103F-16F7-4668-BE54-9A1E7A4F7556}"") = ""($NAMESPACE$).($CATALOGNAME$).($PROVIDERNAME$)"", ""Dal.($PROVIDERNAME$)\($NAMESPACE$).($CATALOGNAME$).($PROVIDERNAME$).csproj"", ""{50FDBEE9-80D5-437D-912E-6C70D6C8A9D1}""
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{D82C9EBF-4922-4722-9BC4-74D0DE8E8718}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{D82C9EBF-4922-4722-9BC4-74D0DE8E8718}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{D82C9EBF-4922-4722-9BC4-74D0DE8E8718}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{D82C9EBF-4922-4722-9BC4-74D0DE8E8718}.Release|Any CPU.Build.0 = Release|Any CPU
		{50FDBEE9-80D5-437D-912E-6C70D6C8A9D1}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{50FDBEE9-80D5-437D-912E-6C70D6C8A9D1}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{50FDBEE9-80D5-437D-912E-6C70D6C8A9D1}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{50FDBEE9-80D5-437D-912E-6C70D6C8A9D1}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(ExtensibilityGlobals) = postSolution
		SolutionGuid = {43EDC8BD-E220-4E35-804D-917D4FC0EC36}
	EndGlobalSection
EndGlobal
";


        //******************************************************************************************************************************************************

        public const string FormatEntityHeader = @"/*
    TWCore.Data.Schema.Generator
*/    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TWCore.Data;
using TWCore.Data.Schema;
";



        public const string FormatEntityWrapper = @"
namespace ($NAMESPACE$).($DATABASENAME$).Entities
{
    [DataContract]
    public class Ent($TABLENAME$)
    {($COLUMNS$)
    }
}     
";

        public const string FormatEntityColumn = @"
        [DataMember]
        public ($COLUMNTYPE$) ($COLUMNNAME$) { get; set; }";


        //******************************************************************************************************************************************************


        public const string FormatDalInterfaceHeader = @"/*
    TWCore.Data.Schema.Generator
*/    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TWCore.Data;
using TWCore.Data.Schema;
using ($NAMESPACE$).($DATABASENAME$).Entities;
";

        public const string FormatDalInterfaceWrapper = @"
namespace ($NAMESPACE$).($DATABASENAME$).Dal
{
    public interface IDal($TABLENAME$)
    {($METHODS$)
    }
}     
";

        public const string FormatDalInterfaceMethod = @"
        ($RETURNTYPE$) ($METHODNAME$)(($METHODPARAMETERS$));";


        //******************************************************************************************************************************************************


        public const string FormatDatabaseEntities = @"
namespace ($NAMESPACE$).($DATABASENAME$).Dal.($PROVIDERNAME$)
{
    public class ($DATABASENAME$)DalSettings : EntityDalSettings
    {
        public ($DATABASENAME$)DalSettings()
        {
            ConnectionString = ""($CONNECTIONSTRING$)"";
            QueryType = DataAccessType.($QUERYTYPE$);
        }
    }
    public class ($DATABASENAME$)DBDal : EntityDal
    {
        protected override IDataAccess OnGetDataAccess(EntityDalSettings settings)
        {
            return new ($PROVIDER$)(Settings.ConnectionString, settings.QueryType);
        }
        protected override IDataAccessAsync OnGetDataAccessAsync(EntityDalSettings settings)
        {
            return new ($PROVIDER$)(Settings.ConnectionString, settings.QueryType);
        }
        protected override EntityDalSettings OnGetSettings()
        {
            return new ($DATABASENAME$)DalSettings();
        }
    }
}
";

        //******************************************************************************************************************************************************


        public const string FormatDalHeader = @"/*
    TWCore.Data.Schema.Generator
*/    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TWCore.Data;
using TWCore.Data.Schema;
using ($NAMESPACE$).($DATABASENAME$).Entities;
";


        public const string FormatDalWrapper = @"
namespace ($NAMESPACE$).($DATABASENAME$).Dal.($PROVIDERNAME$)
{
    public class Dal($TABLENAME$) : ($DATABASENAME$)DBDal, IDal($TABLENAME$)
    {($METHODS$)

        #region Private Methods
        private Dictionary<string, object> PrepareEntity(($DATATYPE$) value, string transaction)
        {
            var param = new Dictionary<string, object>();
($PREPAREENTITY$)
            return param;
        }
        private ($DATATYPE$) FillEntity(EntityBinder binder, object[] rowValues)
        {
            var ($DATATYPE2$) = binder.Bind<($DATATYPE$)>(rowValues);
($FILLENTITY$)
            return ($DATATYPE2$);
        }
($OTHERSQLS$)
        #endregion
    }
}     
";

        public const string FormatDalSelectMethod = @"
        public ($RETURNTYPE$) ($METHODNAME$)(($METHODPARAMETERS$)) 
            => ($DATASELECT$)(($DATASQL$)($DATAPARAMETERS$), FillEntity);
";

        public const string FormatDalExecuteMethod = @"
        public ($RETURNTYPE$) ($METHODNAME$)(($DATATYPE$) value) 
            => Data($ASYNC$).ExecuteNonQuery($ASYNC$)(($DATASQL$), PrepareEntity(value, ""($METHODNAME2$)""));
";

        public const string FormatDalDeleteExecuteMethod = @"
        public ($RETURNTYPE$) ($METHODNAME$)(($METHODPARAMETERS$)) 
            => Data($ASYNC$).ExecuteNonQuery($ASYNC$)(($DATASQL$)($DATAPARAMETERS$));
";
    }
}
