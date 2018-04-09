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

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TWCore.Serialization.NSerializer
{
    internal class SerializerCache<T>
    {
        private readonly Dictionary<T, int> _serializationCache;
        private readonly Dictionary<int, T> _deserializationCache;
        private const int MaxIndex = 2047;
        private int _currentIndex;

        public int Count => _currentIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializerCache(IEqualityComparer<T> sercomparer = null, IEqualityComparer<int> descomparer = null)
        {
            _serializationCache = new Dictionary<T, int>(sercomparer);
            _deserializationCache = new Dictionary<int, T>(descomparer);
            _currentIndex = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_currentIndex == 0) return;
            _currentIndex = 0;
            _serializationCache.Clear();
            _deserializationCache.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SerializerGet(T value)
            => _serializationCache.TryGetValue(value, out var cIdx) ? cIdx : -1;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializerSet(T value)
        {
            if (_currentIndex < MaxIndex)
                _serializationCache.Add(value, _currentIndex++);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T DeserializerGet(int index)
            => _deserializationCache[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeserializerSet(T value)
        {
            if (_currentIndex < MaxIndex)
                _deserializationCache.Add(_currentIndex++, value);
        }
    }
}