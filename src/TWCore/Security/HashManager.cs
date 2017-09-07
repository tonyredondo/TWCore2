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

using System.Runtime.CompilerServices;
using TWCore.Collections;
// ReSharper disable ObjectCreationAsStatement
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Security
{
    /// <summary>
    /// Global Hash Manager
    /// </summary>
    public static class HashManager
    {
        /// <summary>
        /// Registered hashes list
        /// </summary>
        public static readonly KeyStringDelegatedCollection<IHash> Hashes = new KeyStringDelegatedCollection<IHash>(h => h.Algorithm, false);

        /// <summary>
        /// Global Hash Manager
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static HashManager()
        {
            new MD5Hash();
            new SHA1Hash();
            new SHA256Hash();
            new SHA384Hash();
            new SHA512Hash();
        }

        /// <summary>
        /// Register a Hash algorithm instance in the list
        /// </summary>
        /// <param name="hash">Hash instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Register(IHash hash)
        {
            if (hash != null)
                Hashes.Add(hash);
        }
        /// <summary>
        /// Deregister a Hash algorithm instance from the list
        /// </summary>
        /// <param name="hash">Hash instance to remove</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeregisterSerializer(IHash hash)
        {
            if (hash == null) return;
            if (Hashes.Contains(hash.Algorithm))
                Hashes.Remove(hash.Algorithm);
        }
        /// <summary>
        /// Gets a hash instance from a algorithm type
        /// </summary>
        /// <param name="algorithm">Algorithm type</param>
        /// <returns>Hash instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IHash Get(string algorithm) 
            => Hashes.TryGet(algorithm, out var hash) ? hash : null;
    }
}
