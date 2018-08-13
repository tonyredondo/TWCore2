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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TWCore.Serialization;

// ReSharper disable CollectionNeverUpdated.Global

namespace TWCore.Net
{
    /// <inheritdoc />
    /// <summary>
    /// Http client to handle Rest requests
    /// </summary>
    public class RestClient : IDisposable
    {
        private HttpClient _client;

        #region Properties
        /// <summary>
        /// Serializer used to encode and decode data
        /// </summary>
        public ISerializer Serializer { get; }
        /// <summary>
        /// Default http headers on every request
        /// </summary>
        public Dictionary<string, string> DefaultHeaders { get; } = new Dictionary<string, string>();
        #endregion

        #region .ctor
        /// <summary>
        /// Http client to handle Rest requests
        /// </summary>
        /// <param name="baseUrl">Base url address</param>
        /// <param name="serializer">Serializer used to serialize and deserialize object from request.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RestClient(string baseUrl = null, ISerializer serializer = null)
        {
            var handler = new DecompressionHandler
            {
                InnerHandler = new HttpClientHandler()
            };
            _client = new HttpClient(handler);
            if (baseUrl.IsNotNullOrEmpty())
                _client.BaseAddress = new Uri(baseUrl);
            Serializer = serializer ?? (SerializerManager.GetByMimeType<ITextSerializer>(SerializerMimeTypes.Json) ?? SerializerManager.DefaultTextSerializer);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~RestClient()
        {
            Dispose(false);
        }
        #endregion

        #region Prepare Headers
        /// <summary>
        /// Prepare the default headers on the requests
        /// </summary>
        /// <param name="headers">Additional headers to append on the requests</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PrepareHeaders(Dictionary<string, string> headers)
        {
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Clear();
            Serializer.MimeTypes.Each(i => _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(i)));
            foreach (var item in DefaultHeaders)
                _client.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);

            if (headers == null) return;

