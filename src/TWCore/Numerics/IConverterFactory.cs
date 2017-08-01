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
    /// Converter factory interface
    /// </summary>
    public interface IConverterFactory
    {
        /// <summary>
        /// Change value to byte
        /// </summary>
        Func<object, byte> ToByte { get; }
        /// <summary>
        /// Change value to sbyte
        /// </summary>
        Func<object, sbyte> ToSByte { get; }
        /// <summary>
        /// Change value to ushort
        /// </summary>
        Func<object, ushort> ToUShort { get; }
        /// <summary>
        /// Change value to short
        /// </summary>
        Func<object, short> ToShort { get; }
        /// <summary>
        /// Change value to uint
        /// </summary>
        Func<object, uint> ToUInt { get; }
        /// <summary>
        /// Change value to int
        /// </summary>
        Func<object, int> ToInt { get; }
        /// <summary>
        /// Change value to ulong
        /// </summary>
        Func<object, ulong> ToULong { get; }
        /// <summary>
        /// Change value to long
        /// </summary>
        Func<object, long> ToLong { get; }
        /// <summary>
        /// Change value to float
        /// </summary>
        Func<object, float> ToFloat { get; }
        /// <summary>
        /// Change value to double
        /// </summary>
        Func<object, double> ToDouble { get; }
        /// <summary>
        /// Change value to decimal
        /// </summary>
        Func<object, decimal> ToDecimal { get; }
    }
}