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

using System.Runtime.CompilerServices;
#pragma warning disable 1591

namespace TWCore.Serialization.RawSerializer
{
    public readonly struct DeserializerMetadataOfTypeRuntime
    {
        public readonly DeserializerMetaDataOfType MetaDataOfType;
        public readonly DeserializerTypeDescriptor Descriptor;
        public readonly bool EqualToDefinition;
        
        #region .ctor
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DeserializerMetadataOfTypeRuntime(DeserializerMetaDataOfType metaDataOfType, DeserializerTypeDescriptor descriptor)
        {
            MetaDataOfType = metaDataOfType;
            Descriptor = descriptor;
            EqualToDefinition = metaDataOfType == descriptor.Metadata;
        }
        #endregion
    }
}