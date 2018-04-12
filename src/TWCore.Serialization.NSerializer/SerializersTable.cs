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
        #endregion

        //
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue<T>(List<T> valueList)
        {
            var vType = typeof(T);
            _stream.WriteByte(DataBytesDefinition.TypeStart);
            WriteValue(vType.GetTypeName());
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
            var oIdx = _objectCache.SerializerGet(value);
            if (oIdx > -1)
            {
                if (oIdx <= byte.MaxValue)
                    WriteByte(DataBytesDefinition.RefObjectByte, (byte)oIdx);
                else
                    WriteUshort(DataBytesDefinition.RefObjectUShort, (ushort)oIdx);
                return;
            }
            _objectCache.SerializerSet(value);
            
            var vType = typeof(T);
            var descriptor = Descriptors.GetOrAdd(vType, type => new TypeDescriptor(type));
            var tIdx = _typeCache.SerializerGet(vType);
            if (tIdx > -1)
            {
                if (tIdx <= byte.MaxValue)
                    WriteByte(DataBytesDefinition.RefTypeByte, (byte)tIdx);
                else
                    WriteUshort(DataBytesDefinition.RefTypeUShort, (ushort)tIdx);
            }
            else
            {
                WriteInt(DataBytesDefinition.TypeStart, descriptor.Definition.Length);
                _stream.Write(descriptor.Definition, 0, descriptor.Definition.Length);
                _typeCache.SerializerSet(vType);
            }
            if (value is INSerializable instance)
                instance.Serialize(this);
            else
                InternalWriteValue(value, descriptor);
            _stream.WriteByte(DataBytesDefinition.TypeEnd);
        }

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
                case bool[] cValue:
                    WriteInt(DataBytesDefinition.BoolArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;
                case char[] cValue:
                    WriteInt(DataBytesDefinition.CharArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;
                case DateTimeOffset[] cValue:
                    WriteInt(DataBytesDefinition.DateTimeOffsetArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;
                case DateTime[] cValue:
                    WriteInt(DataBytesDefinition.DateTimeArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;
                case Enum[] cValue:
                    WriteInt(DataBytesDefinition.EnumArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;
                case Guid[] cValue:
                    WriteInt(DataBytesDefinition.GuidArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;
                case decimal[] cValue:
                    WriteInt(DataBytesDefinition.DecimalArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;
                case double[] cValue:
                    WriteInt(DataBytesDefinition.DoubleArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;
                case float[] cValue:
                    WriteInt(DataBytesDefinition.FloatArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;
                case long[] cValue:
                    WriteInt(DataBytesDefinition.LongArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;
                case ulong[] cValue:
                    WriteInt(DataBytesDefinition.ULongArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;
                case int[] cValue:
                    WriteInt(DataBytesDefinition.IntArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;
                case uint[] cValue:
                    WriteInt(DataBytesDefinition.UIntArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;
                case short[] cValue:
                    WriteInt(DataBytesDefinition.ShortArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;
                case ushort[] cValue:
                    WriteInt(DataBytesDefinition.UShortArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;
                case sbyte[] cValue:
                    WriteInt(DataBytesDefinition.SByteArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;
                case string[] cValue:
                    WriteInt(DataBytesDefinition.StringArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;
                case TimeSpan[] cValue:
                    WriteInt(DataBytesDefinition.TimeSpanArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        WriteValue(cValue[i]);
                    return;

                case List<bool> cValue:
                    WriteInt(DataBytesDefinition.BoolList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
                case List<char> cValue:
                    WriteInt(DataBytesDefinition.CharList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
                case List<DateTimeOffset> cValue:
                    WriteInt(DataBytesDefinition.DateTimeOffsetList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
                case List<DateTime> cValue:
                    WriteInt(DataBytesDefinition.DateTimeList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
                case List<Enum> cValue:
                    WriteInt(DataBytesDefinition.EnumList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
                case List<Guid> cValue:
                    WriteInt(DataBytesDefinition.GuidList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
                case List<decimal> cValue:
                    WriteInt(DataBytesDefinition.DecimalList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
                case List<double> cValue:
                    WriteInt(DataBytesDefinition.DoubleList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
                case List<float> cValue:
                    WriteInt(DataBytesDefinition.FloatList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
                case List<long> cValue:
                    WriteInt(DataBytesDefinition.LongList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
                case List<ulong> cValue:
                    WriteInt(DataBytesDefinition.ULongList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
                case List<int> cValue:
                    WriteInt(DataBytesDefinition.IntList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
                case List<uint> cValue:
                    WriteInt(DataBytesDefinition.UIntList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
                case List<short> cValue:
                    WriteInt(DataBytesDefinition.ShortList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
                case List<ushort> cValue:
                    WriteInt(DataBytesDefinition.UShortList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
                case List<sbyte> cValue:
                    WriteInt(DataBytesDefinition.SByteList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
                case List<string> cValue:
                    WriteInt(DataBytesDefinition.StringList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
                case List<TimeSpan> cValue:
                    WriteInt(DataBytesDefinition.TimeSpanList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        WriteValue(cValue[i]);
                    return;
            }
            #endregion

            var oIdx = _objectCache.SerializerGet(value);
            if (oIdx > -1)
            {
                if (oIdx <= byte.MaxValue)
                    WriteByte(DataBytesDefinition.RefObjectByte, (byte)oIdx);
                else
                    WriteUshort(DataBytesDefinition.RefObjectUShort, (ushort)oIdx);
                return;
            }
            _objectCache.SerializerSet(value);

            var vType = value.GetType();
            var descriptor = Descriptors.GetOrAdd(vType, type => new TypeDescriptor(type));
            var tIdx = _typeCache.SerializerGet(vType);
            if (tIdx > -1)
            {
                if (tIdx <= byte.MaxValue)
                    WriteByte(DataBytesDefinition.RefTypeByte, (byte)tIdx);
                else
                    WriteUshort(DataBytesDefinition.RefTypeUShort, (ushort)tIdx);
            }
            else
            {
                WriteInt(DataBytesDefinition.TypeStart, descriptor.Definition.Length);
                _stream.Write(descriptor.Definition, 0, descriptor.Definition.Length);
                _typeCache.SerializerSet(vType);
            }
            if (value is INSerializable instance)
                instance.Serialize(this);
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
                {
                    var pValue = prop.Value.GetValue(value);
                    if (pValue == null) continue;
                    WriteInnerValue(pValue);
                }
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

        private static ConcurrentDictionary<Type, TypeDescriptor> Descriptors = new ConcurrentDictionary<Type, TypeDescriptor>();

        public struct TypeDescriptor
        {
            public string TypeName;
            public ActivatorDelegate Activator;
            public Dictionary<string, FastPropertyInfo> Properties;
            public bool IsArray;
            public bool IsList;
            public bool IsDictionary;
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
