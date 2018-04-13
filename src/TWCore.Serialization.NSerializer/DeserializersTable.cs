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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using NonBlocking;
using TWCore.Reflection;

namespace TWCore.Serialization.NSerializer
{
    public partial class DeserializersTable
    {
        internal static readonly Dictionary<byte, (MethodInfo Method, MethodAccessorDelegate Accessor)> ReadValues = new Dictionary<byte, (MethodInfo Method, MethodAccessorDelegate Accessor)>();
        internal static readonly ConcurrentDictionary<Type, DeserializerTypeDescriptor> Descriptors = new ConcurrentDictionary<Type, DeserializerTypeDescriptor>();
        private static readonly byte[] EmptyBytes = new byte[0];
        private readonly byte[] _buffer = new byte[16];
        private readonly object[] _parameters = new object[1];
        private readonly DeserializerCache<Type> _typeCache = new DeserializerCache<Type>();
        private readonly DeserializerCache<object> _objectCache = new DeserializerCache<object>();
        private readonly DeserializerCache<DateTimeOffset> _dateTimeOffsetCache = new DeserializerCache<DateTimeOffset>();
        private readonly DeserializerCache<DateTime> _dateTimeCache = new DeserializerCache<DateTime>();
        private readonly DeserializerCache<Guid> _guidCache = new DeserializerCache<Guid>();
        private readonly DeserializerCache<decimal> _decimalCache = new DeserializerCache<decimal>();
        private readonly DeserializerCache<double> _doubleCache = new DeserializerCache<double>();
        private readonly DeserializerCache<float> _floatCache = new DeserializerCache<float>();
        private readonly DeserializerCache<long> _longCache = new DeserializerCache<long>();
        private readonly DeserializerCache<ulong> _uLongCache = new DeserializerCache<ulong>();
        private readonly DeserializerStringCache _stringCache8 = new DeserializerStringCache();
        private readonly DeserializerStringCache _stringCache16 = new DeserializerStringCache();
        private readonly DeserializerStringCache _stringCache32 = new DeserializerStringCache();
        private readonly DeserializerStringCache _stringCache = new DeserializerStringCache();
        private readonly DeserializerCache<TimeSpan> _timespanCache = new DeserializerCache<TimeSpan>();
        private readonly Dictionary<Type, string[]> _propertiesByType = new Dictionary<Type, string[]>();
        protected Stream Stream;

        #region Attributes
        public class DeserializerMethodAttribute : Attribute
        {
            public byte[] ByteTypes { get; }
            public DeserializerMethodAttribute(params byte[] byteTypes)
            {
                ByteTypes = byteTypes;
            }
        }
        #endregion

