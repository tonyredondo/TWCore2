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

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using TWCore.Diagnostics.Status;

#if NETCOREAPP3_1_OR_GREATER
using Microsoft.Extensions.Hosting;
#endif

// ReSharper disable UnusedMember.Global
// ReSharper disable CheckNamespace

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// AspNet Web Service
    /// </summary>
    [StatusName("Web Service")]
    public class WebService : SimpleServiceAsync
    {
        /// <summary>
        /// Web Service Default Settings
        /// </summary>
        public static WebServiceSettings Settings { get; } = Core.GetSettings<WebServiceSettings>();

#if NETCOREAPP3_1_OR_GREATER
        private readonly Func<string[], IHost> _hostFactory;
        private IWebHostEnvironment _webHostEnvironment;
#endif
        private readonly Func<string[], IWebHost> _webHostFactory;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;
        private ICollection<string> _serverAddresses;

        #region .ctor
#if NETCOREAPP3_1_OR_GREATER
        /// <inheritdoc />
        /// <summary>
        /// AspNet Web Service
        /// </summary>
        /// <param name="hostFactory">Host Builder factory delegate</param>
        public WebService(Func<string[], IHost> hostFactory)
        {
            _hostFactory = hostFactory;
            Core.Status.Attach(collections =>
            {
                if (_hostingEnvironment != null)
                {
                    collections.Add("Environment Name", _hostingEnvironment.EnvironmentName);
                    collections.Add("Content Root Path", _hostingEnvironment.ContentRootPath);
                    collections.Add("Web Root Path", _hostingEnvironment.WebRootPath);
                }
                if (_serverAddresses != null)
                    collections.Add("Addresses", _serverAddresses.Join(", "));
            }, this);
        }
#endif

        /// <inheritdoc />
        /// <summary>
        /// AspNet Web Service
        /// </summary>
        /// <param name="webHostFactory">Host Builder factory delegate</param>
        public WebService(Func<string[], IWebHost> webHostFactory)
        {
            _webHostFactory = webHostFactory;
            Core.Status.Attach(collections =>
            {
                if (_hostingEnvironment != null)
                {
                    collections.Add("Environment Name", _hostingEnvironment.EnvironmentName);
                    collections.Add("Content Root Path", _hostingEnvironment.ContentRootPath);
                    collections.Add("Web Root Path", _hostingEnvironment.WebRootPath);
                }
                if (_serverAddresses != null)
                    collections.Add("Addresses", _serverAddresses.Join(", "));
            }, this);
        }
        #endregion

        #region Statics
#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Create WebService with default Host
        /// </summary>
        /// <typeparam name="TStartUp">StartuUp class</typeparam>
        /// <returns>WebService default instance</returns>
        public static WebService CreateHost<TStartUp>() where TStartUp : class
        {
            if (Settings?.Urls?.Any() == true)
            {
                return new WebService(args => Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(builder =>
                    {
                        builder.UseStartup<TStartUp>()
                            .UseUrls(Settings.Urls);
                    }).Build());
            }

            return new WebService(args =>
            {
                return Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(builder => builder.UseStartup<TStartUp>())
                    .Build();
            });
        }
#endif
        
        /// <summary>
        /// Create WebService with default WebHost
        /// </summary>
        /// <typeparam name="TStartUp">StartuUp class</typeparam>
        /// <returns>WebService default instance</returns>
#if NETCOREAPP3_1_OR_GREATER
        [Obsolete("This method uses the obsolete WebHost. The recommended alternative is to use CreateHost")]
#endif
        public static WebService Create<TStartUp>() where TStartUp : class
        {
            if (Settings?.Urls?.Any() == true)
            {
                return new WebService(args => WebHost.CreateDefaultBuilder(args)
                    .UseStartup<TStartUp>()
                    .UseUrls(Settings.Urls)
                    .Build());
            }

            return new WebService(args => WebHost.CreateDefaultBuilder(args)
                .UseStartup<TStartUp>()
                .Build());
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Create WebService with default WebHost
        /// </summary>
        /// <typeparam name="TStartUp">StartuUp class</typeparam>
        /// <param name="builder">Builder extensions</param>
        /// <returns>WebService default instance</returns>
        public static WebService Create<TStartUp>(Func<IHostBuilder, IHostBuilder> builder) where TStartUp : class
        {
            if (builder is null)
                return Create<TStartUp>();
            if (Settings?.Urls?.Any() == true)
                return new WebService(args =>
                {
                    var hostBuilder = Host.CreateDefaultBuilder(args)
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.UseStartup<TStartUp>()
                                .UseUrls(Settings.Urls);
                        });
                    hostBuilder = builder(hostBuilder);
                    return hostBuilder.Build();
                });

            return new WebService(args =>
            {
                var hostBuilder = Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<TStartUp>());
                hostBuilder = builder(hostBuilder);
                return hostBuilder.Build();
            });
        }
