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

using DasMulli.Win32.ServiceUtils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TWCore.Settings;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Global

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Linux Service container
    /// </summary>
    internal class LinuxServiceContainer : ServiceContainer
    {
        private LinuxServiceSettings _settings;

        #region .ctor
        public LinuxServiceContainer(IService service, Action initAction) :
            base(service, initAction) => Init();
        public LinuxServiceContainer(IService service) :
            base(service) => Init();
        public LinuxServiceContainer(string serviceName, IService service) :
            base(serviceName, service) => Init();
        public LinuxServiceContainer(string serviceName, IService service, Action initAction) :
            base(serviceName, service, initAction) => Init();
        #endregion

        #region Install/Uninstall Service
        private void Init()
        {
            if (Service == null) return;
            _settings = Core.GetSettings<LinuxServiceSettings>();
            RegisterParametersHandler("service-install", "Install Systemd Service", InstallService);
            RegisterParametersHandler("service-uninstall", "Uninstall Systemd Service", UninstallService);
        }
        private void InstallService(ParameterHandlerInfo parameterHandlerInfo)
        {
            try
            {
                // Environment.GetCommandLineArgs() includes the current DLL from a "dotnet my.dll --register-service" call, which is not passed to Main()
                var remainingArgs = Environment.GetCommandLineArgs()
                    .Where(arg => arg != "/service-install")
                    .Select(EscapeCommandLineArgument);

                var host = Process.GetCurrentProcess().MainModule.FileName;
                string directory;
                if (!host.EndsWith("dotnet", StringComparison.OrdinalIgnoreCase))
                {
                    // For self-contained apps, skip the dll path
                    remainingArgs = remainingArgs.Skip(1);
                    directory = Path.GetDirectoryName(host);
                }
                else
                {
                    directory = Path.GetDirectoryName(remainingArgs.First().Replace("\"", string.Empty).Trim());
                }
                
                var fullServiceCommand = host + " " + string.Join(" ", remainingArgs);
                _settings.ServiceName = _settings.ServiceName ?? Core.ApplicationDisplayName;
                _settings.Description = _settings.Description ?? Core.ApplicationDisplayName;
                var serviceName = _settings.ServiceName?.ToLowerInvariant().Replace(" ", "-") + ".service";
                var res = typeof(LinuxServiceContainer).Assembly.GetResourceString("SystemdServicePattern.service");

                Core.Log.Warning(res);
                Core.Log.Warning(serviceName);
                Core.Log.Warning(_settings.Description);
                Core.Log.Warning(directory);
                Core.Log.Warning(fullServiceCommand);
                
                Core.Log.Warning($"The Service \"{ServiceName}\" was installed successfully.");
            }
            catch (Exception ex)
            {
                Core.Log.Error(ex.Message);
            }
        }
        private void UninstallService(ParameterHandlerInfo parameterHandlerInfo)
        {
            try
            {
                var serviceManager = new Win32ServiceManager();
                serviceManager.DeleteService(ServiceName);
                Core.Log.Warning($"The Service \"{ServiceName}\" was uninstalled successfully.");
            }
            catch (Exception ex)
            {
                Core.Log.Error(ex.Message);
            }
        }
        private static string EscapeCommandLineArgument(string arg)
        {
            // http://stackoverflow.com/a/6040946/784387
            arg = Regex.Replace(arg, @"(\\*)" + "\"", @"$1$1\" + "\"");
            //arg = Regex.Replace(arg, @"(\\+)$", @"$1$1");
            arg = "\"" + Regex.Replace(arg, @"(\\+)$", @"$1$1") + "\"";
            return arg;
        }
        #endregion

        #region Nested Types
        [SettingsContainer("Core.Service.Linux")]
        private class LinuxServiceSettings : SettingsBase
        {
            public string ServiceName { get; set; }
            public string Description { get; set; }
            public string User { get; set; }
        }
        #endregion
    }
}