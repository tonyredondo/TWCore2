﻿/*
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
#pragma warning disable 1591

namespace TWCore.Serialization.WSerializer.Types.ValueTypes
{
    /// <inheritdoc />
    /// <summary>
    /// TimeSpan value type serializer
    /// </summary>
	public class TimeSpanSerializer : TypeSerializer<TimeSpan>
    {
        public static readonly HashSet<byte> ReadTypes = new HashSet<byte>(new []
        {
            DataType.TimeSpan, DataType.TimeSpanDefault, DataType.RefTimeSpanByte, DataType.RefTimeSpanUShort
        });

        private SerializerMode _mode;
        private SerializerCache<TimeSpan> _cache;
        private SerializerCache<TimeSpan> Cache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _cache ?? (_cache = new SerializerCache<TimeSpan>(_mode)); }
        }

        /// <inheritdoc />
        /// <summary>
        /// Type serializer initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Init(SerializerMode mode)
        {
            _mode = mode;
            if (_cache != null)
                _cache = new SerializerCache<TimeSpan>(mode);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets if the type serializer can write the type
        /// </summary>
        /// <param name="type">Type of the value to write</param>
        /// <returns>true if the type serializer can write the type; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CanWrite(Type type)
            => type == typeof(TimeSpan);
        /// <inheritdoc />
        /// <summary>
        /// Gets if the type serializer can read the data type
        /// </summary>
        /// <param name="type">DataType value</param>
        /// <returns>true if the type serializer can read the type; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CanRead(byte type)
        {
            return
                type == DataType.TimeSpan ||
                type == DataType.TimeSpanDefault ||
                type == DataType.RefTimeSpanByte ||
                type == DataType.RefTimeSpanUShort;
        }
        /// <inheritdoc />
        /// <summary>
        /// Writes the serialized value to the binary stream.
        /// </summary>
        /// <param name="writer">Binary writer of the stream</param>
        /// <param name="value">Object value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(BinaryWriter writer, object value)
            => WriteValue(writer, (TimeSpan)value);
        /// <inheritdoc />
        /// <summary>
        /// Writes the serialized value to the binary stream.
        /// </summary>
        /// <param name="writer">Binary writer of the stream</param>
        /// <param name="value">Object value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteValue(BinaryWriter writer, TimeSpan value)
        {
            if (value == default(TimeSpan))
            {
                writer.Write(DataType.TimeSpanDefault);
                return;
            }

            var objIdx = Cache.SerializerGet(value);
            if (objIdx > -1)
            {
                if (objIdx <= byte.MaxValue)
                    WriteByte(writer, DataType.RefTimeSpanByte, (byte) objIdx);
                else
                    WriteUshort(writer, DataType.RefTimeSpanUShort, (ushort)objIdx);
            }
            else
            {
                var longBinary = value.Ticks;
                WriteLong(writer, DataType.TimeSpan, longBinary);
                Cache.SerializerSet(value);
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <param name="type">DataType</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Read(BinaryReader reader, byte type)
            => ReadValue(reader, type);
        /// <inheritdoc />
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <param name="type">DataType</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TimeSpan ReadValue(BinaryReader reader, byte type)
        {
            if (type == DataType.TimeSpanDefault)
                return default(TimeSpan);

            var objIdx = -1;
            switch (type)
            {
                case DataType.RefTimeSpanByte:
                    objIdx = reader.ReadByte();
                    break;
                case DataType.RefTimeSpanUShort:
                    objIdx = reader.ReadUInt16();
                    break;
            }

            if (objIdx > -1)
                return Cache.DeserializerGet(objIdx);

            var longBinary = reader.ReadInt64();
            var cValue = TimeSpan.FromTicks(longBinary);
            Cache.DeserializerSet(cValue);
            return cValue;
        }
        /// <inheritdoc />
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TimeSpan ReadValue(BinaryReader reader)
            => ReadValue(reader, reader.ReadByte());
    }
}
