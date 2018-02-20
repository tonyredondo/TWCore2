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

namespace TWCore.Serialization.WSerializer
{
    /// <summary>
    /// Serializer mode
    /// </summary>
    public enum SerializerMode
    {
        /// <summary>
        /// Uses a dictionary cache with a capacity of an ushort
        /// </summary>
        CachedUShort = 251,
        /// <summary>
        /// Uses a dictionary cache with a capacity of an four bytes
        /// </summary>
        Cached2048 = 252,
        /// <summary>
        /// Uses a dictionary cache with a capacity of an two bytes
        /// </summary>
        Cached1024 = 253,
        /// <summary>
        /// Uses a dictionary cache with a capacity of a Byte
        /// </summary>
        Cached512 = 254,
        /// <summary>
        /// Don't use any cache
        /// </summary>
        NoCached = 255
    }
}
