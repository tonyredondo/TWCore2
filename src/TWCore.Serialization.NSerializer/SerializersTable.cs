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
using System.Runtime.InteropServices;
using TWCore.Reflection;
// ReSharper disable UnusedMember.Local
#pragma warning disable 1591

namespace TWCore.Serialization.NSerializer
{
    public partial class SerializersTable
    {
        internal static readonly Dictionary<Type, (MethodInfo Method, MethodAccessorDelegate Accessor)> WriteValues = new Dictionary<Type, (MethodInfo Method, MethodAccessorDelegate Accessor)>();
        internal static readonly ConcurrentDictionary<Type, SerializerTypeDescriptor> Descriptors = new ConcurrentDictionary<Type, SerializerTypeDescriptor>();
        internal static readonly MethodInfo InternalWriteObjectValueMInfo = typeof(SerializersTable).GetMethod("InternalWriteObjectValue", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly MethodInfo InternalSimpleWriteObjectValueMInfo = typeof(SerializersTable).GetMethod("InternalSimpleWriteObjectValue", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly MethodInfo InternalMixedWriteObjectValueMInfo = typeof(SerializersTable).GetMethod("InternalMixedWriteObjectValue", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly MethodInfo WriteDefIntMInfo = typeof(SerializersTable).GetMethod("WriteDefInt", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly MethodInfo WriteByteMethodInfo = typeof(SerializersTable).GetMethod("WriteByte", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly MethodInfo WriteBytesMethodInfo = typeof(SerializersTable).GetMethod("WriteBytes", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly MethodInfo WriteIntMethodInfo = typeof(SerializersTable).GetMethod("WriteInt", BindingFlags.NonPublic | BindingFlags.Instance);
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
        //
        internal static readonly MethodInfo TryGetValueObjectSerializerCacheMethod = typeof(SerializerCache<object>).GetMethod("TryGetValue", BindingFlags.Public | BindingFlags.Instance);
        internal static readonly MethodInfo WriteRefObjectMInfo = typeof(SerializersTable).GetMethod("WriteRefObject", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly MethodInfo SetObjectSerializerCacheMethod = typeof(SerializerCache<object>).GetMethod("Set", BindingFlags.Public | BindingFlags.Instance);

        internal static readonly MethodInfo TryGetValueTypeSerializerCacheMethod = typeof(SerializerCache<Type>).GetMethod("TryGetValue", BindingFlags.Public | BindingFlags.Instance);
        internal static readonly MethodInfo WriteRefTypeMInfo = typeof(SerializersTable).GetMethod("WriteRefType", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly MethodInfo SetTypeSerializerCacheMethod = typeof(SerializerCache<Type>).GetMethod("Set", BindingFlags.Public | BindingFlags.Instance);
        //
        internal static readonly MethodInfo GetTypeMethodInfo = typeof(object).GetMethod("GetType", BindingFlags.Public | BindingFlags.Instance);


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
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.BoolArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(char[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.CharArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTimeOffset[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.DateTimeOffsetArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTime[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.DateTimeArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Enum[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.EnumArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Guid[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.GuidArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(decimal[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.DecimalArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(double[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.DoubleArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(float[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.FloatArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(long[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.LongArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ulong[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.ULongArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(int[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.IntArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(uint[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.UIntArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(short[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.ShortArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ushort[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.UShortArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(sbyte[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.SByteArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(string[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.StringArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(TimeSpan[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.TimeSpanArray, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(object[] value)
        {
            if (value is null)
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
            var length = value.Length;
            WriteDefInt(DataBytesDefinition.ObjectArray, length);
            for (var i = 0; i < length; i++)
                InternalWriteObjectValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<bool> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.BoolList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<char> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.CharList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<DateTimeOffset> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.DateTimeOffsetList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<DateTime> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.DateTimeList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<Enum> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.EnumList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<Guid> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.GuidList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<decimal> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.DecimalList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<double> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.DoubleList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<float> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.FloatList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<long> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.LongList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<ulong> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.ULongList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<int> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.IntList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<uint> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.UIntList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<short> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.ShortList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<ushort> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.UShortList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<sbyte> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.SByteList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<string> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.StringList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<TimeSpan> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.TimeSpanList, length);
            for (var i = 0; i < length; i++)
                WriteValue(value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<object> value)
        {
            if (value is null)
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
            var length = value.Count;
            WriteDefInt(DataBytesDefinition.ObjectList, length);
            for (var i = 0; i < length; i++)
                InternalWriteObjectValue(value[i]);
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Stream stream, object value, Type valueType)
        {
            try
            {
                Stream = stream;
                Stream.WriteByte(DataBytesDefinition.Start);

                if (value is null)
                {
                    Stream.WriteByte(DataBytesDefinition.ValueNull);
                    return;
                }
                if (value is IEnumerable iEValue && (!(iEValue is IList || iEValue is string || iEValue is IDictionary)))
                {
                    if (valueType.ReflectedType == typeof(Enumerable) || valueType.FullName.IndexOf("System.Linq", StringComparison.Ordinal) > -1)
                    {
                        var ienumerable = valueType.AllInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                        valueType = typeof(List<>).MakeGenericType(ienumerable.GenericTypeArguments[0]);
                        value = (IList)Activator.CreateInstance(valueType, iEValue);
                    }
                }
                if (WriteValues.TryGetValue(valueType, out var mTuple))
                {
                    _paramObj[0] = value;
                    mTuple.Accessor(this, _paramObj);
                    Stream.WriteByte(DataBytesDefinition.End);
                    return;
                }
                _objectCache.Set(value);
                var descriptor = Descriptors.GetOrAdd(valueType, type => new SerializerTypeDescriptor(type));
                Stream.Write(descriptor.Definition);
                _typeCache.Set(valueType);
                if (descriptor.IsNSerializable)
                    ((INSerializable)value).Serialize(this);
                else
                    descriptor.SerializeAction(value, this);

                Span<byte> buffer = stackalloc byte[2];
                buffer[0] = DataBytesDefinition.TypeEnd;
                buffer[1] = DataBytesDefinition.End;
                Stream.Write(buffer);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                throw;
            }
            finally
            {
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
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(Stream stream, T value)
        {
            if (!typeof(T).IsSealed)
            {
                Serialize(stream, value, value?.GetType());
                return;
            }

            try
            {
                Stream = stream;
                Stream.WriteByte(DataBytesDefinition.Start);

                if (typeof(T).IsClass && EqualityComparer<T>.Default.Equals(value, default))
                {
                    Stream.WriteByte(DataBytesDefinition.ValueNull);
                    return;
                }
                if (WriteValues.TryGetValue(typeof(T), out var mTuple))
                {
                    _paramObj[0] = value;
                    mTuple.Accessor(this, _paramObj);
                    Stream.WriteByte(DataBytesDefinition.End);
                    return;
                }
                _objectCache.Set(value);
                var descriptor = SerializerTypeDescriptor<T>.Descriptor;
                Stream.Write(descriptor.Definition);
                _typeCache.Set(typeof(T));
                if (descriptor.IsNSerializable)
                    ((INSerializable)value).Serialize(this);
                else
                    descriptor.SerializeAction(value, this);

                Span<byte> buffer = stackalloc byte[2];
                buffer[0] = DataBytesDefinition.TypeEnd;
                buffer[1] = DataBytesDefinition.End;
                Stream.Write(buffer);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                throw;
            }
            finally
            {
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

        }

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InternalWriteObjectValue(object value)
        {
            if (value is null)
            {
                Stream.WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            var vType = value.GetType();
            if (value is IEnumerable iEValue && (!(iEValue is IList || iEValue is string || iEValue is IDictionary)))
            {
                if (vType.ReflectedType == typeof(Enumerable) || vType.FullName.IndexOf("System.Linq", StringComparison.Ordinal) > -1)
                {
                    var ienumerable = vType.AllInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                    vType = typeof(List<>).MakeGenericType(ienumerable.GenericTypeArguments[0]);
                    value = (IList)Activator.CreateInstance(vType, iEValue);
                }
            }
            if (WriteValues.TryGetValue(vType, out var mTuple))
            {
                _paramObj[0] = value;
                mTuple.Accessor(this, _paramObj);
                return;
            }
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteRefObject(oIdx);
                return;
            }
            _objectCache.Set(value);
            var descriptor = Descriptors.GetOrAdd(vType, type => new SerializerTypeDescriptor(type));
            if (_typeCache.TryGetValue(vType, out var tIdx))
            {
                WriteRefType(tIdx);
            }
            else
            {
                Stream.Write(descriptor.Definition);
                _typeCache.Set(vType);
            }
            if (descriptor.IsNSerializable)
                ((INSerializable)value).Serialize(this);
            else
                descriptor.SerializeAction(value, this);
            Stream.WriteByte(DataBytesDefinition.TypeEnd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InternalSimpleWriteObjectValue(object value)
        {
            if (value is null)
            {
                Stream.WriteByte(DataBytesDefinition.ValueNull);
                return;
            }
            if (_objectCache.TryGetValue(value, out var oIdx))
            {
                WriteRefObject(oIdx);
                return;
            }
            _objectCache.Set(value);

            var vType = value.GetType();
            var descriptor = Descriptors.GetOrAdd(vType, type => new SerializerTypeDescriptor(type));
            if (_typeCache.TryGetValue(vType, out var tIdx))
            {
                WriteRefType(tIdx);
            }
            else
            {
                Stream.Write(descriptor.Definition);
                _typeCache.Set(vType);
            }
            if (descriptor.IsNSerializable)
                ((INSerializable)value).Serialize(this);
            else
                descriptor.SerializeAction(value, this);
            Stream.WriteByte(DataBytesDefinition.TypeEnd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InternalMixedWriteObjectValue(object value, Type valueType)
        {
            var descriptor = Descriptors.GetOrAdd(valueType, type => new SerializerTypeDescriptor(type));
            if (_typeCache.TryGetValue(valueType, out var tIdx))
            {
                WriteRefType(tIdx);
            }
            else
            {
                Stream.Write(descriptor.Definition);
                _typeCache.Set(valueType);
            }
            if (descriptor.IsNSerializable)
                ((INSerializable)value).Serialize(this);
            else
                descriptor.SerializeAction(value, this);
        }
        #endregion

        #region Private Write Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteByte(byte value)
        {
            Stream.WriteByte(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteBytes(byte[] value)
        {
            Stream.Write(value, 0, value.Length);
        }

#if COMPATIBILITY
        byte[] _buffer = new byte[17];

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
            MemoryMarshal.Write(_buffer.AsSpan(1), ref value);
            Stream.Write(_buffer, 0, 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefInt(byte type, int value)
        {
            _buffer[0] = type;
            MemoryMarshal.Write(_buffer.AsSpan(1), ref value);
            Stream.Write(_buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefDouble(byte type, double value)
        {
            _buffer[0] = type;
            MemoryMarshal.Write(_buffer.AsSpan(1), ref value);
            Stream.Write(_buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefFloat(byte type, float value)
        {
            _buffer[0] = type;
            MemoryMarshal.Write(_buffer.AsSpan(1), ref value);
            Stream.Write(_buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefLong(byte type, long value)
        {
            _buffer[0] = type;
            MemoryMarshal.Write(_buffer.AsSpan(1), ref value);
            Stream.Write(_buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefULong(byte type, ulong value)
        {
            _buffer[0] = type;
            MemoryMarshal.Write(_buffer.AsSpan(1), ref value);
            Stream.Write(_buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefUInt(byte type, uint value)
        {
            _buffer[0] = type;
            MemoryMarshal.Write(_buffer.AsSpan(1), ref value);
            Stream.Write(_buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefShort(byte type, short value)
        {
            _buffer[0] = type;
            MemoryMarshal.Write(_buffer.AsSpan(1), ref value);
            Stream.Write(_buffer, 0, 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefChar(byte type, char value)
        {
            _buffer[0] = type;
            MemoryMarshal.Write(_buffer.AsSpan(1), ref value);
            Stream.Write(_buffer, 0, 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefDecimal(byte type, decimal value)
        {
            _buffer[0] = type;
            var bits = decimal.GetBits(value);
            for (var i = 0; i < 4; i++)
                MemoryMarshal.Write(_buffer.AsSpan((i * 4) + 1), ref bits[i]);
            Stream.Write(_buffer, 0, 17);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteUshort(ushort value)
        {
            MemoryMarshal.Write(_buffer, ref value);
            Stream.Write(_buffer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteInt(int value)
        {
            MemoryMarshal.Write(_buffer, ref value);
            Stream.Write(_buffer, 0, 4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDouble(double value)
        {
            MemoryMarshal.Write(_buffer, ref value);
            Stream.Write(_buffer, 0, 8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteFloat(float value)
        {
            MemoryMarshal.Write(_buffer, ref value);
            Stream.Write(_buffer, 0, 4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteLong(long value)
        {
            MemoryMarshal.Write(_buffer, ref value);
            Stream.Write(_buffer, 0, 8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteULong(ulong value)
        {
            MemoryMarshal.Write(_buffer, ref value);
            Stream.Write(_buffer, 0, 8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteUInt(uint value)
        {
            MemoryMarshal.Write(_buffer, ref value);
            Stream.Write(_buffer, 0, 4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteShort(short value)
        {
            MemoryMarshal.Write(_buffer, ref value);
            Stream.Write(_buffer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteChar(char value)
        {
            MemoryMarshal.Write(_buffer, ref value);
            Stream.Write(_buffer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDecimal(decimal value)
        {
            var bits = decimal.GetBits(value);
            for (var i = 0; i < 4; i++)
                MemoryMarshal.Write(_buffer.AsSpan(i * 4), ref bits[i]);
            Stream.Write(_buffer, 0, 16);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteSByte(sbyte value)
        {
            MemoryMarshal.Write(_buffer, ref value);
            Stream.Write(_buffer, 0, 1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteRefType(int refType)
        {
            _buffer[0] = DataBytesDefinition.RefType;
            MemoryMarshal.Write(_buffer.AsSpan(1), ref refType);
            Stream.Write(_buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteRefObject(int refObject)
        {
            _buffer[0] = DataBytesDefinition.RefObject;
            MemoryMarshal.Write(_buffer.AsSpan(1), ref refObject);
            Stream.Write(_buffer, 0, 5);
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefByte(byte type, byte value)
        {
            Span<byte> buffer = stackalloc byte[2];
            buffer[0] = type;
            buffer[1] = value;
            Stream.Write(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefUshort(byte type, ushort value)
        {
            Span<byte> buffer = stackalloc byte[3];
            buffer[0] = type;
            BitConverter.TryWriteBytes(buffer.Slice(1), value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefInt(byte type, int value)
        {
            Span<byte> buffer = stackalloc byte[5];
            buffer[0] = type;
            BitConverter.TryWriteBytes(buffer.Slice(1), value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefDouble(byte type, double value)
        {
            Span<byte> buffer = stackalloc byte[9];
            buffer[0] = type;
            BitConverter.TryWriteBytes(buffer.Slice(1), value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefFloat(byte type, float value)
        {
            Span<byte> buffer = stackalloc byte[5];
            buffer[0] = type;
            BitConverter.TryWriteBytes(buffer.Slice(1), value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefLong(byte type, long value)
        {
            Span<byte> buffer = stackalloc byte[9];
            buffer[0] = type;
            BitConverter.TryWriteBytes(buffer.Slice(1), value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefULong(byte type, ulong value)
        {
            Span<byte> buffer = stackalloc byte[9];
            buffer[0] = type;
            BitConverter.TryWriteBytes(buffer.Slice(1), value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefUInt(byte type, uint value)
        {
            Span<byte> buffer = stackalloc byte[5];
            buffer[0] = type;
            BitConverter.TryWriteBytes(buffer.Slice(1), value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefShort(byte type, short value)
        {
            Span<byte> buffer = stackalloc byte[3];
            buffer[0] = type;
            BitConverter.TryWriteBytes(buffer.Slice(1), value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefChar(byte type, char value)
        {
            Span<byte> buffer = stackalloc byte[3];
            buffer[0] = type;
            BitConverter.TryWriteBytes(buffer.Slice(1), value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDefDecimal(byte type, decimal value)
        {
            Span<byte> buffer = stackalloc byte[17];
            buffer[0] = type;
            var bits = decimal.GetBits(value);
            var decBuffer = buffer.Slice(1);
            for (var i = 0; i < 4; i++)
                BitConverter.TryWriteBytes(decBuffer.Slice(i * 4, 4), bits[i]);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteUshort(ushort value)
        {
            Span<byte> buffer = stackalloc byte[2];
            BitConverter.TryWriteBytes(buffer, value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteInt(int value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BitConverter.TryWriteBytes(buffer, value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDouble(double value)
        {
            Span<byte> buffer = stackalloc byte[8];
            BitConverter.TryWriteBytes(buffer, value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteFloat(float value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BitConverter.TryWriteBytes(buffer, value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteLong(long value)
        {
            Span<byte> buffer = stackalloc byte[8];
            BitConverter.TryWriteBytes(buffer, value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteULong(ulong value)
        {
            Span<byte> buffer = stackalloc byte[8];
            BitConverter.TryWriteBytes(buffer, value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteUInt(uint value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BitConverter.TryWriteBytes(buffer, value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteShort(short value)
        {
            Span<byte> buffer = stackalloc byte[2];
            BitConverter.TryWriteBytes(buffer, value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteChar(char value)
        {
            Span<byte> buffer = stackalloc byte[2];
            BitConverter.TryWriteBytes(buffer, value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteDecimal(decimal value)
        {
            var bits = decimal.GetBits(value);
            Span<byte> buffer = stackalloc byte[16];
            for (var i = 0; i < 4; i++)
                BitConverter.TryWriteBytes(buffer.Slice(i * 4, 4), bits[i]);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteSByte(sbyte value)
        {
            Span<byte> buffer = stackalloc byte[1];
            BitConverter.TryWriteBytes(buffer, value);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteRefType(int refType)
        {
            Span<byte> buffer = stackalloc byte[5];
            buffer[0] = DataBytesDefinition.RefType;
            BitConverter.TryWriteBytes(buffer.Slice(1), refType);
            Stream.Write(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteRefObject(int refObject)
        {
            Span<byte> buffer = stackalloc byte[5];
            buffer[0] = DataBytesDefinition.RefObject;
            BitConverter.TryWriteBytes(buffer.Slice(1), refObject);
            Stream.Write(buffer);
        }
#endif

        #endregion
    }
}
