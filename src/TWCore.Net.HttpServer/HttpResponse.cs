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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using TWCore.IO;
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable InvertIf
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable NotAccessedField.Local
// ReSharper disable InconsistentNaming

namespace TWCore.Net.HttpServer
{
    /// <summary>
    /// Represents a Http Response container and processor
    /// </summary>
    public class HttpResponse
    {
        private static readonly Dictionary<HttpStatusCode, (string, string)> StatusCodeResponses = new Dictionary<HttpStatusCode, (string, string)>
        {
            [HttpStatusCode.OK] = ("200", "OK"),
            [HttpStatusCode.Created] = ("201", "Created"),
            [HttpStatusCode.Accepted] = ("202", "Accepted"),
            [HttpStatusCode.No_Content] = ("204", "No Content"),
            [HttpStatusCode.Reset_Content] = ("205", "Reset Content"),
            [HttpStatusCode.Moved_Permanently] = ("301", "Moved Permanently"),
            [HttpStatusCode.Found] = ("302", "Found"),
            [HttpStatusCode.Not_Modified] = ("304", "Not Modified"),
            [HttpStatusCode.Bad_Request] = ("400", "Bad Request"),
            [HttpStatusCode.Unauthorized] = ("401", "Unauthorized"),
            [HttpStatusCode.Payment_Required] = ("402", "Payment Required"),
            [HttpStatusCode.Forbidden] = ("403", "Forbidden"),
            [HttpStatusCode.Not_Found] = ("404", "Not_Found"),
            [HttpStatusCode.Method_Not_Allowed] = ("405", "Method Not Allowed"),
            [HttpStatusCode.Not_Acceptable] = ("406", "Not Acceptable"),
            [HttpStatusCode.Request_Timeout] = ("408", "Request Timeout"),
            [HttpStatusCode.Conflict] = ("409", "Conflict"),
            [HttpStatusCode.Gone] = ("410", "Gone"),
            [HttpStatusCode.PreconditionFailed] = ("412", "PreconditionFailed"),
            [HttpStatusCode.Unsupported_Media_Type] = ("415", "Unsupported Media Type"),
            [HttpStatusCode.Too_Many_Requests] = ("429", "Too Many Requests"),
            [HttpStatusCode.Internal_Server_Error] = ("500", "Internal Server Error"),
            [HttpStatusCode.Not_Implemented] = ("501", "Not Implemented"),
            [HttpStatusCode.Service_Unavailable] = ("503", "Service Unavailable")
        };
        internal readonly EventStream EventStream;
        private readonly HttpContext _context;
        private TcpClient _client;
        private bool _headersSent;

        #region Properties
        /// <summary>
        /// Response headers
        /// </summary>
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        /// <summary>
        /// Http version
        /// </summary>
        public HttpVersion Version { get; set; }
        /// <summary>
        /// Http status code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }
        /// <summary>
        /// Response content type
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// Output stream
        /// </summary>
        public Stream OutputStream => EventStream;
        #endregion

