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
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using TWCore.Serialization.WSerializer.Types.ValueTypes;

namespace TWCore.Serialization.WSerializer.Types
{
    public class SerializersTable
    {
        #region Static Pool
        private static readonly ObjectPool<SerializersTable, TableUshortAllocator> CachedUShortTablePool = new ObjectPool<SerializersTable, TableUshortAllocator>();
        private static readonly ObjectPool<SerializersTable, Table2048Allocator> Cached2048TablePool = new ObjectPool<SerializersTable, Table2048Allocator>();
        private static readonly ObjectPool<SerializersTable, Table1024Allocator> Cached1024TablePool = new ObjectPool<SerializersTable, Table1024Allocator>();
        private static readonly ObjectPool<SerializersTable, Table512Allocator> Cached512TablePool = new ObjectPool<SerializersTable, Table512Allocator>();
        private static readonly ObjectPool<SerializersTable, TableNoCacheAllocator> NoCachedTablePool = new ObjectPool<SerializersTable, TableNoCacheAllocator>();

        #region Allocators
        private struct TableUshortAllocator : IPoolObjectLifecycle<SerializersTable>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SerializersTable New() => new SerializersTable(SerializerMode.CachedUShort);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(SerializersTable value) => Init(value);
        }
        private struct Table2048Allocator : IPoolObjectLifecycle<SerializersTable>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SerializersTable New() => new SerializersTable(SerializerMode.Cached2048);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(SerializersTable value) => Init(value);
        }
        private struct Table1024Allocator : IPoolObjectLifecycle<SerializersTable>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SerializersTable New() => new SerializersTable(SerializerMode.Cached1024);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(SerializersTable value) => Init(value);
        }
        private struct Table512Allocator : IPoolObjectLifecycle<SerializersTable>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SerializersTable New() => new SerializersTable(SerializerMode.Cached512);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(SerializersTable value) => Init(value);
        }
        private struct TableNoCacheAllocator : IPoolObjectLifecycle<SerializersTable>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SerializersTable New() => new SerializersTable(SerializerMode.NoCached);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(SerializersTable value) => Init(value);
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializersTable GetTable(SerializerMode mode)
        {
            switch (mode)
            {
                case SerializerMode.CachedUShort:
                    return CachedUShortTablePool.New();
                case SerializerMode.Cached2048:
                    return Cached2048TablePool.New();
                case SerializerMode.Cached1024:
                    return Cached1024TablePool.New();
                case SerializerMode.Cached512:
                    return Cached512TablePool.New();
                default:
                    return NoCachedTablePool.New();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReturnTable(SerializersTable table)
        {
            switch (table.Mode)
            {
                case SerializerMode.CachedUShort:
                    CachedUShortTablePool.Store(table);
                    break;
                case SerializerMode.Cached2048:
                    Cached2048TablePool.Store(table);
                    break;
                case SerializerMode.Cached1024:
                    Cached1024TablePool.Store(table);
                    break;
                case SerializerMode.Cached512:
                    Cached512TablePool.Store(table);
                    break;
                default:
                    NoCachedTablePool.Store(table);
                    break;
            }
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
        public static readonly SerializedObjectSerializer SerializedObjectSerializer = new SerializedObjectSerializer();

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
            if (serializerType == typeof(SerializedObjectSerializer))
            {
                SerializedObjectSerializer.Write(bw, value);
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
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypeSerializer GetSerializerByValueType(Type type)
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
            else if (SerializedObjectSerializer.CanWrite(type))
                tSer = SerializedObjectSerializer;
            else if (ByteArraySerializer.CanWrite(type))
                tSer = ByteArraySerializer;
            else if (CharSerializer.CanWrite(type))
                tSer = CharSerializer;
            else if (DateTimeOffsetSerializer.CanWrite(type))
                tSer = DateTimeOffsetSerializer;
            _typesCache[type] = tSer;
            return tSer;
        }
    }

    public class DeserializersTable
    {
        #region Static Pool
        private static readonly ObjectPool<DeserializersTable, TableUshortAllocator> CachedUShortTablePool = new ObjectPool<DeserializersTable, TableUshortAllocator>();
        private static readonly ObjectPool<DeserializersTable, Table2048Allocator> Cached2048TablePool = new ObjectPool<DeserializersTable, Table2048Allocator>();
        private static readonly ObjectPool<DeserializersTable, Table1024Allocator> Cached1024TablePool = new ObjectPool<DeserializersTable, Table1024Allocator>();
        private static readonly ObjectPool<DeserializersTable, Table512Allocator> Cached512TablePool = new ObjectPool<DeserializersTable, Table512Allocator>();
        private static readonly ObjectPool<DeserializersTable, TableNoCacheAllocator> NoCachedTablePool = new ObjectPool<DeserializersTable, TableNoCacheAllocator>();

        #region Allocators
        private struct TableUshortAllocator : IPoolObjectLifecycle<DeserializersTable>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public DeserializersTable New() => new DeserializersTable(SerializerMode.CachedUShort);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(DeserializersTable value) => Init(value);
        }
        private struct Table2048Allocator : IPoolObjectLifecycle<DeserializersTable>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public DeserializersTable New() => new DeserializersTable(SerializerMode.Cached2048);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(DeserializersTable value) => Init(value);
        }
        private struct Table1024Allocator : IPoolObjectLifecycle<DeserializersTable>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public DeserializersTable New() => new DeserializersTable(SerializerMode.Cached1024);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(DeserializersTable value) => Init(value);
        }
        private struct Table512Allocator : IPoolObjectLifecycle<DeserializersTable>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public DeserializersTable New() => new DeserializersTable(SerializerMode.Cached512);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(DeserializersTable value) => Init(value);
        }
        private struct TableNoCacheAllocator : IPoolObjectLifecycle<DeserializersTable>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public DeserializersTable New() => new DeserializersTable(SerializerMode.NoCached);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(DeserializersTable value) => Init(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Init(DeserializersTable table)
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DeserializersTable GetTable(SerializerMode mode)
        {
            switch (mode)
            {
                case SerializerMode.CachedUShort:
                    return CachedUShortTablePool.New();
                case SerializerMode.Cached2048:
                    return Cached2048TablePool.New();
                case SerializerMode.Cached1024:
                    return Cached1024TablePool.New();
                case SerializerMode.Cached512:
                    return Cached512TablePool.New();
                default:
                    return NoCachedTablePool.New();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReturnTable(DeserializersTable table)
        {
            switch (table.Mode)
            {
                case SerializerMode.CachedUShort:
                    CachedUShortTablePool.Store(table);
                    break;
                case SerializerMode.Cached2048:
                    Cached2048TablePool.Store(table);
                    break;
                case SerializerMode.Cached1024:
                    Cached1024TablePool.Store(table);
                    break;
                case SerializerMode.Cached512:
                    Cached512TablePool.Store(table);
                    break;
                default:
                    NoCachedTablePool.Store(table);
                    break;
            }
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
            foreach (var i in SerializedObjectSerializer.ReadTypes)
                DeserializerTypesCache[i] = typeof(SerializedObjectSerializer);
            foreach (var i in ByteArraySerializer.ReadTypes)
                DeserializerTypesCache[i] = typeof(ByteArraySerializer);
            foreach (var i in CharSerializer.ReadTypes)
                DeserializerTypesCache[i] = typeof(CharSerializer);
            foreach (var i in DateTimeOffsetSerializer.ReadTypes)
                DeserializerTypesCache[i] = typeof(DateTimeOffsetSerializer);
        }


        public readonly Dictionary<byte, TypeSerializer> ByteCache = new Dictionary<byte, TypeSerializer>();
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
        public static readonly SerializedObjectSerializer SerializedObjectSerializer = new SerializedObjectSerializer();

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
                else if (type == typeof(DateTimeOffsetSerializer)) tSer = DateTimeOffsetSerializer;
            }
            ByteCache[value] = tSer;
            return tSer;
        }

    }
}
