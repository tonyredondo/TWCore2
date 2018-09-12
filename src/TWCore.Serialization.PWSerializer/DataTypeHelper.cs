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
using System.Runtime.CompilerServices;

namespace TWCore.Serialization.PWSerializer
{
    /// <summary>
    /// Numeric value type data helper
    /// </summary>
	internal static class DataTypeHelper
    {
        /// <summary>
        /// Get the maximum DataType that the value can be decreased.
        /// </summary>
        /// <param name="value">Value to be converted to a lower one</param>
        /// <param name="type">Value type to be decreased</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetDecreaseDataType(object value, Type type)
        {
            if (type == typeof(int))
                return GetDecreaseDataType((int)value);
            if (type == typeof(decimal))
                return DataType.Decimal;
            if (type == typeof(byte))
                return DataType.Byte;
            if (type == typeof(double))
                return DataType.Double;
            if (type == typeof(float))
                return DataType.Float;
            
            if (type == typeof(long))
                return GetDecreaseDataType((long)value);
            if (type == typeof(ulong))
                return GetDecreaseDataType((ulong)value);
            if (type == typeof(uint))
                return GetDecreaseDataType((uint)value);
            if (type == typeof(ushort))
                return GetDecreaseDataType((ushort)value);
            if (type == typeof(short))
                return GetDecreaseDataType((short)value);
            if (type == typeof(sbyte))
                return GetDecreaseDataType((sbyte)value);
            return DataType.Unknown;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte GetDecreaseDataType(ulong value)
        {
            if (value <= 255)
                return DataType.Byte;
            if (value <= 65535)
                return DataType.UShort;
            return value <= uint.MaxValue ? DataType.UInt : DataType.ULong;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte GetDecreaseDataType(long value)
        {
            if (value >= 0 && value <= 255)
                return DataType.Byte;
            if (value >= -128 && value <= 127)
                return DataType.SByte;
            if (value >= -32768 && value <= 32767)
                return DataType.Short;
            if (value >= 0 && value <= 65535)
                return DataType.UShort;
            if (value <= int.MaxValue && value >= int.MinValue)
                return DataType.Int;
            if (value <= uint.MaxValue && value >= 0)
                return DataType.UInt;
            return DataType.Long;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte GetDecreaseDataType(uint value)
        {
            if (value <= 255)
                return DataType.Byte;
            return value <= 65535 ? DataType.UShort : DataType.UInt;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetDecreaseDataType(int value)
        {
            if (value >= 0 && value <= 255)
                return DataType.Byte;
            if (value >= -128 && value <= 127)
                return DataType.SByte;
            if (value >= -32768 && value <= 32767)
                return DataType.Short;
            if (value >= 0 && value <= 65535)
                return DataType.UShort;
            return DataType.Int;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte GetDecreaseDataType(ushort value)
        {
            return value <= 255 ? DataType.Byte : DataType.UShort;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte GetDecreaseDataType(short value)
        {
            if (value >= 0 && value <= 255)
                return DataType.Byte;
            if (value >= -128 && value <= 127)
                return DataType.SByte;
            return DataType.Short;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte GetDecreaseDataType(sbyte value)
            => (value >= 0) ? DataType.Byte : DataType.SByte;

        /// <summary>
        /// Change a object type
        /// </summary>
        /// <param name="obj">Object value to change</param>
        /// <param name="typeTo">Type requested</param>
        /// <returns>Object value with new type</returns>
        public static object Change(object obj, Type typeTo)
        {
            if (typeTo == typeof(object))
                return obj;
            if (obj is null)
                return typeTo.IsValueType ? Activator.CreateInstance(typeTo) : null;
            var objType = obj.GetType();
            if (typeTo == objType)
                return obj;
            if (typeTo.IsEnum)
                return Enum.ToObject(typeTo, obj);
            if (typeTo.IsAssignableFrom(objType))
                return obj;

            var underlyingType = Nullable.GetUnderlyingType(typeTo);
            if (underlyingType != null)
                return underlyingType == typeof(object) ? obj : Convert.ChangeType(obj, underlyingType);
            try
            {
                return Convert.ChangeType(obj, typeTo);
            }
            catch
            {
                return obj;
            }
        }

    }
}