        #region Statics
        static DeserializersTable()
        {
            var methods = typeof(DeserializersTable).GetMethods();
            foreach (var method in methods)
            {
                var attr = method.GetAttribute<DeserializerMethodAttribute>();
                if (attr == null) continue;
                foreach (var byteType in attr.ByteTypes)
                    ReadValues[byteType] = (method, Factory.Accessors.BuildMethodAccessor(method));
            }
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Deserialize(Stream stream)
        {
            Stream = stream;
            if (stream.ReadByte() != DataBytesDefinition.Start)
                throw new FormatException("The stream is not in NSerializer format.");

            var value = ReadValue(ReadByte());
            while (ReadByte() != DataBytesDefinition.End)
            {
            }

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
            _propertiesByType.Clear();
            _objectCache.Clear();
            _typeCache.Clear();
            Stream = null;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object ReadValue(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return _objectCache.Get(ReadInt());
            if (ReadValues.TryGetValue(type, out var mTuple))
            {
                _parameters[0] = type;
                return mTuple.Accessor(this, _parameters);
            }
            Type valueType = null;
            string[] properties = null;

            if (type == DataBytesDefinition.TypeStart)
            {
                var length = ReadInt();
                var typeBytes = new byte[length];
                Stream.Read(typeBytes, 0, length);
                var typeData = Encoding.UTF8.GetString(typeBytes, 0, length);
                valueType = Core.GetType(typeData.Substring(0, typeData.IndexOf(";", StringComparison.Ordinal)));
                properties = typeData.Substring(typeData.IndexOf(";", StringComparison.Ordinal) + 1).SplitAndTrim(';');
                _propertiesByType[valueType] = properties;
                _typeCache.Set(valueType);
            }
            else if (type == DataBytesDefinition.RefType)
            {
                valueType = _typeCache.Get(ReadInt());
                properties = _propertiesByType[valueType];
            }
            var descriptor = Descriptors.GetOrAdd(valueType, vType => new DeserializerTypeDescriptor(vType));
            if (descriptor.IsNSerializable)
            {
                var value = (INSerializable) descriptor.Activator();
                value.Fill(this, properties);
                return value;
            }
            return FillObject(ref descriptor, properties);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object FillObject(ref DeserializerTypeDescriptor descriptor, string[] properties)
        {
            object value = null;
            IList iValue = null;
            
            if (!descriptor.IsArray)
            {
                value = descriptor.Activator();
                _objectCache.Set(value);
                if (descriptor.IsList)
                    iValue = (IList) value;
            }
            var flag = ReadByte();
            var propValues = new object[properties.Length];
            if (flag == DataBytesDefinition.PropertiesStart)
            {
                var length = ReadInt();
                for(var i = 0; i < properties.Length; i++)
                    propValues[i] = ReadValue(ReadByte());
                flag = ReadByte();
            }
            var capacity = 0;
            if (flag == DataBytesDefinition.ArrayStart || flag == DataBytesDefinition.ListStart || flag == DataBytesDefinition.DictionaryStart)
            {
                capacity = ReadInt();
                if (descriptor.IsArray)
                {
                    value = descriptor.Activator(capacity);
                    _objectCache.Set(value);
                }
            }

            //***************************************************************************************
            for (var i = 0; i < properties.Length; i++)
            {
                var name = properties[i];
                if (descriptor.Properties.TryGetValue(name, out var fProp))
                {
                    fProp.SetValue(value, propValues[i]);
                }
                else
                {
                    //Notify property not found.
                }
            }
            if (descriptor.IsArray)
            {
                var aValue = (Array) value;
                for (var i = 0; i < capacity; i++)
                {
                    var item = ReadValue(ReadByte());
                    aValue.SetValue(item, i);
                }
            }
            else if (descriptor.IsList)
            {
                for (var i = 0; i < capacity; i++)
                {
                    var item = ReadValue(ReadByte());
                    iValue.Add(item);
                }
            }
            else if (descriptor.IsDictionary)
            {
                var dictio = (IDictionary) value;
                for (var i = 0; i < capacity; i++)
                {
                    var dKey = ReadValue(ReadByte());
                    var dValue = ReadValue(ReadByte());
                    dictio[dKey] = dValue;
                }
            }

            return value;
        }

        #region Read Values
        [DeserializerMethod(DataBytesDefinition.BoolArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool[] ReadBoolArray(byte type)
        {
            if (type != DataBytesDefinition.BoolArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (bool[])_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new bool[length];
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadBool(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.CharArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char[] ReadCharArray(byte type)
        {
            if (type != DataBytesDefinition.CharArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (char[])_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new char[length];
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadChar(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DateTimeOffsetArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset[] ReadDateTimeOffsetArray(byte type)
        {
            if (type != DataBytesDefinition.DateTimeOffsetArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (DateTimeOffset[])_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new DateTimeOffset[length];
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadDateTimeOffset(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DateTimeArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime[] ReadDateTimeArray(byte type)
        {
            if (type != DataBytesDefinition.DateTimeArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (DateTime[])_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new DateTime[length];
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadDateTime(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.GuidArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid[] ReadGuidArray(byte type)
        {
            if (type != DataBytesDefinition.GuidArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (Guid[])_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new Guid[length];
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadGuid(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DecimalArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal[] ReadDecimalArray(byte type)
        {
            if (type != DataBytesDefinition.DecimalArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (decimal[])_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new decimal[length];
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadDecimal(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DoubleArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double[] ReadDoubleArray(byte type)
        {
            if (type != DataBytesDefinition.DoubleArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (double[])_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new double[length];
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadDouble(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.FloatArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] ReadFloatArray(byte type)
        {
            if (type != DataBytesDefinition.FloatArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (float[])_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new float[length];
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadFloat(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.LongArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long[] ReadLongArray(byte type)
        {
            if (type != DataBytesDefinition.LongArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (long[])_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new long[length];
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadLong(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.ULongArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong[] ReadULongArray(byte type)
        {
            if (type != DataBytesDefinition.ULongArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (ulong[])_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new ulong[length];
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadULong(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.IntArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int[] ReadIntArray(byte type)
        {
            if (type != DataBytesDefinition.IntArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (int[])_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new int[length];
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadInt(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.UIntArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint[] ReadUIntArray(byte type)
        {
            if (type != DataBytesDefinition.UIntArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (uint[])_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new uint[length];
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadUInt(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.ShortArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short[] ReadShortArray(byte type)
        {
            if (type != DataBytesDefinition.ShortArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (short[])_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new short[length];
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadShort(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.UShortArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort[] ReadUShortArray(byte type)
        {
            if (type != DataBytesDefinition.UShortArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (ushort[])_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new ushort[length];
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadUShort(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.SByteArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte[] ReadSByteArray(byte type)
        {
            if (type != DataBytesDefinition.SByteArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (sbyte[])_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new sbyte[length];
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadSByte(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.StringArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] ReadStringArray(byte type)
        {
            if (type != DataBytesDefinition.StringArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (string[])_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new string[length];
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadString(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.TimeSpanArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan[] ReadTimeSpanArray(byte type)
        {
            if (type != DataBytesDefinition.TimeSpanArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (TimeSpan[])_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new TimeSpan[length];
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadTimeSpan(ReadByte());
            return value;
        }

        [DeserializerMethod(DataBytesDefinition.BoolList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<bool> ReadBoolList(byte type)
        {
            if (type != DataBytesDefinition.BoolList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<bool>)_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new List<bool>(length);
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadBool(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.CharList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<char> ReadCharList(byte type)
        {
            if (type != DataBytesDefinition.CharList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<char>)_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new List<char>(length);
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadChar(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DateTimeOffsetList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<DateTimeOffset> ReadDateTimeOffsetList(byte type)
        {
            if (type != DataBytesDefinition.DateTimeOffsetList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<DateTimeOffset>)_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new List<DateTimeOffset>(length);
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadDateTimeOffset(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DateTimeList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<DateTime> ReadDateTimeList(byte type)
        {
            if (type != DataBytesDefinition.DateTimeList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<DateTime>)_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new List<DateTime>(length);
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadDateTime(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.GuidList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Guid> ReadGuidList(byte type)
        {
            if (type != DataBytesDefinition.GuidList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<Guid>)_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new List<Guid>(length);
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadGuid(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DecimalList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<decimal> ReadDecimalList(byte type)
        {
            if (type != DataBytesDefinition.DecimalList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<decimal>)_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new List<decimal>(length);
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadDecimal(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DoubleList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<double> ReadDoubleList(byte type)
        {
            if (type != DataBytesDefinition.DoubleList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<double>)_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new List<double>(length);
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadDouble(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.FloatList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<float> ReadFloatList(byte type)
        {
            if (type != DataBytesDefinition.FloatList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<float>)_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new List<float>(length);
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadFloat(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.LongList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<long> ReadLongList(byte type)
        {
            if (type != DataBytesDefinition.LongList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<long>)_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new List<long>(length);
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadLong(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.ULongList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<ulong> ReadULongList(byte type)
        {
            if (type != DataBytesDefinition.ULongList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<ulong>)_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new List<ulong>(length);
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadULong(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.IntList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<int> ReadIntList(byte type)
        {
            if (type != DataBytesDefinition.IntList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<int>)_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new List<int>(length);
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadInt(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.UIntList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<uint> ReadUIntList(byte type)
        {
            if (type != DataBytesDefinition.UIntList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<uint>)_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new List<uint>(length);
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadUInt(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.ShortList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<short> ReadShortList(byte type)
        {
            if (type != DataBytesDefinition.ShortList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<short>)_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new List<short>(length);
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadShort(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.UShortList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<ushort> ReadUShortList(byte type)
        {
            if (type != DataBytesDefinition.UShortList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<ushort>)_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new List<ushort>(length);
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadUShort(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.SByteList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<sbyte> ReadSByteList(byte type)
        {
            if (type != DataBytesDefinition.SByteList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<sbyte>)_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new List<sbyte>(length);
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadSByte(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.StringList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<string> ReadStringList(byte type)
        {
            if (type != DataBytesDefinition.StringList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<string>)_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new List<string>(length);
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadString(ReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.TimeSpanList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<TimeSpan> ReadTimeSpanList(byte type)
        {
            if (type != DataBytesDefinition.TimeSpanList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<TimeSpan>)_objectCache.Get(ReadInt());
            var length = ReadInt();
            var value = new List<TimeSpan>(length);
            _objectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadTimeSpan(ReadByte());
            return value;
        }
        #endregion

        #region Private Read Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected byte ReadByte()
        {
            Stream.Read(_buffer, 0, 1);
            return _buffer[0];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ushort ReadUShort()
        {
            Stream.Read(_buffer, 0, 2);
            return BitConverter.ToUInt16(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int ReadInt()
        {
            Stream.Read(_buffer, 0, 4);
            return BitConverter.ToInt32(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected (byte Type, int Value) ReadDefInt()
        {
            Stream.Read(_buffer, 0, 5);
            return (_buffer[0], BitConverter.ToInt32(_buffer, 1));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected double ReadDouble()
        {
            Stream.Read(_buffer, 0, 8);
            return BitConverter.ToDouble(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float ReadFloat()
        {
            Stream.Read(_buffer, 0, 4);
            return BitConverter.ToSingle(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected long ReadLong()
        {
            Stream.Read(_buffer, 0, 8);
            return BitConverter.ToInt64(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ulong ReadULong()
        {
            Stream.Read(_buffer, 0, 8);
            return BitConverter.ToUInt64(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected uint ReadUInt()
        {
            Stream.Read(_buffer, 0, 4);
            return BitConverter.ToUInt32(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected short ReadShort()
        {
            Stream.Read(_buffer, 0, 2);
            return BitConverter.ToInt16(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected char ReadChar()
        {
            Stream.Read(_buffer, 0, 2);
            return BitConverter.ToChar(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected decimal ReadDecimal()
        {
            Stream.Read(_buffer, 0, 8);
            var val = BitConverter.ToDouble(_buffer, 0);
            return new decimal(val);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sbyte ReadSByte()
        {
            Stream.Read(_buffer, 0, 1);
            return (sbyte)_buffer[0];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Guid ReadGuid()
        {
            Stream.Read(_buffer, 0, 16);
            return new Guid(_buffer);
        }
        #endregion
    }

    public struct DeserializerTypeDescriptor
    {
        public ActivatorDelegate Activator;
        public Dictionary<string, FastPropertyInfo> Properties;
        public bool IsNSerializable;
        public bool IsArray;
        public bool IsList;
        public bool IsDictionary;

        public DeserializerTypeDescriptor(Type type)
        {
            var ifaces = type.GetInterfaces();
            var isIList = ifaces.Any(i => i == typeof(IList) || (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)));
            var isIDictionary = ifaces.Any(i => i == typeof(IDictionary) || (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)));
            var runtimeProperties = type.GetRuntimeProperties().OrderBy(p => p.Name).Where(prop =>
            {
                if (prop.IsSpecialName || !prop.CanRead || !prop.CanWrite) return false;
                if (prop.GetAttribute<NonSerializeAttribute>() != null) return false;
                if (prop.GetIndexParameters().Length > 0) return false;
                if (isIList && prop.Name == "Capacity") return false;
                return true;
            }).ToArray();

            Activator = Factory.Accessors.CreateActivator(type);
            IsNSerializable = ifaces.Any(i => i == typeof(INSerializable));
            Properties = new Dictionary<string, FastPropertyInfo>();
            foreach (var prop in runtimeProperties)
                Properties[prop.Name] = prop.GetFastPropertyInfo();

            IsArray = type.IsArray;
            if (!IsArray)
            {
                IsDictionary = isIDictionary;
                IsList = !IsDictionary && isIList;
            }
            else
            {
                IsList = false;
                IsDictionary = false;
            }
        }
    }
}