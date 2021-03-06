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


namespace TWCore.Serialization.RawSerializer
{
    /// <summary>
    /// Raw serializer interface
    /// </summary>
    public interface IRawSerializable
    {
        /// <summary>
        /// Serialize object
        /// </summary>
        /// <param name="table">Serializers table</param>
        void Serialize(SerializersTable table);
        /// <summary>
        /// Fill instance
        /// </summary>
        /// <param name="table">Deserializer table</param>
        /// <param name="metadata">Deserializer metadata</param>
        void Fill(DeserializersTable table, DeserializerMetaDataOfType metadata);
    }
}
