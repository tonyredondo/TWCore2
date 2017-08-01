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

namespace TWCore.Serialization.PWSerializer
{
    internal class SerializerCache<T>
    {
        public readonly Dictionary<T, int> SerializationCache;
        public readonly Dictionary<int, T> DeserializationCache;
        int MaxIndex;
        int CurrentIndex;

        public int Count => CurrentIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializerCache(SerializerMode mode, IEqualityComparer<T> sercomparer = null, IEqualityComparer<int> descomparer = null)
        {
            SerializationCache = new Dictionary<T, int>(sercomparer);
            DeserializationCache = new Dictionary<int, T>(descomparer);
            switch (mode)
            {
                case SerializerMode.Cached512:
                    MaxIndex = 511;
                    break;
                case SerializerMode.Cached1024:
                    MaxIndex = 1023;
                    break;
                case SerializerMode.Cached2048:
                    MaxIndex = 2047;
                    break;
                case SerializerMode.CachedUShort:
                    MaxIndex = 65534;
                    break;
            }
            CurrentIndex = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(SerializerMode mode)
        {
            switch (mode)
            {
                case SerializerMode.Cached512:
                    MaxIndex = 511;
                    break;
                case SerializerMode.Cached1024:
                    MaxIndex = 1023;
                    break;
                case SerializerMode.Cached2048:
                    MaxIndex = 2047;
                    break;
                case SerializerMode.CachedUShort:
                    MaxIndex = 65534;
                    break;
            }
            CurrentIndex = 0;
            SerializationCache.Clear();
            DeserializationCache.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SerializerGet(T value)
        {
            if (SerializationCache.TryGetValue(value, out int cIdx))
                return cIdx;
            return -1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializerSet(T value)
        {
            if (CurrentIndex < MaxIndex)
                SerializationCache[value] = CurrentIndex++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T DeserializerGet(int index)
            => DeserializationCache[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeserializerSet(T value)
        {
            if (CurrentIndex < MaxIndex)
                DeserializationCache[CurrentIndex++] = value;
        }
    }
}
