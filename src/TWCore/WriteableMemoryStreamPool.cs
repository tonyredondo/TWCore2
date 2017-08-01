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

using System.IO;
using System.Runtime.CompilerServices;

namespace TWCore
{
    /// <summary>
    /// MemoryStream pool for temporal write.
    /// </summary>
    public static class WriteableMemoryStreamPool
    {
        static ReferencePool<MemoryStream> _pool = new ReferencePool<MemoryStream>(0, ms =>
        {
            ms.Position = 0;
            ms.SetLength(0);
        }, null, PoolResetMode.AfterUse, 0);

        /// <summary>
        /// Get a new MemoryStream for Write
        /// </summary>
        /// <returns>MemoryStream instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryStream Get() => _pool.New();
        /// <summary>
        /// Store a MemoryStream instance to the pool
        /// </summary>
        /// <param name="mstream">MemoryStream instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(MemoryStream mstream) => _pool.Store(mstream);
    }
}
