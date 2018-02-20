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


namespace TWCore.Data
{
    /// <summary>
    /// Defines the type of the data access command
    /// </summary>
    public enum DataAccessType : byte
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0x00,
        /// <summary>
        /// Stored Procedure command
        /// </summary>
        StoredProcedure = 0x01,
        /// <summary>
        /// Sql Query command
        /// </summary>
        Query = 0x10
    }
}
