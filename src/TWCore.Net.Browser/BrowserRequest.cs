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

using System.Net;

namespace TWCore.Net.Browser
{
    /// <summary>
    /// Represent a browser request
    /// </summary>
    public class BrowserRequest
    {
        /// <summary>
        /// Request url
        /// </summary>
        public string RequestUrl { get; set; }
        /// <summary>
        /// Http method
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// Http header Url Referer
        /// </summary>
        public string UrlReferer { get; set; }
        /// <summary>
        /// Cookies container
        /// </summary>
        public CookieContainer Cookies { get; set; }
        /// <summary>
        /// Post data byte array
        /// </summary>
        public byte[] PostData { get; set; }
        /// <summary>
        /// Post data ContentType mime type
        /// </summary>
        public string ContentType { get; set; } = "application/x-www-form-urlencoded";
    }
}
