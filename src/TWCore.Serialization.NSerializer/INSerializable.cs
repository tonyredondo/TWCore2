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


namespace TWCore.Serialization.NSerializer
{
    /// <summary>
    /// NSerializable interface
    /// </summary>
    public interface INSerializable
    {
        /// <summary>
        /// Serialize method
        /// </summary>
        /// <param name="table">SerializersTable instance</param>
        void Serialize(SerializersTable table);
        /// <summary>
        /// Fill 
        /// </summary>
        /// <param name="table">DeserializersTable instance</param>
        /// <param name="metadata">Matadata instance</param>
        void Fill(DeserializersTable table, DeserializerMetaDataOfType metadata);
    }
}
