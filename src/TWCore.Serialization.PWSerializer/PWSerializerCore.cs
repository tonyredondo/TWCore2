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
using TWCore.Serialization.PWSerializer.Deserializer;
using TWCore.Serialization.PWSerializer.Serializer;
using TWCore.Serialization.PWSerializer.Types;
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable ForCanBeConvertedToForeach

// ReSharper disable InconsistentNaming

namespace TWCore.Serialization.PWSerializer
{
    /// <summary>
    /// Portable Wanhjör Serializer
    /// </summary>
    public class PWSerializerCore
    {
        private static readonly Encoding DefaultUtf8Encoding = new UTF8Encoding(false);
        private static readonly ArrayEqualityComparer<string> StringArrayComparer = new ArrayEqualityComparer<string>(StringComparer.Ordinal);

        #region Allocators
        private class SerPoolItem
        {
            public readonly SerializerCache<string[]> TypesCache;
            public readonly SerializerCache<object> ObjectCache;
            public StringSerializer PropertiesSerializer;
            //
            public readonly HashSet<Type> CurrentSerializerPlanTypes;
            public readonly Stack<SerializerScope> ScopeStack;

            public SerPoolItem()
            {
                TypesCache = new SerializerCache<string[]>(SerializerMode.CachedUShort, StringArrayComparer);
                ObjectCache = new SerializerCache<object>(SerializerMode.CachedUShort);
                PropertiesSerializer = new StringSerializer();
                PropertiesSerializer.Init(SerializerMode.CachedUShort);
                //
                CurrentSerializerPlanTypes = new HashSet<Type>();
                ScopeStack = new Stack<SerializerScope>();
            }

            public void Clear()
            {
                TypesCache.Clear(SerializerMode.CachedUShort);
                ObjectCache.Clear(SerializerMode.CachedUShort);
                PropertiesSerializer.Init(SerializerMode.CachedUShort);
                CurrentSerializerPlanTypes.Clear();
                ScopeStack.Clear();
            }
        }
        private class DesPoolItem
        {
            public readonly SerializerCache<string[]> TypesCache;
            public readonly SerializerCache<object> ObjectCache;
            public StringSerializer PropertiesSerializer;
            //
            public readonly Dictionary<Type, Type[]> PropertiesTypes;
            public readonly Stack<DeserializerType> DeserializerStack;

            public DesPoolItem()
            {
                TypesCache = new SerializerCache<string[]>(SerializerMode.CachedUShort, StringArrayComparer);
                ObjectCache = new SerializerCache<object>(SerializerMode.CachedUShort);
                PropertiesSerializer = new StringSerializer();
                PropertiesSerializer.Init(SerializerMode.CachedUShort);
                //
                PropertiesTypes = new Dictionary<Type, Type[]>();
                DeserializerStack = new Stack<DeserializerType>();
            }

            public void Clear()
            {
                TypesCache.Clear(SerializerMode.CachedUShort);
                ObjectCache.Clear(SerializerMode.CachedUShort);
                PropertiesSerializer.Init(SerializerMode.CachedUShort);
                PropertiesTypes.Clear();
                DeserializerStack.Clear();
            }
        }

        private struct SerPoolAllocator : IPoolObjectLifecycle<SerPoolItem>
        {
            public int InitialSize => Environment.ProcessorCount;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SerPoolItem New() => new SerPoolItem();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(SerPoolItem value) => value.Clear();
        }
        private struct DesPoolAllocator : IPoolObjectLifecycle<DesPoolItem>
        {
            public int InitialSize => Environment.ProcessorCount;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public DesPoolItem New() => new DesPoolItem();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(DesPoolItem value) => value.Clear();
        }
        #endregion

        /// <summary>
        /// Serializer Mode
        /// </summary>
        public SerializerMode Mode = SerializerMode.Cached2048;

        public PWSerializerCore() { }
        public PWSerializerCore(SerializerMode mode)
        {
            Mode = mode;
        }

