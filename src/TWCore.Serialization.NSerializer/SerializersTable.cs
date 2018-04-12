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
using System.Reflection;
using System.Runtime.CompilerServices;
using TWCore.Reflection;

namespace TWCore.Serialization.NSerializer
{
    public partial class SerializersTable
    {
        internal static readonly Dictionary<Type, (MethodInfo Method, MethodAccessorDelegate Accessor)> WriteValues = new Dictionary<Type, (MethodInfo Method, MethodAccessorDelegate Accessor)>();
        internal static readonly ConcurrentDictionary<Type, TypeDescriptor> Descriptors = new ConcurrentDictionary<Type, TypeDescriptor>();
        internal static readonly MethodInfo WriteObjectValueMInfo = typeof(SerializersTable).GetMethod("WriteObjectValue");
        internal static readonly MethodInfo InternalWriteObjectValueMInfo = typeof(SerializersTable).GetMethod("InternalWriteObjectValue", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly MethodInfo WriteDefIntMInfo = typeof(SerializersTable).GetMethod("WriteDefInt", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly MethodInfo IListLengthGetMethod = typeof(ICollection).GetProperty("Count").GetMethod;
        internal static readonly PropertyInfo IListIndexProperty = typeof(IList).GetProperty("Item");
        internal static readonly MethodInfo ArrayLengthGetMethod = typeof(Array).GetProperty("Length").GetMethod;

        private readonly byte[] _buffer = new byte[9];
        private readonly SerializerCache<Type> _typeCache = new SerializerCache<Type>();
        private readonly SerializerCache<object> _objectCache = new SerializerCache<object>();
        private readonly object[] _paramObj = new object[1];
        protected Stream Stream;

        #region Statics
        static SerializersTable()
        {
            var methods = typeof(SerializersTable).GetMethods();
            foreach (var method in methods)
            {
                if (method.Name == nameof(WriteValue))
                {
                    var parameters = method.GetParameters();
                    WriteValues[parameters[0].ParameterType] = (method, Factory.Accessors.BuildMethodAccessor(method));
                }
            }
        }
        #endregion

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
            Stream = null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStream(Stream stream)
        {
            Stream = stream;
        }
        #endregion

        #region Write Values
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(bool[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.BoolArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(char[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.CharArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTimeOffset[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.DateTimeOffsetArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTime[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.DateTimeArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Enum[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.EnumArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Guid[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.GuidArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(decimal[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.DecimalArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(double[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.DoubleArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(float[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.FloatArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(long[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.LongArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ulong[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.ULongArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(int[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.IntArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(uint[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.UIntArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(short[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.ShortArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ushort[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.UShortArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(sbyte[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.SByteArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(string[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.StringArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(TimeSpan[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.TimeSpanArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<bool> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.BoolList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<char> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.CharList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<DateTimeOffset> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.DateTimeOffsetList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<DateTime> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.DateTimeList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<Enum> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.EnumList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<Guid> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.GuidList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<decimal> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.DecimalList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<double> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.DoubleList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<float> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.FloatList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<long> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.LongList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<ulong> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.ULongList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<int> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.IntList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<uint> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.UIntList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<short> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.ShortList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<ushort> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.UShortList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<sbyte> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.SByteList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<string> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.StringList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<TimeSpan> value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            WriteDefInt(DataBytesDefinition.TimeSpanList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteObjectValue(object value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            var vType = value.GetType();
            if (WriteValues.TryGetValue(vType, out var mTuple))
            {
                _paramObj[0] = value;
                mTuple.Accessor(this, _paramObj);
                return;
            }
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
            var descriptor = Descriptors.GetOrAdd(vType, type => new TypeDescriptor(type));
            if (_typeCache.TryGetValue(vType, out var tIdx))
            {
                WriteDefInt(DataBytesDefinition.RefType, tIdx);
            }
            else
            {
                Stream.Write(descriptor.Definition, 0, descriptor.Definition.Length);
                _typeCache.Set(vType);
            }
            if (descriptor.IsNSerializable)
                ((INSerializable)value).Serialize(this);
            else
                descriptor.SerializeAction(value, this);
            WriteByte(DataBytesDefinition.TypeEnd);
        }

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InternalWriteObjectValue(object value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
            var vType = value.GetType();
            var descriptor = Descriptors.GetOrAdd(vType, type => new TypeDescriptor(type));
            if (_typeCache.TryGetValue(vType, out var tIdx))
            {
                WriteDefInt(DataBytesDefinition.RefType, tIdx);
            }
            else
            {
                Stream.Write(descriptor.Definition, 0, descriptor.Definition.Length);
                _typeCache.Set(vType);
            }
            if (descriptor.IsNSerializable)
                ((INSerializable)value).Serialize(this);
            else
                descriptor.SerializeAction(value, this);
            WriteByte(DataBytesDefinition.TypeEnd);
        }


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private void InternalWriteValue(object value, TypeDescriptor descriptor)
        //{
        //    //Write Properties
        //    if (descriptor.Properties.Length > 0)
        //    {
        //        WriteDefInt(DataBytesDefinition.PropertiesStart, descriptor.Properties.Length);
        //        foreach (var prop in descriptor.FastProperties)
        //            WriteObjectValue(prop.GetValue(value));
        //    }

        //    //Write Array if contains
        //    if (descriptor.IsArray)
        //    {
        //        var aValue = (Array)value;
        //        WriteDefInt(DataBytesDefinition.ArrayStart, aValue.Length);
        //        for (var i = 0; i < aValue.Length; i++)
        //            WriteObjectValue(aValue.GetValue(i));
        //        return;
        //    }

        //    //Write List if contains
        //    if (descriptor.IsList)
        //    {
        //        var iValue = (IList)value;
        //        var count = iValue.Count;
        //        WriteDefInt(DataBytesDefinition.ListStart, count);
        //        for (var i = 0; i < count; i++)
        //            WriteObjectValue(iValue[i]);
        //        return;
        //    }

        //    //Write Dictionary if contains
        //    if (descriptor.IsDictionary)
        //    {
        //        var iValue = (IDictionary)value;
        //        var count = iValue.Count;
        //        WriteDefInt(DataBytesDefinition.DictionaryStart, count);
        //        foreach (DictionaryEntry item in iValue)
        //        {
        //            WriteObjectValue(item.Key);
        //            WriteObjectValue(item.Value);
        //        }
        //        return;
        //    }
        //}
        #endregion

        #region Private Write Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefByte(byte type, byte value)
        {
            _buffer[0] = type;
            _buffer[1] = value;
            Stream.Write(_buffer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefUshort(byte type, ushort value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            Stream.Write(_buffer, 0, 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefInt(byte type, int value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)(value >> 16);
            _buffer[4] = (byte)(value >> 24);
            Stream.Write(_buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe void WriteDefDouble(byte type, double value)
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
            Stream.Write(_buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe void WriteDefFloat(byte type, float value)
        {
            var tmpValue = *(uint*)&value;
            _buffer[0] = type;
            _buffer[1] = (byte)tmpValue;
            _buffer[2] = (byte)(tmpValue >> 8);
            _buffer[3] = (byte)(tmpValue >> 16);
            _buffer[4] = (byte)(tmpValue >> 24);
            Stream.Write(_buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefLong(byte type, long value)
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
            Stream.Write(_buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefULong(byte type, ulong value)
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
            Stream.Write(_buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefUInt(byte type, uint value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)(value >> 16);
            _buffer[4] = (byte)(value >> 24);
            Stream.Write(_buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefShort(byte type, short value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            Stream.Write(_buffer, 0, 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefChar(byte type, char value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            Stream.Write(_buffer, 0, 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteByte(byte value)
        {
            _buffer[0] = value;
            Stream.Write(_buffer, 0, 1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteUshort(ushort value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            Stream.Write(_buffer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteInt(int value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            Stream.Write(_buffer, 0, 4);
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
            Stream.Write(_buffer, 0, 8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe void WriteFloat(float value)
        {
            var tmpValue = *(uint*)&value;
            _buffer[0] = (byte)tmpValue;
            _buffer[1] = (byte)(tmpValue >> 8);
            _buffer[2] = (byte)(tmpValue >> 16);
            _buffer[3] = (byte)(tmpValue >> 24);
            Stream.Write(_buffer, 0, 4);
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
            Stream.Write(_buffer, 0, 8);
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
            Stream.Write(_buffer, 0, 8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteUInt(uint value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            Stream.Write(_buffer, 0, 4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteShort(short value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            Stream.Write(_buffer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteChar(char value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            Stream.Write(_buffer, 0, 2);
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
            Stream.Write(_buffer, 0, 8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteSByte(sbyte value)
        {
            var bytes = BitConverter.GetBytes(value);
            Stream.Write(bytes, 0, bytes.Length);
        }
        #endregion
    }
}
