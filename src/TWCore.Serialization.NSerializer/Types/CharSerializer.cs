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
#pragma warning disable 1591

namespace TWCore.Serialization.NSerializer
{
    public partial class SerializersTable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(char value)
        {
            if (value == default(char))
                WriteByte(DataBytesDefinition.CharDefault);
            else
                WriteDefChar(DataBytesDefinition.Char, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(char? value)
        {
            if (value == null)
                WriteByte(DataBytesDefinition.ValueNull);
            else if (value == default(char))
                WriteByte(DataBytesDefinition.CharDefault);
            else
                WriteDefChar(DataBytesDefinition.Char, value.Value);
        }
    }




    public partial class DeserializersTable
    { 
        [DeserializerMethod(DataBytesDefinition.CharDefault, DataBytesDefinition.Char, ReturnType = typeof(char))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char StreamReadChar(byte value)
        {
            if (value == DataBytesDefinition.CharDefault)
                return default;
            if (value == DataBytesDefinition.Char)
                return StreamReadChar();
            throw new InvalidOperationException("Invalid type value.");
        }

        [DeserializerMethod(ReturnType = typeof(char?))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char? StreamReadCharNullable(byte value)
        {
            if (value == DataBytesDefinition.ValueNull)
                return null;
            if (value == DataBytesDefinition.CharDefault)
                return default;
            if (value == DataBytesDefinition.Char)
                return StreamReadChar();
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}