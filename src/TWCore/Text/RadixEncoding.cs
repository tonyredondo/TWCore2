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
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeTypeMemberModifiers

namespace TWCore.Text
{
    /// <summary>
    /// Endian format types
    /// </summary>
    public enum EndianFormat
    {
        /// <summary>Least Significant Bit order (lsb)</summary>
        /// <remarks>Right-to-Left</remarks>
        /// <see cref="BitConverter.IsLittleEndian"/>
        Little,
        /// <summary>Most Significant Bit order (msb)</summary>
        /// <remarks>Left-to-Right</remarks>
        Big,
    };

    /// <summary>Encodes/decodes bytes to/from a string</summary>
    /// <remarks>
    /// Encoded string is always in big-endian ordering
    /// 
    /// <p>Encode and Decode take a <b>includeProceedingZeros</b> parameter which acts as a work-around
    /// for an edge case with our BigInteger implementation.
    /// MSDN says BigInteger byte arrays are in LSB->MSB ordering. So a byte buffer with zeros at the 
    /// end will have those zeros ignored in the resulting encoded radix string.
    /// If such a loss in precision absolutely cannot occur pass true to <b>includeProceedingZeros</b>
    /// and for a tiny bit of extra processing it will handle the padding of zero digits (encoding)
    /// or bytes (decoding).</p>
    /// <p>Note: doing this for decoding <b>may</b> add an extra byte more than what was originally 
    /// given to Encode.</p>
    /// </remarks>
    // Based on the answers from http://codereview.stackexchange.com/questions/14084/base-36-encoding-of-a-byte-array/
    public class RadixEncoding
    {
        const int kByteBitCount = 8;
        const string _charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÁÉÍÓÚÀÈÌÒÙÄËÏÖÜÂÊÎÔÛÃÕáéíóúàèìòùäëïöüâêîôûãõÑñçÇ~0123456789|!@\"\\#$€%&/()'?¿¡+*-[]{}:.;<>";

        readonly string kDigits;
        readonly double kBitsPerDigit;
        readonly BigInteger kRadixBig;
        readonly EndianFormat kEndian;
        readonly bool kIncludeProceedingZeros;

        /// <summary>Numerial base of this encoding</summary>
        public int Radix => kDigits.Length;

        /// <summary>Endian ordering of bytes input to Encode and output by Decode</summary>
        public EndianFormat Endian => kEndian;

        /// <summary>True if we want ending zero bytes to be encoded</summary>
        public bool IncludeProceedingZeros => kIncludeProceedingZeros;

        /// <summary>
        /// Gets a string representation from the object instance
        /// </summary>
        /// <returns>String representation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return string.Format("Base-{0} {1}", Radix.ToString(), kDigits);
        }

        /// <summary>Create a radix encoder using the given characters as the digits in the radix</summary>
        /// <param name="digits">Digits to use for the radix-encoded string</param>
        /// <param name="bytesEndian">Endian ordering of bytes input to Encode and output by Decode</param>
        /// <param name="includeProceedingZeros">True if we want ending zero bytes to be encoded</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RadixEncoding(string digits = _charset, EndianFormat bytesEndian = EndianFormat.Little, bool includeProceedingZeros = false)
        {
            Ensure.Requires<ArgumentNullException>(digits != null);
            if (digits != null)
            {
                var radix = digits.Length;

                kDigits = digits;
                kBitsPerDigit = Math.Log(radix, 2);
                kRadixBig = new BigInteger(radix);
            }
            kEndian = bytesEndian;
            kIncludeProceedingZeros = includeProceedingZeros;
        }

        // Number of characters needed for encoding the specified number of bytes
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int EncodingCharsCount(int bytesLength)
        {
            return (int)Math.Ceiling((bytesLength * kByteBitCount) / kBitsPerDigit);
        }
        // Number of bytes needed to decoding the specified number of characters
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int DecodingBytesCount(int charsCount)
        {
            return (int)Math.Ceiling((charsCount * kBitsPerDigit) / kByteBitCount);
        }

