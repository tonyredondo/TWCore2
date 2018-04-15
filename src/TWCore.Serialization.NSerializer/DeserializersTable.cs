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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
        internal static readonly MethodInfo StreamReadByteMethod = typeof(DeserializersTable).GetMethod("StreamReadByte", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly MethodInfo StreamReadIntMethod = typeof(DeserializersTable).GetMethod("StreamReadInt", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly byte[] EmptyBytes = new byte[0];
        private readonly byte[] _buffer = new byte[16];
        private readonly object[] _parameters = new object[1];
        internal readonly DeserializerCache<object> ObjectCache = new DeserializerCache<object>();
        private readonly DeserializerCache<Type> _typeCache = new DeserializerCache<Type>();
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

            var value = ReadValue(StreamReadByte());
            while (StreamReadByte() != DataBytesDefinition.End)
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
            ObjectCache.Clear();
            _typeCache.Clear();
            Stream = null;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object ReadValue(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return ObjectCache.Get(StreamReadInt());
            if (ReadValues.TryGetValue(type, out var mTuple))
            {
                _parameters[0] = type;
                return mTuple.Accessor(this, _parameters);
            }
            Type valueType = null;
            string[] properties = null;

            if (type == DataBytesDefinition.TypeStart)
            {
                var length = StreamReadInt();
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
                valueType = _typeCache.Get(StreamReadInt());
                properties = _propertiesByType[valueType];
            }
            var descriptor = Descriptors.GetOrAdd(valueType, vType => new DeserializerTypeDescriptor(vType));
            if (descriptor.IsNSerializable)
            {
                var value = (INSerializable) descriptor.Activator();
                value.Fill(this, properties);
                return value;
            }
            return FillObject(descriptor, properties);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object FillObject(DeserializerTypeDescriptor descriptor, string[] properties)
        {
            object value;
            var flag = StreamReadByte();
            if (flag == DataBytesDefinition.ArrayStart || flag == DataBytesDefinition.ListStart || flag == DataBytesDefinition.DictionaryStart)
            {
                var capacity = StreamReadInt();
                value = descriptor.Activator(capacity);
                ObjectCache.Set(value);
                
                if (descriptor.IsArray)
                {
                    var aValue = (Array) value;
                    for (var i = 0; i < capacity; i++)
                    {
                        var item = ReadValue(StreamReadByte());
                        aValue.SetValue(item, i);
                    }
                }
                else if (descriptor.IsList)
                {
                    var iValue = (IList) value;
                    for (var i = 0; i < capacity; i++)
                    {
                        var item = ReadValue(StreamReadByte());
                        iValue.Add(item);
                    }
                }
                else if (descriptor.IsDictionary)
                {
                    var dictio = (IDictionary) value;
                    for (var i = 0; i < capacity; i++)
                    {
                        var dKey = ReadValue(StreamReadByte());
                        var dValue = ReadValue(StreamReadByte());
                        dictio[dKey] = dValue;
                    }
                }
                
                flag = StreamReadByte();
                if (flag == DataBytesDefinition.TypeEnd)
                    return value;
            }
            else
            {
                value = descriptor.Activator();
                ObjectCache.Set(value);
            }
            
            if (flag == DataBytesDefinition.PropertiesStart)
            {
                for (var i = 0; i < properties.Length; i++)
                {
                    var name = properties[i];
                    if (descriptor.Properties.TryGetValue(name, out var fProp))
                    {
                        fProp.SetValue(value, ReadValue(StreamReadByte()));
                    }
                    else
                    {
                        //Notify property not found.
                    }
                }
            }
            flag = StreamReadByte();
            return value;
        }

        #region Read Values
        [DeserializerMethod(DataBytesDefinition.BoolArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool[] ReadBoolArray(byte type)
        {
            if (type != DataBytesDefinition.BoolArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (bool[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new bool[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadBool(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.CharArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char[] ReadCharArray(byte type)
        {
            if (type != DataBytesDefinition.CharArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (char[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new char[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadChar(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DateTimeOffsetArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset[] ReadDateTimeOffsetArray(byte type)
        {
            if (type != DataBytesDefinition.DateTimeOffsetArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (DateTimeOffset[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new DateTimeOffset[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadDateTimeOffset(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DateTimeArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime[] ReadDateTimeArray(byte type)
        {
            if (type != DataBytesDefinition.DateTimeArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (DateTime[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new DateTime[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadDateTime(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.GuidArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid[] ReadGuidArray(byte type)
        {
            if (type != DataBytesDefinition.GuidArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (Guid[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new Guid[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadGuid(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DecimalArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal[] ReadDecimalArray(byte type)
        {
            if (type != DataBytesDefinition.DecimalArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (decimal[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new decimal[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadDecimal(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DoubleArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double[] ReadDoubleArray(byte type)
        {
            if (type != DataBytesDefinition.DoubleArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (double[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new double[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadDouble(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.FloatArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] ReadFloatArray(byte type)
        {
            if (type != DataBytesDefinition.FloatArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (float[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new float[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadFloat(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.LongArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long[] ReadLongArray(byte type)
        {
            if (type != DataBytesDefinition.LongArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (long[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new long[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadLong(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.ULongArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong[] ReadULongArray(byte type)
        {
            if (type != DataBytesDefinition.ULongArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (ulong[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new ulong[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadULong(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.IntArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int[] ReadIntArray(byte type)
        {
            if (type != DataBytesDefinition.IntArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (int[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new int[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadInt(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.UIntArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint[] ReadUIntArray(byte type)
        {
            if (type != DataBytesDefinition.UIntArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (uint[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new uint[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadUInt(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.ShortArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short[] ReadShortArray(byte type)
        {
            if (type != DataBytesDefinition.ShortArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (short[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new short[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadShort(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.UShortArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort[] ReadUShortArray(byte type)
        {
            if (type != DataBytesDefinition.UShortArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (ushort[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new ushort[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadUShort(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.SByteArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte[] ReadSByteArray(byte type)
        {
            if (type != DataBytesDefinition.SByteArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (sbyte[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new sbyte[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadSByte(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.StringArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] ReadStringArray(byte type)
        {
            if (type != DataBytesDefinition.StringArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (string[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new string[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadString(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.TimeSpanArray)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan[] ReadTimeSpanArray(byte type)
        {
            if (type != DataBytesDefinition.TimeSpanArray) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (TimeSpan[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new TimeSpan[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadTimeSpan(StreamReadByte());
            return value;
        }

        [DeserializerMethod(DataBytesDefinition.BoolList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<bool> ReadBoolList(byte type)
        {
            if (type != DataBytesDefinition.BoolList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<bool>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<bool>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadBool(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.CharList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<char> ReadCharList(byte type)
        {
            if (type != DataBytesDefinition.CharList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<char>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<char>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadChar(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DateTimeOffsetList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<DateTimeOffset> ReadDateTimeOffsetList(byte type)
        {
            if (type != DataBytesDefinition.DateTimeOffsetList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<DateTimeOffset>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<DateTimeOffset>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadDateTimeOffset(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DateTimeList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<DateTime> ReadDateTimeList(byte type)
        {
            if (type != DataBytesDefinition.DateTimeList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<DateTime>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<DateTime>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadDateTime(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.GuidList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Guid> ReadGuidList(byte type)
        {
            if (type != DataBytesDefinition.GuidList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<Guid>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<Guid>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadGuid(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DecimalList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<decimal> ReadDecimalList(byte type)
        {
            if (type != DataBytesDefinition.DecimalList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<decimal>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<decimal>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadDecimal(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DoubleList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<double> ReadDoubleList(byte type)
        {
            if (type != DataBytesDefinition.DoubleList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<double>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<double>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadDouble(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.FloatList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<float> ReadFloatList(byte type)
        {
            if (type != DataBytesDefinition.FloatList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<float>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<float>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadFloat(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.LongList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<long> ReadLongList(byte type)
        {
            if (type != DataBytesDefinition.LongList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<long>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<long>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadLong(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.ULongList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<ulong> ReadULongList(byte type)
        {
            if (type != DataBytesDefinition.ULongList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<ulong>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<ulong>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadULong(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.IntList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<int> ReadIntList(byte type)
        {
            if (type != DataBytesDefinition.IntList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<int>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<int>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadInt(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.UIntList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<uint> ReadUIntList(byte type)
        {
            if (type != DataBytesDefinition.UIntList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<uint>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<uint>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadUInt(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.ShortList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<short> ReadShortList(byte type)
        {
            if (type != DataBytesDefinition.ShortList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<short>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<short>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadShort(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.UShortList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<ushort> ReadUShortList(byte type)
        {
            if (type != DataBytesDefinition.UShortList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<ushort>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<ushort>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadUShort(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.SByteList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<sbyte> ReadSByteList(byte type)
        {
            if (type != DataBytesDefinition.SByteList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<sbyte>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<sbyte>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadSByte(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.StringList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<string> ReadStringList(byte type)
        {
            if (type != DataBytesDefinition.StringList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<string>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<string>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadString(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.TimeSpanList)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<TimeSpan> ReadTimeSpanList(byte type)
        {
            if (type != DataBytesDefinition.TimeSpanList) throw new FormatException();
            if (type == DataBytesDefinition.RefObject)
                return (List<TimeSpan>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<TimeSpan>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadTimeSpan(StreamReadByte());
            return value;
        }
        #endregion

        #region Private Read Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected byte StreamReadByte()
        {
            Stream.Read(_buffer, 0, 1);
            return _buffer[0];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ushort StreamReadUShort()
        {
            Stream.Read(_buffer, 0, 2);
            return BitConverter.ToUInt16(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int StreamReadInt()
        {
            Stream.Read(_buffer, 0, 4);
            return BitConverter.ToInt32(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected double StreamReadDouble()
        {
            Stream.Read(_buffer, 0, 8);
            return BitConverter.ToDouble(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float StreamReadFloat()
        {
            Stream.Read(_buffer, 0, 4);
            return BitConverter.ToSingle(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected long StreamReadLong()
        {
            Stream.Read(_buffer, 0, 8);
            return BitConverter.ToInt64(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ulong StreamReadULong()
        {
            Stream.Read(_buffer, 0, 8);
            return BitConverter.ToUInt64(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected uint StreamReadUInt()
        {
            Stream.Read(_buffer, 0, 4);
            return BitConverter.ToUInt32(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected short StreamReadShort()
        {
            Stream.Read(_buffer, 0, 2);
            return BitConverter.ToInt16(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected char StreamReadChar()
        {
            Stream.Read(_buffer, 0, 2);
            return BitConverter.ToChar(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected decimal StreamReadDecimal()
        {
            Stream.Read(_buffer, 0, 8);
            var val = BitConverter.ToDouble(_buffer, 0);
            return new decimal(val);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sbyte StreamReadSByte()
        {
            Stream.Read(_buffer, 0, 1);
            return (sbyte)_buffer[0];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Guid StreamReadGuid()
        {
            Stream.Read(_buffer, 0, 16);
            return new Guid(_buffer);
        }
        #endregion
    }

    public struct DeserializerTypeDescriptor
    {
        public Type Type;
        public ActivatorDelegate Activator;
        public Dictionary<string, FastPropertyInfo> Properties;
        public bool IsNSerializable;
        public bool IsArray;
        public bool IsList;
        public bool IsDictionary;

        public DeserializerTypeDescriptor(Type type)
        {
            Type = type;
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
            
            //*** Expressions
            var serExpressions = new List<Expression>();
            var varExpressions = new List<ParameterExpression>();
            //
            var table = Expression.Parameter(typeof(DeserializersTable), "table");
            var descriptor = Expression.Parameter(typeof(DeserializerTypeDescriptor), "descriptor");
            var properties = Expression.Parameter(typeof(string[]), "properties");

            var value = Expression.Parameter(typeof(object), "value");
            var flag = Expression.Parameter(typeof(byte), "flag");
            varExpressions.Add(value);
            varExpressions.Add(flag);
            
            //load next flag
            serExpressions.Add(Expression.Assign(flag, Expression.Call(table, DeserializersTable.StreamReadByteMethod)));

            var arrayOrListIf = Expression.Or(
                Expression.Equal(flag, Expression.Constant(DataBytesDefinition.ArrayStart, typeof(byte))),
                Expression.Equal(flag, Expression.Constant(DataBytesDefinition.ListStart, typeof(byte))));
            var dictionaryIf = Expression.Equal(flag, 
                Expression.Constant(DataBytesDefinition.DictionaryStart, typeof(byte)));

            var comparerExpression = Expression.Or(arrayOrListIf, dictionaryIf);
        }

        public delegate object DeserializeDelegate(DeserializersTable table, DeserializerTypeDescriptor descriptor, string[] parameters);
    }
}