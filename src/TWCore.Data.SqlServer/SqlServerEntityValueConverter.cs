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
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;

namespace TWCore.Data.SqlServer
{
    /// <summary>
    /// Value converter for the entity binder for SqlServer databases
    /// </summary>
    public class SqlServerEntityValueConverter : IEntityValueConverter
    {
        SqlServerDataAccess _dataAccess;

        #region .ctor
        /// <summary>
        /// Value converter for the entity binder for SqlServer databases
        /// </summary>
        /// <param name="dataAccess">SqlServerDataAccess instance</param>
        public SqlServerEntityValueConverter(SqlServerDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }
        #endregion

        /// <summary>
        /// Converts a value from the data source to the property type of the entity.
        /// </summary>
        /// <param name="value">Value to convert from the data source</param>
        /// <param name="valueType">Value type from the data source, this is the source type</param>
        /// <param name="propertyType">Property type of the entity, is the destination type</param>
        /// <param name="propertyValue">Output parameter as result of the convertion</param>
        /// <returns>true if the convertion was successful; otherwise, false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Convert(object value, Type valueType, Type propertyType, out object propertyValue)
        {
            propertyValue = propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;
            if (value == DBNull.Value)
                return true;
            if (value == null && propertyValue == null)
                return true;
            if (valueType != null)
            {
                if (valueType.Name == "SqlGeometry" && valueType.Namespace == "Microsoft.SqlServer.Types")
                {
                    propertyValue = value?.ToString();
                    return true;
                }
                if (valueType.Name == "SqlGeography" && valueType.Namespace == "Microsoft.SqlServer.Types")
                {
                    propertyValue = value?.ToString();
                    return true;
                }
            }
            if (_dataAccess.ReplaceDateTimeMinMaxValues)
            {
                var dtValue = value as DateTime?;
                var sdtValue = value as SqlDateTime?;

                if (dtValue.HasValue)
                {
                    if (dtValue.Value == SqlDateTime.MinValue.Value)
                        propertyValue = DateTime.MinValue;
                    else if (dtValue.Value == SqlDateTime.MaxValue.Value)
                        propertyValue = DateTime.MaxValue;
                    else
                        propertyValue = dtValue.Value;
                    return true;
                }

                if (sdtValue.HasValue)
                {
                    if (sdtValue.Value == SqlDateTime.MinValue)
                        propertyValue = DateTime.MinValue;
                    else if (sdtValue.Value == SqlDateTime.MaxValue)
                        propertyValue = DateTime.MaxValue;
                    else
                        propertyValue = sdtValue.Value.Value;
                    return true;
                }
            }
            if (value is IConvertible)
            {
                propertyType = propertyType.GetUnderlyingType();
                propertyValue = System.Convert.ChangeType(value, propertyType);
                return true;
            }
            return false;
        }
    }
}
