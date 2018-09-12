﻿/*
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
using System.Runtime.CompilerServices;

namespace TWCore.Net.HttpServer
{
    /// <summary>
    /// Http utility methods
    /// </summary>
    public static class HttpUtility
    {
        /// <summary>
        /// Parse a query string into a HttpValueCollection
        /// </summary>
        /// <param name="query">Query string value</param>
        /// <returns>HttpValueCollection instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HttpValueCollection ParseQueryString(string query)
        {
            if (query is null)
                throw new ArgumentNullException("query");

            if ((query.Length > 0) && (query[0] == '?'))
                query = query.Substring(1);

            return new HttpValueCollection(query);
        }
    }
}
