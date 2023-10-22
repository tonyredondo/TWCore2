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
using System.Runtime.CompilerServices;
using MySqlConnector;

// ReSharper disable NotAccessedField.Local

namespace TWCore.Data.MySql
{
    /// <inheritdoc />
    /// <summary>
    /// Value converter for the entity binder for SqlServer databases
    /// </summary>
    public class MySqlEntityValueConverter : IEntityValueConverter
    {
        private MySqlDataAccess _dataAccess;

        #region .ctor
        /// <summary>
        /// Value converter for the entity binder for SqlServer databases
        /// </summary>
        /// <param name="dataAccess">SqlServerDataAccess instance</param>
        public MySqlEntityValueConverter(MySqlDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Converts a value from the data source to the property type of the entity.
        /// </summary>
        /// <param name="value">Value to convert from the data source</param>
        /// <param name="valueType">Value type from the data source, this is the source type</param>
        /// <param name="propertyType">Property type of the entity, is the destination type</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="propertyValue">Output parameter as result of the convertion</param>
        /// <returns>true if the convertion was successful; otherwise, false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Convert(object value, Type valueType, Type propertyType, object defaultValue, out object propertyValue)
        {
            propertyValue = defaultValue;
            if (value == DBNull.Value)
                return true;
            if (value is null && propertyValue is null)
                return true;

            //TODO: Not supported by MySqlConnector
            //if (valueType == typeof(MySqlGeometry))
            //{
            //    if (value != null) 
            //        propertyValue = ((MySqlGeometry) value).GetWKT();
            //    return true;
            //}

            var dtValue = value as DateTime?;
            var sdtValue = value as MySqlDateTime?;
            if (dtValue.HasValue)
            {
                propertyValue = dtValue.Value;
                return true;
            }
            if (sdtValue.HasValue)
            {
                propertyValue = sdtValue.Value;
                return true;
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