        #region .ctor
        /// <summary>
        /// Represents a Http Response container and processor
        /// </summary>
        /// <param name="socketClient">Current socket to handle the request</param>
        /// <param name="context">HttpContext object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal HttpResponse(TcpClient socketClient, HttpContext context)
        {
            _client = socketClient;
            EventStream = new EventStream(new BufferedStream(socketClient.GetStream(), 16384));
            EventStream.BeforeWrite += (s, e) => WriteHeaders();
            _context = context;
            Version = HttpVersion.Version1_0;
            StatusCode = HttpStatusCode.OK;
            ContentType = "text/html";
            Headers.Add("Connection", "close");
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteHeaders()
        {
            if (_headersSent || !OutputStream.CanWrite) return;

            //Core.Log.LibVerbose("Writing Response Headers...");
            var useGZip = false;
            var useDeflate = false;

            if (_context.Request.Headers.TryGetValue("Accept-Encoding", out var acceptEncoding))
            {
                var encodings = acceptEncoding.SplitAndTrim(',');
                for (var i = 0; i < encodings.Length; i++)
                {
                    if (string.Equals(encodings[i], "gzip", StringComparison.OrdinalIgnoreCase))
                    {
                        useGZip = true;
                        Headers["Content-Encoding"] = encodings[i];
                        break;
                    }
                    if (string.Equals(encodings[i], "deflate", StringComparison.OrdinalIgnoreCase))
                    {
                        useDeflate = true;
                        Headers["Content-Encoding"] = encodings[i];
                        break;
                    }
                }
            }
            var version = Version == HttpVersion.Version1_0 ? "1.0" : Version == HttpVersion.Version1_1 ? "1.1" : "1.0";
            var status = StatusCodeResponses[StatusCode];
            EventStream.BaseStream.WriteLine(string.Concat("HTTP/", version, " ", status.Item1, " ", status.Item2));
            EventStream.BaseStream.WriteLine("Date: " + Core.Now.ToString("R"));
            EventStream.BaseStream.WriteLine("Content-Type: " + ContentType);
            foreach (var header in Headers)
            {
                if (header.Key == "Date")
                    continue;
                if (header.Key == "Content-Type")
                    continue;
                EventStream.BaseStream.WriteLine(header.Key + ": " + header.Value);
            }
            EventStream.BaseStream.WriteLine("");
            if (useGZip)
                EventStream.BaseStream = new GZipStream(EventStream.BaseStream, CompressionMode.Compress, false);
            else if (useDeflate)
                EventStream.BaseStream = new DeflateStream(EventStream.BaseStream, CompressionMode.Compress, false);
            _headersSent = true;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Writes a stream to the response
        /// </summary>
        /// <param name="stream">Stream to be written on the response</param>
        /// <param name="bufferLength">Buffer read length</param>
        /// <param name="timeOutToReadBytes">Timeout to read and write all the bytes</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Stream stream, int bufferLength = 4096, int timeOutToReadBytes = 0)
        {
            //Core.Log.LibVerbose("Writing a stream to the Response stream using a buffer of {0} bytes", bufferLength);
            stream.WriteToStream(OutputStream, 0, bufferLength, timeOutToReadBytes);
        }
        /// <summary>
        /// Writes a byte array to the response
        /// </summary>
        /// <param name="buffer">Byte array to write on the response</param>
        /// <param name="offset">Initial index of the byte array</param>
        /// <param name="count">Number of bytes to write</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] buffer, int offset, int count)
        {
            if (buffer != null)
            {
                //Core.Log.LibVerbose("Writing a byte array on the Response stream [{0}bytes]", count - offset);
                OutputStream.Write(buffer, offset, count);
            }
        }
        /// <summary>
        /// Writes a byte array to the response
        /// </summary>
        /// <param name="buffer">Byte array to write on the response</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }
        /// <summary>
        /// Writes a text value to the response
        /// </summary>
        /// <param name="content">String value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string content)
        {
            //Core.Log.LibVerbose("Writing text on the Response stream.");
            OutputStream.WriteText(content);
        }
        /// <summary>
        /// Writes a string line value to the response
        /// </summary>
        /// <param name="content">String value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLine(string content)
        {
            //Core.Log.LibVerbose("Writing line on the Response stream.");
            OutputStream.WriteLine(content);
        }
        #endregion

        #region Enums
        /// <summary>
        /// Http response version
        /// </summary>
        public enum HttpVersion
        {
            /// <summary>
            /// Http version 1.0
            /// </summary>
            Version1_0,
            /// <summary>
            /// Http version 1.1
            /// </summary>
            Version1_1
        }
        /// <summary>
        /// Http response status code
        /// </summary>
        public enum HttpStatusCode
        {
            /// <summary>
            /// Http 200 OK
            /// </summary>
            OK = 200,
            /// <summary>
            /// Http 201 Created
            /// </summary>
            Created = 201,
            /// <summary>
            /// Http 202 Accepted
            /// </summary>
            Accepted = 202,
            /// <summary>
            /// Http 204 No Content
            /// </summary>
            No_Content = 204,
            /// <summary>
            /// Http 205 Reset Content
            /// </summary>
            Reset_Content = 205,
            /// <summary>
            /// Http 301 Moved Permanently
            /// </summary>
            Moved_Permanently = 301,
            /// <summary>
            /// Http 302 Found
            /// </summary>
            Found = 302,
            /// <summary>
            /// Http 304 Not Modified
            /// </summary>
            Not_Modified = 304,
            /// <summary>
            /// Http 400 Bad Request
            /// </summary>
            Bad_Request = 400,
            /// <summary>
            /// Http 401 Unauthorized
            /// </summary>
            Unauthorized = 401,
            /// <summary>
            /// Http 402 Payment Required
            /// </summary>
            Payment_Required = 402,
            /// <summary>
            /// Http 403 Forbidden
            /// </summary>
            Forbidden = 403,
            /// <summary>
            /// Http 404 Not Found
            /// </summary>
            Not_Found = 404,
            /// <summary>
            /// Http 405 Method Not Allowed
            /// </summary>
            Method_Not_Allowed = 405,
            /// <summary>
            /// Http 406 Not Acceptable
            /// </summary>
            Not_Acceptable = 406,
            /// <summary>
            /// Http 408 Request Timeout
            /// </summary>
            Request_Timeout = 408,
            /// <summary>
            /// Http 409 Conflict
            /// </summary>
            Conflict = 409,
            /// <summary>
            /// Http 410 Gone
            /// </summary>
            Gone = 410,
            /// <summary>
            /// Http 412 Precondition Failed
            /// </summary>
            PreconditionFailed = 412,
            /// <summary>
            /// Http 415 Unsupported Media Type
            /// </summary>
            Unsupported_Media_Type = 415,
            /// <summary>
            /// Http 429 Too Many Requests
            /// </summary>
            Too_Many_Requests = 429,
            /// <summary>
            /// Http 500 Internal Server Error
            /// </summary>
            Internal_Server_Error = 500,
            /// <summary>
            /// Http 501 Not Implemented
            /// </summary>
            Not_Implemented = 501,
            /// <summary>
            /// Http 503 Service Unavailable
            /// </summary>
            Service_Unavailable = 503
        }
        #endregion
    }
}
