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
using System.Runtime.CompilerServices;
// ReSharper disable IntroduceOptionalParameters.Global

namespace TWCore.IO
{
    /// <summary>
    /// Segment Pool Instance
    /// </summary>
    public static class SegmentPool
    {
        /// <summary>
        /// Maximum segment length
        /// </summary>
        public const int SegmentLength = 1024;
        private static readonly ObjectPool<byte[], BytePoolAllocator> ByteArrayPool = new ObjectPool<byte[], BytePoolAllocator>();
        private static readonly ObjectPool<List<byte[]>, ListBytePoolAllocator> ByteArrayListPool = new ObjectPool<List<byte[]>, ListBytePoolAllocator>();

        #region Allocators
        private readonly struct BytePoolAllocator : IPoolObjectLifecycle<byte[]>
        {
            public int InitialSize => 10;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            public int DropTimeFrequencyInSeconds => 120;
            public void DropAction(byte[] value) { }
            public byte[] New() => new byte[SegmentLength];
            public void Reset(byte[] value) => Array.Clear(value, 0, SegmentLength);
            public int DropMaxSizeThreshold => 15;
        }
        private readonly struct ListBytePoolAllocator : IPoolObjectLifecycle<List<byte[]>>
        {
            public int InitialSize => 5;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            public int DropTimeFrequencyInSeconds => 120;
            public void DropAction(List<byte[]> value) { }
            public List<byte[]> New() => new List<byte[]>();
            public void Reset(List<byte[]> value) => value.Clear();
            public int DropMaxSizeThreshold => 15;
        }
        #endregion

        /// <summary>
        /// Rent a new Segment
        /// </summary>
        /// <returns>Segment</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Rent() => ByteArrayPool.New();
        /// <summary>
        /// Return the Segment
        /// </summary>
        /// <param name="segment">Segment to return</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(byte[] segment)
        {
            if (segment == null) return;
            if (segment.Length != SegmentLength) return;
            ByteArrayPool.Store(segment);
        }
        /// <summary>
        /// Rent a new Container
        /// </summary>
        /// <returns>Container</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<byte[]> RentContainer() => ByteArrayListPool.New();
        /// <summary>
        /// Return the container
        /// </summary>
        /// <param name="container">Container</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReturnContainer(List<byte[]> container)
        {
            if (container == null) return;
            ByteArrayListPool.Store(container);
        }
    }
}
