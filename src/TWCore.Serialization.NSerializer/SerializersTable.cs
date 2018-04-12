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

using NonBlocking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using TWCore.Reflection;
using TWCore.Serialization.NSerializer.Types;

namespace TWCore.Serialization.NSerializer
{
    public partial class SerializersTable : TypeSerializer
    {
        private static ConcurrentDictionary<Type, TypeDescriptor> Descriptors = new ConcurrentDictionary<Type, TypeDescriptor>();
        private readonly SerializerCache<Type> _typeCache = new SerializerCache<Type>();
        private readonly SerializerCache<object> _objectCache = new SerializerCache<object>();

        #region Internal Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Init()
        {
            InitDateTimeOffset();
            InitDateTime();
            InitGuid();
            InitNumber();
            InitString();
            InitTimeSpan();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Clear()
        {
            ClearDateTimeOffset();
            ClearDateTime();
            ClearGuid();
            ClearNumber();
            ClearString();
            ClearTimeSpan();
            _typeCache.Clear();
            _objectCache.Clear();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStream(Stream stream)
        {
            _stream = stream;
        }
        #endregion

        #region Write Values
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(bool[] value)
        {
            WriteInt(DataBytesDefinition.BoolArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(char[] value)
        {
            WriteInt(DataBytesDefinition.CharArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTimeOffset[] value)
        {
            WriteInt(DataBytesDefinition.DateTimeOffsetArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTime[] value)
        {
            WriteInt(DataBytesDefinition.DateTimeArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Enum[] value)
        {
            WriteInt(DataBytesDefinition.EnumArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Guid[] value)
        {
            WriteInt(DataBytesDefinition.GuidArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(decimal[] value)
        {
            WriteInt(DataBytesDefinition.DecimalArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(double[] value)
        {
            WriteInt(DataBytesDefinition.DoubleArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(float[] value)
        {
            WriteInt(DataBytesDefinition.FloatArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(long[] value)
        {
            WriteInt(DataBytesDefinition.LongArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ulong[] value)
        {
            WriteInt(DataBytesDefinition.ULongArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(int[] value)
        {
            WriteInt(DataBytesDefinition.IntArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(uint[] value)
        {
            WriteInt(DataBytesDefinition.UIntArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(short[] value)
        {
            WriteInt(DataBytesDefinition.ShortArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ushort[] value)
        {
            WriteInt(DataBytesDefinition.UShortArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(sbyte[] value)
        {
            WriteInt(DataBytesDefinition.SByteArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(string[] value)
        {
            WriteInt(DataBytesDefinition.StringArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(TimeSpan[] value)
        {
            WriteInt(DataBytesDefinition.TimeSpanArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<bool> value)
        {
            WriteInt(DataBytesDefinition.BoolList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<char> value)
        {
            WriteInt(DataBytesDefinition.CharList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<DateTimeOffset> value)
        {
            WriteInt(DataBytesDefinition.DateTimeOffsetList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<DateTime> value)
        {
            WriteInt(DataBytesDefinition.DateTimeList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<Enum> value)
        {
            WriteInt(DataBytesDefinition.EnumList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<Guid> value)
        {
            WriteInt(DataBytesDefinition.GuidList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<decimal> value)
        {
            WriteInt(DataBytesDefinition.DecimalList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<double> value)
        {
            WriteInt(DataBytesDefinition.DoubleList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<float> value)
        {
            WriteInt(DataBytesDefinition.FloatList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<long> value)
        {
            WriteInt(DataBytesDefinition.LongList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<ulong> value)
        {
            WriteInt(DataBytesDefinition.ULongList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<int> value)
        {
            WriteInt(DataBytesDefinition.IntList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<uint> value)
        {
            WriteInt(DataBytesDefinition.UIntList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<short> value)
        {
            WriteInt(DataBytesDefinition.ShortList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<ushort> value)
        {
            WriteInt(DataBytesDefinition.UShortList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<sbyte> value)
        {
            WriteInt(DataBytesDefinition.SByteList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<string> value)
        {
            WriteInt(DataBytesDefinition.StringList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<TimeSpan> value)
        {
            WriteInt(DataBytesDefinition.TimeSpanList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue<T>(List<T> valueList)
        {
            if (valueList == null)
            {
                _stream.WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            var vType = typeof(T);
            if (_objectCache.SerializerTryGetValue(valueList, out var oIdx))
            {
                WriteInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.SerializerSet(valueList);

            var descriptor = Descriptors.GetOrAdd(vType, type => new TypeDescriptor(type));
            if (_typeCache.SerializerTryGetValue(vType, out var tIdx))
            {
                WriteInt(DataBytesDefinition.RefType, tIdx);
            }
            else
            {
                WriteInt(DataBytesDefinition.TypeStart, descriptor.Definition.Length);
                _stream.Write(descriptor.Definition, 0, descriptor.Definition.Length);
                _typeCache.SerializerSet(vType);
            }
            var count = valueList.Count;
            WriteInt(DataBytesDefinition.ListStart, count);
            for (var i = 0; i < count; i++)
                WriteValue(valueList[i]);
            _stream.WriteByte(DataBytesDefinition.TypeEnd);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue<T>(T value)
        {
            if (value == null)
            {
                _stream.WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            if (_objectCache.SerializerTryGetValue(value, out var oIdx))
            {
                WriteInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.SerializerSet(value);

            var vType = typeof(T);
            var descriptor = Descriptors.GetOrAdd(vType, type => new TypeDescriptor(type));

            if (_typeCache.SerializerTryGetValue(vType, out var tIdx))
            {
                WriteInt(DataBytesDefinition.RefType, tIdx);
            }
            else
            {
                WriteInt(DataBytesDefinition.TypeStart, descriptor.Definition.Length);
                _stream.Write(descriptor.Definition, 0, descriptor.Definition.Length);
                _typeCache.SerializerSet(vType);
            }
            if (descriptor.IsINSerializable)
                ((INSerializable)value).Serialize(this);
            else
                InternalWriteValue(value, descriptor);
            _stream.WriteByte(DataBytesDefinition.TypeEnd);
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInnerValue(object value)
        {
            #region Values
            switch (value)
            {
                case null:
                    _stream.WriteByte(DataBytesDefinition.ValueNull);
                    return;
                case bool cValue:
                    WriteValue(cValue);
                    return;
                case byte[] cValue:
                    WriteValue(cValue);
                    return;
                case char cValue:
                    WriteValue(cValue);
                    return;
                case DateTimeOffset cValue:
                    WriteValue(cValue);
                    return;
                case DateTime cValue:
                    WriteValue(cValue);
                    return;
                case Enum cValue:
                    WriteValue(cValue);
                    return;
                case Guid cValue:
                    WriteValue(cValue);
                    return;
                case decimal cValue:
                    WriteValue(cValue);
                    return;
                case double cValue:
                    WriteValue(cValue);
                    return;
                case float cValue:
                    WriteValue(cValue);
                    return;
                case long cValue:
                    WriteValue(cValue);
                    return;
                case ulong cValue:
                    WriteValue(cValue);
                    return;
                case int cValue:
                    WriteValue(cValue);
                    return;
                case uint cValue:
                    WriteValue(cValue);
                    return;
                case short cValue:
                    WriteValue(cValue);
                    return;
                case ushort cValue:
                    WriteValue(cValue);
                    return;
                case byte cValue:
                    WriteValue(cValue);
                    return;
                case sbyte cValue:
                    WriteValue(cValue);
                    return;
                case SerializedObject cValue:
                    WriteValue(cValue);
                    return;
                case string cValue:
                    WriteValue(cValue);
                    return;
                case TimeSpan cValue:
                    WriteValue(cValue);
                    return;
            }
            #endregion

            var vType = value.GetType();

            if (_objectCache.SerializerTryGetValue(value, out var oIdx))
            {
                WriteInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.SerializerSet(value);

            var descriptor = Descriptors.GetOrAdd(vType, type => new TypeDescriptor(type));
            if (_typeCache.SerializerTryGetValue(vType, out var tIdx))
            {
                WriteInt(DataBytesDefinition.RefType, tIdx);
            }
            else
            {
                WriteInt(DataBytesDefinition.TypeStart, descriptor.Definition.Length);
                _stream.Write(descriptor.Definition, 0, descriptor.Definition.Length);
                _typeCache.SerializerSet(vType);
            }
            if (descriptor.IsINSerializable)
                ((INSerializable)value).Serialize(this);
            else
                InternalWriteValue(value, descriptor);
            _stream.WriteByte(DataBytesDefinition.TypeEnd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InternalWriteValue(object value, TypeDescriptor descriptor)
        {
            //Write Properties
            if (descriptor.Properties.Count > 0)
            {
                WriteInt(DataBytesDefinition.PropertiesStart, descriptor.Properties.Count);
                foreach (var prop in descriptor.Properties)
                    WriteInnerValue(prop.Value.GetValue(value));
            }

            //Write Array if contains
            if (descriptor.IsArray)
            {
                var aValue = (Array)value;
                WriteInt(DataBytesDefinition.ArrayStart, aValue.Length);
                for (var i = 0; i < aValue.Length; i++)
                    WriteInnerValue(aValue.GetValue(i));
                return;
            }

            //Write List if contains
            if (descriptor.IsList)
            {
                var iValue = (IList)value;
                var count = iValue.Count;
                WriteInt(DataBytesDefinition.ListStart, count);
                for (var i = 0; i < count; i++)
                    WriteInnerValue(iValue[i]);
                return;
            }

            //Write Dictionary if contains
            if (descriptor.IsDictionary)
            {
                var iValue = (IDictionary)value;
                var count = iValue.Count;
                WriteInt(DataBytesDefinition.DictionaryStart, count);
                foreach (DictionaryEntry item in iValue)
                {
                    WriteInnerValue(item.Key);
                    WriteInnerValue(item.Value);
                }
                return;
            }
        }


        public struct TypeDescriptor
        {
            public string TypeName;
            public ActivatorDelegate Activator;
            public Dictionary<string, FastPropertyInfo> Properties;
            public bool IsArray;
            public bool IsList;
            public bool IsDictionary;
            public bool IsINSerializable;
            public byte[] Definition;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TypeDescriptor(Type type)
            {
                TypeName = type.GetTypeName();
                Activator = Factory.Accessors.CreateActivator(type);
                Properties = new Dictionary<string, FastPropertyInfo>();
                var ifaces = type.GetInterfaces();
                var isIList = ifaces.Any(i => i == typeof(IList) || (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)));
                var isIDictionary = ifaces.Any(i => i == typeof(IDictionary) || (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)));
                IsINSerializable = ifaces.Any(i => i == typeof(INSerializable));
                var runtimeProperties = type.GetRuntimeProperties();
                foreach (var prop in runtimeProperties)
                {
                    if (prop.IsSpecialName || !prop.CanRead || !prop.CanWrite) continue;
                    if (prop.GetAttribute<NonSerializeAttribute>() != null) continue;
                    if (prop.GetIndexParameters().Length > 0) continue;
                    if (isIList && prop.Name == "Capacity") continue;
                    var fProp = prop.GetFastPropertyInfo();
                    Properties[fProp.Name] = fProp;
                }
                IsArray = type.IsArray;
                if (!IsArray)
                {
                    IsDictionary = isIDictionary;
                    if (IsDictionary)
                        IsList = false;
                    else
                        IsList = isIList;
                }
                else
                {
                    IsList = false;
                    IsDictionary = false;
                }
                var defText = TypeName + ";" + Properties.Keys.Join(";");
                Definition = Encoding.UTF8.GetBytes(defText);
            }
        }
    }
}
