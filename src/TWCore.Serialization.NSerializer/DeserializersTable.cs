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
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using TWCore.Reflection;
// ReSharper disable UnusedMember.Local
#pragma warning disable 1591

namespace TWCore.Serialization.NSerializer
{
    public partial class DeserializersTable
    {
        #region Static fields
        internal static readonly Dictionary<byte, (MethodInfo Method, MethodAccessorDelegate Accessor)> ReadValues = new Dictionary<byte, (MethodInfo Method, MethodAccessorDelegate Accessor)>();
        internal static readonly Dictionary<Type, MethodInfo> ReadValuesFromType = new Dictionary<Type, MethodInfo>();
        internal static readonly ConcurrentDictionary<Type, DeserializerTypeDescriptor> Descriptors = new ConcurrentDictionary<Type, DeserializerTypeDescriptor>();
        internal static readonly ConcurrentDictionary<MultiArray<byte>, DeserializerMetadataOfTypeRuntime> MultiArrayMetadata = new ConcurrentDictionary<MultiArray<byte>, DeserializerMetadataOfTypeRuntime>(MultiArrayBytesComparer.Instance);
        internal static readonly MethodInfo StreamReadByteMethod = typeof(DeserializersTable).GetMethod("StreamReadByte", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly MethodInfo StreamReadIntMethod = typeof(DeserializersTable).GetMethod("StreamReadInt", BindingFlags.NonPublic | BindingFlags.Instance);
        #endregion

        #region Fields
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
        private readonly HashSet<string> _serErrors = new HashSet<string>();
        protected Stream Stream;
        #endregion


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
                if (attr is null) continue;
                foreach (var byteType in attr.ByteTypes)
                    ReadValues[byteType] = (method, Factory.Accessors.BuildMethodAccessor(method));
                ReadValuesFromType[attr.ReturnType] = method;
            }
        }
        #endregion

        #region Normal Deserializer
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Deserialize(Stream stream)
        {
            object value;
            try
            {
                Stream = stream;
                var firstByte = stream.ReadByte();
                if (firstByte == -1)
                    throw new IOException("The stream has been closed.");
                if (firstByte != DataBytesDefinition.Start)
                    throw new FormatException("The stream is not in NSerializer format.");
                value = ReadValue(StreamReadByte());
                while (StreamReadByte() != DataBytesDefinition.End) { }
            }
            catch (Exception ex)
            {
                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    var genericValue = (GenericObject)GenericObjectDeserialize(stream);
                    throw new ExceptionWithGenericObject(ex, genericValue);
                }
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
                _serErrors.Clear();
                Stream = null;
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
                var typeBytes = ArrayPool<byte>.Shared.Rent(length);
                Stream.Read(typeBytes, 0, length);
                var subTypeBytes = new MultiArray<byte>(typeBytes, 0, length);
                if (!MultiArrayMetadata.TryGetValue(subTypeBytes, out metadata))
                {
                    var typeData = Encoding.UTF8.GetString(typeBytes, 0, length);
                    var fsCol1 = typeData.IndexOf(";", StringComparison.Ordinal);
                    var fsCol2 = typeData.IndexOf(";", fsCol1 + 1, StringComparison.Ordinal);
                    var fsCol3 = typeData.IndexOf(";", fsCol2 + 1, StringComparison.Ordinal);
                    var fsCol4 = typeData.IndexOf(";", fsCol3 + 1, StringComparison.Ordinal);
                    var vTypeString = typeData.Substring(0, fsCol1);
                    var isArray = typeData[fsCol1 + 1] == '1';
                    var isList = typeData[fsCol2 + 1] == '1';
                    var isDictionary = typeData[fsCol3 + 1] == '1';
                    var propertiesString = typeData.Substring(fsCol4 + 1);
                    var valueType = Core.GetType(vTypeString, true);
                    if (valueType == null)
                        throw new Exception($"The type: {vTypeString} could not be found or loaded.");
                    var properties = propertiesString.Split(";", StringSplitOptions.RemoveEmptyEntries);
                    var runtimeMeta = new DeserializerMetaDataOfType(valueType, isArray, isList, isDictionary, properties);
                    var descriptor = Descriptors.GetOrAdd(valueType, vType => new DeserializerTypeDescriptor(vType));
                    metadata = new DeserializerMetadataOfTypeRuntime(runtimeMeta, descriptor);
                    MultiArrayMetadata.TryAdd(new MultiArray<byte>(subTypeBytes.ToArray()), metadata);
                }
                ArrayPool<byte>.Shared.Return(typeBytes);
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
                var typeBytes = ArrayPool<byte>.Shared.Rent(length);
                Stream.Read(typeBytes, 0, length);
                var subTypeBytes = new MultiArray<byte>(typeBytes, 0, length);
                if (!MultiArrayMetadata.TryGetValue(subTypeBytes, out metadata))
                {
                    var typeData = Encoding.UTF8.GetString(typeBytes, 0, length);
                    var fsCol1 = typeData.IndexOf(";", StringComparison.Ordinal);
                    var fsCol2 = typeData.IndexOf(";", fsCol1 + 1, StringComparison.Ordinal);
                    var fsCol3 = typeData.IndexOf(";", fsCol2 + 1, StringComparison.Ordinal);
                    var fsCol4 = typeData.IndexOf(";", fsCol3 + 1, StringComparison.Ordinal);
                    var vTypeString = typeData.Substring(0, fsCol1);
                    var isArray = typeData[fsCol1 + 1] == '1';
                    var isList = typeData[fsCol2 + 1] == '1';
                    var isDictionary = typeData[fsCol3 + 1] == '1';
                    var propertiesString = typeData.Substring(fsCol4 + 1);
                    var valueType = Core.GetType(vTypeString);
                    var properties = propertiesString.Split(";", StringSplitOptions.RemoveEmptyEntries);
                    var runtimeMeta = new DeserializerMetaDataOfType(valueType, isArray, isList, isDictionary, properties);
                    var descriptor = Descriptors.GetOrAdd(valueType, vType => new DeserializerTypeDescriptor(vType));
                    metadata = new DeserializerMetadataOfTypeRuntime(runtimeMeta, descriptor);
                    MultiArrayMetadata.TryAdd(new MultiArray<byte>(subTypeBytes.ToArray()), metadata);
                }
                ArrayPool<byte>.Shared.Return(typeBytes);
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
            try
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
                    var propValue = ReadValue(StreamReadByte());

                    if (descriptor.Properties.TryGetValue(name, out var fProp))
                    {
                        try
                        {
                            fProp.SetValue(value, propValue);
                        }
                        catch (Exception ex)
                        {
                            var metaProperties = metadata.Properties.Join(", ");
                            var typeProperties = descriptor.Metadata.Properties?.Join(", ");
                            var strMsg = $"Error trying to fill the property '{name}' of the object type: {metadata.Type?.FullName}; with a different Definition [{metaProperties}] != [{typeProperties}].";
                            if (_serErrors.Add(strMsg))
                                Core.Log.Error(ex, strMsg);
                        }
                    }
                    else
                    {
                        var strMsg = string.Format("The Property '{0}' can't be found in the type '{1}'", name, metadata.Type?.FullName);
                        if (_serErrors.Add(strMsg))
                            Core.Log.Warning(strMsg);
                    }
                }

