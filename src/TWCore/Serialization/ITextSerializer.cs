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
// ReSharper disable UnusedMemberInSuper.Global

namespace TWCore.Serialization
{
    /// <inheritdoc />
    /// <summary>
    /// Text serializer interface
    /// </summary>
    public interface ITextSerializer : ISerializer
    {
        /// <summary>
        /// Serialize an object to a string
        /// </summary>
        /// <param name="item">Object to serialize</param>
        /// <param name="itemType">Object type</param>
        /// <returns>Serialized string</returns>
        string SerializeToString(object item, Type itemType);
        /// <summary>
        /// Serialize an object to a string
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object to serialize</param>
        /// <returns>Serialized string</returns>
        string SerializeToString<T>(T item);
        /// <summary>
        /// Deserialize a string to an object value
        /// </summary>
        /// <param name="value">String to deserialize</param>
        /// <param name="itemType">Object type</param>
        /// <returns>Deserialized object</returns>
        object DeserializeFromString(string value, Type itemType);
        /// <summary>
        /// Deserialize a string to an object value
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">String to deserialize</param>
        /// <returns>Deserialized object</returns>
        T DeserializeFromString<T>(string value);
    }
}
