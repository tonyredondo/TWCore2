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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using TWCore.Security;
using TWCore.Serialization.WSerializer.Deserializer;
using TWCore.Serialization.WSerializer.Serializer;
using TWCore.Serialization.WSerializer.Types;
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable ForCanBeConvertedToForeach

namespace TWCore.Serialization.WSerializer
{
    /// <summary>
    /// Wanhjör Serializer
    /// </summary>
    public class WSerializerCore
    {
        private static readonly Encoding DefaultUtf8Encoding = new UTF8Encoding(false);
        private static readonly DeserializerTypeDefinitionComparer TypeDefinitionComparer = new DeserializerTypeDefinitionComparer();
        private static readonly ConcurrentDictionary<Type, MultiArray<byte>> GlobalKnownTypes = new ConcurrentDictionary<Type, MultiArray<byte>>();
        private static readonly ConcurrentDictionary<MultiArray<byte>, Type> GlobalKnownTypesValues = new ConcurrentDictionary<MultiArray<byte>, Type>(MultiArrayBytesComparer.Instance);
        private static readonly IHash Hash = HashManager.Get("SHA1");
        private static readonly SerializerPlanItem[] EndPlan = { new SerializerPlanItem.WriteBytes(new[] { DataType.TypeEnd }) };
        
        private static readonly ObjectPool<CachePoolItem, CachePoolAllocator> CachePool = new ObjectPool<CachePoolItem, CachePoolAllocator>();
        private static readonly ConcurrentDictionary<Type, SerializerPlan> SerializationPlans = new ConcurrentDictionary<Type, SerializerPlan>();
        private static readonly ObjectPool<Stack<SerializerScope>, StackPoolAllocator> StackPool = new ObjectPool<Stack<SerializerScope>, StackPoolAllocator>();
        private static readonly ObjectPool<DesCachePoolItem, DesCachePoolAllocator> DCachePool = new ObjectPool<DesCachePoolItem, DesCachePoolAllocator>();
        private static readonly ObjectPool<Stack<DeserializerTypeItem>, DesStackPoolAllocator> DTypeStackPool = new ObjectPool<Stack<DeserializerTypeItem>, DesStackPoolAllocator>();
        private static readonly ConcurrentDictionary<(string, string, string), Type> DeserializationTypes = new ConcurrentDictionary<(string, string, string), Type>();
        private static readonly ConcurrentDictionary<Type, DeserializerTypeInfo> DeserializationTypeInfo = new ConcurrentDictionary<Type, DeserializerTypeInfo>();

        private readonly HashSet<Type> _knownTypes = new HashSet<Type>();
        private readonly HashSet<Type> _currentSerializerPlanTypes = new HashSet<Type>();
        private readonly byte[] _buffer = new byte[3];

        #region Allocators
        private struct CachePoolItem
        {
            public readonly SerializerCache<Type> TypeCache;
            public readonly SerializerCache<object> ObjectCache;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CachePoolItem(SerializerCache<Type> typeCache, SerializerCache<object> objectCache)
            {
                TypeCache = typeCache;
                ObjectCache = objectCache;
            }
        }
        private struct DesCachePoolItem
        {
            public readonly SerializerCache<DeserializerTypeDefinition> TypeCache;
            public readonly SerializerCache<object> ObjectCache;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public DesCachePoolItem(SerializerCache<DeserializerTypeDefinition> typeCache, SerializerCache<object> objectCache)
            {
                TypeCache = typeCache;
                ObjectCache = objectCache;
            }
        }
        
        private struct CachePoolAllocator : IPoolObjectLifecycle<CachePoolItem>
        {
            public int InitialSize => Environment.ProcessorCount;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CachePoolItem New() 
                => new CachePoolItem(new SerializerCache<Type>(SerializerMode.CachedUShort), new SerializerCache<object>(SerializerMode.CachedUShort));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(CachePoolItem value)
            {
                value.TypeCache.Clear(SerializerMode.CachedUShort);
                value.ObjectCache.Clear(SerializerMode.CachedUShort);
            }
            public int DropTimeFrequencyInSeconds => 60;
            public void DropAction(CachePoolItem value)
            {
            }
        }
        private struct StackPoolAllocator : IPoolObjectLifecycle<Stack<SerializerScope>>
        {
            public int InitialSize => Environment.ProcessorCount;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Stack<SerializerScope> New() => new Stack<SerializerScope>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(Stack<SerializerScope> value) => value.Clear();
            public int DropTimeFrequencyInSeconds => 60;
            public void DropAction(Stack<SerializerScope> value)
            {
            }
        }
        private struct DesCachePoolAllocator : IPoolObjectLifecycle<DesCachePoolItem>
        {
            public int InitialSize => Environment.ProcessorCount;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public DesCachePoolItem New() 
                => new DesCachePoolItem(new SerializerCache<DeserializerTypeDefinition>(SerializerMode.CachedUShort), new SerializerCache<object>(SerializerMode.CachedUShort));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(DesCachePoolItem value)
            {
                value.TypeCache.Clear(SerializerMode.CachedUShort);
                value.ObjectCache.Clear(SerializerMode.CachedUShort);
            }
            public int DropTimeFrequencyInSeconds => 60;
            public void DropAction(DesCachePoolItem value)
            {
            }
        }
        private struct DesStackPoolAllocator : IPoolObjectLifecycle<Stack<DeserializerTypeItem>>
        {
            public int InitialSize => Environment.ProcessorCount;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Stack<DeserializerTypeItem> New() => new Stack<DeserializerTypeItem>();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(Stack<DeserializerTypeItem> value) => value.Clear();
            public int DropTimeFrequencyInSeconds => 60;
            public void DropAction(Stack<DeserializerTypeItem> value)
            {
            }
        }
        #endregion
        
        private SerializerMode _mode = SerializerMode.Cached2048; 
        
        #region Properties
        /// <summary>
        /// Serializer Mode
        /// </summary>
        public SerializerMode Mode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _mode;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _mode = value;
        }
        #endregion

        #region .ctor
        /// <summary>
        /// Wanhjör Serializer
        /// </summary>
        public WSerializerCore() { }
        /// <summary>
        /// Wanhjör Serializer
        /// </summary>
        /// <param name="mode">Serializer mode</param>
        public WSerializerCore(SerializerMode mode)
        {
            Mode = mode;
        }
        #endregion

        #region Serializer

