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

namespace TWCore.Serialization.NSerializer
{
    public partial class SerializersTable
    {
        private static Dictionary<Type, (MethodInfo Method, MethodAccessorDelegate Accessor)> WriteValues = new Dictionary<Type, (MethodInfo Method, MethodAccessorDelegate Accessor)>();
        private static ConcurrentDictionary<Type, TypeDescriptor> Descriptors = new ConcurrentDictionary<Type, TypeDescriptor>();
        protected Stream _stream;
        private readonly byte[] _buffer = new byte[9];
        private readonly SerializerCache<Type> _typeCache = new SerializerCache<Type>();
        private readonly SerializerCache<object> _objectCache = new SerializerCache<object>();
        private readonly object[] _paramObj = new object[1];

        static SerializersTable()
        {
            var methods = typeof(SerializersTable).GetMethods();
            foreach(var method in methods)
            {
                if (method.Name == nameof(WriteValue))
                {
                    var parameters = method.GetParameters();
                    WriteValues[parameters[0].ParameterType] = (method, method.IsGenericMethod ? null : Factory.Accessors.BuildMethodAccessor(method));
                }
            }
        }

        #region Internal Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            InitDateTimeOffset();
            InitDateTime();
            InitGuid();
            InitNumber();
            InitString();
            InitTimeSpan();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
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
            if (_objectCache.TryGetValue(valueList, out var oIdx))
            {
                WriteInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(valueList);

            var descriptor = Descriptors.GetOrAdd(vType, type => new TypeDescriptor(type));
            if (_typeCache.TryGetValue(vType, out var tIdx))
            {
                WriteInt(DataBytesDefinition.RefType, tIdx);
            }
            else
            {
                WriteInt(DataBytesDefinition.TypeStart, descriptor.Definition.Length);
                _stream.Write(descriptor.Definition, 0, descriptor.Definition.Length);
                _typeCache.Set(vType);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);

            var vType = typeof(T);
            var descriptor = Descriptors.GetOrAdd(vType, type => new TypeDescriptor(type));

            if (_typeCache.TryGetValue(vType, out var tIdx))
            {
                WriteInt(DataBytesDefinition.RefType, tIdx);
            }
            else
            {
                WriteInt(DataBytesDefinition.TypeStart, descriptor.Definition.Length);
                _stream.Write(descriptor.Definition, 0, descriptor.Definition.Length);
                _typeCache.Set(vType);
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
            if (value == null)
            {
                _stream.WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            var vType = value.GetType();
            if (WriteValues.TryGetValue(vType, out var mTuple))
            {
                _paramObj[0] = value;
                if (mTuple.Accessor != null)
                    mTuple.Accessor(this, _paramObj);
                else
                    mTuple.Method.Invoke(this, _paramObj);
                return;
            }
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);

            var descriptor = Descriptors.GetOrAdd(vType, type => new TypeDescriptor(type));
            if (_typeCache.TryGetValue(vType, out var tIdx))
            {
                WriteInt(DataBytesDefinition.RefType, tIdx);
            }
            else
            {
                WriteInt(DataBytesDefinition.TypeStart, descriptor.Definition.Length);
                _stream.Write(descriptor.Definition, 0, descriptor.Definition.Length);
                _typeCache.Set(vType);
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

        #region Private Write Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteByte(byte type, byte value)
        {
            _buffer[0] = type;
            _buffer[1] = value;
            _stream.Write(_buffer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteUshort(byte type, ushort value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _stream.Write(_buffer, 0, 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteInt(byte type, int value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)(value >> 16);
            _buffer[4] = (byte)(value >> 24);
            _stream.Write(_buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe void WriteDouble(byte type, double value)
        {
            var tmpValue = *(ulong*)&value;
            _buffer[0] = type;
            _buffer[1] = (byte)tmpValue;
            _buffer[2] = (byte)(tmpValue >> 8);
            _buffer[3] = (byte)(tmpValue >> 16);
            _buffer[4] = (byte)(tmpValue >> 24);
            _buffer[5] = (byte)(tmpValue >> 32);
            _buffer[6] = (byte)(tmpValue >> 40);
            _buffer[7] = (byte)(tmpValue >> 48);
            _buffer[8] = (byte)(tmpValue >> 56);
            _stream.Write(_buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe void WriteFloat(byte type, float value)
        {
            var tmpValue = *(uint*)&value;
            _buffer[0] = type;
            _buffer[1] = (byte)tmpValue;
            _buffer[2] = (byte)(tmpValue >> 8);
            _buffer[3] = (byte)(tmpValue >> 16);
            _buffer[4] = (byte)(tmpValue >> 24);
            _stream.Write(_buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteLong(byte type, long value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)(value >> 16);
            _buffer[4] = (byte)(value >> 24);
            _buffer[5] = (byte)(value >> 32);
            _buffer[6] = (byte)(value >> 40);
            _buffer[7] = (byte)(value >> 48);
            _buffer[8] = (byte)(value >> 56);
            _stream.Write(_buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteULong(byte type, ulong value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)(value >> 16);
            _buffer[4] = (byte)(value >> 24);
            _buffer[5] = (byte)(value >> 32);
            _buffer[6] = (byte)(value >> 40);
            _buffer[7] = (byte)(value >> 48);
            _buffer[8] = (byte)(value >> 56);
            _stream.Write(_buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteUInt(byte type, uint value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)(value >> 16);
            _buffer[4] = (byte)(value >> 24);
            _stream.Write(_buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteShort(byte type, short value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _stream.Write(_buffer, 0, 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteChar(byte type, char value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _stream.Write(_buffer, 0, 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteUshort(ushort value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _stream.Write(_buffer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteInt(int value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            _stream.Write(_buffer, 0, 4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe void WriteDouble(double value)
        {
            var tmpValue = *(ulong*)&value;
            _buffer[0] = (byte)tmpValue;
            _buffer[1] = (byte)(tmpValue >> 8);
            _buffer[2] = (byte)(tmpValue >> 16);
            _buffer[3] = (byte)(tmpValue >> 24);
            _buffer[4] = (byte)(tmpValue >> 32);
            _buffer[5] = (byte)(tmpValue >> 40);
            _buffer[6] = (byte)(tmpValue >> 48);
            _buffer[7] = (byte)(tmpValue >> 56);
            _stream.Write(_buffer, 0, 8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe void WriteFloat(float value)
        {
            var tmpValue = *(uint*)&value;
            _buffer[0] = (byte)tmpValue;
            _buffer[1] = (byte)(tmpValue >> 8);
            _buffer[2] = (byte)(tmpValue >> 16);
            _buffer[3] = (byte)(tmpValue >> 24);
            _stream.Write(_buffer, 0, 4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteLong(long value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            _buffer[4] = (byte)(value >> 32);
            _buffer[5] = (byte)(value >> 40);
            _buffer[6] = (byte)(value >> 48);
            _buffer[7] = (byte)(value >> 56);
            _stream.Write(_buffer, 0, 8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteULong(ulong value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            _buffer[4] = (byte)(value >> 32);
            _buffer[5] = (byte)(value >> 40);
            _buffer[6] = (byte)(value >> 48);
            _buffer[7] = (byte)(value >> 56);
            _stream.Write(_buffer, 0, 8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteUInt(uint value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            _stream.Write(_buffer, 0, 4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteShort(short value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _stream.Write(_buffer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteChar(char value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _stream.Write(_buffer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe void WriteDecimal(decimal value)
        {
            var val = decimal.ToDouble(value);
            var tmpValue = *(ulong*)&val;
            _buffer[0] = (byte)tmpValue;
            _buffer[1] = (byte)(tmpValue >> 8);
            _buffer[2] = (byte)(tmpValue >> 16);
            _buffer[3] = (byte)(tmpValue >> 24);
            _buffer[4] = (byte)(tmpValue >> 32);
            _buffer[5] = (byte)(tmpValue >> 40);
            _buffer[6] = (byte)(tmpValue >> 48);
            _buffer[7] = (byte)(tmpValue >> 56);
            _stream.Write(_buffer, 0, 8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteSByte(sbyte value)
        {
            var bytes = BitConverter.GetBytes(value);
            _stream.Write(bytes, 0, bytes.Length);
        }
        #endregion

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
