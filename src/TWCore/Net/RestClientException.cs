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
using System.Runtime.CompilerServices;

namespace TWCore.Net
{
    /// <summary>
    /// Exception for the Rest Client object
    /// </summary>
    public class RestClientException : Exception
    {
        /// <summary>
        /// Requested Url
        /// </summary>
        public Uri RequestUri { get; set; }
        /// <summary>
        /// Http response status code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }
        /// <summary>
        /// Phrase with the reason of the exception
        /// </summary>
        public string ReasonPhrase { get; set; }
        /// <summary>
        /// Http response content in bytes
        /// </summary>
        public byte[] ResponseBytes { get; set; }
        /// <summary>
        /// Http response content in text
        /// </summary>
        public string ResponseText { get; set; }
        /// <summary>
        /// Server exception
        /// </summary>
        public SerializableHttpError ServerException { get; set; }
        /// <summary>
        /// Exception for the Rest Client object
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RestClientException() : base("Rest client exception") { }
        /// <summary>
        /// Exception for the Rest Client object
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RestClientException(string message) : base("Rest client exception.\r\n" + message) { }
        /// <summary>
        /// Exception for the Rest Client object
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RestClientException(string message, Exception innerException) : base("Rest client exception.\r\n" + message, innerException) { }
    }
}
