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

using System.Collections.Generic;
using System.Data.Common;

namespace TWCore.Data
{
    /// <summary>
    /// Parameters binder interface definition
    /// </summary>
    public interface IParametersBinder
    {
        /// <summary>
        /// Bind parameter IDictionary to a DbCommand
        /// </summary>
        /// <param name="command">DbCommand to bind the parameters</param>
        /// <param name="parameters">IDictionary with the parameters</param>
        /// <param name="parameterPrefix">DbCommand parameter prefix</param>
        void BindParameters(DbCommand command, IDictionary<string, object> parameters, string parameterPrefix);
        /// <summary>
        /// Retrieves the output parameters and updates the IDictionary
        /// </summary>
        /// <param name="command">DbCommand to retrieve the output parameters</param>
        /// <param name="parameters">IDictionary object where is the output parameters are updated</param>
        /// <param name="parameterPrefix">DbCommand parameter prefix</param>
        void RetrieveOutputParameters(DbCommand command, IDictionary<string, object> parameters, string parameterPrefix);
    }

}
