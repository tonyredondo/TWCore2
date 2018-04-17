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

namespace TWCore.Serialization.NSerializer
{
    public partial class SerializersTable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(bool value)
        {
            if (value)
                WriteByte(DataBytesDefinition.BoolTrue);
            else
                WriteByte(DataBytesDefinition.BoolFalse);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(bool? value)
        {
            if (value == null)
                WriteByte(DataBytesDefinition.ValueNull);
            else if (value.Value)
                WriteByte(DataBytesDefinition.BoolTrue);
            else
                WriteByte(DataBytesDefinition.BoolFalse);
        }
    }




    public partial class DeserializersTable
    {
        [DeserializerMethod(DataBytesDefinition.BoolTrue, DataBytesDefinition.BoolFalse, ReturnType = typeof(bool))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBool(byte value)
        {
            if (value == DataBytesDefinition.BoolTrue)
                return true;
            if (value == DataBytesDefinition.BoolFalse)
                return false;
            throw new InvalidOperationException("Invalid type value.");
        }

        [DeserializerMethod(ReturnType = typeof(bool?))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool? ReadBoolNullable(byte value)
        {
            if (value == DataBytesDefinition.ValueNull) return null;
            return ReadBool(value);
        }
    }
}