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
using System.Data;
using System.Data.Common;

namespace TWCore.Data
{
    /// <inheritdoc />
    /// <summary>
    /// Default parameters binder
    /// </summary>
    public class DefaultParametersBinder : IParametersBinder
    {
        /// <inheritdoc />
        /// <summary>
        /// Bind parameter IDictionary to a DbCommand
        /// </summary>
        /// <param name="command">DbCommand to bind the parameters</param>
        /// <param name="parameters">IDictionary with the parameters</param>
        /// <param name="parameterPrefix">DbCommand parameter prefix</param>
        public void BindParameters(DbCommand command, IDictionary<string, object> parameters, string parameterPrefix)
        {
            if (parameters is null) return;
            foreach (var item in parameters)
            {
                var paramKey = item.Key.StartsWith(parameterPrefix) ? item.Key : parameterPrefix + item.Key;
                if (!command.Parameters.Contains(paramKey))
                {
                    var param = command.CreateParameter();
                    param.ParameterName = paramKey;
                    if (item.Value?.GetType() == typeof(DbType))
                    {
                        param.Direction = ParameterDirection.Output;
                        param.DbType = (DbType)item.Value;
                    }
                    else
                    {
                        param.Direction = ParameterDirection.Input;
                        param.Value = item.Value ?? DBNull.Value;
                    }
                    command.Parameters.Add(param);
                }
                else
                {
                    command.Parameters[paramKey].Value = item.Value ?? DBNull.Value;
                }
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Retrieves the output parameters and updates the IDictionary
        /// </summary>
        /// <param name="command">DbCommand to retrieve the output parameters</param>
        /// <param name="parameters">IDictionary object where is the output parameters are updated</param>
        /// <param name="parameterPrefix">DbCommand parameter prefix</param>
        public void RetrieveOutputParameters(DbCommand command, IDictionary<string, object> parameters, string parameterPrefix)
        {
            if (command?.Parameters is null || parameters is null) return;
            foreach (DbParameter itemParam in command.Parameters)
            {
                if (itemParam.Direction != ParameterDirection.InputOutput &&
                    itemParam.Direction != ParameterDirection.Output) continue;
                
                if (parameters.ContainsKey(itemParam.ParameterName))
                    parameters[itemParam.ParameterName] = itemParam.Value;
                else
                {
                    var pKey = itemParam.ParameterName.SubstringFromFirst(parameterPrefix);
                    if (parameters.ContainsKey(pKey))
                        parameters[pKey] = itemParam.Value;
                }
            }
        }
    }

}