                StreamReadByte();
                return value;
            }
            catch (Exception ex)
            {
                var metaProperties = metadata.Properties?.Join(", ");
                var typeProperties = descriptor.Metadata.Properties?.Join(", ");
                throw new Exception($"Error trying to fill an object of type: {metadata.Type?.FullName}; with a different Definition [{metaProperties}] != [{typeProperties}]", ex);
            }
        }
        #endregion

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
        [DeserializerMethod(DataBytesDefinition.ObjectArray, ReturnType = typeof(object[]))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object[] ReadObjectArray(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (object[])ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new object[length];
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadValue(StreamReadByte());
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
                value.Add(ReadBool(StreamReadByte()));
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
                value.Add(StreamReadChar(StreamReadByte()));
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
                value.Add(ReadDateTimeOffset(StreamReadByte()));
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
                value.Add(ReadDateTime(StreamReadByte()));
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
                value.Add(StreamReadGuid(StreamReadByte()));
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
                value.Add(StreamReadDecimal(StreamReadByte()));
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
                value.Add(StreamReadDouble(StreamReadByte()));
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
                value.Add(StreamReadFloat(StreamReadByte()));
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
                value.Add(StreamReadLong(StreamReadByte()));
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
                value.Add(StreamReadULong(StreamReadByte()));
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
                value.Add(StreamReadInt(StreamReadByte()));
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
                value.Add(StreamReadUInt(StreamReadByte()));
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
                value.Add(StreamReadShort(StreamReadByte()));
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
                value.Add(StreamReadUShort(StreamReadByte()));
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
                value.Add(StreamReadSByte(StreamReadByte()));
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
                value.Add(ReadString(StreamReadByte()));
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
                value.Add(ReadTimeSpan(StreamReadByte()));
            return value;
        }
        [DeserializerMethod(DataBytesDefinition.ObjectList, ReturnType = typeof(List<object>))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<object> ReadObjectList(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
                return (List<object>)ObjectCache.Get(StreamReadInt());
            var length = StreamReadInt();
            var value = new List<object>(length);
            ObjectCache.Set(value);
            for (var i = 0; i < length; i++)
                value[i] = ReadValue(StreamReadByte());
            return value;
        }
        #endregion

        #region Private Read Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected byte StreamReadByte()
        {
            var res = Stream.ReadByte();
            if (res == -1)
                throw new IOException("The stream has been closed.");
            return (byte)res;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sbyte StreamReadSByte()
        {
            return (sbyte)Stream.ReadByte();
        }

#if COMPATIBILITY
        byte[] _buffer = new byte[16];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ushort StreamReadUShort()
        {
            Stream.ReadExact(_buffer, 0, 2);
            return BitConverter.ToUInt16(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int StreamReadInt()
        {
            Stream.ReadExact(_buffer, 0, 4);
            return BitConverter.ToInt32(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected double StreamReadDouble()
        {
            Stream.ReadExact(_buffer, 0, 8);
            return BitConverter.ToDouble(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float StreamReadFloat()
        {
            Stream.ReadExact(_buffer, 0, 4);
            return BitConverter.ToSingle(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected long StreamReadLong()
        {
            Stream.ReadExact(_buffer, 0, 8);
            return BitConverter.ToInt64(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ulong StreamReadULong()
        {
            Stream.ReadExact(_buffer, 0, 8);
            return BitConverter.ToUInt64(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected uint StreamReadUInt()
        {
            Stream.ReadExact(_buffer, 0, 4);
            return BitConverter.ToUInt32(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected short StreamReadShort()
        {
            Stream.ReadExact(_buffer, 0, 2);
            return BitConverter.ToInt16(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected char StreamReadChar()
        {
            Stream.ReadExact(_buffer, 0, 2);
            return BitConverter.ToChar(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected decimal StreamReadDecimal()
        {
            Stream.ReadExact(_buffer, 0, 16);
            var bits = new int[4];
            for (var i = 0; i < 4; i++)
                bits[i] = BitConverter.ToInt32(_buffer, i * 4);
            return new decimal(bits);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Guid StreamReadGuid()
        {
            Stream.ReadExact(_buffer, 0, 16);
            return new Guid(_buffer.AsSpan(0, 16).ToArray());
        }

#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ushort StreamReadUShort()
        {
            Span<byte> buffer = stackalloc byte[2];
            Stream.Fill(buffer);
            return BitConverter.ToUInt16(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int StreamReadInt()
        {
            Span<byte> buffer = stackalloc byte[4];
            Stream.Fill(buffer);
            return BitConverter.ToInt32(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected double StreamReadDouble()
        {
            Span<byte> buffer = stackalloc byte[8];
            Stream.Fill(buffer);
            return BitConverter.ToDouble(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float StreamReadFloat()
        {
            Span<byte> buffer = stackalloc byte[4];
            Stream.Fill(buffer);
            return BitConverter.ToSingle(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected long StreamReadLong()
        {
            Span<byte> buffer = stackalloc byte[8];
            Stream.Fill(buffer);
            return BitConverter.ToInt64(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ulong StreamReadULong()
        {
            Span<byte> buffer = stackalloc byte[8];
            Stream.Fill(buffer);
            return BitConverter.ToUInt64(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected uint StreamReadUInt()
        {
            Span<byte> buffer = stackalloc byte[4];
            Stream.Fill(buffer);
            return BitConverter.ToUInt32(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected short StreamReadShort()
        {
            Span<byte> buffer = stackalloc byte[2];
            Stream.Fill(buffer);
            return BitConverter.ToInt16(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected char StreamReadChar()
        {
            Span<byte> buffer = stackalloc byte[2];
            Stream.Fill(buffer);
            return BitConverter.ToChar(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected decimal StreamReadDecimal()
        {
            Span<byte> buffer = stackalloc byte[16];
            Stream.Fill(buffer);
            var bits = new int[4];
            for (var i = 0; i < 4; i++)
                bits[i] = BitConverter.ToInt32(buffer.Slice(i * 4, 4));
            return new decimal(bits);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Guid StreamReadGuid()
        {
            Span<byte> buffer = stackalloc byte[16];
            Stream.Fill(buffer);
            return new Guid(buffer);
        }

#endif

        #endregion

        //

        #region GenericObject Deserializer
        internal static readonly ConcurrentDictionary<MultiArray<byte>, GenericDeserializerMetaDataOfType> GenericMultiArrayMetadata = new ConcurrentDictionary<MultiArray<byte>, GenericDeserializerMetaDataOfType>(MultiArrayBytesComparer.Instance);
        private readonly DeserializerCache<GenericDeserializerMetaDataOfType> _genericTypeCache = new DeserializerCache<GenericDeserializerMetaDataOfType>();
        private readonly DeserializerCache<GenericObject> _genericObjectCache = new DeserializerCache<GenericObject>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GenericObjectDeserialize(Stream stream)
        {
            object value;
            try
            {
                Stream = stream;
                var firstByte = stream.ReadByte();
                if (firstByte == -1)
                    throw new IOException("The stream has been closed.");
                if (firstByte != DataBytesDefinition.Start)
                    throw new FormatException("The stream is not in NSerializer format.");
                value = GenericReadValue(StreamReadByte());
                while (StreamReadByte() != DataBytesDefinition.End) { }
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
                _genericTypeCache.Clear();
                _serErrors.Clear();
                Stream = null;
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object GenericReadValue(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            if (type == DataBytesDefinition.RefObject)
            {
                var idx = StreamReadInt();
                return _genericObjectCache.Get(idx);
            }
            if (ReadValues.TryGetValue(type, out var mTuple))
            {
                _parameters[0] = type;
                return mTuple.Accessor(this, _parameters);
            }

            GenericDeserializerMetaDataOfType metadata = default;

            if (type == DataBytesDefinition.TypeStart)
            {
                var length = StreamReadInt();
                var typeBytes = ArrayPool<byte>.Shared.Rent(length);
                Stream.Read(typeBytes, 0, length);
                var subTypeBytes = new MultiArray<byte>(typeBytes, 0, length);
                if (!GenericMultiArrayMetadata.TryGetValue(subTypeBytes, out metadata))
                {
                    var typeData = Encoding.UTF8.GetString(typeBytes, 0, length);
                    var fsCol1 = typeData.IndexOf(";", StringComparison.Ordinal);
                    var fsCol2 = typeData.IndexOf(";", fsCol1 + 1, StringComparison.Ordinal);
                    var fsCol3 = typeData.IndexOf(";", fsCol2 + 1, StringComparison.Ordinal);
                    var fsCol4 = typeData.IndexOf(";", fsCol3 + 1, StringComparison.Ordinal);
                    var vTypeString = typeData.Substring(0, fsCol1);
                    var isArray = typeData[fsCol1 + 1] == '1';
                    var isList = typeData[fsCol2 + 1] == '1';
                    var isDictionary = typeData[fsCol3 + 1] == '1';
                    var propertiesString = typeData.Substring(fsCol4 + 1);
                    var properties = propertiesString.Split(";", StringSplitOptions.RemoveEmptyEntries);
                    metadata = new GenericDeserializerMetaDataOfType(vTypeString, isArray, isList, isDictionary, properties);
                    GenericMultiArrayMetadata.TryAdd(new MultiArray<byte>(subTypeBytes.ToArray()), metadata);
                }
                ArrayPool<byte>.Shared.Return(typeBytes);
                _genericTypeCache.Set(metadata);
            }
            else if (type == DataBytesDefinition.RefType)
            {
                metadata = _genericTypeCache.Get(StreamReadInt());
            }
            return GenericFillObject(metadata);

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private GenericObject GenericFillObject(GenericDeserializerMetaDataOfType metadata)
        {
            try
            {
                var value = new GenericObject(metadata);
                _genericObjectCache.Set(value);

                if (metadata.IsArray || metadata.IsList || metadata.IsDictionary)
                {
                    var capacity = StreamReadInt();
                    if (metadata.IsArray)
                    {
                        value.InitArray(capacity);
                        for (var i = 0; i < capacity; i++)
                        {
                            var item = GenericReadValue(StreamReadByte());
                            value.SetArrayValue(i, item);
                        }
                    }
                    else if (metadata.IsList)
                    {
                        for (var i = 0; i < capacity; i++)
                        {
                            var item = GenericReadValue(StreamReadByte());
                            value.AddListValue(item);
                        }
                    }
                    else if (metadata.IsDictionary)
                    {
                        for (var i = 0; i < capacity; i++)
                        {
                            var itemKey = GenericReadValue(StreamReadByte());
                            var itemValue = GenericReadValue(StreamReadByte());
                            value.SetDictionaryValue(itemKey, itemValue);
                        }
                    }

                }

                for (var i = 0; i < metadata.Properties.Length; i++)
                {
                    var name = metadata.Properties[i];
                    var propValue = GenericReadValue(StreamReadByte());
                    value.SetProperty(name, propValue);
                }

                StreamReadByte();
                return value;
            }
            catch (Exception ex)
            {
                var metaProperties = metadata.Properties?.Join(", ");
                throw new Exception($"Error trying to fill an object of type: {metadata.Type}; with the definition [{metaProperties}]", ex);
            }
        }
        #endregion
    }
}