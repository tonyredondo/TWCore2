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
        public void WriteValue(Enum enumValue)
        {
            if (enumValue == null)
                WriteByte(DataBytesDefinition.ValueNull);
            else
                WriteDefInt(DataBytesDefinition.EnumInt, Convert.ToInt32(enumValue));
        }
    }




    public partial class DeserializersTable
    { 
        [DeserializerMethod(DataBytesDefinition.EnumInt, ReturnType = typeof(Enum))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadEnum(byte type)
        {
            if (type == DataBytesDefinition.EnumInt)
                return StreamReadInt();
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}