        /// <summary>Encode a byte array into a radix-encoded string</summary>
        /// <param name="bytes">byte array to encode</param>
        /// <returns>The bytes in encoded into a radix-encoded string</returns>
        /// <remarks>If <paramref name="bytes"/> is zero length, returns an empty string</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Encode(byte[] bytes)
        {
            Ensure.Requires<ArgumentNullException>(bytes != null);
            if (bytes == null) return null;
            
            // Don't really have to do this, our code will build this result (empty string),
            // but why not catch the condition before doing work?
            if (bytes.Length == 0) return string.Empty;

            // if the array ends with zeros, having the capacity set to this will help us know how much
            // 'padding' we will need to add
            int result_length = EncodingCharsCount(bytes.Length);
            // List<> has a(n in-place) Reverse method. StringBuilder doesn't. That's why.
            var result = new List<char>(result_length);

            // BigInteger uses the last byte as the 'sign' byte. If that byte's MSB is set, 
            // we need to pad the input with an extra 0 (ie, make it positive)
            if ((bytes[bytes.Length - 1] & 0x80) == 0x80)
                Array.Resize(ref bytes, bytes.Length + 1);

            var dividend = new BigInteger(bytes);
            // IsZero's computation is less complex than evaluating "dividend > 0"
            // which invokes BigInteger.CompareTo(BigInteger)
            while (!dividend.IsZero)
            {
                dividend = BigInteger.DivRem(dividend, kRadixBig, out var remainder);
                int digit_index = Math.Abs((int)remainder);
                result.Add(kDigits[digit_index]);
            }

            if (kIncludeProceedingZeros)
                for (int x = result.Count; x < result.Capacity; x++)
                    result.Add(kDigits[0]); // pad with the character that represents 'zero'

            // orientate the characters in big-endian ordering
            if (kEndian == EndianFormat.Little)
                result.Reverse();
            // If we didn't end up adding padding, ToArray will end up returning a TrimExcess'd array, 
            // so nothing wasted
            return new string(result.ToArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DecodeImplPadResult(ref byte[] result, int padCount)
        {
            if (padCount <= 0) return;
            var new_length = result.Length + DecodingBytesCount(padCount);
            Array.Resize(ref result, new_length); // new bytes will be zero, just the way we want it
        }
        #region Decode (Little Endian)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        byte[] DecodeImpl(string chars, int startIndex = 0)
        {
            var bi = new BigInteger();
            for (var x = startIndex; x < chars.Length; x++)
            {
                var i = kDigits.IndexOf(chars[x]);
                if (i < 0) return null; // invalid character
                bi *= kRadixBig;
                bi += i;
            }

            return bi.ToByteArray();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        byte[] DecodeImplWithPadding(string chars)
        {
            var pad_count = 0;
            for (var x = 0; x < chars.Length; x++, pad_count++)
                if (chars[x] != kDigits[0]) break;

            var result = DecodeImpl(chars, pad_count);
            DecodeImplPadResult(ref result, pad_count);

            return result;
        }
        #endregion

        #region Decode (Big Endian)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        byte[] DecodeImplReversed(string chars, int startIndex = 0)
        {
            var bi = new BigInteger();
            for (var x = (chars.Length - 1) - startIndex; x >= 0; x--)
            {
                var i = kDigits.IndexOf(chars[x]);
                if (i < 0) return null; // invalid character
                bi *= kRadixBig;
                bi += i;
            }

            return bi.ToByteArray();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        byte[] DecodeImplReversedWithPadding(string chars)
        {
            var pad_count = 0;
            for (var x = chars.Length - 1; x >= 0; x--, pad_count++)
                if (chars[x] != kDigits[0]) break;

            var result = DecodeImplReversed(chars, pad_count);
            DecodeImplPadResult(ref result, pad_count);

            return result;
        }
        #endregion

        /// <summary>Decode a radix-encoded string into a byte array</summary>
        /// <param name="radixChars">radix string</param>
        /// <returns>The decoded bytes, or null if an invalid character is encountered</returns>
        /// <remarks>
        /// If <paramref name="radixChars"/> is an empty string, returns a zero length array
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] Decode(string radixChars)
        {
            Ensure.Requires<ArgumentNullException>(radixChars != null);

            if (kEndian == EndianFormat.Big)
                return kIncludeProceedingZeros ? DecodeImplReversedWithPadding(radixChars) : DecodeImplReversed(radixChars);
            return kIncludeProceedingZeros ? DecodeImplWithPadding(radixChars) : DecodeImpl(radixChars);
        }
    };
}
