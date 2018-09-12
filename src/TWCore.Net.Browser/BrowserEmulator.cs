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

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeMadeStatic.Global

namespace TWCore.Net.Browser
{
    /// <summary>
    /// Emulates a simple browser behaviour with history and cookies but without javascript
    /// </summary>
    public class BrowserEmulator
    {
        /// <summary>
        /// Browser navigation history
        /// </summary>
        public List<(BrowserRequest, BrowserResponse)> History { get; private set; } = new List<(BrowserRequest, BrowserResponse)>();

        /// <summary>
        /// Clears the browser history
        /// </summary>
        public void ClearHistory() => History = new List<(BrowserRequest, BrowserResponse)>();

        /// <summary>
        /// Navigates to a browser request using the history and cookies collection
        /// </summary>
        /// <param name="request">Browser request to navigate to</param>
        /// <returns>Browser response</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<BrowserResponse> NavigateAsync(BrowserRequest request)
        {
            if (request is null) return null;
            if (History.Count > 0)
            {
                request.UrlReferer = History[History.Count - 1].Item2.ResponseUrl;
                request.Cookies = History[History.Count - 1].Item2.Cookies;
            }
            var response = await NavigateNoHistoryAsync(request).ConfigureAwait(false);
            History.Add((request, response));
            return response;
        }

        /// <summary>
        /// Navigates to a browser request without using any history neither cookies
        /// </summary>
        /// <param name="request">Browser request to navigate to</param>
        /// <returns>Browser response</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<BrowserResponse> NavigateNoHistoryAsync(BrowserRequest request)
        {
            if (request is null) return null;
            if (string.IsNullOrEmpty(request.Method)) request.Method = "GET";

            var wClientHandler = new HttpClientHandler();
            var wClient = new HttpClient(wClientHandler);
            wClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            wClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.3");
            wClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Langauge", "es,en-US;q=0.8,en;q=0.6,th;q=0.4");
            wClient.DefaultRequestHeaders.TryAddWithoutValidation("Cache-Control", "max-age=0");
            wClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.1 (KHTML, like Gecko) Chrome/42.0.1180.83 Safari/537.1");
            wClient.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "Keep-Alive");
            wClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", request.UrlReferer);
            wClientHandler.CookieContainer = request.Cookies ?? new CookieContainer();
            var method = HttpMethod.Get;
            switch (request.Method.ToLowerInvariant())
            {
                case "post":
                    method = HttpMethod.Post;
                    break;
                case "put":
                    method = HttpMethod.Put;
                    break;
                case "head":
                    method = HttpMethod.Head;
                    break;
                case "delete":
                    method = HttpMethod.Delete;
                    break;
            }
            var wRequest = new HttpRequestMessage(method, request.RequestUrl);
            if (request.PostData?.Any() == true)
            {
                var buffer = new ByteArrayContent(request.PostData);
                buffer.Headers.ContentType = new MediaTypeHeaderValue(request.ContentType ?? "application/x-www-form-urlencoded");
                wRequest.Content = buffer;
            }

            var wResponse = await wClient.SendAsync(wRequest).ConfigureAwait(false);
            var wResponseContent = await wResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

            return new BrowserResponse
            {
                ContentLength = wResponse.Content.Headers.ContentLength,
                ContentType = wResponse.Content.Headers.ContentType.MediaType,
                ContentEncoding = wResponse.Content.Headers.ContentEncoding.ToString(),
                LastModified = wResponse.Content.Headers.LastModified,
                Cookies = wClientHandler.CookieContainer,
                Headers = wResponse.Headers,
                ContentHeaders = wResponse.Content.Headers,
                Content = wResponseContent,
                ResponseUrl = wResponse.RequestMessage.RequestUri.ToString()
            };
        }

        /// <summary>
        /// Gets a querystring format from a dictionary object
        /// </summary>
        /// <param name="dictionary">Dictionary to convert to a query string format</param>
        /// <returns>A querystring style string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToQueryString(IDictionary<string, string> dictionary)
            => string.Join("&", dictionary.Select(i => string.Format("{0}={1}", i.Key, i.Value)));
    }
}
