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
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace TWCore.Serialization.NSerializer
{
    public partial class SerializersTable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(SerializedObject value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.SerializedObjectNull);
                return;
            }
            WriteByte(DataBytesDefinition.SerializedObject);
            var dataTypeByte = !string.IsNullOrEmpty(value.DataType) ? Encoding.UTF8.GetBytes(value.DataType) : null;
            var serializerMimeTypeByte = !string.IsNullOrEmpty(value.SerializerMimeType) ? Encoding.UTF8.GetBytes(value.SerializerMimeType) : null;
            WriteInt(dataTypeByte?.Length ?? -1);
            if (dataTypeByte != null) Stream.Write(dataTypeByte, 0, dataTypeByte.Length);
            WriteInt(serializerMimeTypeByte?.Length ?? -1);
            if (serializerMimeTypeByte != null) Stream.Write(serializerMimeTypeByte, 0, serializerMimeTypeByte.Length);
            WriteInt(value.Data?.Length ?? -1);
            if (value.Data != null) Stream.Write(value.Data, 0, value.Data.Length);
        }
    }



    public partial class DeserializersTable
    {
        [DeserializerMethod(DataBytesDefinition.SerializedObjectNull, DataBytesDefinition.SerializedObject, ReturnType = typeof(SerializedObject))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializedObject ReadSerializedObject(byte type)
        {
            if (type == DataBytesDefinition.SerializedObjectNull) return null;
            if (type != DataBytesDefinition.SerializedObject) throw new InvalidOperationException("Invalid type value.");

            var dataTypeByteLength = Reader.ReadInt32();
            if (dataTypeByteLength < -1) return null;
            var dataTypeByte = dataTypeByteLength != -1 ? Reader.ReadBytes(dataTypeByteLength) : null;

            var serializerMimeTypeByteLength = Reader.ReadInt32();
            if (serializerMimeTypeByteLength < -1) return null;
            var serializerMimeTypeByte = serializerMimeTypeByteLength != -1 ? Reader.ReadBytes(serializerMimeTypeByteLength) : null;

            var dataLength = Reader.ReadInt32();
            if (dataLength < -1) return null;
            var data = dataLength != -1 ? Reader.ReadBytes(dataLength) : null;

            return new SerializedObject(data,
                dataTypeByte != null ? Encoding.UTF8.GetString(dataTypeByte) : null,
                serializerMimeTypeByte != null ? Encoding.UTF8.GetString(serializerMimeTypeByte) : null);
        }
    }
}