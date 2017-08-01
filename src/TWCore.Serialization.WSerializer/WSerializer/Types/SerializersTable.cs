/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

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
using TWCore.Serialization.WSerializer.Types.ValueTypes;

namespace TWCore.Serialization.WSerializer.Types
{
    public class SerializersTable
    {
        #region Static Pool
        private static readonly ObjectPool<SerializersTable> CachedUShortTablePool = new ObjectPool<SerializersTable>(() => new SerializersTable(SerializerMode.CachedUShort), Init, 1, PoolResetMode.BeforeUse, 0);
        private static readonly ObjectPool<SerializersTable> Cached2048TablePool = new ObjectPool<SerializersTable>(() => new SerializersTable(SerializerMode.Cached2048), Init, 1, PoolResetMode.BeforeUse, 0);
        private static readonly ObjectPool<SerializersTable> Cached1024TablePool = new ObjectPool<SerializersTable>(() => new SerializersTable(SerializerMode.Cached1024), Init, 1, PoolResetMode.BeforeUse, 0);
        private static readonly ObjectPool<SerializersTable> Cached512TablePool = new ObjectPool<SerializersTable>(() => new SerializersTable(SerializerMode.Cached512), Init, 1, PoolResetMode.BeforeUse, 0);
        private static readonly ObjectPool<SerializersTable> NoCachedTablePool = new ObjectPool<SerializersTable>(() => new SerializersTable(SerializerMode.NoCached), Init, 1, PoolResetMode.BeforeUse, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializersTable GetTable(SerializerMode mode)
        {
            ObjectPool<SerializersTable> pool;
            switch (mode)
            {
                case SerializerMode.CachedUShort:
                    pool = CachedUShortTablePool;
                    break;
                case SerializerMode.Cached2048:
                    pool = Cached2048TablePool;
                    break;
                case SerializerMode.Cached1024:
                    pool = Cached1024TablePool;
                    break;
                case SerializerMode.Cached512:
                    pool = Cached512TablePool;
                    break;
                default:
                    pool = NoCachedTablePool;
                    break;
            }
            return pool.New();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReturnTable(SerializersTable table)
        {
            ObjectPool<SerializersTable> pool;
            switch (table.Mode)
            {
                case SerializerMode.CachedUShort:
                    pool = CachedUShortTablePool;
                    break;
                case SerializerMode.Cached2048:
                    pool = Cached2048TablePool;
                    break;
                case SerializerMode.Cached1024:
                    pool = Cached1024TablePool;
                    break;
                case SerializerMode.Cached512:
                    pool = Cached512TablePool;
                    break;
                default:
                    pool = NoCachedTablePool;
                    break;
            }
            pool.Store(table);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Init(SerializersTable table)
        {
            var mode = table.Mode;
            table.DateTimeSerializer.Init(mode);
            table.GuidSerializer.Init(mode);
            table.NumberSerializer.Init(mode);
            table.TimeSpanSerializer.Init(mode);
            table.ByteArraySerializer.Init(mode);
            table.StringSerializer.Init(mode);
        }
        #endregion

        private readonly Dictionary<Type, TypeSerializer> _typesCache = new Dictionary<Type, TypeSerializer>();

        public readonly SerializerMode Mode;
        public readonly NumberSerializer NumberSerializer = new NumberSerializer();
        public readonly StringSerializer StringSerializer = new StringSerializer();
        public readonly GuidSerializer GuidSerializer = new GuidSerializer();
        public readonly DateTimeSerializer DateTimeSerializer = new DateTimeSerializer();
        public readonly TimeSpanSerializer TimeSpanSerializer = new TimeSpanSerializer();
        public readonly ByteArraySerializer ByteArraySerializer = new ByteArraySerializer();
        public readonly DateTimeOffsetSerializer DateTimeOffsetSerializer = new DateTimeOffsetSerializer();

        public static readonly EnumSerializer EnumSerializer = new EnumSerializer();
        public static readonly BoolSerializer BoolSerializer = new BoolSerializer();
        public static readonly CharSerializer CharSerializer = new CharSerializer();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SerializersTable(SerializerMode mode)
        {
            Mode = mode;
            DateTimeOffsetSerializer.Init(mode);
            DateTimeSerializer.Init(mode);
            GuidSerializer.Init(mode);
            NumberSerializer.Init(mode);
            TimeSpanSerializer.Init(mode);
            ByteArraySerializer.Init(mode);
            StringSerializer.Init(mode);
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
            if (serializerType == typeof(GuidSerializer))
            {
                GuidSerializer.Write(bw, value);
                return;
            }
            if (serializerType == typeof(BoolSerializer))
            {
                BoolSerializer.Write(bw, value);
                return;
            }
            if (serializerType == typeof(DateTimeSerializer))
            {
                DateTimeSerializer.Write(bw, value);
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
                return;
            }
            if (serializerType == typeof(DateTimeOffsetSerializer))
            {
                DateTimeOffsetSerializer.Write(bw, value);
                return;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypeSerializer GetSerializerByValueType(Type type)
        {
            if (type == null)
                return null;
            if (!_typesCache.TryGetValue(type, out var tSer))
            {
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
                else if (CharSerializer.CanWrite(type))
                    tSer = CharSerializer;
                else if (DateTimeOffsetSerializer.CanWrite(type))
                    tSer = DateTimeOffsetSerializer;
                _typesCache[type] = tSer;
            }
            return tSer;
        }
    }

    public class DeserializersTable
    {
        #region Static Pool
        private static readonly ObjectPool<DeserializersTable> CachedUShortTablePool = new ObjectPool<DeserializersTable>(() => new DeserializersTable(SerializerMode.CachedUShort), Init, 1, PoolResetMode.BeforeUse, 0);
        private static readonly ObjectPool<DeserializersTable> Cached2048TablePool = new ObjectPool<DeserializersTable>(() => new DeserializersTable(SerializerMode.Cached2048), Init, 1, PoolResetMode.BeforeUse, 0);
        private static readonly ObjectPool<DeserializersTable> Cached1024TablePool = new ObjectPool<DeserializersTable>(() => new DeserializersTable(SerializerMode.Cached1024), Init, 1, PoolResetMode.BeforeUse, 0);
        private static readonly ObjectPool<DeserializersTable> Cached512TablePool = new ObjectPool<DeserializersTable>(() => new DeserializersTable(SerializerMode.Cached512), Init, 1, PoolResetMode.BeforeUse, 0);
        private static readonly ObjectPool<DeserializersTable> NoCachedTablePool = new ObjectPool<DeserializersTable>(() => new DeserializersTable(SerializerMode.NoCached), Init, 1, PoolResetMode.BeforeUse, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DeserializersTable GetTable(SerializerMode mode)
        {
            ObjectPool<DeserializersTable> pool;
            switch (mode)
            {
                case SerializerMode.CachedUShort:
                    pool = CachedUShortTablePool;
                    break;
                case SerializerMode.Cached2048:
                    pool = Cached2048TablePool;
                    break;
                case SerializerMode.Cached1024:
                    pool = Cached1024TablePool;
                    break;
                case SerializerMode.Cached512:
                    pool = Cached512TablePool;
                    break;
                default:
                    pool = NoCachedTablePool;
                    break;
            }
            return pool.New();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReturnTable(DeserializersTable table)
        {
            ObjectPool<DeserializersTable> pool;
            switch (table.Mode)
            {
                case SerializerMode.CachedUShort:
                    pool = CachedUShortTablePool;
                    break;
                case SerializerMode.Cached2048:
                    pool = Cached2048TablePool;
                    break;
                case SerializerMode.Cached1024:
                    pool = Cached1024TablePool;
                    break;
                case SerializerMode.Cached512:
                    pool = Cached512TablePool;
                    break;
                default:
                    pool = NoCachedTablePool;
                    break;
            }
            pool.Store(table);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Init(DeserializersTable table)
        {
            var mode = table.Mode;
            table.DateTimeSerializer.Init(mode);
            table.GuidSerializer.Init(mode);
            table.NumberSerializer.Init(mode);
            table.TimeSpanSerializer.Init(mode);
            table.ByteArraySerializer.Init(mode);
            table.StringSerializer.Init(mode);
        }
        #endregion

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
            foreach (var i in ByteArraySerializer.ReadTypes)
                DeserializerTypesCache[i] = typeof(ByteArraySerializer);
            foreach (var i in CharSerializer.ReadTypes)
                DeserializerTypesCache[i] = typeof(CharSerializer);
            foreach (var i in DateTimeOffsetSerializer.ReadTypes)
                DeserializerTypesCache[i] = typeof(DateTimeOffsetSerializer);
        }


        public readonly Dictionary<byte, TypeSerializer> _byteCache = new Dictionary<byte, TypeSerializer>();
        public readonly SerializerMode Mode;

        public readonly NumberSerializer NumberSerializer = new NumberSerializer();
        public readonly StringSerializer StringSerializer = new StringSerializer();
        public readonly GuidSerializer GuidSerializer = new GuidSerializer();
        public readonly DateTimeSerializer DateTimeSerializer = new DateTimeSerializer();
        public readonly TimeSpanSerializer TimeSpanSerializer = new TimeSpanSerializer();
        public readonly ByteArraySerializer ByteArraySerializer = new ByteArraySerializer();
        public readonly DateTimeOffsetSerializer DateTimeOffsetSerializer = new DateTimeOffsetSerializer();

        public static readonly EnumSerializer EnumSerializer = new EnumSerializer();
        public static readonly BoolSerializer BoolSerializer = new BoolSerializer();
        public static readonly CharSerializer CharSerializer = new CharSerializer();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DeserializersTable(SerializerMode mode)
        {
            Mode = mode;
            DateTimeOffsetSerializer.Init(mode);
            DateTimeSerializer.Init(mode);
            GuidSerializer.Init(mode);
            NumberSerializer.Init(mode);
            TimeSpanSerializer.Init(mode);
            ByteArraySerializer.Init(mode);
            StringSerializer.Init(mode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Read(BinaryReader br, byte currentByte)
            => GetSerializerByValueByte(currentByte).Read(br, currentByte);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypeSerializer GetSerializerByValueByte(byte value)
        {
            if (_byteCache.TryGetValue(value, out var tSer))
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
                else if (type == typeof(ByteArraySerializer)) tSer = ByteArraySerializer;
                else if (type == typeof(CharSerializer)) tSer = CharSerializer;
                else if (type == typeof(DateTimeOffsetSerializer)) tSer = DateTimeOffsetSerializer;
            }
            _byteCache[value] = tSer;
            return tSer;
        }

    }
}
