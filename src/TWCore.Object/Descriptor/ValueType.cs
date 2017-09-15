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

namespace TWCore.Object.Descriptor
{
    /// <summary>
    /// Value type
    /// </summary>
    public enum ValueType
    {
        /// <summary>
        /// Guid type
        /// </summary>
        Guid,
        /// <summary>
        /// String type
        /// </summary>
        String,
        /// <summary>
        /// Number type
        /// </summary>
        Number,
        /// <summary>
        /// Boolean type
        /// </summary>
        Bool,
        /// <summary>
        /// Date type
        /// </summary>
        Date,
        /// <summary>
        /// Time type
        /// </summary>
        Time,
        /// <summary>
        /// Enum type
        /// </summary>
        Enum,
        /// <summary>
        /// Enumerable type
        /// </summary>
        Enumerable,
        /// <summary>
        /// Complex type
        /// </summary>
        Complex,
        /// <summary>
        /// Method type
        /// </summary>
        Method,
        /// <summary>
        /// Is a Type
        /// </summary>
        Type
    }
}