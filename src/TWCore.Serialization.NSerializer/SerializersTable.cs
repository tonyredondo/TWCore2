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
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
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
        internal void Init()
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
        internal void Clear()
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
        internal void SetWriter(BinaryWriter writer)
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
            }
            #endregion

            var vType = value.GetType();
            _writer.Write(DataBytesDefinition.TypeName);
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

        }
    }
}
