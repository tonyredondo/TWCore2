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
using System.Runtime.CompilerServices;
#pragma warning disable 1591

namespace TWCore.Serialization.RawSerializer
{
    public partial class SerializersTable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(SerializedObject value)
        {
            if (value is null)
            {
                WriteByte(DataBytesDefinition.SerializedObjectNull);
                return;
            }
            WriteByte(DataBytesDefinition.SerializedObject);
            value.CopyTo(Stream);
        }
    }



    public partial class DeserializersTable
    {
        [DeserializerMethod(DataBytesDefinition.SerializedObjectNull, DataBytesDefinition.SerializedObject, ReturnType = typeof(SerializedObject))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializedObject ReadSerializedObject(byte type)
        {
            if (type == DataBytesDefinition.SerializedObjectNull)
                return null;
            if (type == DataBytesDefinition.SerializedObject)
                return SerializedObject.FromStream(Stream);
            ThrowInvalidOperationException(type);
            return default;
        }
    }
}