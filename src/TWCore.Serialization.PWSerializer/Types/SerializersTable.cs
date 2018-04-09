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
    public class SerializersTable
    {
        private readonly Dictionary<Type, ITypeSerializer> _typesCache = new Dictionary<Type, ITypeSerializer>();

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
        public SerializersTable()
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
        public void Write(Type serializerType, BinaryWriter bw, object value) 
        {
            if (serializerType == typeof(StringSerializer))
            {
                StringSerializer.WriteValue(bw, (string)value);
                return;
            }
            if (serializerType == typeof(NumberSerializer))
            {
                NumberSerializer.Write(bw, value);
                return;
            }
            if (serializerType == typeof(EnumSerializer))
            {
                EnumSerializer.Write(bw, value);
                return;
            }
            if (serializerType == typeof(DateTimeSerializer))
            {
                DateTimeSerializer.Write(bw, value);
                return;
            }
            if (serializerType == typeof(GuidSerializer))
            {
                GuidSerializer.Write(bw, value);
                return;
            }
            if (serializerType == typeof(SerializedObjectSerializer))
            {
                SerializedObjectSerializer.Write(bw, value);
                return;
            }
            if (serializerType == typeof(BoolSerializer))
            {
                BoolSerializer.Write(bw, value);
                return;
            }
            if (serializerType == typeof(TimeSpanSerializer))
            {
                TimeSpanSerializer.Write(bw, value);
                return;
            }
            if (serializerType == typeof(ByteArraySerializer))
            {
                ByteArraySerializer.Write(bw, value);
                return;
            }
            if (serializerType == typeof(CharSerializer))
            {
                CharSerializer.Write(bw, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITypeSerializer GetSerializerByValueType(Type type)
        {
            if (type == null)
                return null;
            if (_typesCache.TryGetValue(type, out var tSer))
                return tSer;
            if (NumberSerializer.CanWrite(type))
                tSer = NumberSerializer;
            else if (StringSerializer.CanWrite(type))
                tSer = StringSerializer;
            else if (GuidSerializer.CanWrite(type))
                tSer = GuidSerializer;
            else if (BoolSerializer.CanWrite(type))
                tSer = BoolSerializer;
            else if (DateTimeSerializer.CanWrite(type))
                tSer = DateTimeSerializer;
            else if (TimeSpanSerializer.CanWrite(type))
                tSer = TimeSpanSerializer;
            else if (EnumSerializer.CanWrite(type))
                tSer = EnumSerializer;
            else if (ByteArraySerializer.CanWrite(type))
                tSer = ByteArraySerializer;
            else if (SerializedObjectSerializer.CanWrite(type))
                tSer = SerializedObjectSerializer;
            else if (CharSerializer.CanWrite(type))
                tSer = CharSerializer;
            _typesCache[type] = tSer;
            return tSer;
        }
    }

    public class SerializersBag
    {
        public readonly NumberSerializer Number = new NumberSerializer();
        public readonly StringSerializer String = new StringSerializer();
        public readonly GuidSerializer Guid = new GuidSerializer();
        public readonly DateTimeSerializer DateTime = new DateTimeSerializer();
        public readonly TimeSpanSerializer TimeSpan = new TimeSpanSerializer();
        public readonly ByteArraySerializer ByteArray = new ByteArraySerializer();
        public readonly EnumSerializer Enum = new EnumSerializer();
        public readonly BoolSerializer Bool = new BoolSerializer();
        public readonly CharSerializer Char = new CharSerializer();
        public readonly SerializedObjectSerializer SerializedObject = new SerializedObjectSerializer();

        public SerializersBag()
        {
            Number.Init();
            String.Init();
            Guid.Init();
            DateTime.Init();
            TimeSpan.Init();
            ByteArray.Init();
            Enum.Init();
            Bool.Init();
            Char.Init();
            SerializedObject.Init();
        }

        public void Clear()
        {
            Number.Clear();
            String.Clear();
            Guid.Clear();
            DateTime.Clear();
            TimeSpan.Clear();
            ByteArray.Clear();
            Enum.Clear();
            Bool.Clear();
            Char.Clear();
            SerializedObject.Clear();
        }
    }
}
