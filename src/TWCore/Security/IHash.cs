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

namespace TWCore.Security
{
    /// <summary>
    /// Hash algorithm interface
    /// </summary>
    public interface IHash
    {
        /// <summary>
        /// Hash algorithm used
        /// </summary>
        string Algorithm { get; }
        /// <summary>
        /// Gets the hash bytes from a bytes array
        /// </summary>
        /// <param name="bytes">Bytes array to calculate the hash.</param>
        /// <returns>Hash bytes array.</returns>
        MultiArray<byte> GetBytes(MultiArray<byte> bytes);
        /// <summary>
        /// Gets the string hash value from a bytes array
        /// </summary>
        /// <param name="bytes">Bytes array to calculate the hash.</param>
        /// <returns>String value with the hash.</returns>
        string Get(MultiArray<byte> bytes);
        /// <summary>
        /// Gets the guid hash value from a bytes array
        /// </summary>
        /// <param name="bytes">Bytes array to calculate the hash.</param>
        /// <returns>Guid value with the hash.</returns>
        Guid GetGuid(MultiArray<byte> bytes);
        /// <summary>
        /// Gets the hash bytes from an object
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>Hash bytes array.</returns>
        MultiArray<byte> GetBytes(object obj);
        /// <summary>
        /// Gets the hash string value from an object
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>String value with the hash.</returns>
        string Get(object obj);
        /// <summary>
        /// Gets the guid hash value from an object
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>Guid value with the hash.</returns>
        Guid GetGuid(object obj);

        /// <summary>
        /// Gets the hash bytes from a string value
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>Hash bytes array.</returns>
        MultiArray<byte> GetBytes(string obj);
        /// <summary>
        /// Gets the hash string value from a string value
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>String value with the hash.</returns>
        string Get(string obj);
        /// <summary>
        /// Gets the guid hash value from a string value
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>Guid value with the hash.</returns>
        Guid GetGuid(string obj);
    }
}
