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

namespace TWCore.Cache.Client
{
    /// <summary>
    /// Cache pool Read Mode
    /// </summary>
    public enum PoolReadMode
    {
        /// <summary>
        /// Reads only on the fastest cache storage
        /// </summary>
        FastestOnlyRead,
        /// <summary>
        /// Looks for the data in all cache pool
        /// </summary>
        NormalRead
    }
}