        #region Serializer
        private static readonly NonBlocking.ConcurrentDictionary<Type, SerializerPlan> SerializationPlans = new NonBlocking.ConcurrentDictionary<Type, SerializerPlan>();
        private static readonly SerializerPlanItem[] EndPlan = { new SerializerPlanItem.WriteBytes(new[] { DataType.TypeEnd }) };
        private static readonly ObjectPool<SerPoolItem, SerPoolAllocator> SerPool = new ObjectPool<SerPoolItem, SerPoolAllocator>();
        private static readonly ReferencePool<SerializerScope> SerializerScopePool = new ReferencePool<SerializerScope>(10, scope => scope.Init());
        private static readonly ReferencePool<SerializerPlanItem.RuntimeValue> SerializerRuntimePool = new ReferencePool<SerializerPlanItem.RuntimeValue>(20);
        private readonly byte[] _bufferSer = new byte[3];

        #region Public Methods
        /// <summary>
        /// Serialize an object value in a Portable Tony Wanhjör format
        /// </summary>
        /// <param name="stream">Stream where the data is going to be stored</param>
        /// <param name="value">Value to be serialized</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Stream stream, object value)
        {
            var type = value?.GetType() ?? typeof(object);
            var serPool = SerPool.New();
            var currentSerializerPlanTypes = serPool.CurrentSerializerPlanTypes;
            var serializersTable = SerializersTable.GetTable(Mode);
            var numberSerializer = serializersTable.NumberSerializer;
            var typesCache = serPool.TypesCache;
            var objectCache = serPool.ObjectCache;
            var propertySerializer = serPool.PropertiesSerializer;
            var plan = GetSerializerPlan(currentSerializerPlanTypes, serializersTable, type);
            var scopeStack = serPool.ScopeStack;
            var scope = SerializerScopePool.New();
            scope.Init(plan, typeof(object), value);
            scopeStack.Push(scope);

            var bw = new BinaryWriter(stream, DefaultUtf8Encoding, true);
            _bufferSer[0] = DataType.PWFileStart;
            _bufferSer[1] = (byte)Mode;
            bw.Write(_bufferSer, 0, 2);
            do
            {
                var item = scope.NextIfAvailable();

                #region Get the Current Scope
                if (item == null)
                {
                    SerializerScopePool.Store(scopeStack.Pop());
                    scope = (scopeStack.Count > 0) ? scopeStack.Peek() : null;
                    continue;
                }
                #endregion

                #region Switch Plan Type

                switch (item.PlanType)
                {
                    #region WriteBytes

                    case SerializerPlanItemType.WriteBytes:
                        bw.Write(item.ValueBytes);
                        continue;

                    #endregion

                    #region TypeStart

                    case SerializerPlanItemType.TypeStart:
                        if (scope.Value == null)
                        {
                            bw.Write(DataType.TypeStart);
                            numberSerializer.WriteValue(bw, 0);
                            numberSerializer.WriteValue(bw, -1);
                            continue;
                        }
                        var oidx = objectCache.SerializerGet(scope.Value);
                        if (oidx > -1)
                        {
                            if (oidx <= 20)
                                bw.Write((byte)(DataType.RefObjectByte0 + oidx));
                            else if (oidx <= byte.MaxValue)
                                Write(bw, DataType.RefObjectByte, (byte)oidx);
                            else
                                Write(bw, DataType.RefObjectUShort, (ushort)oidx);
                            SerializerScopePool.Store(scopeStack.Pop());
                            scope = (scopeStack.Count > 0) ? scopeStack.Peek() : null;
                        }
                        else
                        {
                            objectCache.SerializerSet(scope.Value);
                            var tStartItem = (SerializerPlanItem.TypeStart)item;
                            if (item.Type != scope.Type)
                            {
                                var tParts = tStartItem.TypeParts;
                                Write(bw, DataType.TypeName, (byte)tParts.Length);
                                for (var i = 0; i < tParts.Length; i++)
                                    propertySerializer.WriteValue(bw, tParts[i]);
                            }

                            var typeIdx = typesCache.SerializerGet(tStartItem.Properties);
                            if (typeIdx < 0)
                            {
                                #region TypeStart write
                                bw.Write(DataType.TypeStart);
                                var props = tStartItem.Properties;
                                var propsLength = props.Length;
                                numberSerializer.WriteValue(bw, propsLength);
                                for (var i = 0; i < propsLength; i++)
                                    propertySerializer.WriteValue(bw, props[i]);
                                typesCache.SerializerSet(props);
                                #endregion
                            }
                            else if (typeIdx <= 24)
                                bw.Write((byte)(DataType.TypeRefByte0 + typeIdx));
                            else if (typeIdx <= byte.MaxValue)
                                Write(bw, DataType.TypeRefByte, (byte)typeIdx);
                            else
                                Write(bw, DataType.TypeRefUShort, (ushort)typeIdx);

                            if (tStartItem.IsIList)
                                numberSerializer.WriteValue(bw, ((IList)scope.Value).Count);
                            else if (tStartItem.IsIDictionary)
                                numberSerializer.WriteValue(bw, ((IDictionary)scope.Value).Count);
                            else
                                numberSerializer.WriteValue(bw, 0);
                        }
                        continue;

                    #endregion

                    #region ListStart

                    case SerializerPlanItemType.ListStart:
                        if (scope.Value != null)
                        {
                            var lType = (SerializerPlanItem.ListStart)item;
                            var iList = (IList)scope.Value;
                            var iListCount = iList.Count;
                            if (iListCount > 0)
                            {
                                bw.Write(DataType.ListStart);
                                var valueTypeSerializer = serializersTable.GetSerializerByValueType(lType.InnerType);
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
                                        valueTypeSerializer = serializersTable.GetSerializerByValueType(itemList.GetType());
                                        if (valueTypeSerializer != null)
                                        {
                                            valueTypeSerializer.Write(bw, itemList);
                                        }
                                        else
                                        {
                                            var aPlan = new SerializerPlanItem.RuntimeValue[1];
                                            itemList = ResolveLinqEnumerables(itemList);
                                            var serType = serializersTable.GetSerializerByValueType(itemList.GetType())?.GetType();
                                            var srpVal = SerializerRuntimePool.New();
                                            srpVal.Init(lType.InnerType, serType, itemList);
                                            aPlan[0] = srpVal;
                                            scope = SerializerScopePool.New();
                                            scope.Init(aPlan, scope.Type);
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
                                                Type serType = null;
                                                if (itemList != null)
                                                    serType = serializersTable.GetSerializerByValueType(itemList.GetType())
                                                        ?.GetType();
                                                var srpVal = SerializerRuntimePool.New();
                                                srpVal.Init(lType.InnerType, serType, itemList);
                                                aPlan[i] = srpVal;
                                            }

                                            scope = SerializerScopePool.New();
                                            scope.Init(aPlan, scope.Type);
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
                                            Type serType = null;
                                            if (itemList != null)
                                                serType = serializersTable.GetSerializerByValueType(itemList.GetType())
                                                    ?.GetType();
                                            var srpVal = SerializerRuntimePool.New();
                                            srpVal.Init(lType.InnerType, serType, itemList);
                                            aPlan[i] = srpVal;
                                        }

                                        scope = SerializerScopePool.New();
                                        scope.Init(aPlan, scope.Type);
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
                            var dictioItem = (SerializerPlanItem.DictionaryStart)item;
                            var iDictio = (IDictionary)scope.Value;
                            var iDictioCount = iDictio.Count;
                            if (iDictioCount > 0)
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
                                    var aPlan = new SerializerPlanItem.RuntimeValue[iDictioCount * 2];
                                    var aIdx = 0;
                                    foreach (var keyValue in iDictio.Keys)
                                    {
                                        var kv = ResolveLinqEnumerables(keyValue);
                                        var valueValue = iDictio[keyValue];
                                        valueValue = ResolveLinqEnumerables(valueValue);
                                        var aPlanKeyVal = SerializerRuntimePool.New();
                                        aPlanKeyVal.Init(dictioItem.KeyType, dictioItem.KeySerializerType, kv);
                                        aPlan[aIdx++] = aPlanKeyVal;

                                        var aPlanValVal = SerializerRuntimePool.New();
                                        aPlanValVal.Init(dictioItem.ValueType, dictioItem.ValueSerializerType,
                                            valueValue);
                                        aPlan[aIdx++] = aPlanValVal;
                                    }

                                    scope = SerializerScopePool.New();
                                    scope.Init(aPlan, scope.Type);
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

                    #region PropertyValue

                    case SerializerPlanItemType.PropertyValue:
                        var cItem = (SerializerPlanItem.PropertyValue)item;
                        var pVal = cItem.Property.GetValue(scope.Value);
                        if (pVal == cItem.DefaultValue)
                            bw.Write(DataType.ValueNull);
                        else
                            serializersTable.Write(cItem.SerializerType, bw, pVal);
                        continue;

                    #endregion

                    #region PropertyReference

                    case SerializerPlanItemType.PropertyReference:
                        var rItem = (SerializerPlanItem.PropertyReference)item;
                        var rVal = rItem.Property.GetValue(scope.Value);
                        if (rVal == null)
                            bw.Write(DataType.ValueNull);
                        else
                        {
                            rVal = ResolveLinqEnumerables(rVal);
                            scope = SerializerScopePool.New();
                            scope.Init(GetSerializerPlan(currentSerializerPlanTypes, serializersTable, rVal?.GetType() ?? rItem.Type), rItem.Type, rVal);
                            scopeStack.Push(scope);
                        }
                        continue;

                    #endregion

                    #region Value

                    case SerializerPlanItemType.Value:
                        var vItem = (SerializerPlanItem.ValueItem)item;
                        if (scope.Value == null)
                            bw.Write(DataType.ValueNull);
                        else if (vItem.SerializerType != null)
                            serializersTable.Write(vItem.SerializerType, bw, scope.Value);
                        else
                        {
                            scope = SerializerScopePool.New();
                            scope.Init(GetSerializerPlan(currentSerializerPlanTypes, serializersTable, scope.Value?.GetType() ?? vItem.Type), vItem.Type, scope.Value);
                            scopeStack.Push(scope);
                        }
                        continue;

                    #endregion

                    #region RuntimeValue

                    case SerializerPlanItemType.RuntimeValue:
                        var rvItem = (SerializerPlanItem.RuntimeValue)item;
                        if (rvItem.Value == null)
                            bw.Write(DataType.ValueNull);
                        else if (rvItem.SerializerType != null)
                            serializersTable.Write(rvItem.SerializerType, bw, rvItem.Value);
                        else
                        {
                            scope = SerializerScopePool.New();
                            scope.Init(GetSerializerPlan(currentSerializerPlanTypes, serializersTable, rvItem.Value?.GetType() ?? rvItem.Type), rvItem.Type, rvItem.Value);
                            scopeStack.Push(scope);
                        }
                        rvItem.Type = null;
                        rvItem.SerializerType = null;
                        rvItem.Value = null;
                        SerializerRuntimePool.Store(rvItem);
                        continue;

                    #endregion

                    default:
                        break;
                }

                #endregion

            } while (scope != null);
            SerPool.Store(serPool);
            SerializersTable.ReturnTable(serializersTable);
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(BinaryWriter bw, byte type, byte value)
        {
            _bufferSer[0] = type;
            _bufferSer[1] = value;
            bw.Write(_bufferSer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(BinaryWriter bw, byte type, ushort value)
        {
            _bufferSer[0] = type;
            _bufferSer[1] = (byte)value;
            _bufferSer[2] = (byte)(value >> 8);
            bw.Write(_bufferSer, 0, 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SerializerPlan GetSerializerPlan(HashSet<Type> currentSerializerPlanTypes, SerializersTable serializerTable, Type type)
        {
            return SerializationPlans.GetOrAdd(type, iType =>
            {
                var plan = new List<SerializerPlanItem>();
                var typeInfo = iType.GetTypeInfo();
                var genTypeDefinition = typeInfo.IsGenericType ? typeInfo.GetGenericTypeDefinition() : null;
                var serBase = serializerTable.GetSerializerByValueType(iType);
                var isIList = false;
                var isIDictionary = false;

                if (serBase != null)
                {
                    //Value type
                    plan.Add(new SerializerPlanItem.ValueItem(iType, serBase.GetType()));
                }
                else if (genTypeDefinition == typeof(Nullable<>))
                {
                    //Nullable type
                    iType = Nullable.GetUnderlyingType(iType);
                    serBase = serializerTable.GetSerializerByValueType(iType);
                    plan.Add(new SerializerPlanItem.ValueItem(iType, serBase.GetType()));
                }
                else
                {
                    currentSerializerPlanTypes.Add(iType);
                    var tStart = new SerializerPlanItem.TypeStart(iType);
                    plan.Add(tStart);
                    var endBytes = new List<byte>();

                    if (typeInfo.ImplementedInterfaces.Any(i => i == typeof(IList) || i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
                        isIList = true;
                    if (typeInfo.ImplementedInterfaces.Any(i => i == typeof(IDictionary) || i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                        isIDictionary = true;

                    tStart.IsIList = isIList;
                    tStart.IsIDictionary = isIDictionary;

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
                            if (!currentSerializerPlanTypes.Contains(propType))
                                GetSerializerPlan(currentSerializerPlanTypes, serializerTable, propType);
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
                        var ilist = ifaces.FirstOrDefault(i => i == typeof(IList) || (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)));
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
                            if (!currentSerializerPlanTypes.Contains(innerType))
                                GetSerializerPlan(currentSerializerPlanTypes, serializerTable, innerType);
                            endBytes.Add(DataType.ListEnd);
                        }
                    }
                    #endregion

                    #region DictionaryInfo
                    if (isIDictionary)
                    {
                        var ifaces = typeInfo.ImplementedInterfaces;
                        var idictio = ifaces.FirstOrDefault(i => i == typeof(IDictionary) || (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)));
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

                            if (keySer == null && !currentSerializerPlanTypes.Contains(keyType))
                                GetSerializerPlan(currentSerializerPlanTypes, serializerTable, keyType);
                            if (valueSer == null && !currentSerializerPlanTypes.Contains(valueType))
                                GetSerializerPlan(currentSerializerPlanTypes, serializerTable, valueType);

                            plan.Add(new SerializerPlanItem.DictionaryStart(type, keyType, keySer?.GetType(), keyIsNullable, valueType, valueSer?.GetType(), valueIsNullable));
                            endBytes.Add(DataType.DictionaryEnd);
                        }
                    }
                    #endregion

                    endBytes.Add(DataType.TypeEnd);
                    plan.Add(new SerializerPlanItem.WriteBytes(endBytes.ToArray()));
                    currentSerializerPlanTypes.Remove(iType);
                }
                var sPlan = new SerializerPlan();
                sPlan.Init(plan.ToArray(), iType, isIList, isIDictionary);
                return sPlan;
            });
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object ResolveLinqEnumerables(object value)
        {
            if (value is IEnumerable ieValue && !(value is string))
                value = ieValue.Enumerate();
            return value;
        }
        #endregion

        #endregion

        #region Deserializer
        private static readonly ObjectPool<DesPoolItem, DesPoolAllocator> DesPool = new ObjectPool<DesPoolItem, DesPoolAllocator>();
        private static readonly ReferencePool<DeserializerType> DesarializerTypePool = new ReferencePool<DeserializerType>(Environment.ProcessorCount, d => d.Clear());
        private static readonly ReferencePool<Stack<DynamicDeserializedType>> GdStackPool = new ReferencePool<Stack<DynamicDeserializedType>>(Environment.ProcessorCount, s => s.Clear());
        private readonly byte[] _bufferDes = new byte[8];

        /// <summary>
        /// Deserialize a Portable Tony Wanhjor stream into a object value
        /// </summary>
        /// <param name="stream">Stream where the data is going to be readed (source data)</param>
        /// <param name="type">Declared type of the value to be serialized</param>
        /// <returns>Deserialized object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Deserialize(Stream stream, Type type)
        {
            if (type == null)
                return Deserialize(stream);
            var br = new BinaryReader(stream, DefaultUtf8Encoding, true);
            if (br.Read(_bufferDes, 0, 2) != 2)
                throw new EndOfStreamException("Error reading the PW header.");
            var fStart = _bufferDes[0];
            var sMode = (SerializerMode)_bufferDes[1];
            if (fStart != DataType.PWFileStart)
                throw new FormatException(string.Format("The stream is not in PWBinary format. Byte {0} was expected, received: {1}", DataType.PWFileStart, fStart));
            Mode = sMode;

            var desPool = DesPool.New();
            var serializersTable = DeserializersTable.GetTable(sMode);
            var numberSerializer = serializersTable.NumberSerializer;
            var typesCache = desPool.TypesCache;
            var objectCache = desPool.ObjectCache;
            var propertySerializer = desPool.PropertiesSerializer;
            var propertiesTypes = desPool.PropertiesTypes;
            var desStack = desPool.DeserializerStack;
            DeserializerType typeItem = null;
            Type valueType = null;

            do
            {
                var currentType = valueType ?? typeItem?.CurrentType ?? type;
                var currentByte = br.ReadByte();
                switch (currentByte)
                {
                    #region TypeStart
                    case DataType.TypeName:
                        var vTypePartsLength = (int)br.ReadByte();
                        var vTypeParts = new string[vTypePartsLength];
                        for (var i = 0; i < vTypePartsLength; i++)
                            vTypeParts[i] = propertySerializer.ReadValue(br);
                        var vType = string.Join(".", vTypeParts);
                        valueType = Core.GetType(vType) ?? valueType;
                        continue;
                    case DataType.TypeStart:
                        var typePropertiesLength = numberSerializer.ReadValue(br);
                        var typeProperties = new string[typePropertiesLength];
                        for (var i = 0; i < typePropertiesLength; i++)
                            typeProperties[i] = propertySerializer.ReadValue(br);
                        var length = numberSerializer.ReadValue(br);
                        typesCache.DeserializerSet(typeProperties);
                        typeItem = DesarializerTypePool.New();
                        typeItem.SetProperties(currentType, typeProperties, propertiesTypes, length);
                        objectCache.DeserializerSet(typeItem.Value);
                        desStack.Push(typeItem);
                        valueType = null;
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
                        typeProperties = typesCache.DeserializerGet(byteIdx);
                        length = numberSerializer.ReadValue(br);
                        typeItem = DesarializerTypePool.New();
                        typeItem.SetProperties(currentType, typeProperties, propertiesTypes, length);
                        objectCache.DeserializerSet(typeItem.Value);
                        desStack.Push(typeItem);
                        valueType = null;
                        continue;
                    case DataType.TypeRefByte:
                        var refByteVal = br.ReadByte();
                        typeProperties = typesCache.DeserializerGet(refByteVal);
                        length = numberSerializer.ReadValue(br);
                        typeItem = DesarializerTypePool.New();
                        typeItem.SetProperties(currentType, typeProperties, propertiesTypes, length);
                        objectCache.DeserializerSet(typeItem.Value);
                        desStack.Push(typeItem);
                        valueType = null;
                        continue;
                    case DataType.TypeRefUShort:
                        var refUshortVal = br.ReadUInt16();
                        typeProperties = typesCache.DeserializerGet(refUshortVal);
                        length = numberSerializer.ReadValue(br);
                        typeItem = DesarializerTypePool.New();
                        typeItem.SetProperties(currentType, typeProperties, propertiesTypes, length);
                        objectCache.DeserializerSet(typeItem.Value);
                        desStack.Push(typeItem);
                        valueType = null;
                        continue;
                    #endregion

                    #region TypeEnd
                    case DataType.TypeEnd:
                        var lastItem = desStack.Pop();
                        if (desStack.Count > 0)
                        {
                            typeItem = desStack.Peek();
                            typeItem.AddValue(lastItem.Value);
                            DesarializerTypePool.Store(lastItem);
                        }
                        continue;
                    #endregion

                    #region ListStart
                    case DataType.ListStart:
                        typeItem?.ListStart();
                        continue;
                    #endregion

                    #region ListEnd
                    case DataType.ListEnd:
                        typeItem?.ListEnd();
                        continue;
                    #endregion

                    #region DictionaryStart
                    case DataType.DictionaryStart:
                        typeItem?.DictionaryStart();
                        continue;
                    #endregion

                    #region DictionaryEnd
                    case DataType.DictionaryEnd:
                        typeItem?.DictionaryEnd();
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
                    case DataType.RefObjectByte17:
                    case DataType.RefObjectByte18:
                    case DataType.RefObjectByte19:
                    case DataType.RefObjectByte20:
                    case DataType.RefObjectUShort:
                    case DataType.RefObjectByte:
                        var objRef = -1;
                        #region Get Object Reference

                        switch (currentByte)
                        {
                            case DataType.RefObjectByte0:
                                objRef = 0;
                                break;
                            case DataType.RefObjectByte1:
                                objRef = 1;
                                break;
                            case DataType.RefObjectByte2:
                                objRef = 2;
                                break;
                            case DataType.RefObjectByte3:
                                objRef = 3;
                                break;
                            case DataType.RefObjectByte4:
                                objRef = 4;
                                break;
                            case DataType.RefObjectByte5:
                                objRef = 5;
                                break;
                            case DataType.RefObjectByte6:
                                objRef = 6;
                                break;
                            case DataType.RefObjectByte7:
                                objRef = 7;
                                break;
                            case DataType.RefObjectByte8:
                                objRef = 8;
                                break;
                            case DataType.RefObjectByte9:
                                objRef = 9;
                                break;
                            case DataType.RefObjectByte10:
                                objRef = 10;
                                break;
                            case DataType.RefObjectByte11:
                                objRef = 11;
                                break;
                            case DataType.RefObjectByte12:
                                objRef = 12;
                                break;
                            case DataType.RefObjectByte13:
                                objRef = 13;
                                break;
                            case DataType.RefObjectByte14:
                                objRef = 14;
                                break;
                            case DataType.RefObjectByte15:
                                objRef = 15;
                                break;
                            case DataType.RefObjectByte16:
                                objRef = 16;
                                break;
                            case DataType.RefObjectByte17:
                                objRef = 17;
                                break;
                            case DataType.RefObjectByte18:
                                objRef = 18;
                                break;
                            case DataType.RefObjectByte19:
                                objRef = 19;
                                break;
                            case DataType.RefObjectByte20:
                                objRef = 20;
                                break;
                            case DataType.RefObjectByte:
                                objRef = br.ReadByte();
                                break;
                            case DataType.RefObjectUShort:
                                objRef = br.ReadUInt16();
                                break;
                        }
                        #endregion
                        var objValue = objectCache.DeserializerGet(objRef);
                        typeItem?.AddValue(objValue);
                        continue;
                    #endregion

                    #region ValueNull
                    case DataType.ValueNull:
                        typeItem?.AddValue(null);
                        continue;
                    #endregion

                    default:
                        var value = serializersTable.Read(br, currentByte);
                        if (typeItem != null)
                            typeItem.AddValue(value);
                        else
                            return DataTypeHelper.Change(value, type);
                        break;
                }
            }
            while (desStack.Count > 0 || valueType != null);
            DesPool.Store(desPool);
            DeserializersTable.ReturnTable(serializersTable);
            return typeItem?.Value;
        }

        /// <summary>
        /// Generic Deserialize a Portable Tony Wanhjor stream into a object value
        /// </summary>
        /// <param name="stream">Stream where the data is going to be readed (source data)</param>
        /// <returns>Deserialized object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Deserialize(Stream stream)
        {
            var br = new BinaryReader(stream, DefaultUtf8Encoding, true);
            if (br.Read(_bufferDes, 0, 2) != 2)
                throw new EndOfStreamException("Error reading the PW header.");
            var fStart = _bufferDes[0];
            var sMode = (SerializerMode)_bufferDes[1];
            Mode = sMode;
            if (fStart != DataType.PWFileStart)
                throw new FormatException(string.Format("The stream is not in PWBinary format. Byte {0} was expected, received: {1}", DataType.PWFileStart, fStart));

            var serializersTable = DeserializersTable.GetTable(sMode);
            var numberSerializer = serializersTable.NumberSerializer;
            var serPool = SerPool.New();
            var typesCache = serPool.TypesCache;
            var objectCache = serPool.ObjectCache;
            var propertySerializer = serPool.PropertiesSerializer;

            var desStack = GdStackPool.New();
            DynamicDeserializedType typeItem = null;
            string valueType = null;
            do
            {
                var currentByte = br.ReadByte();
                switch (currentByte)
                {
                    #region TypeStart
                    case DataType.TypeName:
                        var vTypePartsLength = (int)br.ReadByte();
                        var vTypeParts = new string[vTypePartsLength];
                        for (var i = 0; i < vTypePartsLength; i++)
                            vTypeParts[i] = propertySerializer.ReadValue(br);
                        valueType = string.Join(".", vTypeParts);
                        continue;
                    case DataType.TypeStart:
                        var typePropertiesLength = numberSerializer.ReadValue(br);
                        var typeProperties = new string[typePropertiesLength];
                        for (var i = 0; i < typePropertiesLength; i++)
                            typeProperties[i] = propertySerializer.ReadValue(br);
                        var length = numberSerializer.ReadValue(br);
                        typesCache.DeserializerSet(typeProperties);
                        typeItem = new DynamicDeserializedType(valueType, typeProperties, length);
                        objectCache.DeserializerSet(typeItem);
                        desStack.Push(typeItem);
                        valueType = null;
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
                        typeProperties = typesCache.DeserializerGet(byteIdx);
                        length = numberSerializer.ReadValue(br);
                        typeItem = new DynamicDeserializedType(valueType, typeProperties, length);
                        objectCache.DeserializerSet(typeItem);
                        desStack.Push(typeItem);
                        valueType = null;
                        continue;
                    case DataType.TypeRefByte:
                        var refByteVal = br.ReadByte();
                        typeProperties = typesCache.DeserializerGet(refByteVal);
                        length = numberSerializer.ReadValue(br);
                        typeItem = new DynamicDeserializedType(valueType, typeProperties, length);
                        objectCache.DeserializerSet(typeItem);
                        desStack.Push(typeItem);
                        valueType = null;
                        continue;
                    case DataType.TypeRefUShort:
                        var refUshortVal = br.ReadUInt16();
                        typeProperties = typesCache.DeserializerGet(refUshortVal);
                        length = numberSerializer.ReadValue(br);
                        typeItem = new DynamicDeserializedType(valueType, typeProperties, length);
                        objectCache.DeserializerSet(typeItem);
                        desStack.Push(typeItem);
                        valueType = null;
                        continue;
                    #endregion

                    #region TypeEnd

                    case DataType.TypeEnd:
                        var lastItem = desStack.Pop();
                        if (desStack.Count > 0)
                        {
                            typeItem = desStack.Peek();
                            typeItem.AddValue(lastItem);
                        }
                        continue;

                    #endregion

                    #region ListStart

                    case DataType.ListStart:
                        typeItem?.ListStart();
                        continue;

                    #endregion

                    #region ListEnd

                    case DataType.ListEnd:
                        typeItem?.ListEnd();
                        continue;

                    #endregion

                    #region DictionaryStart

                    case DataType.DictionaryStart:
                        typeItem?.DictionaryStart();
                        continue;

                    #endregion

                    #region DictionaryEnd

                    case DataType.DictionaryEnd:
                        typeItem?.DictionaryEnd();
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
                    case DataType.RefObjectByte17:
                    case DataType.RefObjectByte18:
                    case DataType.RefObjectByte19:
                    case DataType.RefObjectByte20:
                    case DataType.RefObjectUShort:
                    case DataType.RefObjectByte:
                        var objRef = -1;

                        #region Get Object Reference

                        switch (currentByte)
                        {
                            case DataType.RefObjectByte0:
                                objRef = 0;
                                break;
                            case DataType.RefObjectByte1:
                                objRef = 1;
                                break;
                            case DataType.RefObjectByte2:
                                objRef = 2;
                                break;
                            case DataType.RefObjectByte3:
                                objRef = 3;
                                break;
                            case DataType.RefObjectByte4:
                                objRef = 4;
                                break;
                            case DataType.RefObjectByte5:
                                objRef = 5;
                                break;
                            case DataType.RefObjectByte6:
                                objRef = 6;
                                break;
                            case DataType.RefObjectByte7:
                                objRef = 7;
                                break;
                            case DataType.RefObjectByte8:
                                objRef = 8;
                                break;
                            case DataType.RefObjectByte9:
                                objRef = 9;
                                break;
                            case DataType.RefObjectByte10:
                                objRef = 10;
                                break;
                            case DataType.RefObjectByte11:
                                objRef = 11;
                                break;
                            case DataType.RefObjectByte12:
                                objRef = 12;
                                break;
                            case DataType.RefObjectByte13:
                                objRef = 13;
                                break;
                            case DataType.RefObjectByte14:
                                objRef = 14;
                                break;
                            case DataType.RefObjectByte15:
                                objRef = 15;
                                break;
                            case DataType.RefObjectByte16:
                                objRef = 16;
                                break;
                            case DataType.RefObjectByte17:
                                objRef = 17;
                                break;
                            case DataType.RefObjectByte18:
                                objRef = 18;
                                break;
                            case DataType.RefObjectByte19:
                                objRef = 19;
                                break;
                            case DataType.RefObjectByte20:
                                objRef = 20;
                                break;
                            case DataType.RefObjectByte:
                                objRef = br.ReadByte();
                                break;
                            case DataType.RefObjectUShort:
                                objRef = br.ReadUInt16();
                                break;
                        }

                        #endregion

                        var objValue = objectCache.DeserializerGet(objRef);
                        typeItem?.AddValue(objValue);
                        continue;

                    #endregion

                    #region ValueNull

                    case DataType.ValueNull:
                        typeItem?.AddValue(null);
                        continue;

                    #endregion

                    default:
                        var value = serializersTable.Read(br, currentByte);
                        if (typeItem != null)
                            typeItem.AddValue(value);
                        else
                            return value;
                        break;
                }
            }
            while (desStack.Count > 0 || valueType != null);
            GdStackPool.Store(desStack);
            DeserializersTable.ReturnTable(serializersTable);
            return typeItem;
        }
        #endregion
    }
}
