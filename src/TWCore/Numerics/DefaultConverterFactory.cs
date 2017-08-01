/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

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

namespace TWCore.Numerics
{
    /// <summary>
    /// Converter factory
    /// </summary>
    public class DefaultConverterFactory : IConverterFactory
    {
        /// <summary>
        /// Change value to byte
        /// </summary>
        public Func<object, byte> ToByte { get; } = new Func<object, byte>(val => ((IConvertible)val).ToByte(null));
        /// <summary>
        /// Change value to sbyte
        /// </summary>
        public Func<object, sbyte> ToSByte { get; } = new Func<object, sbyte>(val => ((IConvertible)val).ToSByte(null));
        /// <summary>
        /// Change value to ushort
        /// </summary>
        public Func<object, ushort> ToUShort { get; } = new Func<object, ushort>(val => ((IConvertible)val).ToUInt16(null));
        /// <summary>
        /// Change value to short
        /// </summary>
        public Func<object, short> ToShort { get; } = new Func<object, short>(val => ((IConvertible)val).ToInt16(null));
        /// <summary>
        /// Change value to uint
        /// </summary>
        public Func<object, uint> ToUInt { get; } = new Func<object, uint>(val => ((IConvertible)val).ToUInt32(null));
        /// <summary>
        /// Change value to int
        /// </summary>
        public Func<object, int> ToInt { get; } = new Func<object, int>(val => ((IConvertible)val).ToInt32(null));
        /// <summary>
        /// Change value to ulong
        /// </summary>
        public Func<object, ulong> ToULong { get; } = new Func<object, ulong>(val => ((IConvertible)val).ToUInt64(null));
        /// <summary>
        /// Change value to long
        /// </summary>
        public Func<object, long> ToLong { get; } = new Func<object, long>(val => ((IConvertible)val).ToInt64(null));
        /// <summary>
        /// Change value to float
        /// </summary>
        public Func<object, float> ToFloat { get; } = new Func<object, float>(val => ((IConvertible)val).ToSingle(null));
        /// <summary>
        /// Change value to double
        /// </summary>
        public Func<object, double> ToDouble { get; } = new Func<object, double>(val => ((IConvertible)val).ToDouble(null));
        /// <summary>
        /// Change value to decimal
        /// </summary>
        public Func<object, decimal> ToDecimal { get; } = new Func<object, decimal>(val => ((IConvertible)val).ToDecimal(null));
    }
}