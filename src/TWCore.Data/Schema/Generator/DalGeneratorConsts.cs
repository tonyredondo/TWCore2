﻿/*
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

/// <summary>
/// DalGenerator Consts
/// </summary>
internal class DalGeneratorConsts
{
    public const string formatHeader = @"
/*
    DalGenerator
*/    
using System;
using System.Collections.Generic;
using TWCore.Data;
";



    public const string formatDatabaseEntities = @"
namespace ($NAMESPACE$).($DATABASENAME$)
{
    public class ($DATABASENAME$)DalSettings : EntityDalSettings
    {
        public ($DATABASENAME$)DalSettings()
        {
            ConnectionString = ""($CONNECTIONSTRING$)"";
        }
    }
    public class ($DATABASENAME$)DBDal : EntityDal
    {
        protected override IDataAccess OnGetDataAccess(EntityDalSettings settings)
        {
            return new ($PROVIDER$)(Settings.ConnectionString, settings.QueryType);
        }
        protected override EntityDalSettings OnGetSettings()
        {
            return new ($DATABASENAME$)DalSettings();
        }
    }
    public class ($DATABASENAME$)DBDalAsync : EntityDalAsync
    {
        protected override IDataAccessAsync OnGetDataAccess(EntityDalSettings settings)
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

}
