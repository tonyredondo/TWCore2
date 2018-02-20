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

using TWCore.Cache.Storages.IO;
using TWCore.Collections;
using TWCore.Compression;
using TWCore.Serialization;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable SwitchStatementMissingSomeCases

namespace TWCore.Cache.Configuration
{
    /// <inheritdoc />
    /// <summary>
    /// File storage factory using the configuration parameters
    /// </summary>
    public class FileStorageFactory : CacheStorageFactoryBase
    {
        /// <inheritdoc />
        /// <summary>
        /// Create a new storage from a KeyValueCollection parameters
        /// </summary>
        /// <param name="parameters">Parameters to create the storage</param>
        /// <returns>Storage</returns>
        protected override StorageBase CreateStorage(KeyValueCollection parameters)
        {
            var basePath = parameters["BasePath"];
            var serializerMimeType = parameters["SerializerMimeType"];
            var metaSerializerMimeType = parameters["MetaSerializerMimeType"] ?? "application/json";
            var compressorEncodingType = parameters["CompressorEncodingType"];
            var numberOfSubFolder = parameters["NumberOfSubFolder"].ParseTo((byte)25);
            var transactionLogThreshold = parameters["TransactionLogThreshold"].ParseTo(250);
            var slowDownWriteThreshold = parameters["SlowDownWriteThreshold"].ParseTo(1000);
            var storageType = parameters["StorageType"].ParseTo(FileStorageType.Normal);

            var metaSerializer = SerializerManager.GetByMimeType<ISerializer>(metaSerializerMimeType);
            var serializer = SerializerManager.GetByMimeType(serializerMimeType);
            if (compressorEncodingType.IsNotNullOrWhitespace())
                serializer.Compressor = CompressorManager.GetByEncodingType(compressorEncodingType);

            SerializerManager.SupressFileExtensionWarning = true;

            switch (storageType)
            {
                case FileStorageType.Normal:
                    Core.Log.LibDebug("Creating a new FileStorage with the parameters:");
                    Core.Log.LibDebug("\tBasePath: {0}", basePath);
                    Core.Log.LibDebug("\tNumberOfSubFolders: {0}", numberOfSubFolder);
                    Core.Log.LibDebug("\tTransactionLogThreshold: {0}", transactionLogThreshold);
                    Core.Log.LibDebug("\tSlowDownWriteThreshold: {0}", slowDownWriteThreshold);
                    Core.Log.LibDebug("\tMetaSerializer: {0}", metaSerializer);
                    Core.Log.LibDebug("\tSerializer: {0}", serializer);
                    return new FileStorage(basePath)
                    {
						NumberOfSubFolders = numberOfSubFolder,
                        TransactionLogThreshold = transactionLogThreshold,
                        SlowDownWriteThreshold = slowDownWriteThreshold,
						MetaSerializer = (BinarySerializer)metaSerializer,
						Serializer = (BinarySerializer)serializer
                    };
            }
            return null;
        }

        /// <summary>
        /// File storage type
        /// </summary>
        public enum FileStorageType
        {
            /// <summary>
            /// Normal file storage with a single index and journal
            /// </summary>
            Normal
        }
    }
}
