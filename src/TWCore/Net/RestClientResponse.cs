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
    /// Response of the Rest client object
    /// </summary>
    public class RestClientResponse
    {
        /// <summary>
        /// true if the received status code was a 200 OK; otherwise, false.
        /// </summary>
        public bool IsSuccessStatusCode { get; set; }
        /// <summary>
        /// Gets the response content in bytes
        /// </summary>
        public byte[] ValueInBytes { get; set; }
        /// <summary>
        /// Gets the response status code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }
        /// <summary>
        /// Gets the reason phrase from the response status code
        /// </summary>
        public string ReasonPhrase { get; set; }
        /// <summary>
        /// Gets the server exception if an error occurs, usually when the status code is not a 200 OK.
        /// </summary>
        public RestClientException Exception { get; set; }
        /// <summary>
        /// Gets the requested url
        /// </summary>
        public Uri RequestUri { get; set; }
    }
    /// <summary>
    /// Response of the Rest client object
    /// </summary>
    public class RestClientResponse<T> : RestClientResponse
    {
        /// <summary>
        /// Gets the deserialized response object
        /// </summary>
        public T Value { get; private set; }
        /// <summary>
        /// Response of the Rest client object
        /// </summary>
        /// <param name="response">Original client response object</param>
        /// <param name="responseObject">Deserialized response object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RestClientResponse(RestClientResponse response, T responseObject)
        {
            IsSuccessStatusCode = response.IsSuccessStatusCode;
            ReasonPhrase = response.ReasonPhrase;
            ValueInBytes = response.ValueInBytes;
            StatusCode = response.StatusCode;
            Exception = response.Exception;
            RequestUri = response.RequestUri;
            Value = responseObject;
        }
    }

}
