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

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TWCore.Net.Multicast;
using TWCore.Serialization;
using TWCore.Web;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ArgumentsStyleStringLiteral

namespace TWCore.Object.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.SetDefaultTWCoreValues();

            // Adds a default in-memory implementation of IDistributedCache.
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.Name = ".TWCore.Object.Api.Session";
                options.Cookie.HttpOnly = true;
            });
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                });
            });
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("AllowAllOrigins"));
            });

            DiscoveryService.RegisterService(DiscoveryService.FrameworkCategory, "OBJECT.API", "TWCore Object Api", new SerializedObject(Core.Settings["WebService.Urls"]));

            DiscoveryService.OnNewServiceReceived += DiscoveryService_OnNewServiceReceived;
            DiscoveryService.OnServiceExpired += DiscoveryService_OnServiceExpired;
        }

        private void DiscoveryService_OnNewServiceReceived(object sender, EventArgs<DiscoveryService.ReceivedService> e)
        {
            var rcvSrv = e.Item1;
            Core.Log.InfoBasic($"New Service: Machine={rcvSrv.MachineName}, Application={rcvSrv.ApplicationName}, Environment={rcvSrv.EnvironmentName}, Name={rcvSrv.Name}, Category={rcvSrv.Category}");
        }
        private void DiscoveryService_OnServiceExpired(object sender, EventArgs<DiscoveryService.ReceivedService> e)
        {
            var rcvSrv = e.Item1;
            Core.Log.InfoBasic($"Expired Service: Machine={rcvSrv.MachineName}, Application={rcvSrv.ApplicationName}, Environment={rcvSrv.EnvironmentName}, Name={rcvSrv.Name}, Category={rcvSrv.Category}");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSession();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseResponseCompression();
            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
