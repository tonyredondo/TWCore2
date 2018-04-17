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
        internal static readonly Dictionary<Type, MethodInfo> ReadValuesFromType = new Dictionary<Type, MethodInfo>();
        internal static readonly ConcurrentDictionary<Type, DeserializerTypeDescriptor> Descriptors = new ConcurrentDictionary<Type, DeserializerTypeDescriptor>();
        internal static readonly MethodInfo StreamReadByteMethod = typeof(DeserializersTable).GetMethod("StreamReadByte", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly MethodInfo StreamReadIntMethod = typeof(DeserializersTable).GetMethod("StreamReadInt", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly byte[] EmptyBytes = new byte[0];
        private readonly byte[] _buffer = new byte[16];
        private readonly object[] _parameters = new object[1];
        internal readonly DeserializerCache<object> ObjectCache = new DeserializerCache<object>();
        private readonly DeserializerCache<DeserializerMetadataOfTypeRuntime> _typeCache = new DeserializerCache<DeserializerMetadataOfTypeRuntime>();
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
        //private readonly Dictionary<Type, DeserializerMetadataOfTypeRuntime> _metadataInTypes = new Dictionary<Type, DeserializerMetadataOfTypeRuntime>();
        protected Stream Stream;
        protected BinaryReader Reader;

        #region Attributes
        public class DeserializerMethodAttribute : Attribute
        {
            public byte[] ByteTypes { get; }
            public Type ReturnType { get; set; }
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
                ReadValuesFromType[attr.ReturnType] = method;
            }
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Deserialize(Stream stream)
        {
            object value;
            try
            {
                Stream = stream;
                Reader = new BinaryReader(stream, Encoding.UTF8, true);
                if (stream.ReadByte() != DataBytesDefinition.Start)
                    throw new FormatException("The stream is not in NSerializer format.");

                value = ReadValue(StreamReadByte());
                while (StreamReadByte() != DataBytesDefinition.End)
                {
                }
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
                ObjectCache.Clear();
                _typeCache.Clear();
                Stream = null;
                Reader = null;
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object ReadValue(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
            {
                var idx = StreamReadInt();
                return ObjectCache.Get(idx);
            }
            if (ReadValues.TryGetValue(type, out var mTuple))
            {
                _parameters[0] = type;
                return mTuple.Accessor(this, _parameters);
            }

            DeserializerMetadataOfTypeRuntime metadata = default;

            if (type == DataBytesDefinition.TypeStart)
            {
                var length = StreamReadInt();
                var typeBytes = new byte[length];
                Stream.Read(typeBytes, 0, length);
                var typeData = Encoding.UTF8.GetString(typeBytes, 0, length);
                var fsCol1 = typeData.IndexOf(";", StringComparison.Ordinal);
                var fsCol2 = typeData.IndexOf(";", fsCol1 + 1, StringComparison.Ordinal);
                var fsCol3 = typeData.IndexOf(";", fsCol2 + 1, StringComparison.Ordinal);
                var fsCol4 = typeData.IndexOf(";", fsCol3 + 1, StringComparison.Ordinal);
                var vTypeString = typeData.Substring(0, fsCol1);
                var isArrayString = typeData.Substring(fsCol1 + 1, 1);
                var isListString = typeData.Substring(fsCol2 + 1, 1);
                var isDictionaryString = typeData.Substring(fsCol3 + 1, 1);
                var propertiesString = typeData.Substring(fsCol4 + 1);

                var valueType = Core.GetType(vTypeString);
                var isArray = isArrayString == "1";
                var isList = isListString == "1";
                var isDictionary = isDictionaryString == "1";
                var properties = propertiesString.Split(";", StringSplitOptions.RemoveEmptyEntries);
                var runtimeMeta = new DeserializerMetaDataOfType(valueType, isArray, isList, isDictionary, properties);
                var descriptor = Descriptors.GetOrAdd(valueType, vType => new DeserializerTypeDescriptor(vType));
                metadata = new DeserializerMetadataOfTypeRuntime(runtimeMeta, descriptor);
                _typeCache.Set(metadata);
            }
            else if (type == DataBytesDefinition.RefType)
            {
                metadata = _typeCache.Get(StreamReadInt());
            }
            if (metadata.Descriptor.IsNSerializable)
            {
                var value = (INSerializable)metadata.Descriptor.Activator();
                value.Fill(this, metadata.MetaDataOfType);
                return value;
            }
            if (metadata.EqualToDefinition)
                return metadata.Descriptor.DeserializeFunc(this);
            return FillObject(metadata.Descriptor, metadata.MetaDataOfType);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object InnerReadValue(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
            {
                var idx = StreamReadInt();
                return ObjectCache.Get(idx);
            }

            DeserializerMetadataOfTypeRuntime metadata = default;

            if (type == DataBytesDefinition.TypeStart)
            {
                var length = StreamReadInt();
                var typeBytes = new byte[length];
                Stream.Read(typeBytes, 0, length);
                var typeData = Encoding.UTF8.GetString(typeBytes, 0, length);
                var fsCol1 = typeData.IndexOf(";", StringComparison.Ordinal);
                var fsCol2 = typeData.IndexOf(";", fsCol1 + 1, StringComparison.Ordinal);
                var fsCol3 = typeData.IndexOf(";", fsCol2 + 1, StringComparison.Ordinal);
                var fsCol4 = typeData.IndexOf(";", fsCol3 + 1, StringComparison.Ordinal);
                var vTypeString = typeData.Substring(0, fsCol1);
                var isArrayString = typeData.Substring(fsCol1 + 1, 1);
                var isListString = typeData.Substring(fsCol2 + 1, 1);
                var isDictionaryString = typeData.Substring(fsCol3 + 1, 1);
                var propertiesString = typeData.Substring(fsCol4 + 1);

                var valueType = Core.GetType(vTypeString);
                var isArray = isArrayString == "1";
                var isList = isListString == "1";
                var isDictionary = isDictionaryString == "1";
                var properties = propertiesString.Split(";", StringSplitOptions.RemoveEmptyEntries);
                var runtimeMeta = new DeserializerMetaDataOfType(valueType, isArray, isList, isDictionary, properties);
                var descriptor = Descriptors.GetOrAdd(valueType, vType => new DeserializerTypeDescriptor(vType));
                metadata = new DeserializerMetadataOfTypeRuntime(runtimeMeta, descriptor);
                _typeCache.Set(metadata);
            }
            else if (type == DataBytesDefinition.RefType)
            {
                metadata = _typeCache.Get(StreamReadInt());
            }
            if (metadata.Descriptor.IsNSerializable)
            {
                var value = (INSerializable)metadata.Descriptor.Activator();
                value.Fill(this, metadata.MetaDataOfType);
                return value;
            }
            if (metadata.EqualToDefinition)
                return metadata.Descriptor.DeserializeFunc(this);
            return FillObject(metadata.Descriptor, metadata.MetaDataOfType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object FillObject(DeserializerTypeDescriptor descriptor, DeserializerMetaDataOfType metadata)
        {
            object value;
            if (metadata.IsArray || metadata.IsList || metadata.IsDictionary)
            {
                var capacity = StreamReadInt();
                value = descriptor.Activator(capacity);
                ObjectCache.Set(value);

                if (descriptor.Metadata.IsArray)
                {
                    var aValue = (Array)value;
                    for (var i = 0; i < capacity; i++)
                    {
                        var item = ReadValue(StreamReadByte());
                        aValue.SetValue(item, i);
                    }
                }
                else if (descriptor.Metadata.IsList)
                {
                    var iValue = (IList)value;
                    for (var i = 0; i < capacity; i++)
                    {
                        var item = ReadValue(StreamReadByte());
                        iValue.Add(item);
                    }
                }
                else if (descriptor.Metadata.IsDictionary)
                {
                    var dictio = (IDictionary)value;
                    for (var i = 0; i < capacity; i++)
                    {
                        var dKey = ReadValue(StreamReadByte());
                        var dValue = ReadValue(StreamReadByte());
                        dictio[dKey] = dValue;
                    }
                }
            }
            else
            {
                value = descriptor.Activator();
                ObjectCache.Set(value);
            }

            for (var i = 0; i < metadata.Properties.Length; i++)
            {
                var name = metadata.Properties[i];
                if (descriptor.Properties.TryGetValue(name, out var fProp))
                {
                    fProp.SetValue(value, ReadValue(StreamReadByte()));
                }
            }

            StreamReadByte();
            return value;
        }

        #region Read Values
        [DeserializerMethod(DataBytesDefinition.BoolArray, ReturnType = typeof(bool[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool[] ReadBoolArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (bool[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new bool[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadBool(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.CharArray, ReturnType = typeof(char[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char[] ReadCharArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (char[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new char[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadChar(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DateTimeOffsetArray, ReturnType = typeof(DateTimeOffset[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset[] ReadDateTimeOffsetArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (DateTimeOffset[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new DateTimeOffset[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadDateTimeOffset(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DateTimeArray, ReturnType = typeof(DateTime[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime[] ReadDateTimeArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (DateTime[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new DateTime[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadDateTime(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.GuidArray, ReturnType = typeof(Guid[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid[] ReadGuidArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (Guid[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new Guid[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadGuid(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DecimalArray, ReturnType = typeof(decimal[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal[] ReadDecimalArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (decimal[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new decimal[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadDecimal(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DoubleArray, ReturnType = typeof(double[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double[] ReadDoubleArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (double[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new double[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadDouble(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.FloatArray, ReturnType = typeof(float[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] ReadFloatArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (float[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new float[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadFloat(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.LongArray, ReturnType = typeof(long[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long[] ReadLongArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (long[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new long[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadLong(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.ULongArray, ReturnType = typeof(ulong[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong[] ReadULongArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (ulong[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new ulong[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadULong(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.IntArray, ReturnType = typeof(int[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int[] ReadIntArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (int[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new int[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadInt(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.UIntArray, ReturnType = typeof(uint[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint[] ReadUIntArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (uint[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new uint[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadUInt(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.ShortArray, ReturnType = typeof(short[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short[] ReadShortArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (short[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new short[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadShort(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.UShortArray, ReturnType = typeof(ushort[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort[] ReadUShortArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (ushort[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new ushort[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadUShort(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.SByteArray, ReturnType = typeof(sbyte[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte[] ReadSByteArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (sbyte[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new sbyte[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadSByte(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.StringArray, ReturnType = typeof(string[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] ReadStringArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (string[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new string[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadString(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.TimeSpanArray, ReturnType = typeof(TimeSpan[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan[] ReadTimeSpanArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (TimeSpan[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new TimeSpan[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadTimeSpan(StreamReadByte());
            return value;
        }

        [DeserializerMethod(DataBytesDefinition.BoolList, ReturnType = typeof(List<bool>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<bool> ReadBoolList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (List<bool>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<bool>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadBool(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.CharList, ReturnType = typeof(List<char>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<char> ReadCharList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (List<char>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<char>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadChar(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DateTimeOffsetList, ReturnType = typeof(List<DateTimeOffset>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<DateTimeOffset> ReadDateTimeOffsetList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (List<DateTimeOffset>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<DateTimeOffset>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadDateTimeOffset(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DateTimeList, ReturnType = typeof(List<DateTime>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<DateTime> ReadDateTimeList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (List<DateTime>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<DateTime>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadDateTime(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.GuidList, ReturnType = typeof(List<Guid>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Guid> ReadGuidList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (List<Guid>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<Guid>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadGuid(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DecimalList, ReturnType = typeof(List<decimal>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<decimal> ReadDecimalList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (List<decimal>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<decimal>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadDecimal(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.DoubleList, ReturnType = typeof(List<double>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<double> ReadDoubleList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (List<double>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<double>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadDouble(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.FloatList, ReturnType = typeof(List<float>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<float> ReadFloatList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (List<float>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<float>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadFloat(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.LongList, ReturnType = typeof(List<long>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<long> ReadLongList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (List<long>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<long>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadLong(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.ULongList, ReturnType = typeof(List<ulong>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<ulong> ReadULongList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (List<ulong>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<ulong>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadULong(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.IntList, ReturnType = typeof(List<int>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<int> ReadIntList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (List<int>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<int>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadInt(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.UIntList, ReturnType = typeof(List<uint>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<uint> ReadUIntList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (List<uint>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<uint>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadUInt(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.ShortList, ReturnType = typeof(List<short>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<short> ReadShortList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (List<short>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<short>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadShort(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.UShortList, ReturnType = typeof(List<ushort>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<ushort> ReadUShortList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (List<ushort>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<ushort>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadUShort(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.SByteList, ReturnType = typeof(List<sbyte>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<sbyte> ReadSByteList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (List<sbyte>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<sbyte>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = StreamReadSByte(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.StringList, ReturnType = typeof(List<string>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<string> ReadStringList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (List<string>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<string>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadString(StreamReadByte());
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.TimeSpanList, ReturnType = typeof(List<TimeSpan>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<TimeSpan> ReadTimeSpanList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
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
        protected byte StreamReadByte() => Reader.ReadByte();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ushort StreamReadUShort() => Reader.ReadUInt16();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int StreamReadInt() => Reader.ReadInt32();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected double StreamReadDouble() => Reader.ReadDouble();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float StreamReadFloat() => Reader.ReadSingle();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected long StreamReadLong() => Reader.ReadInt64();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ulong StreamReadULong() => Reader.ReadUInt64();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected uint StreamReadUInt() => Reader.ReadUInt32();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected short StreamReadShort() => Reader.ReadInt16();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected char StreamReadChar() => Reader.ReadChar();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected decimal StreamReadDecimal() => Reader.ReadDecimal();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sbyte StreamReadSByte() => Reader.ReadSByte();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Guid StreamReadGuid()
        {
            Stream.Read(_buffer, 0, 16);
            return new Guid(_buffer);
        }
        #endregion
    }
}