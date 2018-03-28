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
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using TWCore.Serialization.PWSerializer.Types.ValueTypes;

namespace TWCore.Serialization.PWSerializer.Types
{
    public class DeserializersTable
    {
        public static readonly Dictionary<byte, Type> DeserializerTypesCache = new Dictionary<byte, Type>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static DeserializersTable()
        {
            foreach (var i in NumberSerializer.ReadTypes)
                DeserializerTypesCache[i] = typeof(NumberSerializer);
            foreach (var i in StringSerializer.ReadTypes)
                DeserializerTypesCache[i] = typeof(StringSerializer);
            foreach (var i in EnumSerializer.ReadTypes)
                DeserializerTypesCache[i] = typeof(EnumSerializer);
            foreach (var i in GuidSerializer.ReadTypes)
                DeserializerTypesCache[i] = typeof(GuidSerializer);
            foreach (var i in BoolSerializer.ReadTypes)
                DeserializerTypesCache[i] = typeof(BoolSerializer);
            foreach (var i in DateTimeSerializer.ReadTypes)
                DeserializerTypesCache[i] = typeof(DateTimeSerializer);
            foreach (var i in TimeSpanSerializer.ReadTypes)
                DeserializerTypesCache[i] = typeof(TimeSpanSerializer);
            foreach (var i in SerializedObjectSerializer.ReadTypes)
                DeserializerTypesCache[i] = typeof(SerializedObjectSerializer);
            foreach (var i in ByteArraySerializer.ReadTypes)
                DeserializerTypesCache[i] = typeof(ByteArraySerializer);
            foreach (var i in CharSerializer.ReadTypes)
                DeserializerTypesCache[i] = typeof(CharSerializer);
        }


        public readonly Dictionary<byte, ITypeSerializer> ByteCache = new Dictionary<byte, ITypeSerializer>();

        public NumberSerializer NumberSerializer = new NumberSerializer();
        public StringSerializer StringSerializer = new StringSerializer();
        public GuidSerializer GuidSerializer = new GuidSerializer();
        public DateTimeSerializer DateTimeSerializer = new DateTimeSerializer();
        public TimeSpanSerializer TimeSpanSerializer = new TimeSpanSerializer();
        public ByteArraySerializer ByteArraySerializer = new ByteArraySerializer();
        public static EnumSerializer EnumSerializer = new EnumSerializer();
        public static BoolSerializer BoolSerializer = new BoolSerializer();
        public static CharSerializer CharSerializer = new CharSerializer();
        public static SerializedObjectSerializer SerializedObjectSerializer = new SerializedObjectSerializer();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DeserializersTable()
        {
            DateTimeSerializer.Init();
            GuidSerializer.Init();
            NumberSerializer.Init();
            TimeSpanSerializer.Init();
            ByteArraySerializer.Init();
            StringSerializer.Init();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            DateTimeSerializer.Clear();
            GuidSerializer.Clear();
            NumberSerializer.Clear();
            TimeSpanSerializer.Clear();
            ByteArraySerializer.Clear();
            StringSerializer.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Read(BinaryReader br, byte currentByte)
            => GetSerializerByValueByte(currentByte).Read(br, currentByte);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITypeSerializer GetSerializerByValueByte(byte value)
        {
            if (ByteCache.TryGetValue(value, out var tSer))
                return tSer;
            if (DeserializerTypesCache.TryGetValue(value, out var type))
            {
                if (type == typeof(NumberSerializer)) tSer = NumberSerializer;
                else if (type == typeof(StringSerializer)) tSer = StringSerializer;
                else if (type == typeof(EnumSerializer)) tSer = EnumSerializer;
                else if (type == typeof(GuidSerializer)) tSer = GuidSerializer;
                else if (type == typeof(BoolSerializer)) tSer = BoolSerializer;
                else if (type == typeof(DateTimeSerializer)) tSer = DateTimeSerializer;
                else if (type == typeof(TimeSpanSerializer)) tSer = TimeSpanSerializer;
                else if (type == typeof(SerializedObjectSerializer)) tSer = SerializedObjectSerializer;
                else if (type == typeof(ByteArraySerializer)) tSer = ByteArraySerializer;
                else if (type == typeof(CharSerializer)) tSer = CharSerializer;
            }
            ByteCache[value] = tSer;
            return tSer;
        }
    }
}
