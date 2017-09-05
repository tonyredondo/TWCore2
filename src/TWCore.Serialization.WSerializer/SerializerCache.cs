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

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TWCore.Serialization.WSerializer
{
    internal class SerializerCache<T>
    {
        private readonly Dictionary<T, int> _serializationCache;
        private readonly Dictionary<int, T> _deserializationCache;
        private int _maxIndex;
        private int _currentIndex;

        public int Count => _currentIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializerCache(SerializerMode mode, IEqualityComparer<T> sercomparer = null, IEqualityComparer<int> descomparer = null)
        {
            _serializationCache = new Dictionary<T, int>(sercomparer);
            _deserializationCache = new Dictionary<int, T>(descomparer);
            switch (mode)
            {
                case SerializerMode.Cached512:
                    _maxIndex = 511;
                    break;
                case SerializerMode.Cached1024:
                    _maxIndex = 1023;
                    break;
                case SerializerMode.Cached2048:
                    _maxIndex = 2047;
                    break;
                case SerializerMode.CachedUShort:
                    _maxIndex = 65534;
                    break;
            }
            _currentIndex = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(SerializerMode mode)
        {
            switch (mode)
            {
                case SerializerMode.Cached512:
                    _maxIndex = 511;
                    break;
                case SerializerMode.Cached1024:
                    _maxIndex = 1023;
                    break;
                case SerializerMode.Cached2048:
                    _maxIndex = 2047;
                    break;
                case SerializerMode.CachedUShort:
                    _maxIndex = 65534;
                    break;
            }
            _currentIndex = 0;
            _serializationCache.Clear();
            _deserializationCache.Clear();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SerializerGet(T value)
        {
            if (_serializationCache.TryGetValue(value, out int cIdx))
                return cIdx;
            return -1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializerSet(T value)
        {
            if (_currentIndex < _maxIndex)
                _serializationCache[value] = _currentIndex++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T DeserializerGet(int index)
            => _deserializationCache[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeserializerSet(T value)
        {
            if (_currentIndex < _maxIndex)
                _deserializationCache[_currentIndex++] = value;
        }
    }
}
