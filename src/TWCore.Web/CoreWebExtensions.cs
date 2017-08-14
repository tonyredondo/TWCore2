﻿/*
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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using TWCore.Serialization;
using TWCore.Web.Logger;

namespace TWCore.Web
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
        /// <param name="serializer">ISerializer instance</param>
        public static MvcOptions AddISerializerOutputFormatter(this MvcOptions options, ISerializer serializer)
        {
            options.OutputFormatters.Add(new Formatters.OutputSerializerFormatter(serializer));
            return options;
        }
        /// <summary>
        /// Adds the ISerializer instance as an InputFormatter
        /// </summary>
        /// <param name="serializer">ISerializer instance</param>
        public static MvcOptions AddISerializerInputFormatter(this MvcOptions options, ISerializer serializer)
        {
            options.InputFormatters.Add(new Formatters.InputSerializerFormatter(serializer));
            return options;
        }
        /// <summary>
        /// Sets the default TWCoreValues
        /// </summary>
        public static void SetDefaultTWCoreValues(this IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                try
                {
                    var serializers = SerializerManager.GetBinarySerializers();
                    foreach (var serializer in serializers)
                    {
                        options.AddISerializerInputFormatter(serializer);
                        options.AddISerializerOutputFormatter(serializer);
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            });
            services.AddLogging(cfg =>
            {
                cfg.AddTWCoreLogger();
            });
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression();
        }
    }
}
