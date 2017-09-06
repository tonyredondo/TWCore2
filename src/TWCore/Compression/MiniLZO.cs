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

#pragma warning disable CS0164 // No existe ninguna referencia a esta etiqueta
using System;
using System.Runtime.CompilerServices;
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMethodReturnValue.Local
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantCast
// ReSharper disable ConvertIfDoToWhile

namespace TWCore.Compression
{
    /// <summary>
    /// MiniLZO Algorithm
    /// </summary>
    public static class MiniLZO
    {
        private static readonly int[] MultiplyDeBruijnBitPosition = { 0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9 };
        
        #region Public Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe SubArray<byte> Decompress(byte[] @in)
        {
            var @out = new byte[@in.Length * 8];
            uint out_len = 0;
            fixed (byte* @pIn = @in, wrkmem = new byte[IntPtr.Size * 16384], pOut = @out)
            {
                Lzo1x_decompress(pIn, (uint)@in.Length, @pOut, ref @out_len, wrkmem);
            }
            return new SubArray<byte>(@out, 0, (int)out_len);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe SubArray<byte> Decompress(SubArray<byte> @in)
        {
            var @out = new byte[@in.Count * 8];
            uint out_len = 0;
            fixed (byte* wrkmem = new byte[IntPtr.Size * 16384], pOut = @out)
            {
                Lzo1x_decompress((byte*)@in.GetPointer(), (uint)@in.Count, @pOut, ref @out_len, wrkmem);
            }
            return new SubArray<byte>(@out, 0, (int)out_len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Decompress(byte* r, uint size_in, byte* w, ref uint size_out)
        {
            fixed (byte* wrkmem = new byte[IntPtr.Size * 16384])
            {
                Lzo1x_decompress(r, size_in, w, ref size_out, wrkmem);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe SubArray<byte> Compress(byte[] input)
        {
            var @out = new byte[input.Length + (input.Length / 16) + 64 + 3];
            uint out_len = 0;
            fixed (byte* @pIn = input, wrkmem = new byte[IntPtr.Size * 16384], pOut = @out)
            {
                Lzo1x_1_compress(pIn, (uint)input.Length, @pOut, ref @out_len, wrkmem);
            }
            return new SubArray<byte>(@out, 0, (int)out_len);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe SubArray<byte> Compress(SubArray<byte> input)
        {
            var @out = new byte[input.Count + (input.Count / 16) + 64 + 3];
            uint out_len = 0;
            fixed (byte* wrkmem = new byte[IntPtr.Size * 16384], pOut = @out)
            {
                Lzo1x_1_compress((byte*)input.GetPointer(), (uint)input.Count, @pOut, ref @out_len, wrkmem);
            }
            return new SubArray<byte>(@out, 0, (int)out_len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Compress(byte* r, uint size_in, byte* w, ref uint size_out)
        {
            fixed (byte* wrkmem = new byte[IntPtr.Size * 16384])
            {
                Lzo1x_1_compress(r, size_in, w, ref size_out, wrkmem);
            }
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe uint Lzo1x_1_compress_core(byte* @in, uint in_len, byte* @out, ref uint out_len, uint ti, void* wrkmem)
        {
            byte* ip;
            byte* op;
            byte* in_end = @in + in_len;
            byte* ip_end = @in + in_len - 20;
            byte* ii;
            ushort* dict = (ushort*)wrkmem;
            op = @out;
            ip = @in;
            ii = ip;
            ip += ti < 4 ? 4 - ti : 0;

            byte* m_pos;
            uint m_off;
            uint m_len;

            for (;;)
            {

                uint dv;
                uint dindex;
                literal:
                ip += 1 + ((ip - ii) >> 5);
                next:
                if (ip >= ip_end)
                    break;
                dv = (*(uint*)(void*)(ip));
                dindex = ((uint)(((((((uint)((0x1824429d) * (dv)))) >> (32 - 14))) & (((1u << (14)) - 1) >> (0))) << (0)));
                m_pos = @in + dict[dindex];
                dict[dindex] = ((ushort)((uint)((ip) - (@in))));
                if (dv != (*(uint*)(void*)(m_pos)))
                    goto literal;

                ii -= ti; ti = 0;
                {
                    uint t = ((uint)((ip) - (ii)));
                    if (t != 0)
                    {
                        if (t <= 3)
                        {
                            op[-2] |= ((byte)(t));
                            *(uint*)(op) = *(uint*)(ii);
                            op += t;
                        }
                        else if (t <= 16)
                        {
                            *op++ = ((byte)(t - 3));
                            *(uint*)(op) = *(uint*)(ii);
                            *(uint*)(op + 4) = *(uint*)(ii + 4);
                            *(uint*)(op + 8) = *(uint*)(ii + 8);
                            *(uint*)(op + 12) = *(uint*)(ii + 12);
                            op += t;
                        }
                        else
                        {
                            if (t <= 18)
                                *op++ = ((byte)(t - 3));
                            else
                            {
                                uint tt = t - 18;
                                *op++ = 0;
                                while (tt > 255)
                                {
                                    tt -= 255;
                                    *(byte*)op++ = 0;
                                }

                                *op++ = ((byte)(tt));
                            }
                            do
                            {
                                *(uint*)(op) = *(uint*)(ii);
                                *(uint*)(op + 4) = *(uint*)(ii + 4);
                                *(uint*)(op + 8) = *(uint*)(ii + 8);
                                *(uint*)(op + 12) = *(uint*)(ii + 12);
                                op += 16; ii += 16; t -= 16;
                            } while (t >= 16); if (t > 0) { do *op++ = *ii++; while (--t > 0); }
                        }
                    }
                }
                m_len = 4;
                {
                    uint v;
                    v = (*(uint*)(void*)(ip + m_len)) ^ (*(uint*)(void*)(m_pos + m_len));
                    if (v == 0)
                    {
                        do
                        {
                            m_len += 4;
                            v = (*(uint*)(void*)(ip + m_len)) ^ (*(uint*)(void*)(m_pos + m_len));
                            if (ip + m_len >= ip_end)
                                goto m_len_done;
                        } while (v == 0);
                    }
                    m_len += (uint)Lzo_bitops_ctz32(v) / 8;
                }
                m_len_done:
                m_off = ((uint)((ip) - (m_pos)));
                ip += m_len;
                ii = ip;
                if (m_len <= 8 && m_off <= 0x0800)
                {
                    m_off -= 1;
                    *op++ = ((byte)(((m_len - 1) << 5) | ((m_off & 7) << 2)));
                    *op++ = ((byte)(m_off >> 3));
                }
                else if (m_off <= 0x4000)
                {
                    m_off -= 1;
                    if (m_len <= 33)
                        *op++ = ((byte)(32 | (m_len - 2)));
                    else
                    {
                        m_len -= 33;
                        *op++ = 32 | 0;
                        while (m_len > 255)
                        {
                            m_len -= 255;
                            *(byte*)op++ = 0;
                        }
                        *op++ = ((byte)(m_len));
                    }
                    *op++ = ((byte)(m_off << 2));
                    *op++ = ((byte)(m_off >> 6));
                }
                else
                {
                    m_off -= 0x4000;
                    if (m_len <= 9)
                        *op++ = ((byte)(16 | ((m_off >> 11) & 8) | (m_len - 2)));
                    else
                    {
                        m_len -= 9;
                        *op++ = ((byte)(16 | ((m_off >> 11) & 8)));
                        while (m_len > 255)
                        {
                            m_len -= 255;
                            *(byte*)op++ = 0;
                        }
                        *op++ = ((byte)(m_len));
                    }
                    *op++ = ((byte)(m_off << 2));
                    *op++ = ((byte)(m_off >> 6));
                }
                goto next;
            }
            out_len = ((uint)((op) - (@out)));
            return ((uint)((in_end) - (ii - ti)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int Lzo_bitops_ctz32(uint v) 
            => MultiplyDeBruijnBitPosition[((uint)((v & -v) * 0x077CB531U)) >> 27];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe int Lzo1x_1_compress(byte* @in, uint in_len, byte* @out, ref uint out_len, byte* wrkmem)
        {
            byte* ip = @in;
            byte* op = @out;
            uint l = in_len;
            uint t = 0;
            while (l > 20)
            {
                uint ll = l;
                ulong ll_end;
                ll = ((ll) <= (49152) ? (ll) : (49152));
                ll_end = (ulong)ip + ll;
                if ((ll_end + ((t + ll) >> 5)) <= ll_end || (byte*)(ll_end + ((t + ll) >> 5)) <= ip + ll)
                    break;

                for (int i = 0; i < (1 << 14) * sizeof(ushort); i++)
                    wrkmem[i] = 0;
                t = Lzo1x_1_compress_core(ip, ll, op, ref out_len, t, wrkmem);
                ip += ll;
                op += out_len;
                l -= ll;
            }
            t += l;
            if (t > 0)
            {
                byte* ii = @in + in_len - t;
                if (op == @out && t <= 238)
                    *op++ = ((byte)(17 + t));
                else if (t <= 3)
                    op[-2] |= ((byte)(t));
                else if (t <= 18)
                    *op++ = ((byte)(t - 3));
                else
                {
                    uint tt = t - 18;
                    *op++ = 0;
                    while (tt > 255)
                    {
                        tt -= 255;
                        *(byte*)op++ = 0;
                    }

                    *op++ = ((byte)(tt));
                }
                do *op++ = *ii++; while (--t > 0);
            }
            *op++ = 16 | 1;
            *op++ = 0;
            *op++ = 0;
            out_len = ((uint)((op) - (@out)));
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe int Lzo1x_decompress(byte* @in, uint in_len, byte* @out, ref uint out_len, void* wrkmem)
        {
            byte* op;
            byte* ip;
            uint t;
            byte* m_pos;
            byte* ip_end = @in + in_len;
            out_len = 0;
            op = @out;
            ip = @in;
            bool gt_first_literal_run = false;
            bool gt_match_done = false;
            if (*ip > 17)
            {
                t = (uint)(*ip++ - 17);
                if (t < 4)
                {
                    Match_next(ref op, ref ip, ref t);
                }
                else
                {
                    do *op++ = *ip++; while (--t > 0);
                    gt_first_literal_run = true;
                }
            }
            while (true)
            {
                if (gt_first_literal_run)
                {
                    gt_first_literal_run = false;
                    goto first_literal_run;
                }

                t = *ip++;
                if (t >= 16)
                    goto match;
                if (t == 0)
                {
                    while (*ip == 0)
                    {
                        t += 255;
                        ip++;
                    }
                    t += (uint)(15 + *ip++);
                }
                *(uint*)op = *(uint*)ip;
                op += 4; ip += 4;
                if (--t > 0)
                {
                    if (t >= 4)
                    {
                        do
                        {
                            *(uint*)op = *(uint*)ip;
                            op += 4; ip += 4; t -= 4;
                        } while (t >= 4);
                        if (t > 0) do *op++ = *ip++; while (--t > 0);
                    }
                    else
                        do *op++ = *ip++; while (--t > 0);
                }
                first_literal_run:
                t = *ip++;
                if (t >= 16)
                    goto match;
                m_pos = op - (1 + 0x0800);
                m_pos -= t >> 2;
                m_pos -= *ip++ << 2;

                *op++ = *m_pos++; *op++ = *m_pos++; *op++ = *m_pos;
                gt_match_done = true;

                match:
                do
                {
                    if (gt_match_done)
                    {
                        gt_match_done = false;
                        goto match_done;
                    }
                    if (t >= 64)
                    {
                        m_pos = op - 1;
                        m_pos -= (t >> 2) & 7;
                        m_pos -= *ip++ << 3;
                        t = (t >> 5) - 1;

                        Copy_match(ref op, ref m_pos, ref t);
                        goto match_done;
                    }
                    else if (t >= 32)
                    {
                        t &= 31;
                        if (t == 0)
                        {
                            while (*ip == 0)
                            {
                                t += 255;
                                ip++;
                            }
                            t += (uint)(31 + *ip++);
                        }
                        m_pos = op - 1;
                        m_pos -= (*(ushort*)(void*)(ip)) >> 2;
                        ip += 2;
                    }
                    else if (t >= 16)
                    {
                        m_pos = op;
                        m_pos -= (t & 8) << 11;
                        t &= 7;
                        if (t == 0)
                        {
                            while (*ip == 0)
                            {
                                t += 255;
                                ip++;
                            }
                            t += (uint)(7 + *ip++);
                        }
                        m_pos -= (*(ushort*)ip) >> 2;
                        ip += 2;
                        if (m_pos == op)
                            goto eof_found;
                        m_pos -= 0x4000;
                    }
                    else
                    {
                        m_pos = op - 1;
                        m_pos -= t >> 2;
                        m_pos -= *ip++ << 2;
                        *op++ = *m_pos++; *op++ = *m_pos;
                        goto match_done;
                    }

                    if (t >= 2 * 4 - (3 - 1) && (op - m_pos) >= 4)
                    {
                        *(uint*)op = *(uint*)m_pos;
                        op += 4; m_pos += 4; t -= 4 - (3 - 1);
                        do
                        {
                            *(uint*)op = *(uint*)m_pos;
                            op += 4; m_pos += 4; t -= 4;
                        } while (t >= 4);
                        if (t > 0) do *op++ = *m_pos++; while (--t > 0);
                    }
                    else
                    {
                        copy_match:
                        *op++ = *m_pos++; *op++ = *m_pos++;
                        do *op++ = *m_pos++; while (--t > 0);
                    }
                    match_done:
                    t = (uint)(ip[-2] & 3);
                    if (t == 0)
                        break;
                    match_next:
                    *op++ = *ip++;
                    if (t > 1) { *op++ = *ip++; if (t > 2) { *op++ = *ip++; } }
                    t = *ip++;
                } while (true);
            }
            eof_found:

            out_len = ((uint)((op) - (@out)));
            return (ip == ip_end ? 0 :
                   (ip < ip_end ? (-8) : (-4)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void Match_next(ref byte* op, ref byte* ip, ref uint t)
        {
            do *op++ = *ip++; while (--t > 0);
            t = *ip++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void Copy_match(ref byte* op, ref byte* m_pos, ref uint t)
        {
            *op++ = *m_pos++; *op++ = *m_pos++;
            do *op++ = *m_pos++; while (--t > 0);
        }
        #endregion
    }
}
