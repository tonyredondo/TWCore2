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

using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Threading.Tasks;
using TWCore.Serialization;

namespace TWCore.AspNetCore.Formatters
{
    /// <inheritdoc />
    /// <summary>
    /// OutputFormatter for ISerializer instances
    /// </summary>
    public class OutputSerializerFormatter : OutputFormatter
    {
        private readonly ISerializer _serializer;

        /// <summary>
        /// Serializer
        /// </summary>
        public ISerializer Serializer => _serializer;

        /// <summary>
        /// OutputFormatter for ISerializer instances
        /// </summary>
        /// <param name="serializer">ISerializer instance</param>
        public OutputSerializerFormatter(ISerializer serializer)
        {
            _serializer = serializer;
            foreach (var mime in _serializer.MimeTypes)
                SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(mime));
        }

        /// <summary>
        /// Writes the response body.
        /// </summary>
        /// <param name="context">The formatter context associated with the call.</param>
        /// <returns>A task which can write the response body.</returns>
        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            try
            {
                _serializer.Serialize(context.Object, context.ObjectType, context.HttpContext.Response.Body);
                return Task.FromResult(context.Object);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                return Task.FromException(ex);
            }
        }
    }
}
