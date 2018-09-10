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
using System.Runtime.Serialization;

namespace TWCore.Data.Schema
{
    /// <summary>
    /// Index Type
    /// </summary>
    [DataContract]
    public enum IndexType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,
        /// <summary>
        /// Primary key
        /// </summary>
        PrimaryKey,
        /// <summary>
        /// Unique key
        /// </summary>
        UniqueKey,
        /// <summary>
        /// Foreign key
        /// </summary>
        ForeignKey,
        /// <summary>
        /// Non clustered non unique index
        /// </summary>
        NonClusteredNonUniqueIndex,
        /// <summary>
        /// Unique index
        /// </summary>
        UniqueIndex,
        /// <summary>
        /// Alternate index
        /// </summary>
        AlternateIndex,
        /// <summary>
        /// Clustered index
        /// </summary>
        ClusteredIndex,
        /// <summary>
        /// Covering index
        /// </summary>
        CoveringIndex,
        /// <summary>
        /// Unique clustered index
        /// </summary>
        UniqueClusteredIndex
    }
}
