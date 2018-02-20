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
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using TWCore.Serialization;
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Net.HttpServer
{
    /// <summary>
    /// Represents a Http Request container and processor
    /// </summary>
    public class HttpRequest
    {
        private const int MaxPostSize = 10 * 1024 * 1024; // 10MB
        private TcpClient _client;
        private HttpContext _context;
        private readonly StreamReader _streamReader;

        #region Properties
        /// <summary>
        /// Client remote address
        /// </summary>
        public string RemoteAddress { get; }
        /// <summary>
        /// Client remote port
        /// </summary>
        public int RemotePort { get; }
        /// <summary>
        /// Http method
        /// </summary>
        public HttpMethod Method { get; private set; }
        /// <summary>
        /// Raw url address
        /// </summary>
        public string RawUrl { get; private set; }
        /// <summary>
        /// Url Address
        /// </summary>
        public Uri Url { get; private set; }
        /// <summary>
        /// Http version
        /// </summary>
        public string HttpVersion { get; private set; }
        /// <summary>
        /// Request headers
        /// </summary>
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        /// <summary>
        /// Query string values collection
        /// </summary>
        public HttpValueCollection QueryString { get; private set; }
        /// <summary>
        /// Request content length
        /// </summary>
        public int ContentLength { get; private set; }
        /// <summary>
        /// Request content type
        /// </summary>
        public string ContentType { get; private set; }
        /// <summary>
        /// Post data byte array
        /// </summary>
        public byte[] PostData { get; private set; }
        /// <summary>
        /// Form value collection
        /// </summary>
        public FormUrlEncodedContentParser Form { get; private set; }
        /// <summary>
        /// Input stream
        /// </summary>
        public Stream InputStream { get; }
        /// <summary>
        /// Has post object
        /// </summary>
        public bool HasPostObject { get; private set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Represents a Http Request container and processor
        /// </summary>
        /// <param name="socketClient">Current socket to handle the request</param>
        /// <param name="context">HttpContext object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal HttpRequest(TcpClient socketClient, HttpContext context)
        {
            _client = socketClient;
            _context = context;
            socketClient.ReceiveTimeout = 120000;
            InputStream = new BufferedStream(socketClient.GetStream(), 16384);
            _streamReader = new StreamReader(InputStream);
            var ipEndPoint = (IPEndPoint)socketClient.Client.RemoteEndPoint;
            RemoteAddress = ipEndPoint.Address.ToString();
            RemotePort = ipEndPoint.Port;
        }
        #endregion

        #region Internal Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Process()
        {
            //Core.Log.LibVerbose("Connection from {0}:{1}", RemoteAddress, RemotePort);

            #region Init
            //Core.Log.LibVerbose("Initializing request...");
            var request = _streamReader.ReadLine();
            if (string.IsNullOrEmpty(request))
                throw new Exception("Invalid http request line");

            //Core.Log.LibVerbose("Request: {0}", request);

            int lastIndex, newIndex;
            newIndex = request.IndexOf(' ', 2);
            if (newIndex < 0)
                throw new Exception("Invalid http request line");
            var strMethod = request.Substring(0, newIndex);
            lastIndex = newIndex + 1;
            newIndex = request.IndexOf(' ', lastIndex);
            if (newIndex < 0)
                throw new Exception("Invalid http request line");
            RawUrl = request.Substring(lastIndex, newIndex - lastIndex);
            HttpVersion = request.Substring(newIndex + 1);

            switch(strMethod)
            {
                case "GET":
                    Method = HttpMethod.GET;
                    break;
                case "POST":
                    Method = HttpMethod.POST;
                    break;
                case "OPTIONS":
                    Method = HttpMethod.OPTIONS;
                    break;
                case "HEAD":
                    Method = HttpMethod.HEAD;
                    break;
                case "PUT":
                    Method = HttpMethod.PUT;
                    break;
                case "DELETE":
                    Method = HttpMethod.DELETE;
                    break;
                case "TRACE":
                    Method = HttpMethod.TRACE;
                    break;
                default:
                    throw new Exception("Http method invalid");
            }
            #endregion

            #region Extract Headers
            //Core.Log.LibVerbose("Extracting headers...");
            string line;
            var hostHeader = false;
            string host = null;
            var contentTypeHeader = false;
            while (!string.IsNullOrEmpty(line = _streamReader.ReadLine()))
            {               
                var separator = line.IndexOf(':', 1);
                if (separator > -1)
                {
                    var name = line.Substring(0, separator);
                    var value = line[separator + 1] == ' ' ? line.Substring(separator + 2) : line.Substring(separator + 1);
                    Headers[name] = value;
                    if (!hostHeader && name == "Host")
                    {
                        host = value;
                        hostHeader = true;
                    }
                    if (!contentTypeHeader && name == "Content-Type")
                    {
                        ContentType = value;
                        contentTypeHeader = true;
                    }
                    //Core.Log.LibVerbose("Header: {0}:{1}", name, value);
                }
                else
                    Core.Log.Warning("Invalid http header line: " + line);
            }
            //Core.Log.LibVerbose("Headers done.");
            #endregion

            #region Extract Post Data
            if (Method == HttpMethod.POST || Method == HttpMethod.PUT)
            {
                HasPostObject = true;
                Core.Log.LibVerbose("Getting Post Data...");
                ContentLength = 0;
                if (Headers.ContainsKey("Content-Length"))
                {
                    ContentLength = Convert.ToInt32(Headers["Content-Length"]);
                    if (ContentLength > MaxPostSize)
                        throw new Exception(string.Format("POST Content-Length({0}) too big for this simple server", ContentLength));
                    if (ContentLength > 0)
                    {
                        var fReader = new BinaryReader(InputStream, Encoding.UTF8, true);
                        PostData = fReader.ReadBytes(ContentLength);
                    }
                }
                Core.Log.LibVerbose("Post Data done.");
            }
            #endregion

            Url = new Uri("http://" + host + RawUrl);
            QueryString = HttpUtility.ParseQueryString(Url.Query);
            Form = ContentType == "application/x-www-form-urlencoded" ? 
                new FormUrlEncodedContentParser(PostData) : 
                new FormUrlEncodedContentParser();

            return true;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the multipart parser of the form data
        /// </summary>
        /// <param name="filePartName">File part name</param>
        /// <returns>Form multipart instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FormMultipartParser GetMultipartParser(string filePartName) => new FormMultipartParser(PostData, filePartName);
        /// <summary>
        /// Gets the object from the Post data
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <returns>Object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetPostObject<T>()
        {
            var serializer = SerializerManager.GetByMimeType(ContentType);
            return serializer != null ? serializer.Deserialize<T>(PostData) : default(T);
        }
        /// <summary>
        /// Gets the object from the Post data
        /// </summary>
        /// <param name="type">Object type</param>
        /// <returns>Object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetPostObject(Type type)
        {
            var serializer = SerializerManager.GetByMimeType(ContentType);
            return serializer?.Deserialize(PostData, type);
        }
        #endregion
    }
}