        #region Public Methods
        /// <summary>
        /// Serialize an object value in a Tony Wanhjor format
        /// </summary>
        /// <param name="stream">Stream where the data is going to be stored</param>
        /// <param name="value">Value to be serialized</param>
        /// <param name="type">Declared type of the value to be serialized</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Stream stream, object value, Type type = null)
        {
            SerializersTable serializersTable = null;
            Stack<SerializerScope> scopeStack = null;
            try
            {
                if (value != null)
                {
                    value = ResolveLinqEnumerables(value);
                    type = value?.GetType();
                }

                serializersTable = SerializersTable.GetTable(Mode);
                var stringSerializer = serializersTable.StringSerializer;
                var numberSerializer = serializersTable.NumberSerializer;
                var cacheTuple = CachePool.New();
                var typesCache = cacheTuple.TypeCache;
                var objectCache = cacheTuple.ObjectCache;

                var plan = GetSerializerPlan(serializersTable, type);
                scopeStack = StackPool.New();
                var scope = new SerializerScope(plan, value);
                scopeStack.Push(scope);

                var bw = new BinaryWriter(stream, DefaultUtf8Encoding, true);
                bw.Write(DataType.FileStart);
                bw.Write((byte) _mode);
                do
                {
                    #region Get the Current Scope

                    if (scope.Index >= scope.PlanLength)
                    {
                        scopeStack.Pop();
                        scope = (scopeStack.Count > 0) ? scopeStack.Peek() : null;
                        continue;
                    }

                    #endregion

                    var item = scope.Next();
                    //var tab = new string(' ', _scopeStack.Count * 4);
                    //Core.Log.Verbose(tab + "PlanType: " + item.PlanType);

                    #region Switch Plan Type

                    switch (item.PlanType)
                    {
                        #region WriteBytes

                        case SerializerPlanItemType.WriteBytes:
                            var bValues = item.ValueBytes;
                            //debugText += " = " + bValue;
                            bw.Write(bValues);
                            continue;

                        #endregion

                        #region TypeStart

                        case SerializerPlanItemType.TypeStart:
                            var oidx = objectCache.SerializerGet(scope.Value);
                            if (oidx > -1)
                            {
                                //debugText += " = Write ObjReference " + _oidx;

                                #region Object Reference

                                switch (oidx)
                                {
                                    case 0:
                                        bw.Write(DataType.RefObjectByte0);
                                        break;
                                    case 1:
                                        bw.Write(DataType.RefObjectByte1);
                                        break;
                                    case 2:
                                        bw.Write(DataType.RefObjectByte2);
                                        break;
                                    case 3:
                                        bw.Write(DataType.RefObjectByte3);
                                        break;
                                    case 4:
                                        bw.Write(DataType.RefObjectByte4);
                                        break;
                                    case 5:
                                        bw.Write(DataType.RefObjectByte5);
                                        break;
                                    case 6:
                                        bw.Write(DataType.RefObjectByte6);
                                        break;
                                    case 7:
                                        bw.Write(DataType.RefObjectByte7);
                                        break;
                                    case 8:
                                        bw.Write(DataType.RefObjectByte8);
                                        break;
                                    case 9:
                                        bw.Write(DataType.RefObjectByte9);
                                        break;
                                    case 10:
                                        bw.Write(DataType.RefObjectByte10);
                                        break;
                                    case 11:
                                        bw.Write(DataType.RefObjectByte11);
                                        break;
                                    case 12:
                                        bw.Write(DataType.RefObjectByte12);
                                        break;
                                    case 13:
                                        bw.Write(DataType.RefObjectByte13);
                                        break;
                                    case 14:
                                        bw.Write(DataType.RefObjectByte14);
                                        break;
                                    case 15:
                                        bw.Write(DataType.RefObjectByte15);
                                        break;
                                    case 16:
                                        bw.Write(DataType.RefObjectByte16);
                                        break;
                                    default:
                                        if (oidx <= byte.MaxValue)
                                            Write(bw, DataType.RefObjectByte, (byte) oidx);
                                        else
                                            Write(bw, DataType.RefObjectUShort, (ushort) oidx);
                                        break;
                                }

                                #endregion

                                scopeStack.Pop();
                                scope = (scopeStack.Count > 0) ? scopeStack.Peek() : null;
                            }
                            else
                            {
                                var tStartItem = (SerializerPlanItem.TypeStart) item;
                                if (!tStartItem.IsArray)
                                    objectCache.SerializerSet(scope.Value);

                                var typeIdx = typesCache.SerializerGet(tStartItem.Type);
                                if (typeIdx < 0)
                                {
                                    if (_knownTypes.Contains(tStartItem.Type))
                                    {
                                        bw.Write(DataType.KnownType);
                                        GlobalKnownTypes[tStartItem.Type].CopyTo(bw.BaseStream);
                                    }
                                    else
                                    {
                                        #region TypeStart write

                                        bw.Write(DataType.TypeStart);
                                        stringSerializer.WriteValue(bw, tStartItem.TypeAssembly);
                                        stringSerializer.WriteValue(bw, tStartItem.TypeNamespace);
                                        stringSerializer.WriteValue(bw, tStartItem.TypeName);
                                        numberSerializer.WriteValue(bw, tStartItem.Quantity);
                                        for (var i = 0; i < tStartItem.Quantity; i++)
                                        {
                                            switch (tStartItem.DTypes[i])
                                            {
                                                case DataType.Unknown:
                                                    stringSerializer.WriteValue(bw, tStartItem.TypeAssemblies[i]);
                                                    stringSerializer.WriteValue(bw, tStartItem.TypeNamespaces[i]);
                                                    stringSerializer.WriteValue(bw, tStartItem.TypeNames[i]);
                                                    break;
                                                default:
                                                    bw.Write(tStartItem.DTypes[i]);
                                                    break;
                                            }
                                        }

                                        numberSerializer.WriteValue(bw, tStartItem.Properties.Length);
                                        for (var i = 0; i < tStartItem.Properties.Length; i++)
                                            stringSerializer.WriteValue(bw, tStartItem.Properties[i]);

                                        #endregion
                                    }

                                    typesCache.SerializerSet(tStartItem.Type);
                                }
                                else
                                {
                                    switch (typeIdx)
                                    {
                                        case 0:
                                            bw.Write(DataType.TypeRefByte0);
                                            break;
                                        case 1:
                                            bw.Write(DataType.TypeRefByte1);
                                            break;
                                        case 2:
                                            bw.Write(DataType.TypeRefByte2);
                                            break;
                                        case 3:
                                            bw.Write(DataType.TypeRefByte3);
                                            break;
                                        case 4:
                                            bw.Write(DataType.TypeRefByte4);
                                            break;
                                        case 5:
                                            bw.Write(DataType.TypeRefByte5);
                                            break;
                                        case 6:
                                            bw.Write(DataType.TypeRefByte6);
                                            break;
                                        case 7:
                                            bw.Write(DataType.TypeRefByte7);
                                            break;
                                        case 8:
                                            bw.Write(DataType.TypeRefByte8);
                                            break;
                                        case 9:
                                            bw.Write(DataType.TypeRefByte9);
                                            break;
                                        case 10:
                                            bw.Write(DataType.TypeRefByte10);
                                            break;
                                        case 11:
                                            bw.Write(DataType.TypeRefByte11);
                                            break;
                                        case 12:
                                            bw.Write(DataType.TypeRefByte12);
                                            break;
                                        case 13:
                                            bw.Write(DataType.TypeRefByte13);
                                            break;
                                        case 14:
                                            bw.Write(DataType.TypeRefByte14);
                                            break;
                                        case 15:
                                            bw.Write(DataType.TypeRefByte15);
                                            break;
                                        case 16:
                                            bw.Write(DataType.TypeRefByte16);
                                            break;
                                        case 17:
                                            bw.Write(DataType.TypeRefByte17);
                                            break;
                                        case 18:
                                            bw.Write(DataType.TypeRefByte18);
                                            break;
                                        case 19:
                                            bw.Write(DataType.TypeRefByte19);
                                            break;
                                        case 20:
                                            bw.Write(DataType.TypeRefByte20);
                                            break;
                                        case 21:
                                            bw.Write(DataType.TypeRefByte21);
                                            break;
                                        case 22:
                                            bw.Write(DataType.TypeRefByte22);
                                            break;
                                        case 23:
                                            bw.Write(DataType.TypeRefByte23);
                                            break;
                                        case 24:
                                            bw.Write(DataType.TypeRefByte24);
                                            break;
                                        default:
                                            if (typeIdx <= byte.MaxValue)
                                                Write(bw, DataType.TypeRefByte, (byte) typeIdx);
                                            else
                                                Write(bw, DataType.TypeRefUShort, (ushort) typeIdx);
                                            break;
                                    }
                                }

                                //debugText += " = {0} {1},{2} [{3}]".ApplyFormat(tStartItem.TypeNamespace, tStartItem.TypeName, tStartItem.TypeAssembly, tStartItem.TypeNames.Join(","));
                            }

                            continue;

                        #endregion

                        #region ListStart

                        case SerializerPlanItemType.ListStart:
                            if (scope.Value != null)
                            {
                                var lType = (SerializerPlanItem.ListStart) item;
                                var iList = (IList) scope.Value;
                                var iListCount = iList.Count;
                                if (iListCount > 0)
                                {
                                    bw.Write(DataType.ListStart);
                                    var valueTypeSerializer =
                                        serializersTable.GetSerializerByValueType(lType.InnerType);
                                    if (valueTypeSerializer != null)
                                    {
                                        for (var i = 0; i < iListCount; i++)
                                        {
                                            var itemList = iList[i];
                                            if (itemList == null)
                                                bw.Write(DataType.ValueNull);
                                            else
                                                valueTypeSerializer.Write(bw, itemList);
                                        }
                                    }
                                    else if (iListCount == 1)
                                    {
                                        if (iList[0] == null)
                                            bw.Write(DataType.ValueNull);
                                        else
                                        {
                                            var itemList = iList[0];
                                            valueTypeSerializer =
                                                serializersTable.GetSerializerByValueType(itemList.GetType());
                                            if (valueTypeSerializer != null)
                                            {
                                                valueTypeSerializer.Write(bw, itemList);
                                            }
                                            else
                                            {
                                                var aPlan = new SerializerPlanItem.RuntimeValue[1];
                                                itemList = ResolveLinqEnumerables(itemList);
                                                var itemType = itemList?.GetType() ?? lType.InnerType;
                                                aPlan[0] = new SerializerPlanItem.RuntimeValue(itemType,
                                                    itemType != lType.InnerType
                                                        ? serializersTable.GetSerializerByValueType(itemType)?.GetType()
                                                        : null, itemList);
                                                scope = new SerializerScope(aPlan, scope.Type);
                                                scopeStack.Push(scope);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (lType.InnerType == typeof(object) && iList[0] != null)
                                        {
                                            var canValueType = true;
                                            var vType = iList[0].GetType();
                                            valueTypeSerializer = serializersTable.GetSerializerByValueType(vType);
                                            if (valueTypeSerializer != null)
                                            {
                                                for (var i = 1; i < iListCount; i++)
                                                {
                                                    var itemList = iList[i];
                                                    if (itemList != null && itemList.GetType() == vType) continue;
                                                    canValueType = false;
                                                    break;
                                                }
                                            }
                                            else
                                                canValueType = false;

                                            if (canValueType)
                                            {
                                                for (var i = 0; i < iListCount; i++)
                                                {
                                                    var itemList = iList[i];
                                                    if (itemList == null)
                                                        bw.Write(DataType.ValueNull);
                                                    else
                                                        valueTypeSerializer.Write(bw, itemList);
                                                }
                                            }
                                            else
                                            {
                                                var aPlan = new SerializerPlanItem.RuntimeValue[iListCount];
                                                for (var i = 0; i < iListCount; i++)
                                                {
                                                    var itemList = iList[i];
                                                    if (itemList != null)
                                                        itemList = ResolveLinqEnumerables(itemList);
                                                    var itemType = itemList?.GetType() ?? lType.InnerType;
                                                    aPlan[i] = new SerializerPlanItem.RuntimeValue(itemType,
                                                        itemType != lType.InnerType
                                                            ? serializersTable.GetSerializerByValueType(itemType)
                                                                ?.GetType()
                                                            : null, itemList);
                                                }

                                                scope = new SerializerScope(aPlan, scope.Type);
                                                scopeStack.Push(scope);
                                            }
                                        }
                                        else
                                        {
                                            var aPlan = new SerializerPlanItem.RuntimeValue[iListCount];
                                            for (var i = 0; i < iListCount; i++)
                                            {
                                                var itemList = iList[i];
                                                if (itemList != null)
                                                    itemList = ResolveLinqEnumerables(itemList);
                                                var itemType = itemList?.GetType() ?? lType.InnerType;
                                                aPlan[i] = new SerializerPlanItem.RuntimeValue(itemType,
                                                    itemType != lType.InnerType
                                                        ? serializersTable.GetSerializerByValueType(itemType)?.GetType()
                                                        : null, itemList);
                                            }

                                            scope = new SerializerScope(aPlan, scope.Type);
                                            scopeStack.Push(scope);
                                        }
                                    }
                                }
                                else
                                {
                                    scope.ChangeScopePlan(EndPlan);
                                }
                            }

                            continue;

                        #endregion

                        #region DictionaryStart

                        case SerializerPlanItemType.DictionaryStart:
                            if (scope.Value != null)
                            {
                                var dictioItem = (SerializerPlanItem.DictionaryStart) item;
                                var iDictio = (IDictionary) scope.Value;
                                if (iDictio.Count > 0)
                                {
                                    bw.Write(DataType.DictionaryStart);
                                    if (dictioItem.KeySerializerType != null && dictioItem.ValueSerializerType != null)
                                    {
                                        foreach (var keyValue in iDictio.Keys)
                                        {
                                            if (keyValue == null)
                                                bw.Write(DataType.ValueNull);
                                            else
                                                serializersTable.Write(dictioItem.KeySerializerType, bw, keyValue);

                                            var valueValue = iDictio[keyValue];
                                            if (valueValue == null)
                                                bw.Write(DataType.ValueNull);
                                            else
                                                serializersTable.Write(dictioItem.ValueSerializerType, bw, valueValue);
                                        }
                                    }
                                    else
                                    {
                                        var aPlan = new SerializerPlanItem.RuntimeValue[iDictio.Count * 2];
                                        var aIdx = 0;
                                        foreach (var keyValue in iDictio.Keys)
                                        {
                                            var kv = ResolveLinqEnumerables(keyValue);
                                            var valueValue = iDictio[keyValue];
                                            valueValue = ResolveLinqEnumerables(valueValue);
                                            aPlan[aIdx++] = new SerializerPlanItem.RuntimeValue(
                                                kv?.GetType() ?? dictioItem.KeyType, dictioItem.KeySerializerType, kv);
                                            aPlan[aIdx++] = new SerializerPlanItem.RuntimeValue(
                                                valueValue?.GetType() ?? dictioItem.ValueType,
                                                dictioItem.ValueSerializerType, valueValue);
                                        }

                                        scope = new SerializerScope(aPlan, scope.Type);
                                        scopeStack.Push(scope);
                                    }
                                }
                                else
                                {
                                    scope.ChangeScopePlan(EndPlan);
                                }
                            }

                            continue;

                        #endregion

                        #region KeyValueStart

                        case SerializerPlanItemType.KeyValueStart:
                            if (scope.Value != null)
                            {
                                var kvpItem = (SerializerPlanItem.KeyValueStart) item;
                                bw.Write(DataType.KeyValueStart);
                                if (kvpItem.KeyDType != DataType.Unknown)
                                    bw.Write(kvpItem.KeyDType);
                                else
                                {
                                    stringSerializer.WriteValue(bw, kvpItem.KeyTypeAssembly);
                                    stringSerializer.WriteValue(bw, kvpItem.KeyTypeNamespace);
                                    stringSerializer.WriteValue(bw, kvpItem.KeyTypeName);
                                }

                                if (kvpItem.ValueDType != DataType.Unknown)
                                    bw.Write(kvpItem.ValueDType);
                                else
                                {
                                    stringSerializer.WriteValue(bw, kvpItem.ValueTypeAssembly);
                                    stringSerializer.WriteValue(bw, kvpItem.ValueTypeNamespace);
                                    stringSerializer.WriteValue(bw, kvpItem.ValueTypeName);
                                }

                                //debugText += " = {0},{1} | {2},{3}".ApplyFormat(kvpItem.KeyTypeName, kvpItem.KeyTypeAssembly, kvpItem.ValueTypeName, kvpItem.ValueTypeAssembly);
                                var keyVal = kvpItem.Key.GetValue(scope.Value);
                                var valueVal = kvpItem.Value.GetValue(scope.Value);
                                keyVal = ResolveLinqEnumerables(keyVal);
                                valueVal = ResolveLinqEnumerables(valueVal);
                                var aPlan = new SerializerPlanItem.RuntimeValue[2];
                                aPlan[0] = new SerializerPlanItem.RuntimeValue(keyVal?.GetType() ?? kvpItem.KeyType,
                                    kvpItem.KeySerializerType, keyVal);
                                aPlan[1] = new SerializerPlanItem.RuntimeValue(valueVal?.GetType() ?? kvpItem.ValueType,
                                    kvpItem.ValueSerializerType, valueVal);
                                scope = new SerializerScope(aPlan, scope.Type);
                                scopeStack.Push(scope);
                            }

                            continue;

                        #endregion

                        #region TupleStart

                        case SerializerPlanItemType.TupleStart:
                            if (scope.Value != null)
                            {
                                var tupleItem = (SerializerPlanItem.TupleStart) item;
                                var length = tupleItem.Quantity;
                                bw.Write(DataType.TupleStart);
                                var aPlan = new SerializerPlanItem.RuntimeValue[length];
                                for (var i = 0; i < length; i++)
                                {
                                    var val = tupleItem.Props[i].GetValue(scope.Value);
                                    val = ResolveLinqEnumerables(val);
                                    var ser = tupleItem.SerializerTypes[i];
                                    aPlan[i] = new SerializerPlanItem.RuntimeValue(val?.GetType() ?? tupleItem.Types[i],
                                        ser, val);
                                }

                                scope = new SerializerScope(aPlan, scope.Type);
                                scopeStack.Push(scope);
                            }

                            continue;

                        #endregion

                        #region PropertyValue

                        case SerializerPlanItemType.PropertyValue:
                            var cItem = (SerializerPlanItem.PropertyValue) item;
                            //debugText += " = " + cItem.Name + ", " + cItem.Type;
                            var pVal = cItem.Property.GetValue(scope.Value);
                            if (pVal == cItem.DefaultValue)
                                bw.Write(DataType.ValueNull);
                            else
                                serializersTable.Write(cItem.SerializerType, bw, pVal);
                            continue;

                        #endregion

                        #region PropertyReference

                        case SerializerPlanItemType.PropertyReference:
                            var rItem = (SerializerPlanItem.PropertyReference) item;
                            //debugText += " = " + rItem.Name;
                            var rVal = rItem.Property.GetValue(scope.Value);
                            if (rVal == null)
                                bw.Write(DataType.ValueNull);
                            else
                            {
                                rVal = ResolveLinqEnumerables(rVal);
                                scope = new SerializerScope(
                                    GetSerializerPlan(serializersTable, rVal?.GetType() ?? rItem.Type), rVal);
                                scopeStack.Push(scope);
                            }

                            continue;

                        #endregion

                        #region Value

                        case SerializerPlanItemType.Value:
                            var vItem = (SerializerPlanItem.ValueItem) item;
                            //debugText += " (" + vItem.Type + ")";
                            if (scope.Value == null)
                                bw.Write(DataType.ValueNull);
                            else if (vItem.SerializerType != null)
                                serializersTable.Write(vItem.SerializerType, bw, scope.Value);
                            else
                            {
                                scope = new SerializerScope(
                                    GetSerializerPlan(serializersTable, scope.Value?.GetType() ?? vItem.Type),
                                    scope.Value);
                                scopeStack.Push(scope);
                            }

                            continue;

                        #endregion

                        #region RuntimeValue

                        case SerializerPlanItemType.RuntimeValue:
                            var rvItem = (SerializerPlanItem.RuntimeValue) item;
                            //debugText += " (" + rvItem.Type + ")";
                            if (rvItem.Value == null)
                                bw.Write(DataType.ValueNull);
                            else if (rvItem.SerializerType != null)
                                serializersTable.Write(rvItem.SerializerType, bw, rvItem.Value);
                            else
                            {
                                scope = new SerializerScope(
                                    GetSerializerPlan(serializersTable, rvItem.Value?.GetType() ?? rvItem.Type),
                                    rvItem.Value);
                                scopeStack.Push(scope);
                            }

                            continue;

                        #endregion
                    }

                    #endregion

                } while (scope != null);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                throw;
            }
            finally
            {
                SerializersTable.ReturnTable(serializersTable);
                StackPool.Store(scopeStack);
            }
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(BinaryWriter bw, byte type, byte value)
        {
            _buffer[0] = type;
            _buffer[1] = value;
            bw.Write(_buffer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(BinaryWriter bw, byte type, ushort value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            bw.Write(_buffer, 0, 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SerializerPlan GetSerializerPlan(SerializersTable serializerTable, Type type)
        {
            return SerializationPlans.GetOrAdd(type, mType =>
            {
                var plan = new List<SerializerPlanItem>();
                var typeInfo = mType.GetTypeInfo();
                var genTypeDefinition = typeInfo.IsGenericType ? typeInfo.GetGenericTypeDefinition() : null;
                var serBase = serializerTable.GetSerializerByValueType(mType);
                var isIList = false;
                var isIDictionary = false;

                if (serBase != null)
                {
                    //Value type
                    plan.Add(new SerializerPlanItem.ValueItem(mType, serBase.GetType()));
                }
                else if (genTypeDefinition == typeof(Nullable<>))
                {
                    //Nullable type
                    mType = Nullable.GetUnderlyingType(mType);
                    serBase = serializerTable.GetSerializerByValueType(mType);
                    plan.Add(new SerializerPlanItem.ValueItem(mType, serBase.GetType()));
                }
                else if (genTypeDefinition == typeof(Tuple<>) ||
                        genTypeDefinition == typeof(Tuple<,>) ||
                        genTypeDefinition == typeof(Tuple<,,>) ||
                        genTypeDefinition == typeof(Tuple<,,,>) ||
                        genTypeDefinition == typeof(Tuple<,,,,>) ||
                        genTypeDefinition == typeof(Tuple<,,,,,>) ||
                        genTypeDefinition == typeof(Tuple<,,,,,,>))
                {
                    //Tuple type
                    var types = typeInfo.GenericTypeArguments;
                    var serTypes = new Type[types.Length];
                    for (var i = 0; i < types.Length; i++)
                    {
                        var tInfo = types[i].GetTypeInfo();
                        var isNullable = tInfo.IsGenericType && tInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
                        var ser = isNullable ? serializerTable.GetSerializerByValueType(Nullable.GetUnderlyingType(types[i])) : serializerTable.GetSerializerByValueType(types[i]);
                        serTypes[i] = ser?.GetType();
                    }
                    var tStart = new SerializerPlanItem.TypeStart(mType, typeInfo)
                    {
                        Properties = new string[0]
                    };
                    plan.Add(tStart);
                    plan.Add(new SerializerPlanItem.TupleStart(mType, types, serTypes));
                    plan.Add(new SerializerPlanItem.WriteBytes(new[] { DataType.TupleEnd }));
                }
                else if (genTypeDefinition == typeof(KeyValuePair<,>))
                {
                    //KeyValye Type
                    var types = typeInfo.GenericTypeArguments;
                    var keyType = types[0];
                    var keyTypeInfo = keyType.GetTypeInfo();
                    var keyIsNullable = keyTypeInfo.IsGenericType && keyTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
                    var keySer = keyIsNullable ? serializerTable.GetSerializerByValueType(Nullable.GetUnderlyingType(keyType)) : serializerTable.GetSerializerByValueType(keyType);

                    var valueType = types[1];
                    var valueTypeInfo = valueType.GetTypeInfo();
                    var valueIsNullable = valueTypeInfo.IsGenericType && valueTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
                    var valueSer = valueIsNullable ? serializerTable.GetSerializerByValueType(Nullable.GetUnderlyingType(valueType)) : serializerTable.GetSerializerByValueType(valueType);

                    if (keySer == null && !_currentSerializerPlanTypes.Contains(keyType))
                        GetSerializerPlan(serializerTable, keyType);
                    if (valueSer == null && !_currentSerializerPlanTypes.Contains(valueType))
                        GetSerializerPlan(serializerTable, valueType);

                    plan.Add(new SerializerPlanItem.KeyValueStart(mType, keyType, keySer?.GetType(), keyIsNullable, valueType, valueSer?.GetType(), valueIsNullable));
                    plan.Add(new SerializerPlanItem.WriteBytes(new[] { DataType.KeyValueEnd }));
                }
                else
                {
                    _currentSerializerPlanTypes.Add(mType);
                    var tStart = new SerializerPlanItem.TypeStart(mType, typeInfo);
                    plan.Add(tStart);
                    var endBytes = new List<byte>();

                    if (typeInfo.ImplementedInterfaces.Any(i => i == typeof(IList) || i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
                        isIList = true;
                    if (typeInfo.ImplementedInterfaces.Any(i => i == typeof(IDictionary) || i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                        isIDictionary = true;

                    #region Properties
                    var properties = type.GetRuntimeProperties().OrderBy(n => n.Name).ToArray();
                    var propNames = new List<string>();
                    foreach (var prop in properties)
                    {
                        if (!prop.CanRead || !prop.CanWrite) continue;
                        if (isIList && prop.Name == "Capacity")
                            continue;
                        if (prop.GetAttribute<NonSerializeAttribute>() != null)
                            continue;
                        if (prop.GetIndexParameters().Length > 0)
                            continue;
                        var propType = prop.PropertyType;
                        var propTypeInfo = propType.GetTypeInfo();
                        var propIsNullable = propTypeInfo.IsGenericType && propTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
                        if (propIsNullable)
                            propType = Nullable.GetUnderlyingType(propType);
                        var serType = serializerTable.GetSerializerByValueType(propType)?.GetType();
                        propNames.Add(prop.Name);
                        if (serType == null)
                        {
                            plan.Add(new SerializerPlanItem.PropertyReference(prop));
                            if (!_currentSerializerPlanTypes.Contains(propType))
                                GetSerializerPlan(serializerTable, propType);
                        }
                        else
                            plan.Add(new SerializerPlanItem.PropertyValue(prop, serType, propIsNullable));
                    }
                    tStart.Properties = propNames.ToArray();
                    #endregion

                    #region ListInfo
                    if (isIList)
                    {
                        var ifaces = typeInfo.ImplementedInterfaces;
                        var ilist = ifaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
                        if (ilist != null)
                        {
                            Type innerType = null;
                            if (type.IsArray)
                                innerType = type.GetElementType();
                            else
                            {
                                var gargs = ilist.GenericTypeArguments;
                                if (gargs.Length == 0)
                                    gargs = type.GenericTypeArguments;
                                if (gargs.Length > 0)
                                    innerType = gargs[0];
                                else
                                {
                                    var iListType = typeInfo.ImplementedInterfaces.FirstOrDefault(m => (m.IsGenericType && m.GetGenericTypeDefinition() == typeof(IList<>)));
                                    if (iListType != null && iListType.GenericTypeArguments.Length > 0)
                                        innerType = iListType.GenericTypeArguments[0];
                                }
                            }
                            plan.Add(new SerializerPlanItem.ListStart(type, innerType));
                            if (!_currentSerializerPlanTypes.Contains(innerType))
                                GetSerializerPlan(serializerTable, innerType);
                            endBytes.Add(DataType.ListEnd);
                        }
                    }
                    #endregion

                    #region DictionaryInfo
                    if (isIDictionary)
                    {
                        var ifaces = typeInfo.ImplementedInterfaces;
                        var idictio = ifaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
                        if (idictio != null)
                        {
                            //KeyValye Type
                            var types = idictio.GenericTypeArguments;
                            var keyType = types[0];
                            var keyTypeInfo = keyType.GetTypeInfo();
                            var keyIsNullable = keyTypeInfo.IsGenericType && keyTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
                            var keySer = keyIsNullable ? serializerTable.GetSerializerByValueType(Nullable.GetUnderlyingType(keyType)) : serializerTable.GetSerializerByValueType(keyType);

                            var valueType = types[1];
                            var valueTypeInfo = valueType.GetTypeInfo();
                            var valueIsNullable = valueTypeInfo.IsGenericType && valueTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
                            var valueSer = valueIsNullable ? serializerTable.GetSerializerByValueType(Nullable.GetUnderlyingType(valueType)) : serializerTable.GetSerializerByValueType(valueType);

                            if (keySer == null && !_currentSerializerPlanTypes.Contains(keyType))
                                GetSerializerPlan(serializerTable, keyType);
                            if (valueSer == null && !_currentSerializerPlanTypes.Contains(valueType))
                                GetSerializerPlan(serializerTable, valueType);

                            plan.Add(new SerializerPlanItem.DictionaryStart(type, keyType, keySer?.GetType(), keyIsNullable, valueType, valueSer?.GetType(), valueIsNullable));
                            endBytes.Add(DataType.DictionaryEnd);
                        }
                    }
                    #endregion

                    endBytes.Add(DataType.TypeEnd);
                    plan.Add(new SerializerPlanItem.WriteBytes(endBytes.ToArray()));
                    _currentSerializerPlanTypes.Remove(mType);
                }
                return new SerializerPlan(plan.ToArray(), mType, isIList, isIDictionary);
            });
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object ResolveLinqEnumerables(object value)
        {
            if (value is IEnumerable iEValue && !(iEValue is string))
                value = iEValue.Enumerate();
            return value;
        }
        #endregion

        #endregion

        #region Deserializer

        #region Public Methods
        /// <summary>
        /// Deserialize a Tony Wanhjor stream into a object value
        /// </summary>
        /// <param name="stream">Stream where the data is going to be readed (source data)</param>
        /// <param name="type">Declared type of the value to be serialized</param>
        /// <returns>Deserialized object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Deserialize(Stream stream, Type type = null)
        {
            type = type ?? typeof(object);
            var fStart = stream.ReadByte();
            if (fStart != DataType.FileStart)
                throw new FormatException(string.Format("The stream is not in WBinary format. Byte {0} was expected, received: {1}", DataType.FileStart, fStart));
            
            var br = new BinaryReader(stream, DefaultUtf8Encoding, true);
            var bMode = br.ReadByte();
            var sMode = (SerializerMode)bMode;
            var serializersTable = DeserializersTable.GetTable(sMode);
            var dCache = DCachePool.New();
            var typesCache = dCache.TypeCache;
            var objectCache = dCache.ObjectCache;
            var stringSerializer = serializersTable.StringSerializer;
            var numberSerializer = serializersTable.NumberSerializer;

            var typeStack = DTypeStackPool.New();
            DeserializerTypeItem typeItem = null;
            //var idx = 0;
            do
            {
                //if (idx == 1261)
                //    System.Diagnostics.Debugger.Break();
                //idx++;
                var currentByte = br.ReadByte();
                switch (currentByte)
                {
                    #region TypeStart
                    case DataType.TypeStart:
                        #region Get Type Definition
                        var typeAssembly = stringSerializer.ReadValue(br);
                        var typeNamespace = stringSerializer.ReadValue(br);
                        var typeName = stringSerializer.ReadValue(br);
                        var typeQuantity = numberSerializer.ReadValue(br);
                        var typeType = GetDeserializationType((typeAssembly, typeNamespace, typeName));
                        if (typeQuantity > 0)
                        {
                            var gTypes = new Type[typeQuantity];
                            var someNull = false;
                            for (var i = 0; i < typeQuantity; i++)
                            {
                                gTypes[i] = DeserializeType(br, stringSerializer, br.ReadByte());
                                if (gTypes[i] == null)
                                    someNull = true;
                            }
                            if (!someNull)
                                typeType = typeType.MakeGenericType(gTypes);
                        }
                        if (typeType == null)
                            throw new NullReferenceException($"The type can't be found: {typeNamespace}.{typeName} from assembly: {typeAssembly}");
                        var typePropertiesLength = numberSerializer.ReadValue(br);
                        var typeProperties = new string[typePropertiesLength];
                        for (var i = 0; i < typePropertiesLength; i++)
                            typeProperties[i] = stringSerializer.ReadValue(br);
                        var typeDefinition = new DeserializerTypeDefinition(typeType, GetDeserializationTypeInfo(typeType), typeProperties);
                        typesCache.DeserializerSet(typeDefinition);
                        #endregion

                        typeItem = CreateDeserializerNewTypeItem(objectCache, typeDefinition);
                        typeStack.Push(typeItem);
                        continue;
                    case DataType.KnownType:
                        var hashBytes = br.ReadBytes(20);
                        if (GlobalKnownTypesValues.TryGetValue(hashBytes, out var hashType))
                        {
                            var tDef = new DeserializerTypeDefinition(hashType, GetDeserializationTypeInfo(hashType), GetPropertiesFromType(hashType));
                            typesCache.DeserializerSet(tDef);
                            typeItem = CreateDeserializerNewTypeItem(objectCache, tDef);
                            typeStack.Push(typeItem);
                        }
                        else
                            throw new KeyNotFoundException("The Serializer data uses a KnownType that can't be found on the deserializer");
                        continue;
                    case DataType.TypeRefByte0:
                    case DataType.TypeRefByte1:
                    case DataType.TypeRefByte2:
                    case DataType.TypeRefByte3:
                    case DataType.TypeRefByte4:
                    case DataType.TypeRefByte5:
                    case DataType.TypeRefByte6:
                    case DataType.TypeRefByte7:
                    case DataType.TypeRefByte8:
                    case DataType.TypeRefByte9:
                    case DataType.TypeRefByte10:
                    case DataType.TypeRefByte11:
                    case DataType.TypeRefByte12:
                    case DataType.TypeRefByte13:
                    case DataType.TypeRefByte14:
                    case DataType.TypeRefByte15:
                    case DataType.TypeRefByte16:
                    case DataType.TypeRefByte17:
                    case DataType.TypeRefByte18:
                    case DataType.TypeRefByte19:
                    case DataType.TypeRefByte20:
                    case DataType.TypeRefByte21:
                    case DataType.TypeRefByte22:
                    case DataType.TypeRefByte23:
                    case DataType.TypeRefByte24:
                        var byteIdx = currentByte - DataType.TypeRefByte0;
                        typeItem = CreateDeserializerNewTypeItem(objectCache, typesCache.DeserializerGet(byteIdx));
                        typeStack.Push(typeItem);
                        continue;
                    case DataType.TypeRefByte:
                        var refByteVal = br.ReadByte();
                        typeItem = CreateDeserializerNewTypeItem(objectCache, typesCache.DeserializerGet(refByteVal));
                        typeStack.Push(typeItem);
                        continue;
                    case DataType.TypeRefUShort:
                        var refUshoftVal = br.ReadUInt16();
                        typeItem = CreateDeserializerNewTypeItem(objectCache, typesCache.DeserializerGet(refUshoftVal));
                        typeStack.Push(typeItem);
                        continue;
                    #endregion

                    #region TypeEnd
                    case DataType.TypeEnd:
                        var lastObject = typeItem.Value;
                        typeStack.Pop();
                        if (typeStack.Count > 0)
                        {
                            typeItem = typeStack.Peek();
                            ProcessTypeEnd(typeItem, lastObject);
                        }
                        else
                        {
                            DeserializersTable.ReturnTable(serializersTable);
                            DCachePool.Store(dCache);
                            DTypeStackPool.Store(typeStack);
                            return DataTypeHelper.Change(lastObject, type);
                        }
                        continue;
                    #endregion

                    #region KeyValueStart
                    case DataType.KeyValueStart:
                        var kType = DeserializeType(br, stringSerializer, br.ReadByte());
                        var vType = DeserializeType(br, stringSerializer, br.ReadByte());
                        var kvTypeItem = new DeserializerTypeItem(typeof(KeyValuePair<,>).MakeGenericType(kType, vType))
                        {
                            Items = new List<object>(),
                            Last = currentByte
                        };
                        typeStack.Push(kvTypeItem);
                        typeItem = kvTypeItem;
                        continue;
                    #endregion

                    #region KeyValueEnd
                    case DataType.KeyValueEnd:
                        var keyValue = Activator.CreateInstance(typeItem.Type, typeItem.Items[0], typeItem.Items[1]);
                        typeItem.Last = 0;
                        typeStack.Pop();
                        if (typeStack.Count > 0)
                        {
                            typeItem = typeStack.Peek();
                            ProcessTypeEnd(typeItem, keyValue);
                        }
                        else
                        {
                            DeserializersTable.ReturnTable(serializersTable);
                            DCachePool.Store(dCache);
                            DTypeStackPool.Store(typeStack);
                            return keyValue;
                        }
                        continue;
                    #endregion

                    #region TupleStart
                    case DataType.TupleStart:
                        typeItem.Last = currentByte;
                        typeItem.Items = new List<object>();
                        continue;
                    #endregion

                    #region TupleEnd
                    case DataType.TupleEnd:
                        typeItem.Last = 0;
                        var tupleValue = typeItem.TypeInfo.Activator(typeItem.Items.ToArray());
                        typeStack.Pop();
                        if (typeStack.Count > 0)
                        {
                            typeItem = typeStack.Peek();
                            ProcessTypeEnd(typeItem, tupleValue);
                        }
                        else
                        {
                            DeserializersTable.ReturnTable(serializersTable);
                            DCachePool.Store(dCache);
                            DTypeStackPool.Store(typeStack);
                            return DataTypeHelper.Change(tupleValue, type);
                        }
                        continue;
                    #endregion

                    #region ListStart
                    case DataType.ListStart:
                        typeItem.Last = currentByte;
                        continue;
                    #endregion

                    #region ListEnd
                    case DataType.ListEnd:
                        typeItem.Last = 0;
                        if (typeItem.TypeInfo.IsArray)
                        {
                            typeItem.Value = typeItem.TypeInfo.Activator(typeItem.Items.Count);
                            typeItem.ValueIList = (IList)typeItem.Value;
                            for (var i = 0; i < typeItem.Items.Count; i++)
                                typeItem.ValueIList[i] = typeItem.Items[i];
                        }
                        continue;
                    #endregion

                    #region DictionaryStart
                    case DataType.DictionaryStart:
                        typeItem.Last = currentByte;
                        typeItem.Items = new List<object>();
                        continue;
                    #endregion

                    #region DictionaryEnd
                    case DataType.DictionaryEnd:
                        typeItem.Last = 0;
                        var valueIDictionary = (IDictionary)typeItem.Value;
                        for (var i = 0; i < typeItem.Items.Count; i += 2)
                        {
                            var key = DataTypeHelper.Change(typeItem.Items[i], typeItem.TypeInfo.InnerTypes[0]);
                            var value = DataTypeHelper.Change(typeItem.Items[i + 1], typeItem.TypeInfo.InnerTypes[1]);
                            valueIDictionary.Add(key, value);
                        }
                        continue;
                    #endregion

                    #region Ref Object
                    case DataType.RefObjectByte0:
                    case DataType.RefObjectByte1:
                    case DataType.RefObjectByte2:
                    case DataType.RefObjectByte3:
                    case DataType.RefObjectByte4:
                    case DataType.RefObjectByte5:
                    case DataType.RefObjectByte6:
                    case DataType.RefObjectByte7:
                    case DataType.RefObjectByte8:
                    case DataType.RefObjectByte9:
                    case DataType.RefObjectByte10:
                    case DataType.RefObjectByte11:
                    case DataType.RefObjectByte12:
                    case DataType.RefObjectByte13:
                    case DataType.RefObjectByte14:
                    case DataType.RefObjectByte15:
                    case DataType.RefObjectByte16:
                    case DataType.RefObjectByte:
                    case DataType.RefObjectUShort:
                        var objRef = -1;
                        #region Get Object Reference
                        switch (currentByte)
                        {
                            case DataType.RefObjectByte0: objRef = 0; break;
                            case DataType.RefObjectByte1: objRef = 1; break;
                            case DataType.RefObjectByte2: objRef = 2; break;
                            case DataType.RefObjectByte3: objRef = 3; break;
                            case DataType.RefObjectByte4: objRef = 4; break;
                            case DataType.RefObjectByte5: objRef = 5; break;
                            case DataType.RefObjectByte6: objRef = 6; break;
                            case DataType.RefObjectByte7: objRef = 7; break;
                            case DataType.RefObjectByte8: objRef = 8; break;
                            case DataType.RefObjectByte9: objRef = 9; break;
                            case DataType.RefObjectByte10: objRef = 10; break;
                            case DataType.RefObjectByte11: objRef = 11; break;
                            case DataType.RefObjectByte12: objRef = 12; break;
                            case DataType.RefObjectByte13: objRef = 13; break;
                            case DataType.RefObjectByte14: objRef = 14; break;
                            case DataType.RefObjectByte15: objRef = 15; break;
                            case DataType.RefObjectByte16: objRef = 16; break;
                            case DataType.RefObjectByte:
                                objRef = br.ReadByte();
                                break;
                            case DataType.RefObjectUShort:
                                objRef = br.ReadUInt16();
                                break;
                        }
                        #endregion
                        var objValue = objectCache.DeserializerGet(objRef);
                        ProcessTypeEnd(typeItem, objValue);
                        continue;
                    #endregion

                    #region ValueNull
                    case DataType.ValueNull:
                        if (typeItem != null)
                            ProcessTypeEnd(typeItem, null);
                        else
                        {
                            DeserializersTable.ReturnTable(serializersTable);
                            DCachePool.Store(dCache);
                            DTypeStackPool.Store(typeStack);
                            return null;
                        }
                        continue;
                    #endregion

                    default:
                        #region Default Handle

                        if (typeItem != null)
                        {
                            var value = serializersTable.GetSerializerByValueByte(currentByte).Read(br, currentByte);
                            if (typeItem.Value != null && typeItem.Last == 0)
                            {
                                var propName = typeItem.Properties[typeItem.PropertyIndex++];
                                if (typeItem.TypeInfo.Properties.TryGetValue(propName, out var fprop))
                                {
                                    if (value.GetType() == fprop.PropertyType)
                                        fprop.SetValue(typeItem.Value, value);
                                    else
                                    {
                                        fprop.SetValue(typeItem.Value, 
                                            fprop.PropertyType.IsEnum
                                                ? Enum.ToObject(fprop.PropertyType, value)
                                                : DataTypeHelper.Change(value, fprop.PropertyType.GetUnderlyingType()));
                                    }
                                }
                                else
                                    Core.Log.Warning("Property '{0}' doesn't exist on Type '{1}'", propName, typeItem.Type);
                            }
                            else
                                ProcessTypeEnd(typeItem, value);
                        }
                        else
                        {
                            DeserializersTable.ReturnTable(serializersTable);
                            DCachePool.Store(dCache);
                            DTypeStackPool.Store(typeStack);
                            return DataTypeHelper.Change(serializersTable.GetSerializerByValueByte(currentByte).Read(br, currentByte), type);
                        }

                        #endregion
                        break;
                }

            } while (typeStack.Count > 0);

            return null;
        }

        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DeserializerTypeItem CreateDeserializerNewTypeItem(SerializerCache<object> objectCache, DeserializerTypeDefinition typeDefinition)
        {
            var newTypeItem = new DeserializerTypeItem(typeDefinition);
            if (newTypeItem.Type == null)
                objectCache.DeserializerSet(null);
            else if (!newTypeItem.Type.IsArray)
                objectCache.DeserializerSet(newTypeItem.Value);
            return newTypeItem;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ProcessTypeEnd(DeserializerTypeItem typeItem, object lastObject)
        {
            if (typeItem == null) return;

            switch (typeItem.Last)
            {
                case DataType.Unknown:
                    if (typeItem.Value != null)
                    {
                        var propName = typeItem.Properties[typeItem.PropertyIndex++];
                        if (typeItem.TypeInfo.Properties.TryGetValue(propName, out var fprop))
                        {
                            if (lastObject == null)
                                fprop.SetValue(typeItem.Value, null);
                            else if (lastObject.GetType() == fprop.PropertyType)
                                fprop.SetValue(typeItem.Value, lastObject);
                            else
                                fprop.SetValue(typeItem.Value, DataTypeHelper.Change(lastObject, fprop.PropertyType.GetUnderlyingType()));
                        }
                        else
                            Core.Log.Warning("Property '{0}' doesn't exist on Type '{1}'", propName, typeItem.Type);


                        typeItem.LastPropertyName = null;
                        typeItem.Last = 0;
                    }
                    break;
                case DataType.ListStart:
                    var valueList = typeItem.ValueIList;
                    if (valueList != null)
                    {
                        var iTypes = typeItem.TypeInfo.InnerTypes;
                        if (lastObject != null && iTypes.Length > 0 && iTypes[0] != lastObject.GetType())
                            lastObject = DataTypeHelper.Change(lastObject, iTypes[0]);
                        valueList.Add(lastObject);
                    }
                    else
                        typeItem.Items.Add(lastObject);
                    break;
                case DataType.KeyValueStart:
                    typeItem.Items.Add(lastObject);
                    break;
                case DataType.DictionaryStart:
                    typeItem.Items.Add(lastObject);
                    break;
                case DataType.TupleStart:
                    typeItem.Items.Add(lastObject);
                    break;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Type DeserializeType(BinaryReader br, StringSerializer stringSerializer, byte _byte)
        {
            switch (_byte)
            {
                case DataType.TypeNameBool: return typeof(bool);
                case DataType.TypeNameByte: return typeof(byte);
                case DataType.TypeNameSerializedObject: return typeof(SerializedObject);
                case DataType.TypeNameChar: return typeof(char);
                case DataType.TypeNameDateTime: return typeof(DateTime);
                case DataType.TypeNameDateTimeOffset: return typeof(DateTimeOffset);
                case DataType.TypeNameDecimal: return typeof(decimal);
                case DataType.TypeNameDouble: return typeof(double);
                case DataType.TypeNameFloat: return typeof(float);
                case DataType.TypeNameGuid: return typeof(Guid);
                case DataType.TypeNameInt: return typeof(int);
                case DataType.TypeNameLong: return typeof(long);
                case DataType.TypeNameSByte: return typeof(sbyte);
                case DataType.TypeNameShort: return typeof(short);
                case DataType.TypeNameString: return typeof(string);
                case DataType.TypeNameTimeSpan: return typeof(TimeSpan);
                case DataType.TypeNameUInt: return typeof(uint);
                case DataType.TypeNameULong: return typeof(ulong);
                case DataType.TypeNameUShort: return typeof(ushort);
                default:
                    var tassembly = stringSerializer.ReadValue(br, _byte);
                    var tnamespace = stringSerializer.ReadValue(br, br.ReadByte());
                    var tname = stringSerializer.ReadValue(br, br.ReadByte());
                    return GetDeserializationType((tassembly, tnamespace, tname));
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Type GetDeserializationType((string, string, string) type)
        {
            return DeserializationTypes.GetOrAdd(type, mType =>
            {
                var key = mType.Item2 + "." + mType.Item3;
                if (!string.IsNullOrEmpty(mType.Item1))
                    key += ", " + mType.Item1;
                return Core.GetType(key);
            });
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DeserializerTypeInfo GetDeserializationTypeInfo(Type type)
        {
            if (type == null) return null;
            return DeserializationTypeInfo.GetOrAdd(type, valueType =>
            {
                if (valueType == null) return null;

                var typeInfo = valueType.GetTypeInfo();
                var isGenericType = typeInfo.IsGenericType;

                var tinfo = new DeserializerTypeInfo
                {
                    Type = valueType,
                    Properties = valueType.GetRuntimeProperties().Where(p =>
                    {
                        var ok = !p.IsSpecialName && p.CanRead && p.CanWrite;
                        if (!ok) return false;
                        return p.GetIndexParameters().Length <= 0;
                    }).Select(p => p.GetFastPropertyInfo()).ToDictionary(k => k.Name, v => v)
                };
                var constructor = typeInfo.DeclaredConstructors.First();
                tinfo.Activator = Factory.Accessors.CreateActivator(constructor);
                tinfo.ActivatorParametersTypes = constructor.GetParameters().Select(p => p.ParameterType).ToArray();
                tinfo.IsArray = valueType.IsArray;

                //
                var ifaces = typeInfo.ImplementedInterfaces.ToArray();


                var ilist = ifaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
                if (ilist != null)
                {
                    Type innerType = null;
                    if (type.IsArray)
                        innerType = type.GetElementType();
                    else
                    {
                        var gargs = ilist.GenericTypeArguments;
                        if (gargs.Length == 0)
                            gargs = type.GenericTypeArguments;
                        if (gargs.Length > 0)
                            innerType = gargs[0];
                        else
                        {
                            var iListType = typeInfo.ImplementedInterfaces.FirstOrDefault(m => (m.IsGenericType && m.GetGenericTypeDefinition() == typeof(IList<>)));
                            if (iListType != null && iListType.GenericTypeArguments.Length > 0)
                                innerType = iListType.GenericTypeArguments[0];
                        }
                    }
                    tinfo.IsIList = true;
                    tinfo.InnerTypes = new[] { innerType };
                }

                var idictio = ifaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
                if (idictio != null)
                {
                    tinfo.IsIDictionary = true;
                    tinfo.InnerTypes = idictio.GenericTypeArguments;
                }

                if (!tinfo.IsIList && !tinfo.IsIDictionary && isGenericType)
                    tinfo.InnerTypes = typeInfo.GenericTypeArguments;

                return tinfo;
            });
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string[] GetPropertiesFromType(Type type)
        {
            if (type == null) return null;
            var typeInfo = type.GetTypeInfo();
            var isIList = typeInfo.ImplementedInterfaces.Any(i => i == typeof(IList) || i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));

            var properties = type.GetRuntimeProperties().OrderBy(n => n.Name).ToArray();
            var propNames = new List<string>();
            foreach (var prop in properties)
            {
                if (!prop.CanRead || !prop.CanWrite) continue;
                if (isIList && prop.Name == "Capacity") continue;
                if (prop.GetIndexParameters().Length > 0) continue;
                propNames.Add(prop.Name);
            }
            return propNames.ToArray();
        }
        #endregion

        #endregion

        #region Public Methods
        /// <summary>
        /// Add Known Type
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="includeInnerTypes">Include inner types</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddKnownType(Type type, bool includeInnerTypes = false)
        {
            if (type == null) return;
            if (!_knownTypes.Add(type)) return;

            GlobalKnownTypes.GetOrAdd(type, mType =>
            {
                var hashBytes = Hash.GetBytes(mType.GetTypeName());
                GlobalKnownTypesValues[hashBytes] = mType;
                return hashBytes;
            });
            if (!includeInnerTypes) return;

            var props = type.GetRuntimeProperties();
            foreach (var prop in props)
                AddKnownType(prop.PropertyType, true);
        }
        /// <summary>
        /// Clear Known Types
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearKnownTypes()
        {
            _knownTypes.Clear();
        }
        #endregion
    }
}
