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

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using TWCore.Serialization;
using Microsoft.Net.Http.Headers;
using System;

namespace TWCore.Web.Formatters
{
    /// <inheritdoc />
    /// <summary>
    /// OutputFormatter for ISerializer instances
    /// </summary>
    public class OutputSerializerFormatter : OutputFormatter
    {
        private readonly ISerializer _serializer;

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

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            try
            {
                _serializer.Serialize(context.Object, context.ObjectType, context.HttpContext.Response.Body);
                return Task.FromResult(context.Object);
            }
            catch(Exception ex)
            {
                Core.Log.Write(ex);
                return Task.FromException(ex);
            }
        }
    }
}
