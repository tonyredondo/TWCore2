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

namespace TWCore.Cache.Client
{
    /// <summary>
    /// Defines the storage item mode in the pool
    /// </summary>
    public enum StorageItemMode
    {
        /// <summary>
        /// The item will be used for read and write transactions
        /// </summary>
        ReadAndWrite = 0x11,
        /// <summary>
        /// The item will be used only for read transactions
        /// </summary>
        Read = 0x01,
        /// <summary>
        /// The item will be used only for write transactions
        /// </summary>
        Write = 0x10
    }
}
