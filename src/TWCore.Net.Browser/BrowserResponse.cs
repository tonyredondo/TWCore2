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

using System;
using System.Net;
using System.Net.Http.Headers;

namespace TWCore.Net.Browser
{
    /// <summary>
    /// Represent a browser response
    /// </summary>
    public class BrowserResponse
    {
        /// <summary>
        /// Response Url
        /// </summary>
        public string ResponseUrl { get; set; }
        /// <summary>
        /// Response headers collection
        /// </summary>
        public HttpResponseHeaders Headers { get; set; }
        /// <summary>
        /// Content headers collection
        /// </summary>
        public HttpContentHeaders ContentHeaders { get; set; }

        /// <summary>
        /// Content encoding
        /// </summary>
        public string ContentEncoding { get; set; }
        /// <summary>
        /// Content mime type
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// Content length
        /// </summary>
        public long? ContentLength { get; set; }
        /// <summary>
        /// Response content
        /// </summary>
        public byte[] Content { get; set; }
        /// <summary>
        /// Response cookies
        /// </summary>
        public CookieContainer Cookies { get; set; }
        /// <summary>
        /// Response last modified date and time
        /// </summary>
        public DateTimeOffset? LastModified { get; set; }
        /// <summary>
        /// Get if the response is from cache
        /// </summary>
        public bool IsFromCache { get; set; }
    }
}
