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

namespace TWCore.Serialization.RawSerializer
{
    internal sealed class SerializerCache<T>
    {
        private readonly Dictionary<T, int> _serializationCache;
        private int _serCurrentIndex;

        public int Count => _serCurrentIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializerCache(IEqualityComparer<T> sercomparer = null)
        {
            _serializationCache = new Dictionary<T, int>(sercomparer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_serCurrentIndex == 0) return;
            _serCurrentIndex = 0;
            _serializationCache.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(T value, out int index)
            => _serializationCache.TryGetValue(value, out index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(T value)
        {
            if (_serCurrentIndex < 2047)
                _serializationCache.Add(value, _serCurrentIndex++);
        }
    }
    internal sealed class DeserializerCache<T>
    {
        private readonly Dictionary<int, T> _deserializationCache;
        private int _desCurrentIndex;

        public int Count => _desCurrentIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DeserializerCache(IEqualityComparer<int> descomparer = null)
        {
            _deserializationCache = new Dictionary<int, T>(descomparer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_desCurrentIndex == 0) return;
            _desCurrentIndex = 0;
            _deserializationCache.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(int index)
            => _deserializationCache[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(T value)
        {
            if (_desCurrentIndex < 2047)
                _deserializationCache.Add(_desCurrentIndex++, value);
        }
    }

}