#else
        /// <summary>
        /// Create WebService with default WebHost
        /// </summary>
        /// <typeparam name="TStartUp">StartuUp class</typeparam>
        /// <param name="builder">Builder extensions</param>
        /// <returns>WebService default instance</returns>
        public static WebService Create<TStartUp>(Func<IWebHostBuilder, IWebHostBuilder> builder) where TStartUp : class
        {
            if (builder is null)
                return Create<TStartUp>();
            if (Settings?.Urls?.Any() == true)
                return new WebService(args =>
                {
                    var wbuilder = WebHost.CreateDefaultBuilder(args)
                        .UseStartup<TStartUp>()
                        .UseUrls(Settings.Urls);
                    wbuilder = builder(wbuilder);
                    return wbuilder.Build();
                });
            return new WebService(args =>
            {
                var wbuilder = WebHost.CreateDefaultBuilder(args)
                        .UseStartup<TStartUp>();
                wbuilder = builder(wbuilder);
                return wbuilder.Build();
            });
        }
#endif
        #endregion

        /// <summary>
        /// On Action async method.
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Action task</returns>
        protected override async Task OnActionAsync(CancellationToken token)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (_hostFactory is not null)
            {
                var host = _hostFactory(StartArguments);
                await host.StartAsync(token).ConfigureAwait(false);
                token.Register(() => host?.Dispose());
                _webHostEnvironment = (IWebHostEnvironment)host.Services.GetService(typeof(IWebHostEnvironment));
                Core.Log.InfoBasic($"Hosting environment: {_webHostEnvironment.EnvironmentName}");
                Core.Log.InfoBasic($"Content root path: {_webHostEnvironment.ContentRootPath}");
                Core.Log.InfoBasic($"Web root path: {_webHostEnvironment.WebRootPath}");

                var server = (IServer)host.Services.GetService(typeof(IServer));
                _serverAddresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
                if (_serverAddresses != null)
                    foreach (var address in _serverAddresses)
                        Core.Log.InfoBasic($"WebService is Listening on: {address}");
            }
#endif

            if (_webHostFactory is not null)
            {
                var webHost = _webHostFactory(StartArguments);
                await webHost.StartAsync(token).ConfigureAwait(false);
                _hostingEnvironment = (Microsoft.AspNetCore.Hosting.IHostingEnvironment)webHost.Services.GetService(typeof(Microsoft.AspNetCore.Hosting.IHostingEnvironment));
                Core.Log.InfoBasic($"WebService Hosting environment: {_hostingEnvironment.EnvironmentName}");
                Core.Log.InfoBasic($"WebService Content root path: {_hostingEnvironment.ContentRootPath}");
                Core.Log.InfoBasic($"WebService Web root path: {_hostingEnvironment.WebRootPath}");
                _serverAddresses = webHost.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses;
                if (_serverAddresses != null)
                    foreach (var address in _serverAddresses)
                        Core.Log.InfoBasic($"WebService is Listening on: {address}");
            }
        }
    }
}