            foreach (var item in headers)
            {
                if (_client.DefaultRequestHeaders.Contains(item.Key))
                    _client.DefaultRequestHeaders.Remove(item.Key);
                _client.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);
            }
        }
        #endregion



        #region HeadAsync
        /// <summary>
        /// Sends a HEAD request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RestClientResponse> HeadAsync(string requestUri, object headers)
            => HeadAsync(requestUri, headers.ToDictionary().ToDictionary(k => k.Key, v => v.Value?.ToString()));
        /// <summary>
        /// Sends a HEAD request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RestClientResponse> HeadAsync(string requestUri, Dictionary<string, string> headers = null)
        {
            PrepareHeaders(headers);
            Core.Log.LibVerbose("Sending HEAD request to {0}", requestUri);
            var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Head, requestUri)).ConfigureAwait(false);
            return await HandleResponseMessageAsync(response).ConfigureAwait(false);
        }
        /// <summary>
        /// Sends a HEAD request to the specified url with the additional headers if are available
        /// </summary>
        /// <typeparam name="T">Type of object to expect</typeparam>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RestClientResponse<T>> HeadAsync<T>(string requestUri, object headers)
            => HeadAsync<T>(requestUri, headers.ToDictionary().ToDictionary(k => k.Key, v => v.Value?.ToString()));
        /// <summary>
        /// Sends a HEAD request to the specified url with the additional headers if are available
        /// </summary>
        /// <typeparam name="T">Type of object to expect</typeparam>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RestClientResponse<T>> HeadAsync<T>(string requestUri, Dictionary<string, string> headers = null)
        {
            var response = await HeadAsync(requestUri, headers).ConfigureAwait(false);
            return GetResponseObject<T>(response);
        }
        /// <summary>
        /// Sends a HEAD request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="responseType">Response object type</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RestClientResponse<object>> HeadAsync(string requestUri, Type responseType, object headers)
            => HeadAsync(requestUri, responseType, headers.ToDictionary().ToDictionary(k => k.Key, v => v.Value?.ToString()));
        /// <summary>
        /// Sends a HEAD request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="responseType">Response object type</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RestClientResponse<object>> HeadAsync(string requestUri, Type responseType, Dictionary<string, string> headers = null)
        {
            var response = await HeadAsync(requestUri, headers).ConfigureAwait(false);
            return GetResponseObject(response, responseType);
        }
        #endregion

        #region GetAsync
        /// <summary>
        /// Sends a GET request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RestClientResponse> GetAsync(string requestUri, object headers)
            => GetAsync(requestUri, headers.ToDictionary().ToDictionary(k => k.Key, v => v.Value?.ToString()));
        /// <summary>
        /// Sends a GET request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RestClientResponse> GetAsync(string requestUri, Dictionary<string, string> headers = null)
        {
            PrepareHeaders(headers);
            Core.Log.LibVerbose("Sending GET request to {0}", requestUri);
            var response = await _client.GetAsync(requestUri).ConfigureAwait(false);
            return await HandleResponseMessageAsync(response).ConfigureAwait(false);
        }
        /// <summary>
        /// Sends a GET request to the specified url with the additional headers if are available
        /// </summary>
        /// <typeparam name="T">Type of object to expect</typeparam>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RestClientResponse<T>> GetAsync<T>(string requestUri, object headers)
            => GetAsync<T>(requestUri, headers.ToDictionary().ToDictionary(k => k.Key, v => v.Value?.ToString()));
        /// <summary>
        /// Sends a GET request to the specified url with the additional headers if are available
        /// </summary>
        /// <typeparam name="T">Type of object to expect</typeparam>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RestClientResponse<T>> GetAsync<T>(string requestUri, Dictionary<string, string> headers = null)
        {
            var response = await GetAsync(requestUri, headers).ConfigureAwait(false);
            return GetResponseObject<T>(response);
        }
        /// <summary>
        /// Sends a GET request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="responseType">Response object type</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RestClientResponse<object>> GetAsync(string requestUri, Type responseType, object headers)
            => GetAsync(requestUri, responseType, headers.ToDictionary().ToDictionary(k => k.Key, v => v.Value?.ToString()));
        /// <summary>
        /// Sends a GET request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="responseType">Response object type</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RestClientResponse<object>> GetAsync(string requestUri, Type responseType, Dictionary<string, string> headers = null)
        {
            var response = await GetAsync(requestUri, headers).ConfigureAwait(false);
            return GetResponseObject(response, responseType);
        }
        #endregion

        #region DeleteAsync
        /// <summary>
        /// Sends a DELETE request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RestClientResponse> DeleteAsync(string requestUri, object headers)
            => DeleteAsync(requestUri, headers.ToDictionary().ToDictionary(k => k.Key, v => v.Value?.ToString()));
        /// <summary>
        /// Sends a DELETE request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RestClientResponse> DeleteAsync(string requestUri, Dictionary<string, string> headers = null)
        {
            PrepareHeaders(headers);
            Core.Log.LibVerbose("Sending DELETE request to {0}", requestUri);
            var response = await _client.DeleteAsync(requestUri).ConfigureAwait(false);
            return await HandleResponseMessageAsync(response).ConfigureAwait(false);
        }
        /// <summary>
        /// Sends a DELETE request to the specified url with the additional headers if are available
        /// </summary>
        /// <typeparam name="T">Type of object to expect</typeparam>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RestClientResponse<T>> DeleteAsync<T>(string requestUri, object headers)
            => DeleteAsync<T>(requestUri, headers.ToDictionary().ToDictionary(k => k.Key, v => v.Value?.ToString()));
        /// <summary>
        /// Sends a DELETE request to the specified url with the additional headers if are available
        /// </summary>
        /// <typeparam name="T">Type of object to expect</typeparam>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RestClientResponse<T>> DeleteAsync<T>(string requestUri, Dictionary<string, string> headers = null)
        {
            var response = await DeleteAsync(requestUri, headers).ConfigureAwait(false);
            return GetResponseObject<T>(response);
        }
        /// <summary>
        /// Sends a DELETE request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="responseType">Response object type</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RestClientResponse<object>> DeleteAsync(string requestUri, Type responseType, object headers)
            => DeleteAsync(requestUri, responseType, headers.ToDictionary().ToDictionary(k => k.Key, v => v.Value?.ToString()));
        /// <summary>
        /// Sends a DELETE request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="responseType">Response object type</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RestClientResponse<object>> DeleteAsync(string requestUri, Type responseType, Dictionary<string, string> headers = null)
        {
            var response = await DeleteAsync(requestUri, headers).ConfigureAwait(false);
            return GetResponseObject(response, responseType);
        }
        #endregion

        #region PutAsync
        /// <summary>
        /// Sends a PUT request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="data">Data to be serialized and send to the url</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RestClientResponse> PutAsync(string requestUri, object data, object headers)
            => PutAsync(requestUri, data, headers.ToDictionary().ToDictionary(k => k.Key, v => v.Value?.ToString()));
        /// <summary>
        /// Sends a PUT request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="data">Data to be serialized and send to the url</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RestClientResponse> PutAsync(string requestUri, object data, Dictionary<string, string> headers = null)
        {
            PrepareHeaders(headers);
            var stream = data != null ? Serializer.Serialize(data, data.GetType()).AsReadOnlyStream() : Stream.Null;
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(Serializer.MimeTypes[0]);
            Core.Log.LibVerbose("Sending PUT request to {0} with a data length of {1} bytes", requestUri, stream.Length);
            var response = await _client.PutAsync(requestUri, streamContent).ConfigureAwait(false);
            return await HandleResponseMessageAsync(response).ConfigureAwait(false);
        }
        /// <summary>
        /// Sends a PUT request to the specified url with the additional headers if are available
        /// </summary>
        /// <typeparam name="T">Type of object to expect</typeparam>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="data">Data to be serialized and send to the url</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RestClientResponse<T>> PutAsync<T>(string requestUri, object data, object headers)
            => PutAsync<T>(requestUri, data, headers.ToDictionary().ToDictionary(k => k.Key, v => v.Value?.ToString()));
        /// <summary>
        /// Sends a PUT request to the specified url with the additional headers if are available
        /// </summary>
        /// <typeparam name="T">Type of object to expect</typeparam>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="data">Data to be serialized and send to the url</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RestClientResponse<T>> PutAsync<T>(string requestUri, object data, Dictionary<string, string> headers = null)
        {
            var response = await PutAsync(requestUri, data, headers).ConfigureAwait(false);
            return GetResponseObject<T>(response);
        }
        /// <summary>
        /// Sends a PUT request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="data">Data to be serialized and send to the url</param>
        /// <param name="responseType">Response object type</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RestClientResponse<object>> PutAsync(string requestUri, object data, Type responseType, object headers)
            => PutAsync(requestUri, data, responseType, headers.ToDictionary().ToDictionary(k => k.Key, v => v.Value?.ToString()));
        /// <summary>
        /// Sends a PUT request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="data">Data to be serialized and send to the url</param>
        /// <param name="responseType">Response object type</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RestClientResponse<object>> PutAsync(string requestUri, object data, Type responseType, Dictionary<string, string> headers = null)
        {
            var response = await PutAsync(requestUri, data, headers).ConfigureAwait(false);
            return GetResponseObject(response, responseType);
        }
        #endregion

        #region PostAsync
        /// <summary>
        /// Sends a POST request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="data">Data to be serialized and send to the url</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RestClientResponse> PostAsync(string requestUri, object data, object headers)
            => PostAsync(requestUri, data, headers.ToDictionary().ToDictionary(k => k.Key, v => v.Value?.ToString()));
        /// <summary>
        /// Sends a POST request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="data">Data to be serialized and send to the url</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RestClientResponse> PostAsync(string requestUri, object data, Dictionary<string, string> headers = null)
        {
            PrepareHeaders(headers);
            var stream = data != null ? Serializer.Serialize(data, data.GetType()).AsReadOnlyStream() : Stream.Null;
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(Serializer.MimeTypes[0]);
            Core.Log.LibVerbose("Sending POST request to {0} with a data length of {1} bytes", requestUri, stream.Length);
            var response = await _client.PutAsync(requestUri, streamContent).ConfigureAwait(false);
            return await HandleResponseMessageAsync(response).ConfigureAwait(false);
        }
        /// <summary>
        /// Sends a POST request to the specified url with the additional headers if are available
        /// </summary>
        /// <typeparam name="T">Type of object to expect</typeparam>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="data">Data to be serialized and send to the url</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RestClientResponse<T>> PostAsync<T>(string requestUri, object data, object headers)
            => PostAsync<T>(requestUri, data, headers.ToDictionary().ToDictionary(k => k.Key, v => v.Value?.ToString()));
        /// <summary>
        /// Sends a POST request to the specified url with the additional headers if are available
        /// </summary>
        /// <typeparam name="T">Type of object to expect</typeparam>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="data">Data to be serialized and send to the url</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RestClientResponse<T>> PostAsync<T>(string requestUri, object data, Dictionary<string, string> headers = null)
        {
            var response = await PostAsync(requestUri, data, headers).ConfigureAwait(false);
            return GetResponseObject<T>(response);
        }

        /// <summary>
        /// Sends a POST request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="data">Data to be serialized and send to the url</param>
        /// <param name="responseType">Response object type</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RestClientResponse<object>> PostAsync(string requestUri, object data, Type responseType, object headers)
            => PostAsync(requestUri, data, responseType, headers.ToDictionary().ToDictionary(k => k.Key, v => v.Value?.ToString()));
        /// <summary>
        /// Sends a POST request to the specified url with the additional headers if are available
        /// </summary>
        /// <param name="requestUri">Request url to make the request</param>
        /// <param name="data">Data to be serialized and send to the url</param>
        /// <param name="responseType">Response object type</param>
        /// <param name="headers">Additional headers to append the default ones, null for no additional headers</param>
        /// <returns>Response object for the sent request</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RestClientResponse<object>> PostAsync(string requestUri, object data, Type responseType, Dictionary<string, string> headers = null)
        {
            var response = await PostAsync(requestUri, data, headers).ConfigureAwait(false);
            return GetResponseObject(response, responseType);
        }
        #endregion



        #region Private Methods
        /// <summary>
        /// Handle the response message for a request
        /// </summary>
        /// <param name="response">Response message</param>
        /// <returns>RestClientResponse object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<RestClientResponse> HandleResponseMessageAsync(HttpResponseMessage response)
        {
            Core.Log.LibVerbose("Reading the response data from: {0}", response.RequestMessage.RequestUri);
            var respObj = new RestClientResponse
            {
                IsSuccessStatusCode = response.IsSuccessStatusCode,
                ReasonPhrase = response.ReasonPhrase,
                StatusCode = response.StatusCode,
                RequestUri = response.RequestMessage.RequestUri,
                ValueInBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false)
            };
            if (!respObj.IsSuccessStatusCode)
            {
                SerializableHttpError serHttpError = null;
                if (respObj.ValueInBytes?.Length > 0)
                {
                    if (Serializer.MimeTypes.Contains(response.Content.Headers.ContentType.MediaType))
                    {
                        try
                        {
                            serHttpError = (SerializableHttpError)Serializer.Deserialize(respObj.ValueInBytes, typeof(SerializableHttpError));
                        }
                        catch(Exception ex)
                        {
                            Core.Log.Write(ex);
                        }
                    }
                    else if (response.Content.Headers.ContentType.MediaType == "text/plain")
                    {
                        serHttpError = new SerializableHttpError { Message = Encoding.UTF8.GetString(respObj.ValueInBytes)?.RemoveInvalidXmlChars() };
                        serHttpError.ExceptionMessage = serHttpError.Message;
                    }
                }
                SerializableException sEx = null;
                if (serHttpError != null)
                {
                    sEx = new SerializableException
                    {
                        Message = serHttpError.Message?.RemoveInvalidXmlChars(),
                        StackTrace = serHttpError.StackTrace,
                        ExceptionType = serHttpError.ExceptionType
                    };
                }

                var responseText = (respObj.ValueInBytes?.Length > 0) ? Encoding.UTF8.GetString(respObj.ValueInBytes)?.RemoveInvalidXmlChars() : string.Empty;
                var rce = new RestClientException(responseText, sEx?.GetException())
                {
                    RequestUri = respObj.RequestUri,
                    StatusCode = respObj.StatusCode,
                    ReasonPhrase = respObj.ReasonPhrase,
                    ResponseBytes = respObj.ValueInBytes,
                    ResponseText = responseText,
                    ServerException = serHttpError
                };
                respObj.Exception = rce;
            }
            Core.Log.LibVerbose("Object response created.");
            return respObj;
        }
        /// <summary>
        /// Deserialize a RestClientResponse byte values to a object
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize</typeparam>
        /// <param name="response">RestClientResponse object to being deserialized</param>
        /// <returns>RestClientResponse with a typed value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private RestClientResponse<T> GetResponseObject<T>(RestClientResponse response)
        {
            try
            {
                if (!response.IsSuccessStatusCode && response.Exception != null)
                    throw response.Exception;
                Core.Log.LibVerbose("Deserializing response byte array to an object type.");
                return (response.ValueInBytes?.Length > 0) ?
                    new RestClientResponse<T>(response, Serializer.Deserialize<T>(response.ValueInBytes)) :
                    new RestClientResponse<T>(response, default(T));
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                if (response.IsSuccessStatusCode) throw;
                return new RestClientResponse<T>(response, default(T));
            }
        }
        /// <summary>
        /// Deserialize a RestClientResponse byte values to a object
        /// </summary>
        /// <param name="response">RestClientResponse object to being deserialized</param>
        /// <param name="responseType">Response object type</param>
        /// <returns>RestClientResponse with a typed value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private RestClientResponse<object> GetResponseObject(RestClientResponse response, Type responseType)
        {
            try
            {
                if (!response.IsSuccessStatusCode && response.Exception != null)
                    throw response.Exception;
                Core.Log.LibVerbose("Deserializing response byte array to an object type.");
                return (response.ValueInBytes?.Length > 0) ?
                    new RestClientResponse<object>(response, Serializer.Deserialize(response.ValueInBytes, responseType)) :
                    new RestClientResponse<object>(response, null);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                if (response.IsSuccessStatusCode) throw;
                return new RestClientResponse<object>(response, null);
            }
        }
        #endregion

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls
        /// <summary>
        /// Dispose all object resources
        /// </summary>
        /// <param name="disposing">true if a managed object is needed to be disposed</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
                _client.Dispose();
                _client = null;
            }
            _disposedValue = true;
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose all object resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
