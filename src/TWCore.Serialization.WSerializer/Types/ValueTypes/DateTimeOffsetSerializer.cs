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

namespace TWCore.Serialization.WSerializer.Types.ValueTypes
{
    /// <inheritdoc />
    /// <summary>
    /// DateTimeOffset value type serializer
    /// </summary>
	public class DateTimeOffsetSerializer : TypeSerializer<DateTimeOffset>
    {
        public static readonly HashSet<byte> ReadTypes = new HashSet<byte>(new [] 
        {
            DataType.DateTimeOffset, DataType.DateTimeOffsetDefault, DataType.RefDateTimeOffsetByte, DataType.RefDateTimeOffsetUShort
        });

        private SerializerMode _mode;
        private SerializerCache<DateTimeOffset> _cache;
        private SerializerCache<DateTimeOffset> Cache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _cache ?? (_cache = new SerializerCache<DateTimeOffset>(_mode)); }
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
                _cache = new SerializerCache<DateTimeOffset>(mode);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets if the type serializer can write the type
        /// </summary>
        /// <param name="type">Type of the value to write</param>
        /// <returns>true if the type serializer can write the type; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CanWrite(Type type)
            => type == typeof(DateTimeOffset);
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
                type == DataType.DateTimeOffset ||
                type == DataType.DateTimeOffsetDefault ||
                type == DataType.RefDateTimeOffsetByte ||
                type == DataType.RefDateTimeOffsetUShort;
        }
        /// <inheritdoc />
        /// <summary>
        /// Writes the serialized value to the binary stream.
        /// </summary>
        /// <param name="writer">Binary writer of the stream</param>
        /// <param name="value">Object value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(BinaryWriter writer, object value)
            => WriteValue(writer, (DateTimeOffset)value);
        /// <inheritdoc />
        /// <summary>
        /// Writes the serialized value to the binary stream.
        /// </summary>
        /// <param name="writer">Binary writer of the stream</param>
        /// <param name="value">Object value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteValue(BinaryWriter writer, DateTimeOffset value)
        {
            if (value == default(DateTimeOffset))
            {
                writer.Write(DataType.DateTimeOffsetDefault);
                return;
            }
            var objIdx = Cache.SerializerGet(value);
            if (objIdx > -1)
            {
                #region Write reference
                if (objIdx <= byte.MaxValue)
                    WriteByte(writer, DataType.RefDateTimeOffsetByte, (byte)objIdx);
                else
                    WriteUshort(writer, DataType.RefDateTimeOffsetUShort, (ushort)objIdx);
                #endregion
            }
            else
            {
                var longBinary = value.ToFileTime();
                WriteLong(writer, DataType.DateTimeOffset, longBinary);
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
        public override DateTimeOffset ReadValue(BinaryReader reader, byte type)
        {
            if (type == DataType.DateTimeOffsetDefault)
                return default(DateTimeOffset);

            var objIdx = -1;
            switch (type)
            {
                case DataType.RefDateTimeOffsetByte:
                    objIdx = reader.ReadByte();
                    break;
                case DataType.RefDateTimeOffsetUShort:
                    objIdx = reader.ReadUInt16();
                    break;
            }

            if (objIdx > -1)
                return Cache.DeserializerGet(objIdx);

            var longBinary = reader.ReadInt64();
            var cValue = DateTimeOffset.FromFileTime(longBinary);
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
        public override DateTimeOffset ReadValue(BinaryReader reader)
            => ReadValue(reader, reader.ReadByte());
    }
}
