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

namespace TWCore.Data
{
    /// <inheritdoc />
    /// <summary>
    /// Value converter for the entity binder for Sql databases
    /// </summary>
    public class DefaultEntityValueConverter : IEntityValueConverter
    {
        /// <inheritdoc />
        /// <summary>
        /// Converts a value from the data source to the property type of the entity.
        /// </summary>
        /// <param name="value">Value to convert from the data source</param>
        /// <param name="valueType">Value type from the data source, this is the source type</param>
        /// <param name="propertyType">Property type of the entity, is the destination type</param>
        /// <param name="propertyValue">Output parameter as result of the convertion</param>
        /// <returns>true if the convertion was successful; otherwise, false</returns>
        public bool Convert(object value, Type valueType, Type propertyType, out object propertyValue)
        {
            propertyValue = propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;
            if (value == DBNull.Value)
                return true;
            if (value == null && propertyValue == null)
                return true;
            if (value is IConvertible)
            {
                propertyValue = System.Convert.ChangeType(value, propertyType);
                return true;
            }
            return false;
        }
    }
}
