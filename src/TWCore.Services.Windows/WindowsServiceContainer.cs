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
using System.Linq;

namespace TWCore.Services.Windows
{
    /// <inheritdoc />
    /// <summary>
    /// Windows Service container
    /// </summary>
    internal class WindowsServiceContainer : ServiceContainer
    {
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

        void Init()
        {
            RegisterParametersHandler("service-install", "Install Windows Service", InstallService);
            RegisterParametersHandler("service-uninstall", "Uninstall Windows Service", UninstallService);
        }

        #region Override Methods

        protected override void ShowHelp()
        {
            ShowHeader();
            Console.WriteLine("Parameters:");
            Console.WriteLine("  {0,-30}  : {1}", "/help | /?", "This Help.");
            Console.WriteLine("  {0,-30}  : {1}", "/service-install", "Install Windows Service.");
            Console.WriteLine("  {0,-30}  : {1}", "/service-uninstall", "Uninstall Windows Service.");
            Console.WriteLine("  {0,-30}  : {1}", "/service-run", "Run as a Windows Service.");
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
            if (args.Contains("/service-run", StringComparer.OrdinalIgnoreCase) && Service == null)
            {
                Core.Log.Warning("THERE IS NO SERVICE TO START.");
                return;
            }
            base.Run(args);
        }

        protected override void InternalRun(string[] args)
        {
            if (args.Contains("/service-run", StringComparer.OrdinalIgnoreCase))
            {
                initAction?.Invoke();
                Core.Log.InfoBasic("*** RUNNING AS WINDOWS SERVICE ***");
                ShowFullHeader();
                try
                {
                    var win32Service = new Win32Service(ServiceName, Service);
                    var serviceHost = new Win32ServiceHost(win32Service);
                    serviceHost.Run();
                }
                catch(Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }
            else
                base.InternalRun(args);
        }

        #endregion

        #region Install/Uninstall Service
        static void InstallService(ParameterHandlerInfo parameterHandlerInfo)
        {
            throw new NotImplementedException();
        }
        static void UninstallService(ParameterHandlerInfo parameterHandlerInfo)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}