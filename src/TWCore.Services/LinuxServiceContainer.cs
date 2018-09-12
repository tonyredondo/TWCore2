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
            if (Service is null) return;
            _settings = Core.GetSettings<LinuxServiceSettings>();
            RegisterParametersHandler("service-create", "Create Systemd service file", CreateService);
            RegisterParametersHandler("service-remove", "Remove Systemd service file", RemoveService);
        }
        private void CreateService(ParameterHandlerInfo parameterHandlerInfo)
        {
            try
            {
                // Environment.GetCommandLineArgs() includes the current DLL from a "dotnet my.dll --register-service" call, which is not passed to Main()
                var remainingArgs = Environment.GetCommandLineArgs()
                    .Where(arg => arg != "/service-create")
                    .Select(EscapeCommandLineArgument)
                    .ToArray();

                var host = Process.GetCurrentProcess().MainModule.FileName;
                string directory;
                if (!host.EndsWith("dotnet", StringComparison.OrdinalIgnoreCase))
                {
                    // For self-contained apps, skip the dll path
                    remainingArgs = remainingArgs.Skip(1).ToArray();
                    directory = Path.GetDirectoryName(host);
                }
                else
                    directory = Path.GetDirectoryName(remainingArgs.First().Replace("\"", string.Empty).Trim());

                var fullServiceCommand = host + " " + string.Join(" ", remainingArgs);
                _settings.ServiceName = _settings.ServiceName ?? Core.ApplicationName;
                _settings.Description = _settings.Description ?? Core.ApplicationDisplayName;

                if (string.IsNullOrWhiteSpace(_settings.User))
                {
                    string user;
                    while (true)
                    {
                        Console.WriteLine("Please enter the user to run the service or press enter [{0}]:", Environment.UserName);
                        user = Console.ReadLine();
                        if (string.IsNullOrEmpty(user))
                            user = Environment.UserName;
                        if (!string.IsNullOrWhiteSpace(user))
                            break;
                    }
                    _settings.User = user;
                }

                var withInstall = true;
                var servicePath = "/etc/systemd/system/";
                if (!Directory.Exists(servicePath))
                {
                    Core.Log.Error("The systemd path can't be found: {0}, copying on the same folder.", servicePath);
                    servicePath = "./";
                    withInstall = false;
                }

                var serviceName = _settings.ServiceName?.ToLowerInvariant().Replace(" ", "-") + ".service";
                servicePath = Path.Combine(servicePath, serviceName);
                var res = typeof(LinuxServiceContainer).Assembly.GetResourceString("SystemdServicePattern.service");
                res = res.Replace("{{DESCRIPTION}}", _settings.Description);
                res = res.Replace("{{USER}}", _settings.User);
                res = res.Replace("{{WORKINGDIRECTORY}}", directory);
                res = res.Replace("{{EXECUTIONPATH}}", fullServiceCommand);
                using (var fStream = File.Open(servicePath, FileMode.Create, FileAccess.Write))
                using (var sWriter = new StreamWriter(fStream))
                    sWriter.WriteLine(res);

                Core.Log.Warning(withInstall
                    ? $"The file {serviceName}, was copied to /etc/systemd/system/ path."
                    : $"The file {serviceName}, was copied to the current path.");
            }
            catch (Exception ex)
            {
                Core.Log.Error(ex.Message);
            }
        }
        private void RemoveService(ParameterHandlerInfo parameterHandlerInfo)
        {
            try
            {
                // Environment.GetCommandLineArgs() includes the current DLL from a "dotnet my.dll --register-service" call, which is not passed to Main()
                /*
                var remainingArgs = Environment.GetCommandLineArgs()
                    .Where(arg => arg != "/service-uninstall")
                    .Select(EscapeCommandLineArgument)
                    .ToArray();

                var host = Process.GetCurrentProcess().MainModule.FileName;
                if (!host.EndsWith("dotnet", StringComparison.OrdinalIgnoreCase))
                {
                    // For self-contained apps, skip the dll path
                    remainingArgs = remainingArgs.Skip(1).ToArray();
                }*/

                _settings.ServiceName = _settings.ServiceName ?? Core.ApplicationName;

                var withInstall = true;
                var servicePath = "/etc/systemd/system/";
                if (!Directory.Exists(servicePath))
                {
                    servicePath = "./";
                    withInstall = false;
                }

                var serviceName = _settings.ServiceName?.ToLowerInvariant().Replace(" ", "-") + ".service";
                servicePath = Path.Combine(servicePath, serviceName);

                if (File.Exists(servicePath))
                {
                    File.Delete(servicePath);

                    Core.Log.Warning(withInstall
                        ? $"The file {serviceName}, was deleted from /etc/systemd/system/ path."
                        : $"The file {serviceName}, was deleted from the current path.");
                }
                else
                    Core.Log.InfoDetail("Nothing to do.");
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