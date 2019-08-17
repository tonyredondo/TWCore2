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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using TWCore.AspNetCore.Logger;
using TWCore.Serialization;

namespace TWCore.AspNetCore
{
    /// <summary>
    /// TWCore Web Extensions
    /// </summary>
    public static class CoreWebExtensions
    {
        /// <summary>
        /// Adds the TWCore logger provider for Core logging system
        /// </summary>
        public static ILoggerFactory AddTWCoreLogger(this ILoggerFactory loggerFactory)
        {
            loggerFactory.AddProvider(new TWCoreLoggerProvider());
            return loggerFactory;
        }
        /// <summary>
        /// Adds the TWCore logger provider for Core logging system
        /// </summary>
        public static ILoggingBuilder AddTWCoreLogger(this ILoggingBuilder loggerBuilder)
        {
            loggerBuilder.AddProvider(new TWCoreLoggerProvider());
            return loggerBuilder;
        }
        /// <summary>
        /// Adds the ISerializer instance as an OutputFormatter
        /// </summary>
        /// <param name="options">MvcOptions instance</param>
        /// <param name="serializer">ISerializer instance</param>
        public static MvcOptions AddISerializerOutputFormatter(this MvcOptions options, ISerializer serializer)
        {
            options.OutputFormatters.Add(new Formatters.OutputSerializerFormatter(serializer));
            return options;
        }
        /// <summary>
        /// Adds the ISerializer instance as an InputFormatter
        /// </summary>
        /// <param name="options">MvcOptions instance</param>
        /// <param name="serializer">ISerializer instance</param>
        public static MvcOptions AddISerializerInputFormatter(this MvcOptions options, ISerializer serializer)
        {
            options.InputFormatters.Add(new Formatters.InputSerializerFormatter(serializer));
            return options;
        }
        /// <summary>
        /// Sets the default TWCoreValues
        /// </summary>
        public static void SetDefaultTWCoreValues(this IServiceCollection services, CompatibilityVersion compatibilityVersion = CompatibilityVersion.Version_3_0, CoreWebSettings settings = null)
        {
            settings = settings ?? new CoreWebSettings();
            services.AddMvc(options =>
            {
                try
                {
                    if (settings.UseCustomXmlSerializer)
                    {
                        var xmlSerializer = new XmlTextSerializer();
                        options.AddISerializerInputFormatter(xmlSerializer);
                        options.AddISerializerOutputFormatter(xmlSerializer);
                    }
                    else
                    {
                        options.InputFormatters.Add(new XmlSerializerInputFormatter(options));
                        options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                    }

                    if (settings.EnableFormatMapping)
                    {
                        options.FormatterMappings.SetMediaTypeMappingForFormat
                            ("xml", MediaTypeHeaderValue.Parse("application/xml"));
                        options.FormatterMappings.SetMediaTypeMappingForFormat
                            ("js", MediaTypeHeaderValue.Parse("application/json"));
                    }

                    if (settings.EnableTWCoreSerializers)
                    {
                        var serializers = SerializerManager.GetBinarySerializers();
                        foreach (var serializer in serializers)
                        {
                            options.AddISerializerInputFormatter(serializer);
                            options.AddISerializerOutputFormatter(serializer);
                            if (settings.EnableFormatMapping)
                            {
                                options.FormatterMappings.SetMediaTypeMappingForFormat(serializer.Extensions[0].Substring(1),
                                    MediaTypeHeaderValue.Parse(serializer.MimeTypes[0]));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }).SetCompatibilityVersion(compatibilityVersion)
            .AddJsonOptions(options =>
            {
                if (settings.EnableJsonStringEnum)
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });
            services.AddLogging(cfg =>
            {
                if (settings.EnableTWCoreLogger)
                    cfg.AddTWCoreLogger();
            });
            if (settings.EnableGZipCompressor)
            {
                services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Fastest);
                services.AddResponseCompression();
                if (settings.EnableTWCoreSerializers)
                {
                    var serializers = SerializerManager.GetBinarySerializers();

                    services.Configure<ResponseCompressionOptions>(options =>
                    {
                        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(serializers.SelectMany(i => i.MimeTypes));
                    });
                }
            }
        }
    }

}
