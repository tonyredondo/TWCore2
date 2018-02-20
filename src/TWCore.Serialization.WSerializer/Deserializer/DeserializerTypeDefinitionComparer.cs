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

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TWCore.Serialization.WSerializer.Deserializer
{
    internal class DeserializerTypeDefinitionComparer : IEqualityComparer<DeserializerTypeDefinition>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(DeserializerTypeDefinition x, DeserializerTypeDefinition y)
        {
            if (x?.Type == null && y?.Type != null)
                return false;
            if (x?.Type != null && y?.Type == null)
                return false;
            if (x?.Type == null && y?.Type == null)
                return true;
            return x?.Type != null && x.Type == y?.Type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(DeserializerTypeDefinition obj)
            => obj?.Type?.GetHashCode() ?? obj?.GetHashCode() ?? -1;
    }
}
