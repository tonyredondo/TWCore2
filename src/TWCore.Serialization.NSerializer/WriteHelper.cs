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

using System.IO;
using System.Runtime.CompilerServices;

namespace TWCore.Serialization.NSerializer
{
    internal static class WriteHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteByte(BinaryWriter bw, byte type, byte value)
        {
            var buffer = new byte[2];
            buffer[0] = type;
            buffer[1] = value;
            bw.Write(buffer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUshort(BinaryWriter bw, byte type, ushort value)
        {
            var buffer = new byte[3];
            buffer[0] = type;
            buffer[1] = (byte)value;
            buffer[2] = (byte)(value >> 8);
            bw.Write(buffer, 0, 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt(BinaryWriter bw, byte type, int value)
        {
            var buffer = new byte[5];
            buffer[0] = type;
            buffer[1] = (byte)value;
            buffer[2] = (byte)(value >> 8);
            buffer[3] = (byte)(value >> 16);
            buffer[4] = (byte)(value >> 24);
            bw.Write(buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteDouble(BinaryWriter bw, byte type, double value)
        {
            var buffer = new byte[9];
            var tmpValue = *(ulong*)&value;
            buffer[0] = type;
            buffer[1] = (byte)tmpValue;
            buffer[2] = (byte)(tmpValue >> 8);
            buffer[3] = (byte)(tmpValue >> 16);
            buffer[4] = (byte)(tmpValue >> 24);
            buffer[5] = (byte)(tmpValue >> 32);
            buffer[6] = (byte)(tmpValue >> 40);
            buffer[7] = (byte)(tmpValue >> 48);
            buffer[8] = (byte)(tmpValue >> 56);
            bw.Write(buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteFloat(BinaryWriter bw, byte type, float value)
        {
            var buffer = new byte[5];
            var tmpValue = *(uint*)&value;
            buffer[0] = type;
            buffer[1] = (byte)tmpValue;
            buffer[2] = (byte)(tmpValue >> 8);
            buffer[3] = (byte)(tmpValue >> 16);
            buffer[4] = (byte)(tmpValue >> 24);
            bw.Write(buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLong(BinaryWriter bw, byte type, long value)
        {
            var buffer = new byte[9];
            buffer[0] = type;
            buffer[1] = (byte)value;
            buffer[2] = (byte)(value >> 8);
            buffer[3] = (byte)(value >> 16);
            buffer[4] = (byte)(value >> 24);
            buffer[5] = (byte)(value >> 32);
            buffer[6] = (byte)(value >> 40);
            buffer[7] = (byte)(value >> 48);
            buffer[8] = (byte)(value >> 56);
            bw.Write(buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteULong(BinaryWriter bw, byte type, ulong value)
        {
            var buffer = new byte[9];
            buffer[0] = type;
            buffer[1] = (byte)value;
            buffer[2] = (byte)(value >> 8);
            buffer[3] = (byte)(value >> 16);
            buffer[4] = (byte)(value >> 24);
            buffer[5] = (byte)(value >> 32);
            buffer[6] = (byte)(value >> 40);
            buffer[7] = (byte)(value >> 48);
            buffer[8] = (byte)(value >> 56);
            bw.Write(buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt(BinaryWriter bw, byte type, uint value)
        {
            var buffer = new byte[5];
            buffer[0] = type;
            buffer[1] = (byte)value;
            buffer[2] = (byte)(value >> 8);
            buffer[3] = (byte)(value >> 16);
            buffer[4] = (byte)(value >> 24);
            bw.Write(buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteShort(BinaryWriter bw, byte type, short value)
        {
            var buffer = new byte[3];
            buffer[0] = type;
            buffer[1] = (byte)value;
            buffer[2] = (byte)(value >> 8);
            bw.Write(buffer, 0, 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteChar(BinaryWriter bw, byte type, char value)
        {
            bw.Write(type);
            bw.Write(value);
        }
    }
}