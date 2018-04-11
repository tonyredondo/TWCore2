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
    public class SerializersTable : TypeSerializer
    {
        private Stream _stream;
        public readonly BooleanSerializer Boolean = new BooleanSerializer();
        public readonly ByteArraySerializer ByteArray = new ByteArraySerializer();
        public readonly CharSerializer Char = new CharSerializer();
        public readonly DateTimeOffsetSerializer DateTimeOffset = new DateTimeOffsetSerializer();
        public readonly DateTimeSerializer DateTime = new DateTimeSerializer();
        public readonly EnumSerializer Enum = new EnumSerializer();
        public readonly GuidSerializer Guid = new GuidSerializer();
        public readonly NumberSerializer Number = new NumberSerializer();
        public readonly SerializedObjectSerializer SerializedObject = new SerializedObjectSerializer();
        public readonly StringSerializer String = new StringSerializer();
        public readonly StringSerializer Property = new StringSerializer();
        public readonly TimeSpanSerializer TimeSpan = new TimeSpanSerializer();

        #region Internal Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Init()
        {
            Boolean.Init();
            ByteArray.Init();
            Char.Init();
            DateTimeOffset.Init();
            DateTime.Init();
            Enum.Init();
            Guid.Init();
            Number.Init();
            SerializedObject.Init();
            String.Init();
            Property.Init();
            TimeSpan.Init();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Clear()
        {
            Boolean.Clear();
            ByteArray.Clear();
            Char.Clear();
            DateTimeOffset.Clear();
            DateTime.Clear();
            Enum.Clear();
            Guid.Clear();
            Number.Clear();
            SerializedObject.Clear();
            String.Clear();
            Property.Clear();
            TimeSpan.Clear();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStream(Stream stream)
        {
            _stream = stream;
        }
        #endregion

        #region Write Properties
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, bool value)
        {
            Property.Write(_stream, name);
            Boolean.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, bool? value)
        {
            Property.Write(_stream, name);
            Boolean.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, byte[] value)
        {
            Property.Write(_stream, name);
            ByteArray.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, char value)
        {
            Property.Write(_stream, name);
            Char.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, char? value)
        {
            Property.Write(_stream, name);
            Char.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, DateTimeOffset value)
        {
            Property.Write(_stream, name);
            DateTimeOffset.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, DateTimeOffset? value)
        {
            Property.Write(_stream, name);
            DateTimeOffset.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, DateTime value)
        {
            Property.Write(_stream, name);
            DateTime.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, DateTime? value)
        {
            Property.Write(_stream, name);
            DateTime.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, Enum value)
        {
            Property.Write(_stream, name);
            Enum.Write(_stream, Convert.ToInt32(value));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, Guid value)
        {
            Property.Write(_stream, name);
            Guid.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, Guid? value)
        {
            Property.Write(_stream, name);
            Guid.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, SerializedObject value)
        {
            Property.Write(_stream, name);
            SerializedObject.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, string value)
        {
            if (value == null) return;
            Property.Write(_stream, name);
            String.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, TimeSpan value)
        {
            Property.Write(_stream, name);
            TimeSpan.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, TimeSpan? value)
        {
            Property.Write(_stream, name);
            TimeSpan.Write(_stream, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, decimal value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, decimal? value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, double value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, double? value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, float value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, float? value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, long value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, long? value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, ulong value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, ulong? value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, int value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, int? value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, uint value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, uint? value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, short value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, short? value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, ushort value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, ushort? value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, byte value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, byte? value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, sbyte value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, sbyte? value)
        {
            Property.Write(_stream, name);
            Number.Write(_stream, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty<T>(string name, T value)
        {
            if (value == null) return;
            Property.Write(_stream, name);
            WriteValue(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, object value)
        {
            if (value == null) return;
            Property.Write(_stream, name);
            WriteValue(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, bool[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.BoolArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Boolean.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, char[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.CharArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Char.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, DateTimeOffset[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.DateTimeOffsetArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                DateTimeOffset.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, DateTime[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.DateTimeArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                DateTimeOffset.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, Enum[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.EnumArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Enum.Write(_stream, Convert.ToInt32(value[i]));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, Guid[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.GuidArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Guid.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, decimal[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.DecimalArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, double[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.DoubleArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, float[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.FloatArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, long[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.LongArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, ulong[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.ULongArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, int[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.IntArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, uint[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.UIntArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, short[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.ShortArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, ushort[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.UShortArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, sbyte[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.SByteArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, string[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.StringArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                String.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, TimeSpan[] value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.TimeSpanArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                TimeSpan.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<bool> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.BoolList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Boolean.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<char> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.CharList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Char.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<DateTimeOffset> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.DateTimeOffsetList, value.Count);
            for (var i = 0; i < value.Count; i++)
                DateTimeOffset.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<DateTime> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.DateTimeList, value.Count);
            for (var i = 0; i < value.Count; i++)
                DateTimeOffset.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<Enum> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.EnumList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Enum.Write(_stream, Convert.ToInt32(value[i]));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<Guid> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.GuidList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Guid.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<decimal> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.DecimalList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<double> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.DoubleList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<float> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.FloatList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<long> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.LongList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<ulong> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.ULongList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<int> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.IntList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<uint> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.UIntList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<short> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.ShortList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<ushort> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.UShortList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<sbyte> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.SByteList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<string> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.StringList, value.Count);
            for (var i = 0; i < value.Count; i++)
                String.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, List<TimeSpan> value)
        {
            Property.Write(_stream, name);
            WriteInt(_stream, DataBytesDefinition.TimeSpanList, value.Count);
            for (var i = 0; i < value.Count; i++)
                TimeSpan.Write(_stream, value[i]);
        }

        #endregion

        #region Write Values
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(bool value) => Boolean.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(bool? value) => Boolean.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(byte[] value) => ByteArray.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(char value) => Char.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(char? value) => Char.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTimeOffset value) => DateTimeOffset.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTimeOffset? value) => DateTimeOffset.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTime value) => DateTime.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTime? value) => DateTime.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Enum value) => Enum.Write(_stream, Convert.ToInt32(value));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Guid value) => Guid.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Guid? value) => Guid.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(SerializedObject value) => SerializedObject.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(string value) => String.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(TimeSpan value) => TimeSpan.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(TimeSpan? value) => TimeSpan.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(decimal value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(decimal? value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(double value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(double? value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(float value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(float? value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(long value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(long? value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ulong value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ulong? value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(int value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(int? value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(uint value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(uint? value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(short value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(short? value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ushort value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ushort? value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(byte value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(byte? value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(sbyte value) => Number.Write(_stream, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(sbyte? value) => Number.Write(_stream, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(bool[] value)
        {
            WriteInt(_stream, DataBytesDefinition.BoolArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Boolean.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(char[] value)
        {
            WriteInt(_stream, DataBytesDefinition.CharArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Char.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTimeOffset[] value)
        {
            WriteInt(_stream, DataBytesDefinition.DateTimeOffsetArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                DateTimeOffset.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTime[] value)
        {
            WriteInt(_stream, DataBytesDefinition.DateTimeArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                DateTimeOffset.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Enum[] value)
        {
            WriteInt(_stream, DataBytesDefinition.EnumArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Enum.Write(_stream, Convert.ToInt32(value[i]));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Guid[] value)
        {
            WriteInt(_stream, DataBytesDefinition.GuidArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Guid.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(decimal[] value)
        {
            WriteInt(_stream, DataBytesDefinition.DecimalArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(double[] value)
        {
            WriteInt(_stream, DataBytesDefinition.DoubleArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(float[] value)
        {
            WriteInt(_stream, DataBytesDefinition.FloatArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(long[] value)
        {
            WriteInt(_stream, DataBytesDefinition.LongArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ulong[] value)
        {
            WriteInt(_stream, DataBytesDefinition.ULongArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(int[] value)
        {
            WriteInt(_stream, DataBytesDefinition.IntArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(uint[] value)
        {
            WriteInt(_stream, DataBytesDefinition.UIntArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(short[] value)
        {
            WriteInt(_stream, DataBytesDefinition.ShortArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ushort[] value)
        {
            WriteInt(_stream, DataBytesDefinition.UShortArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(sbyte[] value)
        {
            WriteInt(_stream, DataBytesDefinition.SByteArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(string[] value)
        {
            WriteInt(_stream, DataBytesDefinition.StringArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                String.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(TimeSpan[] value)
        {
            WriteInt(_stream, DataBytesDefinition.TimeSpanArray, value.Length);
            for (var i = 0; i < value.Length; i++)
                TimeSpan.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<bool> value)
        {
            WriteInt(_stream, DataBytesDefinition.BoolList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Boolean.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<char> value)
        {
            WriteInt(_stream, DataBytesDefinition.CharList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Char.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<DateTimeOffset> value)
        {
            WriteInt(_stream, DataBytesDefinition.DateTimeOffsetList, value.Count);
            for (var i = 0; i < value.Count; i++)
                DateTimeOffset.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<DateTime> value)
        {
            WriteInt(_stream, DataBytesDefinition.DateTimeList, value.Count);
            for (var i = 0; i < value.Count; i++)
                DateTimeOffset.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<Enum> value)
        {
            WriteInt(_stream, DataBytesDefinition.EnumList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Enum.Write(_stream, Convert.ToInt32(value[i]));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<Guid> value)
        {
            WriteInt(_stream, DataBytesDefinition.GuidList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Guid.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<decimal> value)
        {
            WriteInt(_stream, DataBytesDefinition.DecimalList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<double> value)
        {
            WriteInt(_stream, DataBytesDefinition.DoubleList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<float> value)
        {
            WriteInt(_stream, DataBytesDefinition.FloatList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<long> value)
        {
            WriteInt(_stream, DataBytesDefinition.LongList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<ulong> value)
        {
            WriteInt(_stream, DataBytesDefinition.ULongList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<int> value)
        {
            WriteInt(_stream, DataBytesDefinition.IntList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<uint> value)
        {
            WriteInt(_stream, DataBytesDefinition.UIntList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<short> value)
        {
            WriteInt(_stream, DataBytesDefinition.ShortList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<ushort> value)
        {
            WriteInt(_stream, DataBytesDefinition.UShortList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<sbyte> value)
        {
            WriteInt(_stream, DataBytesDefinition.SByteList, value.Count);
            for (var i = 0; i < value.Count; i++)
                Number.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<string> value)
        {
            WriteInt(_stream, DataBytesDefinition.StringList, value.Count);
            for (var i = 0; i < value.Count; i++)
                String.Write(_stream, value[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(List<TimeSpan> value)
        {
            WriteInt(_stream, DataBytesDefinition.TimeSpanList, value.Count);
            for (var i = 0; i < value.Count; i++)
                TimeSpan.Write(_stream, value[i]);
        }
        #endregion

        //
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue<T>(List<T> valueList)
        {
            var vType = typeof(T);
            _stream.WriteByte(DataBytesDefinition.TypeStart);
            Property.Write(_stream, vType.GetTypeName());
            var count = valueList.Count;
            WriteInt(_stream, DataBytesDefinition.ListStart, count);
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
            var vType = typeof(T);
            var descriptor = Descriptors.GetOrAdd(vType, type => new TypeDescriptor(type));
            WriteInt(_stream, DataBytesDefinition.TypeStart, descriptor.Definition.Length);
            _stream.Write(descriptor.Definition, 0, descriptor.Definition.Length);
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
                    Boolean.Write(_stream, cValue);
                    return;
                case byte[] cValue:
                    ByteArray.Write(_stream, cValue);
                    return;
                case char cValue:
                    Char.Write(_stream, cValue);
                    return;
                case DateTimeOffset cValue:
                    DateTimeOffset.Write(_stream, cValue);
                    return;
                case DateTime cValue:
                    DateTime.Write(_stream, cValue);
                    return;
                case Enum cValue:
                    Enum.Write(_stream, Convert.ToInt32(cValue));
                    return;
                case Guid cValue:
                    Guid.Write(_stream, cValue);
                    return;
                case decimal cValue:
                    Number.Write(_stream, cValue);
                    return;
                case double cValue:
                    Number.Write(_stream, cValue);
                    return;
                case float cValue:
                    Number.Write(_stream, cValue);
                    return;
                case long cValue:
                    Number.Write(_stream, cValue);
                    return;
                case ulong cValue:
                    Number.Write(_stream, cValue);
                    return;
                case int cValue:
                    Number.Write(_stream, cValue);
                    return;
                case uint cValue:
                    Number.Write(_stream, cValue);
                    return;
                case short cValue:
                    Number.Write(_stream, cValue);
                    return;
                case ushort cValue:
                    Number.Write(_stream, cValue);
                    return;
                case byte cValue:
                    Number.Write(_stream, cValue);
                    return;
                case sbyte cValue:
                    Number.Write(_stream, cValue);
                    return;
                case SerializedObject cValue:
                    SerializedObject.Write(_stream, cValue);
                    return;
                case string cValue:
                    String.Write(_stream, cValue);
                    return;
                case TimeSpan cValue:
                    TimeSpan.Write(_stream, cValue);
                    return;
                case bool[] cValue:
                    WriteInt(_stream, DataBytesDefinition.BoolArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Boolean.Write(_stream, cValue[i]);
                    return;
                case char[] cValue:
                    WriteInt(_stream, DataBytesDefinition.CharArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Char.Write(_stream, cValue[i]);
                    return;
                case DateTimeOffset[] cValue:
                    WriteInt(_stream, DataBytesDefinition.DateTimeOffsetArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        DateTimeOffset.Write(_stream, cValue[i]);
                    return;
                case DateTime[] cValue:
                    WriteInt(_stream, DataBytesDefinition.DateTimeArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        DateTime.Write(_stream, cValue[i]);
                    return;
                case Enum[] cValue:
                    WriteInt(_stream, DataBytesDefinition.EnumArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Enum.Write(_stream, Convert.ToInt32(cValue[i]));
                    return;
                case Guid[] cValue:
                    WriteInt(_stream, DataBytesDefinition.GuidArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Guid.Write(_stream, cValue[i]);
                    return;
                case decimal[] cValue:
                    WriteInt(_stream, DataBytesDefinition.DecimalArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case double[] cValue:
                    WriteInt(_stream, DataBytesDefinition.DoubleArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case float[] cValue:
                    WriteInt(_stream, DataBytesDefinition.FloatArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case long[] cValue:
                    WriteInt(_stream, DataBytesDefinition.LongArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case ulong[] cValue:
                    WriteInt(_stream, DataBytesDefinition.ULongArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case int[] cValue:
                    WriteInt(_stream, DataBytesDefinition.IntArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case uint[] cValue:
                    WriteInt(_stream, DataBytesDefinition.UIntArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case short[] cValue:
                    WriteInt(_stream, DataBytesDefinition.ShortArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case ushort[] cValue:
                    WriteInt(_stream, DataBytesDefinition.UShortArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case sbyte[] cValue:
                    WriteInt(_stream, DataBytesDefinition.SByteArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case string[] cValue:
                    WriteInt(_stream, DataBytesDefinition.StringArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        String.Write(_stream, cValue[i]);
                    return;
                case TimeSpan[] cValue:
                    WriteInt(_stream, DataBytesDefinition.TimeSpanArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        TimeSpan.Write(_stream, cValue[i]);
                    return;

                case List<bool> cValue:
                    WriteInt(_stream, DataBytesDefinition.BoolList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Boolean.Write(_stream, cValue[i]);
                    return;
                case List<char> cValue:
                    WriteInt(_stream, DataBytesDefinition.CharList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Char.Write(_stream, cValue[i]);
                    return;
                case List<DateTimeOffset> cValue:
                    WriteInt(_stream, DataBytesDefinition.DateTimeOffsetList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        DateTimeOffset.Write(_stream, cValue[i]);
                    return;
                case List<DateTime> cValue:
                    WriteInt(_stream, DataBytesDefinition.DateTimeList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        DateTime.Write(_stream, cValue[i]);
                    return;
                case List<Enum> cValue:
                    WriteInt(_stream, DataBytesDefinition.EnumList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Enum.Write(_stream, Convert.ToInt32(cValue[i]));
                    return;
                case List<Guid> cValue:
                    WriteInt(_stream, DataBytesDefinition.GuidList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Guid.Write(_stream, cValue[i]);
                    return;
                case List<decimal> cValue:
                    WriteInt(_stream, DataBytesDefinition.DecimalList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case List<double> cValue:
                    WriteInt(_stream, DataBytesDefinition.DoubleList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case List<float> cValue:
                    WriteInt(_stream, DataBytesDefinition.FloatList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case List<long> cValue:
                    WriteInt(_stream, DataBytesDefinition.LongList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case List<ulong> cValue:
                    WriteInt(_stream, DataBytesDefinition.ULongList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case List<int> cValue:
                    WriteInt(_stream, DataBytesDefinition.IntList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case List<uint> cValue:
                    WriteInt(_stream, DataBytesDefinition.UIntList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case List<short> cValue:
                    WriteInt(_stream, DataBytesDefinition.ShortList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case List<ushort> cValue:
                    WriteInt(_stream, DataBytesDefinition.UShortList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case List<sbyte> cValue:
                    WriteInt(_stream, DataBytesDefinition.SByteList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_stream, cValue[i]);
                    return;
                case List<string> cValue:
                    WriteInt(_stream, DataBytesDefinition.StringList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        String.Write(_stream, cValue[i]);
                    return;
                case List<TimeSpan> cValue:
                    WriteInt(_stream, DataBytesDefinition.TimeSpanList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        TimeSpan.Write(_stream, cValue[i]);
                    return;
            }
            #endregion

            var vType = value.GetType();
            var descriptor = Descriptors.GetOrAdd(vType, type => new TypeDescriptor(type));
            WriteInt(_stream, DataBytesDefinition.TypeStart, descriptor.Definition.Length);
            _stream.Write(descriptor.Definition, 0, descriptor.Definition.Length);
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
                WriteInt(_stream, DataBytesDefinition.PropertiesStart, descriptor.Properties.Count);
                foreach (var prop in descriptor.Properties)
                {
                    var pValue = prop.Value.GetValue(value);
                    if (pValue == null) continue;
                    Property.Write(_stream, prop.Key);
                    WriteInnerValue(pValue);
                }
            }

            //Write Array if contains
            if (descriptor.IsArray)
            {
                var aValue = (Array)value;
                WriteInt(_stream, DataBytesDefinition.ArrayStart, aValue.Length);
                for (var i = 0; i < aValue.Length; i++)
                    WriteInnerValue(aValue.GetValue(i));
                return;
            }

            //Write List if contains
            if (descriptor.IsList)
            {
                var iValue = (IList)value;
                var count = iValue.Count;
                WriteInt(_stream, DataBytesDefinition.ListStart, count);
                for (var i = 0; i < count; i++)
                    WriteInnerValue(iValue[i]);
                return;
            }

            //Write Dictionary if contains
            if (descriptor.IsDictionary)
            {
                var iValue = (IDictionary)value;
                var count = iValue.Count;
                WriteInt(_stream, DataBytesDefinition.DictionaryStart, count);
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
