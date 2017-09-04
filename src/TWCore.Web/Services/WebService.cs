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

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TWCore.Services
{
    /// <summary>
    /// AspNet Web Service
    /// </summary>
    public class WebService : SimpleServiceAsync
    {
        Func<string[], IWebHost> _webHostFactory;

        #region .ctor
        /// <summary>
        /// AspNet Web Service
        /// </summary>
        /// <param name="webHostFactory">WebHost Builder factory delegate</param>
        public WebService(Func<string[], IWebHost> webHostFactory)
        {
            _webHostFactory = webHostFactory;
        }
        #endregion

        protected override async Task OnActionAsync(CancellationToken token)
        {
            var webHost = _webHostFactory(StartArguments);
            await webHost.StartAsync(token).ConfigureAwait(false);
            var hostingEnvironment = (IHostingEnvironment)webHost.Services.GetService(typeof(IHostingEnvironment));
            //var applicationLifetime = (IApplicationLifetime)webHost.Services.GetService(typeof(IApplicationLifetime));
            Core.Log.InfoBasic($"WebService Hosting environment: {hostingEnvironment.EnvironmentName}");
            Core.Log.InfoBasic($"WebService Content root path: {hostingEnvironment.ContentRootPath}");
            var serverAddresses = webHost.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses;
            if (serverAddresses != null)
                foreach (var address in serverAddresses)
                    Core.Log.InfoBasic($"WebService is Listening on: {address}");
        }
    }
}
