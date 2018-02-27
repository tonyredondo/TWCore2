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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TWCore.Reflection;
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable MemberCanBeProtected.Local
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable ForCanBeConvertedToForeach

// ReSharper disable UnusedMember.Local
// ReSharper disable NotAccessedField.Local
// ReSharper disable CoVariantArrayConversion
// ReSharper disable MemberCanBePrivate.Local
#pragma warning disable 414

namespace TWCore
{
    /// <summary>
    /// Object Cloner
    /// </summary>
    public static class ObjectCloner
    {
        private static readonly NonBlocking.ConcurrentDictionary<Type, ObjectPlan> ObjectPlans = new NonBlocking.ConcurrentDictionary<Type, ObjectPlan>();

        /// <summary>
        /// Clone a object and copy Properties and Fields
        /// </summary>
        /// <param name="value">Source object</param>
        /// <returns>Destination object</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Clone(object value)
        {
            if (value == null) return null;
            var objPlan = GetObjectPlan(value.GetType());
            var scopeStack = new Stack<Scope>();
            var scope = new Scope();
            scope.Init(objPlan, value);
            scopeStack.Push(scope);
            var initialScope = scope;
            do
            {
                var item = scope.NextIfAvailable();

                #region Get the Current Scope
                if (item == null)
                {
                    var oldScope = scopeStack.Pop();
                    scope = (scopeStack.Count > 0) ? scopeStack.Peek() : null;
                    if (scope != null)
                    {
                        var current = scope.Current();
                        switch (current.PlanType)
                        {
                            case PlanItemType.PropertyReference:
                                var pCurrent = (PropertyReferencePlanItem)current;
                                pCurrent.Property.SetValue(scope.DestinationValue, oldScope.DestinationValue);
                                break;

                            case PlanItemType.FieldReference:
                                var fCurrent = (FieldReferencePlanItem)current;
                                fCurrent.Field.SetValue(scope.DestinationValue, oldScope.DestinationValue);
                                break;

                            case PlanItemType.RuntimeValue:
                                var rvItem = (RuntimeValuePlanItem)current;
                                rvItem.DestinationValue = oldScope.DestinationValue;
                                break;

                            case PlanItemType.List:
                                if (scope.IsArray)
                                {
                                    for (var i = 0; i < oldScope.Plan.Length; i++)
                                    {
                                        var rVal = (RuntimeValuePlanItem)oldScope.Plan[i];
                                        scope.DestinationIListValue[i] = rVal.DestinationValue;
                                    }
                                }
                                else
                                {
                                    for (var i = 0; i < oldScope.Plan.Length; i++)
                                    {
                                        var rVal = (RuntimeValuePlanItem)oldScope.Plan[i];
                                        scope.DestinationIListValue.Add(rVal.DestinationValue);
                                    }
                                }
                                break;

                            case PlanItemType.Dictionary:
                                for (var i = 0; i < oldScope.Plan.Length - 1; i += 2)
                                {
                                    var dKey = ((RuntimeValuePlanItem)oldScope.Plan[i]).DestinationValue;
                                    var dValue = ((RuntimeValuePlanItem)oldScope.Plan[i + 1]).DestinationValue;
                                    scope.DestinationIDictionaryValue[dKey] = dValue;
                                }
                                break;
                        }
                    }
                    continue;
                }
                #endregion

                #region Switch Plan Types
                switch (item.PlanType)
                {
                    case PlanItemType.Type:
                        if (scope.IsArray)
                        {
                            var destValue = Activator.CreateInstance(scope.Type, ((IList)scope.SourceValue).Count);
                            scope.DestinationValue = destValue;
                            scope.DestinationIListValue = (IList)destValue;
                        }
                        else
                        {
                            var destValue = Activator.CreateInstance(scope.Type);
                            scope.DestinationValue = destValue;
                            if (scope.IsIList)
                                scope.DestinationIListValue = (IList)destValue;
                            if (scope.IsIDictionary)
                                scope.DestinationIDictionaryValue = (IDictionary)destValue;
                        }
                        break;

                    case PlanItemType.PropertyValue:
                        var pValueItem = (PropertyValuePlanItem)item;
                        var pValue = pValueItem.Property.GetValue(scope.SourceValue);
                        pValueItem.Property.SetValue(scope.DestinationValue, pValue);
                        break;

                    case PlanItemType.FieldValue:
                        var fValueItem = (FieldValuePlanItem)item;
                        var fValue = fValueItem.Field.GetValue(scope.SourceValue);
                        fValueItem.Field.SetValue(scope.DestinationValue, fValue);
                        break;

                    case PlanItemType.Value:
                        return scope.SourceValue;

                    case PlanItemType.PropertyReference:
                        var pReferenceItem = (PropertyReferencePlanItem)item;
                        var pReferenceValue = pReferenceItem.Property.GetValue(scope.SourceValue);
                        if (pReferenceValue != null)
                        {
                            scope = new Scope();
                            scope.Init(GetObjectPlan(pReferenceValue.GetType()), pReferenceValue);
                            scopeStack.Push(scope);
                        }
                        break;

                    case PlanItemType.FieldReference:
                        var fReferenceItem = (FieldReferencePlanItem)item;
                        var fReferenceValue = fReferenceItem.Field.GetValue(scope.SourceValue);
                        if (fReferenceValue != null)
                        {
                            scope = new Scope();
                            scope.Init(GetObjectPlan(fReferenceValue.GetType()), fReferenceValue);
                            scopeStack.Push(scope);
                        }
                        break;

                    case PlanItemType.List:
                        var lType = (ListPlanItem)item;
                        var iList = (IList)scope.SourceValue;
                        if (iList.Count > 0)
                        {
                            var aPlan = new RuntimeValuePlanItem[iList.Count];
                            for (var i = 0; i < iList.Count; i++)
                            {
                                var itemList = iList[i];
                                var itemType = itemList?.GetType() ?? lType.InnerType;
                                var rtmVal = new RuntimeValuePlanItem();
                                rtmVal.Init(itemType, IsValueType(itemType, itemType.GetTypeInfo()), itemList);
                                aPlan[i] = rtmVal;
                            }
                            scope = new Scope();
                            scope.Init(aPlan, scope.Type);
                            scopeStack.Push(scope);
                        }
                        break;

                    case PlanItemType.Dictionary:
                        var dctItem = (DictionaryPlanItem)item;
                        var iDictio = (IDictionary)scope.SourceValue;
                        if (iDictio.Count > 0)
                        {
                            var aPlan = new RuntimeValuePlanItem[iDictio.Count * 2];
                            var aIdx = 0;
                            foreach (var keyValue in iDictio.Keys)
                            {
                                var valueValue = iDictio[keyValue];
                                var aPlanKeyVal = new RuntimeValuePlanItem();
                                aPlanKeyVal.Init(keyValue.GetType(), dctItem.KeyIsValue, keyValue);
                                aPlan[aIdx++] = aPlanKeyVal;

                                var aPlanValVal = new RuntimeValuePlanItem();
                                aPlanValVal.Init(valueValue?.GetType() ?? dctItem.ValueType, dctItem.ValueIsValue, valueValue);
                                aPlan[aIdx++] = aPlanValVal;
                            }
                            scope = new Scope();
                            scope.Init(aPlan, scope.Type);
                            scopeStack.Push(scope);
                        }
                        break;

                    case PlanItemType.RuntimeValue:
                        var rvItem = (RuntimeValuePlanItem)item;
                        if (rvItem.IsValue)
                            rvItem.DestinationValue = rvItem.SourceValue;
                        else
                        {
                            scope = new Scope();
                            scope.Init(GetObjectPlan(rvItem.SourceValue?.GetType() ?? rvItem.Type), rvItem.SourceValue);
                            scopeStack.Push(scope);
                        }
                        break;
                }
                #endregion

            } while (scope != null);
            return initialScope.DestinationValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ObjectPlan GetObjectPlan(Type type)
        {
			var pool = ReferencePool<HashSet<Type>>.Shared;
			var hSet = pool.New();
			ObjectPlan plan;
			try
			{
				plan = GetObjectPlan(hSet, type);
			}
			finally
			{
				hSet.Clear();
				pool.Store(hSet);
			}
			return plan;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ObjectPlan GetObjectPlan(HashSet<Type> currentObjectPlanTypes, Type type)
        {
            return ObjectPlans.GetOrAdd(type, iType =>
            {
                var typeInfo = iType.GetTypeInfo();
                var genTypeDefinition = typeInfo.IsGenericType ? typeInfo.GetGenericTypeDefinition() : null;

                if (genTypeDefinition == typeof(Nullable<>))
                {
                    //Nullable Type
                    iType = Nullable.GetUnderlyingType(iType);
                    typeInfo = iType.GetTypeInfo();
                }

                var isValue = IsValueType(iType, typeInfo);

                if (isValue)
                {
                    var oPlan = new ObjectPlan();
                    var plan = new ObjectPlanItem[] { new ValuePlanItem(iType) };
                    oPlan.Init(plan, iType, false, false, false);
                    return oPlan;
                }
                else
                {
                    currentObjectPlanTypes.Add(iType);
                    var ifaces = typeInfo.ImplementedInterfaces;
                    var ifacesArray = ifaces as Type[] ?? ifaces.ToArray();
                    var ilist = ifacesArray.FirstOrDefault(i => i == typeof(IList) || (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)));
                    var isIList = ilist != null;
                    var idictio = ifacesArray.FirstOrDefault(i => i == typeof(IDictionary) || i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
                    var isIDictionary = idictio != null;

                    var lPlan = new List<ObjectPlanItem>();
                    var tStart = new TypePlanItem(iType)
                    {
                        IsIList = isIList,
                        IsIDictionary = isIDictionary
                    };
                    lPlan.Add(tStart);

                    #region Fields
                    var fields = type.GetRuntimeFields().ToArray();
                    foreach (var field in fields)
                    {
                        if (field.IsInitOnly) continue;
                        if (field.IsStatic) continue;
                        if (field.IsPrivate) continue;
                        var fieldType = field.FieldType;
                        var fieldTypeInfo = fieldType.GetTypeInfo();
                        var fieldIsNullable = fieldTypeInfo.IsGenericType && fieldTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
                        if (fieldIsNullable)
                            fieldType = Nullable.GetUnderlyingType(fieldType);
                        var isFieldValue = IsValueType(fieldType, fieldType.GetTypeInfo());
                        if (isFieldValue)
                            lPlan.Add(new FieldValuePlanItem(field, fieldIsNullable));
                        else
                        {
                            lPlan.Add(new FieldReferencePlanItem(field));
                            if (!currentObjectPlanTypes.Contains(fieldType))
                                GetObjectPlan(currentObjectPlanTypes, fieldType);
                        }
                    }
                    #endregion

                    #region Properties
                    var properties = type.GetRuntimeProperties().ToArray();
                    foreach (var prop in properties)
                    {
                        if (!prop.CanRead || !prop.CanWrite) continue;
                        if (isIList && prop.Name == "Capacity") continue;
                        if (prop.GetIndexParameters().Length > 0) continue;
                        var propType = prop.PropertyType;
                        var propTypeInfo = propType.GetTypeInfo();
                        var propIsNullable = propTypeInfo.IsGenericType && propTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
                        if (propIsNullable)
                            propType = Nullable.GetUnderlyingType(propType);
                        var isPropValue = IsValueType(propType, propType.GetTypeInfo());
                        if (isPropValue)
                            lPlan.Add(new PropertyValuePlanItem(prop, propIsNullable));
                        else
                        {
                            lPlan.Add(new PropertyReferencePlanItem(prop));
                            if (!currentObjectPlanTypes.Contains(propType))
                                GetObjectPlan(currentObjectPlanTypes, propType);
                        }
                    }
                    #endregion

                    #region List
                    if (isIList)
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
                        }
                        var innerTypeInfo = innerType.GetTypeInfo();
                        var innerIsNullable = innerTypeInfo.IsGenericType && innerTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
                        var innerType2 = innerType;
                        if (innerIsNullable)
                            innerType2 = Nullable.GetUnderlyingType(innerType2);
                        var innerIsValue = IsValueType(innerType2, innerType2.GetTypeInfo());

                        lPlan.Add(new ListPlanItem(type, innerType, innerIsValue, innerIsNullable));
                        if (!currentObjectPlanTypes.Contains(innerType))
                            GetObjectPlan(currentObjectPlanTypes, innerType);
                    }
                    #endregion

                    #region Dictionary
                    if (isIDictionary)
                    {
                        //KeyValye Type
                        var types = idictio.GenericTypeArguments;
                        var keyType = types[0];
                        var keyTypeInfo = keyType.GetTypeInfo();
                        var keyIsNullable = keyTypeInfo.IsGenericType && keyTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
                        var keyTypeTmp = keyType;
                        var keyTypeInfoTmp = keyTypeInfo;
                        if (keyIsNullable)
                        {
                            keyTypeTmp = Nullable.GetUnderlyingType(keyType);
                            keyTypeInfoTmp = keyTypeTmp.GetTypeInfo();
                        }
                        var keyIsValue = IsValueType(keyTypeTmp, keyTypeInfoTmp);

                        var valueType = types[1];
                        var valueTypeInfo = valueType.GetTypeInfo();
                        var valueIsNullable = valueTypeInfo.IsGenericType && valueTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
                        var valueTypeTmp = valueType;
                        var valueTypeInfoTmp = valueTypeInfo;
                        if (valueIsNullable)
                        {
                            valueTypeTmp = Nullable.GetUnderlyingType(valueTypeTmp);
                            valueTypeInfoTmp = valueTypeTmp.GetTypeInfo();
                        }
                        var valueIsValue = IsValueType(valueTypeTmp, valueTypeInfoTmp);

                        lPlan.Add(new DictionaryPlanItem(type, keyType, keyIsValue, keyIsNullable, valueType, valueIsValue, valueIsNullable));

                        if (!currentObjectPlanTypes.Contains(keyType))
                            GetObjectPlan(currentObjectPlanTypes, keyType);

                        if (!currentObjectPlanTypes.Contains(valueType))
                            GetObjectPlan(currentObjectPlanTypes, valueType);
                    }
                    #endregion

                    currentObjectPlanTypes.Remove(iType);

                    var oPlan = new ObjectPlan();
                    oPlan.Init(lPlan.ToArray(), iType, isIList, typeInfo.IsArray, isIDictionary);
                    return oPlan;
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsValueType(Type type, TypeInfo typeInfo)
        {
            if (typeInfo.IsEnum)
                return true;
            if (type == typeof(byte))
                return true;
            if (type == typeof(sbyte))
                return true;
            if (type == typeof(short))
                return true;
            if (type == typeof(ushort))
                return true;
            if (type == typeof(int))
                return true;
            if (type == typeof(uint))
                return true;
            if (type == typeof(long))
                return true;
            if (type == typeof(ulong))
                return true;
            if (type == typeof(float))
                return true;
            if (type == typeof(double))
                return true;
            if (type == typeof(decimal))
                return true;
            if (type == typeof(string))
                return true;
            if (type == typeof(bool))
                return true;
            if (type == typeof(char))
                return true;
            if (type == typeof(DateTime))
                return true;
            if (type == typeof(DateTimeOffset))
                return true;
            if (type == typeof(Guid))
                return true;
            if (type == typeof(TimeSpan))
                return true;
            return false;
        }

        #region Scope
        private class Scope : ObjectPlan
        {
            private int _index;
            public object SourceValue;
            public object DestinationValue;
            public IList DestinationIListValue;
            public IDictionary DestinationIDictionaryValue;
            //public int DestinationIListIndex = 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Init()
            {
                Plan = null;
                Type = null;
                PlanLength = 0;
                SourceValue = null;
                _index = 0;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Init(ObjectPlan plan, object value)
            {
                if (plan != null)
                {
                    Plan = plan.Plan;
                    Type = plan.Type;
                    IsIList = plan.IsIList;
                    IsIDictionary = plan.IsIDictionary;
                    PlanLength = plan.Plan.Length;
                }
                SourceValue = value;
                _index = 0;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Init(RuntimeValuePlanItem[] runtimePlan, Type type)
            {
                if (runtimePlan != null)
                {
                    Plan = runtimePlan;
                    Type = type;
                    IsIList = false;
                    IsIDictionary = false;
                    PlanLength = runtimePlan.Length;
                }
                _index = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override void ReplacePlan(ObjectPlanItem[] plan)
            {
                Plan = plan;
                PlanLength = plan?.Length ?? 0;
                _index = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ObjectPlanItem Next() => Plan[_index++];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ObjectPlanItem Current() => Plan[_index - 1];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool HasPendingIndex() => _index < PlanLength;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ObjectPlanItem NextIfAvailable() => _index < PlanLength ? Plan[_index++] : null;
        }
        #endregion

        #region Object Plan

        private class ObjectPlan
        {
            public ObjectPlanItem[] Plan;
            public Type Type;
            public bool IsIList;
            public bool IsArray;
            public bool IsIDictionary;
            public int PlanLength;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Init(ObjectPlanItem[] plan, Type type, bool isIList, bool isArray, bool isIDictionary)
            {
                Plan = plan;
                PlanLength = plan?.Length ?? 0;
                Type = type;
                IsIList = isIList;
                IsArray = isArray;
                IsIDictionary = isIDictionary;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public virtual void ReplacePlan(ObjectPlanItem[] plan)
            {
                Plan = plan;
                PlanLength = plan?.Length ?? 0;
            }
        }

        #region Plan Items

        private enum PlanItemType
        {
            Type,
            List,
            Dictionary,
            PropertyValue,
            PropertyReference,
            FieldValue,
            FieldReference,
            Value,
            RuntimeValue
        }

        private abstract class ObjectPlanItem
        {
            public PlanItemType PlanType;
            public Type Type;
        }

        #region Type
        private class TypePlanItem : ObjectPlanItem
        {
            public bool IsArray;
            public bool IsIList;
            public bool IsIDictionary;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TypePlanItem(Type type)
            {
                PlanType = PlanItemType.Type;
                Type = type;
                IsArray = type.IsArray;
            }
        }
        #endregion

        #region List
        private class ListPlanItem : ObjectPlanItem
        {
            public Type InnerType;
            public bool InnerIsValue;
            public bool InnerIsNullable;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ListPlanItem(Type listType, Type innerType, bool innerIsValue, bool innerIsNullable)
            {
                PlanType = PlanItemType.List;
                Type = listType;
                InnerType = innerType;
                InnerIsValue = innerIsValue;
                InnerIsNullable = innerIsNullable;
            }
        }
        #endregion

        #region Dictionary
        private class DictionaryPlanItem : ObjectPlanItem
        {
            public Type KeyType;
            public bool KeyIsValue;
            public bool KeyIsNullable;

            public Type ValueType;
            public bool ValueIsValue;
            public bool ValueIsNullable;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public DictionaryPlanItem(Type type, Type keyType, bool keyIsValue, bool keyIsNullable, Type valueType, bool valueIsValue, bool valueIsNullable)
            {
                PlanType = PlanItemType.Dictionary;
                Type = type;

                KeyType = keyType;
                KeyIsValue = keyIsValue;
                KeyIsNullable = keyIsNullable;

                ValueType = valueType;
                ValueIsValue = valueIsValue;
                ValueIsNullable = valueIsNullable;
            }
        }
        #endregion

        #region FieldValue
        private class FieldValuePlanItem : ObjectPlanItem
        {
            public string Name;
            public FieldInfo Field;
            public bool IsNullable;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public FieldValuePlanItem(FieldInfo fInfo, bool isNullable)
            {
                PlanType = PlanItemType.FieldValue;
                Type = fInfo.FieldType;
                IsNullable = isNullable;
                Field = fInfo;
                Name = fInfo.Name;
            }
        }
        #endregion

        #region PropertyValue
        private class PropertyValuePlanItem : ObjectPlanItem
        {
            public string Name;
            public FastPropertyInfo Property;
            public bool IsNullable;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public PropertyValuePlanItem(PropertyInfo pInfo, bool isNullable)
            {
                PlanType = PlanItemType.PropertyValue;
                Type = pInfo.PropertyType;
                IsNullable = isNullable;
                Property = pInfo.GetFastPropertyInfo();
                Name = pInfo.Name;
            }
        }
        #endregion

        #region FieldReference
        private class FieldReferencePlanItem : ObjectPlanItem
        {
            public string Name;
            public FieldInfo Field;
            public object DefaultValue;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public FieldReferencePlanItem(FieldInfo fInfo)
            {
                PlanType = PlanItemType.FieldReference;
                Type = fInfo.FieldType;
                DefaultValue = Type.GetTypeInfo().IsValueType ? Activator.CreateInstance(Type) : null;
                Field = fInfo;
                Name = fInfo.Name;
            }
        }
        #endregion

        #region PropertyReference
        private class PropertyReferencePlanItem : ObjectPlanItem
        {
            public string Name;
            public FastPropertyInfo Property;
            public object DefaultValue;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public PropertyReferencePlanItem(PropertyInfo pInfo)
            {
                PlanType = PlanItemType.PropertyReference;
                Type = pInfo.PropertyType;
                DefaultValue = Type.GetTypeInfo().IsValueType ? Activator.CreateInstance(Type) : null;
                Property = pInfo.GetFastPropertyInfo();
                Name = pInfo.Name;
            }
        }
        #endregion

        #region Value
        private class ValuePlanItem : ObjectPlanItem
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValuePlanItem(Type type)
            {
                PlanType = PlanItemType.Value;
                Type = type;
            }
        }
        #endregion

        #region RuntimeValue
        private class RuntimeValuePlanItem : ObjectPlanItem
        {
            public bool IsValue;
            public object SourceValue;
            public object DestinationValue;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public RuntimeValuePlanItem()
            {
                PlanType = PlanItemType.RuntimeValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Init(Type type, bool isValue, object sourceValue)
            {
                Type = type;
                SourceValue = sourceValue;
                IsValue = isValue;
                DestinationValue = null;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Init()
            {
                Type = null;
                SourceValue = null;
                IsValue = false;
                DestinationValue = null;
            }
        }
        #endregion

        #endregion

        #endregion

    }
}
