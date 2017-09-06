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
using TWCore.IO;

namespace TWCore.Web.Formatters
{
    /// <inheritdoc />
    /// <summary>
    /// InputFormatter for ISerializer instances
    /// </summary>
    public class InputSerializerFormatter : InputFormatter
    {
        private readonly ISerializer _serializer;

        /// <summary>
        /// InputFormatter for ISerializer instances
        /// </summary>
        /// <param name="serializer">ISerializer instance</param>
        public InputSerializerFormatter(ISerializer serializer)
        {
            _serializer = serializer;
            foreach (var mime in _serializer.MimeTypes)
                SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(mime));
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            try
            {
                var origin = context.HttpContext.Request.Body;
                using (var rms = new RecycleMemoryStream())
                {
                    await origin.CopyToAsync(rms).ConfigureAwait(false);
                    rms.Position = 0;
                    var obj = _serializer.Deserialize(rms, context.ModelType);
                    return InputFormatterResult.Success(obj);
                }
            }
            catch(Exception ex)
            {
                Core.Log.Write(ex);
                return InputFormatterResult.Failure();
            }
        }
    }
}
