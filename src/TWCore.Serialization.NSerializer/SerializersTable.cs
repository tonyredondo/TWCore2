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
    public class SerializersTable
    {
        private BinaryWriter _writer;
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
        public readonly TimeSpanSerializer TimeSpan = new TimeSpanSerializer();

        #region Internal Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
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
            TimeSpan.Init();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
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
            TimeSpan.Clear();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWriter(BinaryWriter writer)
        {
            _writer = writer;
        }
        #endregion

        #region Write Properties
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, bool value)
        {
            String.Write(_writer, name);
            Boolean.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, bool? value)
        {
            String.Write(_writer, name);
            Boolean.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, byte[] value)
        {
            String.Write(_writer, name);
            ByteArray.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, char value)
        {
            String.Write(_writer, name);
            Char.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, char? value)
        {
            String.Write(_writer, name);
            Char.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, DateTimeOffset value)
        {
            String.Write(_writer, name);
            DateTimeOffset.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, DateTimeOffset? value)
        {
            String.Write(_writer, name);
            DateTimeOffset.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, DateTime value)
        {
            String.Write(_writer, name);
            DateTime.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, DateTime? value)
        {
            String.Write(_writer, name);
            DateTime.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, Enum value)
        {
            String.Write(_writer, name);
            Enum.Write(_writer, Convert.ToInt32(value));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, Guid value)
        {
            String.Write(_writer, name);
            Guid.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, Guid? value)
        {
            String.Write(_writer, name);
            Guid.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, SerializedObject value)
        {
            String.Write(_writer, name);
            SerializedObject.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, string value)
        {
            String.Write(_writer, name);
            String.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, TimeSpan value)
        {
            String.Write(_writer, name);
            TimeSpan.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, TimeSpan? value)
        {
            String.Write(_writer, name);
            TimeSpan.Write(_writer, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, decimal value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, decimal? value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, double value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, double? value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, float value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, float? value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, long value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, long? value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, ulong value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, ulong? value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, int value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, int? value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, uint value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, uint? value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, short value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, short? value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, ushort value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, ushort? value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, byte value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, byte? value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, sbyte value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, sbyte? value)
        {
            String.Write(_writer, name);
            Number.Write(_writer, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteProperty(string name, object value)
        {
            String.Write(_writer, name);
            WriteValue(value);
        }
        #endregion

        #region Write Values
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(bool value) => Boolean.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(bool? value) => Boolean.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(byte[] value) => ByteArray.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(char value) => Char.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(char? value) => Char.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTimeOffset value) => DateTimeOffset.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTimeOffset? value) => DateTimeOffset.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTime value) => DateTime.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTime? value) => DateTime.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Enum value) => Enum.Write(_writer, Convert.ToInt32(value));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Guid value) => Guid.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Guid? value) => Guid.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(SerializedObject value) => SerializedObject.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(string value) => String.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(TimeSpan value) => TimeSpan.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(TimeSpan? value) => TimeSpan.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(decimal value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(decimal? value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(double value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(double? value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(float value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(float? value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(long value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(long? value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ulong value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ulong? value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(int value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(int? value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(uint value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(uint? value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(short value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(short? value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ushort value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ushort? value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(byte value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(byte? value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(sbyte value) => Number.Write(_writer, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(sbyte? value) => Number.Write(_writer, value);
        #endregion
        
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(object value)
        {
            #region Values
            switch(value)
            {
                case null:
                    _writer.Write(DataBytesDefinition.ValueNull);
                    return;
                case bool cValue:
                    Boolean.Write(_writer, cValue);
                    return;
                case byte[] cValue:
                    ByteArray.Write(_writer, cValue);
                    return;
                case char cValue:
                    Char.Write(_writer, cValue);
                    return;
                case DateTimeOffset cValue:
                    DateTimeOffset.Write(_writer, cValue);
                    return;
                case DateTime cValue:
                    DateTime.Write(_writer, cValue);
                    return;
                case Enum cValue:
                    Enum.Write(_writer, Convert.ToInt32(cValue));
                    return;
                case Guid cValue:
                    Guid.Write(_writer, cValue);
                    return;
                case decimal cValue:
                    Number.Write(_writer, cValue);
                    return;
                case double cValue:
                    Number.Write(_writer, cValue);
                    return;
                case float cValue:
                    Number.Write(_writer, cValue);
                    return;
                case long cValue:
                    Number.Write(_writer, cValue);
                    return;
                case ulong cValue:
                    Number.Write(_writer, cValue);
                    return;
                case int cValue:
                    Number.Write(_writer, cValue);
                    return;
                case uint cValue:
                    Number.Write(_writer, cValue);
                    return;
                case short cValue:
                    Number.Write(_writer, cValue);
                    return;
                case ushort cValue:
                    Number.Write(_writer, cValue);
                    return;
                case byte cValue:
                    Number.Write(_writer, cValue);
                    return;
                case sbyte cValue:
                    Number.Write(_writer, cValue);
                    return;
                case SerializedObject cValue:
                    SerializedObject.Write(_writer, cValue);
                    return;
                case string cValue:
                    String.Write(_writer, cValue);
                    return;
                case TimeSpan cValue:
                    TimeSpan.Write(_writer, cValue);
                    return;


                case bool[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.BoolArray, cValue.Length);
                    for(var i = 0; i < cValue.Length; i++)
                        Boolean.Write(_writer, cValue[i]);
                    return;
                case char[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.CharArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Char.Write(_writer, cValue[i]);
                    return;
                case DateTimeOffset[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.DateTimeOffsetArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        DateTimeOffset.Write(_writer, cValue[i]);
                    return;
                case DateTime[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.DateTimeArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        DateTime.Write(_writer, cValue[i]);
                    return;
                case Enum[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.EnumArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Enum.Write(_writer, Convert.ToInt32(cValue[i]));
                    return;
                case Guid[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.GuidArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Guid.Write(_writer, cValue[i]);
                    return;
                case decimal[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.DecimalArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case double[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.DoubleArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case float[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.FloatArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case long[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.LongArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case ulong[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.ULongArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case int[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.IntArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case uint[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.UIntArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case short[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.ShortArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case ushort[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.UShortArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case sbyte[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.SByteArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case string[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.StringArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        String.Write(_writer, cValue[i]);
                    return;
                case TimeSpan[] cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.TimeSpanArray, cValue.Length);
                    for (var i = 0; i < cValue.Length; i++)
                        TimeSpan.Write(_writer, cValue[i]);
                    return;

                case List<bool> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.BoolList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Boolean.Write(_writer, cValue[i]);
                    return;
                case List<char> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.CharList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Char.Write(_writer, cValue[i]);
                    return;
                case List<DateTimeOffset> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.DateTimeOffsetList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        DateTimeOffset.Write(_writer, cValue[i]);
                    return;
                case List<DateTime> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.DateTimeList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        DateTime.Write(_writer, cValue[i]);
                    return;
                case List<Enum> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.EnumList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Enum.Write(_writer, Convert.ToInt32(cValue[i]));
                    return;
                case List<Guid> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.GuidList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Guid.Write(_writer, cValue[i]);
                    return;
                case List<decimal> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.DecimalList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case List<double> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.DoubleList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case List<float> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.FloatList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case List<long> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.LongList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case List<ulong> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.ULongList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case List<int> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.IntList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case List<uint> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.UIntList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case List<short> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.ShortList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case List<ushort> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.UShortList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case List<sbyte> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.SByteList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        Number.Write(_writer, cValue[i]);
                    return;
                case List<string> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.StringList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        String.Write(_writer, cValue[i]);
                    return;
                case List<TimeSpan> cValue:
                    WriteHelper.WriteInt(_writer, DataBytesDefinition.TimeSpanList, cValue.Count);
                    for (var i = 0; i < cValue.Count; i++)
                        TimeSpan.Write(_writer, cValue[i]);
                    return;
            }
            #endregion

            var vType = value.GetType();
            _writer.Write(DataBytesDefinition.TypeStart);
            String.Write(_writer, vType.GetTypeName());
            if (value is INSerializable instance)
                instance.Serialize(this);
            else
                InternalObjectWrite(value);
            _writer.Write(DataBytesDefinition.TypeEnd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InternalObjectWrite(object value)
        {
            var descriptor = Descriptors.GetOrAdd(value.GetType(), type => new TypeDescriptor(type));

            //Write Properties
            if (descriptor.Properties.Count > 0)
            {
                WriteHelper.WriteInt(_writer, DataBytesDefinition.PropertiesStart, descriptor.Properties.Count);
                foreach(var prop in descriptor.Properties)
                    WriteProperty(prop.Key, prop.Value.GetValue(value));
            }

            //Write Array if contains
            if (descriptor.IsArray)
            {
                var aValue = (Array)value;
                WriteHelper.WriteInt(_writer, DataBytesDefinition.ArrayStart, aValue.Length);
                for (var i = 0; i < aValue.Length; i++)
                    WriteValue(aValue.GetValue(i));
            }

            //Write List if contains
            if (descriptor.IsList)
            {
                var iValue = (IList)value;
                var count = iValue.Count;
                WriteHelper.WriteInt(_writer, DataBytesDefinition.ListStart, count);
                for (var i = 0; i < count; i++)
                    WriteValue(iValue[i]);
            }

            //Write Dictionary if contains
            if (descriptor.IsDictionary)
            {
                var iValue = (IDictionary)value;
                var count = iValue.Count;
                WriteHelper.WriteInt(_writer, DataBytesDefinition.DictionaryStart, count);
                foreach(DictionaryEntry item in iValue)
                {
                    WriteValue(item.Key);
                    WriteValue(item.Value);
                }
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

            public TypeDescriptor(Type type)
            {
                TypeName = type.Name;
                Activator = Factory.Accessors.CreateActivator(type);
                Properties = new Dictionary<string, FastPropertyInfo>();
                var runtimeProperties = type.GetRuntimeProperties();
                foreach(var prop in runtimeProperties)
                {
                    if (prop.IsSpecialName || !prop.CanRead || !prop.CanWrite) continue;
                    if (prop.GetIndexParameters().Length > 0) continue;
                    var fProp = prop.GetFastPropertyInfo();
                    Properties[fProp.Name] = fProp;
                }
                IsArray = type.IsArray;
                if (IsArray)
                {
                    var ifaces = type.GetInterfaces();
                    IsDictionary = ifaces.Any(i => i == typeof(IDictionary) || (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)));
                    if (IsDictionary)
                        IsList = false;
                    else
                        IsList = ifaces.Any(i => i == typeof(IList) || (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)));
                }
                else
                {
                    IsList = false;
                    IsDictionary = false;
                }
            }
        }
    }
}
