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
        internal static readonly MethodInfo InternalWriteObjectValueMInfo = typeof(SerializersTable).GetMethod("InternalWriteObjectValue", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly MethodInfo WriteDefIntMInfo = typeof(SerializersTable).GetMethod("WriteDefInt", BindingFlags.NonPublic | BindingFlags.Instance);
        //
        internal static readonly MethodInfo ListCountGetMethod = typeof(ICollection).GetProperty("Count").GetMethod;
        internal static readonly PropertyInfo ListIndexProperty = typeof(IList).GetProperty("Item");
        //
        internal static readonly MethodInfo ArrayLengthGetMethod = typeof(Array).GetProperty("Length").GetMethod;
        //
        internal static readonly MethodInfo DictionaryGetEnumeratorMethod = typeof(IDictionary).GetMethod("GetEnumerator");
        internal static readonly MethodInfo EnumeratorMoveNextMethod = typeof(IEnumerator).GetMethod("MoveNext");
        internal static readonly MethodInfo DictionaryEnumeratorKeyMethod = typeof(IDictionaryEnumerator).GetProperty("Key").GetMethod;
        internal static readonly MethodInfo DictionaryEnumeratorValueMethod = typeof(IDictionaryEnumerator).GetProperty("Value").GetMethod;


        private readonly byte[] _buffer = new byte[9];
        private readonly SerializerCache<Type> _typeCache = new SerializerCache<Type>();
        private readonly SerializerCache<object> _objectCache = new SerializerCache<object>();
        private readonly SerializerCache<DateTimeOffset> _dateTimeOffsetCache = new SerializerCache<DateTimeOffset>();
        private readonly SerializerCache<DateTime> _dateTimeCache = new SerializerCache<DateTime>();
        private readonly SerializerCache<Guid> _guidCache = new SerializerCache<Guid>();
        private readonly SerializerCache<decimal> _decimalCache = new SerializerCache<decimal>();
        private readonly SerializerCache<double> _doubleCache = new SerializerCache<double>();
        private readonly SerializerCache<float> _floatCache = new SerializerCache<float>();
        private readonly SerializerCache<long> _longCache = new SerializerCache<long>();
        private readonly SerializerCache<ulong> _uLongCache = new SerializerCache<ulong>();
        private readonly SerializerStringCache _stringCache8 = new SerializerStringCache();
        private readonly SerializerStringCache _stringCache16 = new SerializerStringCache();
        private readonly SerializerStringCache _stringCache32 = new SerializerStringCache();
        private readonly SerializerStringCache _stringCache = new SerializerStringCache();
        private readonly SerializerCache<TimeSpan> _timespanCache = new SerializerCache<TimeSpan>();

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

        #region Write Values
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(bool[] value)
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
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
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteDefInt(DataBytesDefinition.RefObject, oIdx);
                return;
            }
            _objectCache.Set(value);
            WriteDefInt(DataBytesDefinition.TimeSpanList, value.Count);
            for (var i = 0; i < value.Count; i++)
                WriteValue(value[i]);
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Stream stream, object value, Type valueType)
        {
            Stream = stream;
            WriteByte(DataBytesDefinition.Start);

            if (value == null)
            {
                WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            if (WriteValues.TryGetValue(valueType, out var mTuple))
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
            var descriptor = Descriptors.GetOrAdd(valueType, type => new TypeDescriptor(type));
            if (_typeCache.TryGetValue(valueType, out var tIdx))
            {
                WriteDefInt(DataBytesDefinition.RefType, tIdx);
            }
            else
            {
                Stream.Write(descriptor.Definition, 0, descriptor.Definition.Length);
                _typeCache.Set(valueType);
            }
            if (descriptor.IsNSerializable)
                ((INSerializable)value).Serialize(this);
            else
                descriptor.SerializeAction(value, this);

            WriteDefByte(DataBytesDefinition.TypeEnd, DataBytesDefinition.End);
            _dateTimeOffsetCache.Clear();
            _dateTimeCache.Clear();
            _guidCache.Clear();
            _decimalCache.Clear();
            _doubleCache.Clear();
            _floatCache.Clear();
            _longCache.Clear();
            _uLongCache.Clear();
            _stringCache8.Clear();
            _stringCache16.Clear();
            _stringCache32.Clear();
            _stringCache.Clear();
            _timespanCache.Clear();
            _typeCache.Clear();
            _objectCache.Clear();
            Stream = null;
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
