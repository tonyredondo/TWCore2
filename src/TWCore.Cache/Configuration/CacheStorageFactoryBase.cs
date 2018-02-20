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
using TWCore.Collections;
using TWCore.Configuration;

namespace TWCore.Cache.Configuration
{
    /// <inheritdoc />
    /// <summary>
    /// Cache Storage type factory
    /// </summary>
    public abstract class CacheStorageFactoryBase : ITypeFactory
    {
        /// <inheritdoc />
        /// <summary>
        /// Object type of the result of the factory
        /// </summary>
        public Type ObjectType => typeof(StorageBase);
        /// <inheritdoc />
        /// <summary>
        /// Create a new object from a KeyValueCollection parameters
        /// </summary>
        /// <param name="config">Configuration for object creation</param>
        /// <returns>Object instance</returns>
        public T Create<T>(BasicConfigurationItem config)
            => (T)(IStorage)CreateStorage(config?.Parameters);
        /// <summary>
        /// Create a new Storage from a KeyValueCollection parameters
        /// </summary>
        /// <param name="parameters">Parameters to create the storage</param>
        /// <returns>Storage instance</returns>
        protected abstract StorageBase CreateStorage(KeyValueCollection parameters);
    }
}
