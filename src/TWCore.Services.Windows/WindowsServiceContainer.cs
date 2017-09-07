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
using System.Linq;
using System.Text.RegularExpressions;
using TWCore.Settings;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Global

namespace TWCore.Services.Windows
{
    /// <inheritdoc />
    /// <summary>
    /// Windows Service container
    /// </summary>
    internal class WindowsServiceContainer : ServiceContainer
    {
        private WindowsServiceSettings _settings;

        #region .ctor
        public WindowsServiceContainer(IService service, Action initAction) :
            base(service, initAction) => Init();
        public WindowsServiceContainer(IService service) :
            base(service) => Init();
        public WindowsServiceContainer(string serviceName, IService service) :
            base(serviceName, service) => Init();
        public WindowsServiceContainer(string serviceName, IService service, Action initAction) :
            base(serviceName, service, initAction) => Init();
        #endregion

        #region Override Methods

        protected override void ShowHelp()
        {
            ShowHeader();
            Console.WriteLine("Parameters:");
            Console.WriteLine("  {0,-30}  : {1}", "/help | /?", "This Help.");
            if (Service != null)
            {
                Console.WriteLine("  {0,-30}  : {1}", "/service-install", "Install Windows Service.");
                Console.WriteLine("  {0,-30}  : {1}", "/service-uninstall", "Uninstall Windows Service.");
                Console.WriteLine("  {0,-30}  : {1}", "/service-run", "Run as a Windows Service.");
            }
            Console.WriteLine("  {0,-30}  : {1}", "/showsettings | /w", "Show all loaded settings from app.config and settings.xml file.");
            Console.WriteLine("  {0,-30}  : {1}", "/version", "Show all loaded assemblies version.");
            if (ParametersHandlers?.Any() == true)
            {
                foreach (var par in ParametersHandlers.OrderBy(p => p.Key))
                {
                    switch (par.Key)
                    {
                        case "service-install":
                            continue;
                        case "service-uninstall":
                            continue;
                        default:
                            Console.WriteLine("  {0,-30}  : {1}", "/" + par.Key, par.Value.Description);
                            break;
                    }
                }
            }
            Console.WriteLine();
        }

        public override void Run(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "/service-install":
                        InstallService(null);
                        Core.Dispose();
                        return;
                    case "/service-uninstall":
                        UninstallService(null);
                        Core.Dispose();
                        return;
                }
            }
            if (args.Contains("/service-run", StringComparer.OrdinalIgnoreCase) && Service == null)
            {
                Core.Log.Error("THERE IS NO SERVICE TO START.");
                return;
            }
            base.Run(args);
        }

        protected override void InternalRun(string[] args)
        {
            if (args.Contains("/service-run", StringComparer.OrdinalIgnoreCase))
            {
                InitAction?.Invoke();
                Core.Log.InfoBasic("*** RUNNING AS WINDOWS SERVICE ***");
                ShowFullHeader();
                try
                {
                    var win32Service = new Win32Service(ServiceName, Service);
                    var serviceHost = new Win32ServiceHost(win32Service);
                    serviceHost.Run();
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }
            else
                base.InternalRun(args);
        }

        #endregion

        #region Install/Uninstall Service
        private void Init()
        {
            if (Service == null) return;
            _settings = Core.GetSettings<WindowsServiceSettings>();
            RegisterParametersHandler("service-install", "Install Windows Service", InstallService);
            RegisterParametersHandler("service-uninstall", "Uninstall Windows Service", UninstallService);
        }
        private void InstallService(ParameterHandlerInfo parameterHandlerInfo)
        {
            try
            {
                // Environment.GetCommandLineArgs() includes the current DLL from a "dotnet my.dll --register-service" call, which is not passed to Main()
                var remainingArgs = Environment.GetCommandLineArgs()
                    .Where(arg => arg != "/service-install")
                    .Select(EscapeCommandLineArgument)
                    .Append("/service-run");

                var host = Process.GetCurrentProcess().MainModule.FileName;
                if (!host.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase))
                {
                    // For self-contained apps, skip the dll path
                    remainingArgs = remainingArgs.Skip(1);
                }
                var fullServiceCommand = host + " " + string.Join(" ", remainingArgs);


                _settings.DisplayName = _settings.DisplayName ?? ServiceName;
                _settings.Description = _settings.Description ?? ServiceName;

                Win32ServiceCredentials credentials;
                switch (_settings.Credentials)
                {
                    case ServiceCredentials.LocalSystem:
                        credentials = Win32ServiceCredentials.LocalSystem;
                        break;
                    case ServiceCredentials.LocalService:
                        credentials = Win32ServiceCredentials.LocalService;
                        break;
                    case ServiceCredentials.NetworkService:
                        credentials = Win32ServiceCredentials.NetworkService;
                        break;
                    case ServiceCredentials.Custom:
                        credentials = new Win32ServiceCredentials(_settings.Username, _settings.Password);
                        break;
                    default:
                        throw new ArgumentException("The Credentials enum has a wrong value.");
                }


                var serviceManager = new Win32ServiceManager();
                serviceManager.CreateOrUpdateService(
                    ServiceName,
                    _settings.DisplayName,
                    _settings.Description,
                    fullServiceCommand,
                    credentials,
                    _settings.AutoStart
                );

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
            arg = "\"" + Regex.Replace(arg, @"(\\+)$", @"$1$1") + "\"";
            return arg;
        }
        #endregion


        #region Nested Types
        private class WindowsServiceSettings : SettingsBase
        {
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public ServiceCredentials Credentials { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public bool AutoStart { get; set; }
        }
        public enum ServiceCredentials
        {
            LocalSystem,
            LocalService,
            NetworkService,
            Custom
        }
        #endregion
    }
}