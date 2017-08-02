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

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TWCore.Services
{
    /// <summary>
    /// Service container
    /// </summary>
    public class ServiceContainer : IServiceContainer
    {
        static ConcurrentDictionary<string, ContainerParameterHandler> ParametersHandlers = new ConcurrentDictionary<string, ContainerParameterHandler>();
        static string[] currentArgs;
        static volatile bool serviceEndAfterStart = false;
        /// <summary>
        /// Gets if the Console is available
        /// </summary>
        public static bool HasConsole
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                try
                {
                    return Console.WindowHeight > 0;
                }
                catch
                {
                    return false;
                }
            }
        }
        internal Action initAction;

        #region Properties
        /// <summary>
        /// Service to execute
        /// </summary>
        public IService Service { get; internal set; }
        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName { get; private set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Default service container
        /// </summary>
        /// <param name="service">Service instance</param>
        /// <param name="initAction">Init Action</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceContainer(IService service, Action initAction) : this(Core.ApplicationDisplayName, service)
        {
            this.initAction = initAction;
        }
        /// <summary>
        /// Default service container
        /// </summary>
        /// <param name="service">Service instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceContainer(IService service) : this(Core.ApplicationDisplayName, service)
        {
        }
        /// <summary>
        /// Default service container
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="service">Service instance</param>
        /// <param name="initAction">Init Action</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceContainer(string serviceName, IService service, Action initAction) : this(serviceName, service)
        {
            this.initAction = initAction;
        }
        /// <summary>
        /// Default service container
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="service">Service instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceContainer(string serviceName, IService service)
        {
            ServiceName = serviceName;
            Service = service;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Service Exit container
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ServiceExit()
            => serviceEndAfterStart = true;

        /// <summary>
        /// Execute the service
        /// </summary>
        /// <param name="args">Arguments</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run(string[] args)
        {
            currentArgs = args ?? new string[0];

			if (currentArgs.Any(a => !a.Trim().StartsWith("/", StringComparison.OrdinalIgnoreCase)))
            {
                ShowHeader();
                Console.WriteLine("Error!: Wrong parameters.");
                Console.WriteLine("All parameters needs to begin with '/' char, check if there are invalid spaces between each parameter.");
                Console.WriteLine("If you need a paramter with spaces you have to put it between \"{parameter}\".");
                Console.WriteLine();
                Console.WriteLine("If you need more help, use: /? parameter");
                Console.WriteLine();
                return;
            }

			bool exec = false;
            foreach (string param in currentArgs)
            {
                var lParam = param.Trim().ToLowerInvariant();
                string value = null;
				var doubleDot = lParam.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                if (doubleDot > -1)
                {
                    value = lParam.Substring(doubleDot + 1);
                    lParam = lParam.Substring(0, doubleDot);
                }
                switch (lParam)
                {
                    case "/?":
                    case "/help":
                        ShowHelp();
                        return;
                    case "/version":
                        ShowVersion();
                        return;
                    case "/showsettings":
                    case "/w":
                        ShowSettings();
                        return;
                    default:
						if (lParam.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                            lParam = lParam.Substring(1);
                        if (ParametersHandlers.TryGetValue(lParam, out var paramHandler))
                        {
                            var paramHandlerInfo = new ParameterHandlerInfo(this, args, value);
                            paramHandler?.Handler?.Invoke(paramHandlerInfo);
							exec = true;
                            if (paramHandlerInfo.ShouldEndExecution)
                            {
                                Core.Dispose();
                                return;
                            }
                        }
                        break;
                }
            }
            if (Service != null)
            {
                if (HasConsole)
                    RunAsConsole(args);
                else
                    RunWithNoConsole(args);
            }
			else if (!exec)
            {
                ShowHelp();
            }
            Core.Dispose();
        }

        /// <summary>
        /// Register paramter handler
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="description">Parameter description</param>
        /// <param name="handler">Parameter handler</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterParametersHandler(string name, string description, Action<ParameterHandlerInfo> handler)
            => RegisterParametersHandler(new ContainerParameterHandler { Name = name, Description = description, Handler = handler });
        /// <summary>
        /// Register parameter handler
        /// </summary>
        /// <param name="paramHandler">Parameter handler</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterParametersHandler(ContainerParameterHandler paramHandler)
        {
            if (paramHandler?.Name != null)
            {
                paramHandler.Name = paramHandler.Name.ToLowerInvariant();
                ParametersHandlers.TryAdd(paramHandler.Name, paramHandler);
            }
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ShowHeader()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            if (!string.IsNullOrWhiteSpace(ServiceName))
                Console.WriteLine("Service Name: {0}", ServiceName);
            if (!string.IsNullOrWhiteSpace(Core.EnvironmentName))
                Console.WriteLine("Environment Name: {0}", Core.EnvironmentName);
            if (!string.IsNullOrWhiteSpace(Core.MachineName))
                Console.WriteLine("Machine Name: {0}", Core.MachineName);
            if (!string.IsNullOrWhiteSpace(Core.ApplicationName))
                Console.WriteLine("Application Name: {0}", Core.ApplicationName);
            if (Core.ApplicationDisplayName != Core.ApplicationName)
                Console.WriteLine("Application Display: {0}", Core.ApplicationDisplayName);
            Console.WriteLine("TWCore Version: {0}", Core.FrameworkVersion);
            Console.ResetColor();
            Console.WriteLine();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ShowFullHeader(bool showSettings = true)
        {
            var strArgs = currentArgs.Join(" ");

            Core.Log.InfoBasic("**************************************************************************************");
            if (!string.IsNullOrWhiteSpace(strArgs))
                Core.Log.InfoBasic("Arguments: {0}", strArgs);
            if (!string.IsNullOrWhiteSpace(ServiceName))
                Core.Log.InfoBasic("Service Name: {0}", ServiceName);
            if (!string.IsNullOrWhiteSpace(Core.EnvironmentName))
                Core.Log.InfoBasic("Environment Name: {0}", Core.EnvironmentName);
            if (!string.IsNullOrWhiteSpace(Core.MachineName))
                Core.Log.InfoBasic("Machine Name: {0}", Core.MachineName);
            if (!string.IsNullOrWhiteSpace(Core.ApplicationName))
                Core.Log.InfoBasic("Application Name: {0}", Core.ApplicationName);
            if (!string.IsNullOrWhiteSpace(Core.ApplicationDisplayName))
                Core.Log.InfoBasic("Application Display: {0}", Core.ApplicationDisplayName);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Core.Log.InfoBasic("TWCore Version: {0}", Core.FrameworkVersion);
            Console.ResetColor();
            if (showSettings && Core.Settings?.Any() == true)
            {
                Core.Log.InfoBasic("Settings for this app:");
                Core.Log.InfoBasic("======================");
                foreach (var setting in Core.Settings.OrderBy(i => i.Key))
                    Core.Log.InfoBasic("     {0} = {1}", setting.Key, setting.Value);
            }
            Core.Log.InfoBasic("**************************************************************************************");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ShowHelp()
        {
            ShowHeader();
            Console.WriteLine("Parameters:");
            Console.WriteLine("  {0,-25}  : {1}", "/help | /?", "This Help.");
            Console.WriteLine("  {0,-25}  : {1}", "/showsettings | /w", "Show all loaded settings from app.config and settings.xml file.");
            Console.WriteLine("  {0,-25}  : {1}", "/version", "Show all loaded assemblies version.");
            if (ParametersHandlers?.Any() == true)
            {
                foreach (var par in ParametersHandlers.OrderBy(p => p.Key))
                    Console.WriteLine("  {0,-25}  : {1}", "/" + par.Key, par.Value.Description);
            }
            Console.WriteLine();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ShowVersion()
        {
            ShowHeader();
            Console.WriteLine("All assemblies versions:");
            Console.WriteLine("========================");
            var assemblies = Factory.GetAllAssemblies();
            foreach (var assembly in assemblies)
            {
                var name = assembly.GetName().Name;
                if (name == "Microsoft.GeneratedCode")
                    continue;
                var version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
                Console.WriteLine("{0} = {1}", name, version);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ShowSettings()
        {
            ShowHeader();
            if (Core.Settings?.Any() == true)
            {
                Console.WriteLine("Settings for this app:");
                Console.WriteLine("======================");
                foreach (var setting in Core.Settings)
                    Console.WriteLine("{0} = {1}", setting.Key, setting.Value);
            }
            else
            {
                Console.WriteLine("There aren't any settings configured for this app.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RunAsConsole(string[] args)
        {
            initAction?.Invoke();
            ShowFullHeader();
            Core.Log.InfoBasic("Available Console Commands:");
            Core.Log.InfoBasic("\t ESC = Stop the service.");
            if (Service.CanPauseAndContinue)
            {
                Core.Log.InfoBasic("\t P = Pause a running service.");
                Core.Log.InfoBasic("\t C = Continue a paused service.");
            }
            Core.Log.InfoBasic("**************************************************************************************");
            Service.OnStart(null);
            bool paused = false;
            while (!serviceEndAfterStart)
            {
                var key = Console.ReadKey().Key;
                if (Service.CanPauseAndContinue) 
                {
                    if (key == ConsoleKey.P && !paused)
                    {
                        Core.Log.InfoBasic("*** Pausing Service ***");
                        Service.OnPause();
                        paused = true;
                        Core.Log.InfoBasic("*** Service Paused ***");
                    }
                    if (key == ConsoleKey.C && paused)
                    {
                        Core.Log.InfoBasic("*** Restoring Service ***");
                        Service.OnContinue();
                        paused = false;
                        Core.Log.InfoBasic("*** Service Restored ***");
                    }
                }
                if (key == ConsoleKey.Escape)
                    break;
            }
            Core.Log.InfoBasic("*** Stopping Service ***");
            Service.OnStop();
            Core.Log.InfoBasic("*** Service Stopped ***");
            Core.Log.InfoBasic("**************************************************************************************");
            if (!serviceEndAfterStart)
            {
                Core.Log.WriteEmptyLine();
                Core.Log.InfoBasic("Press ENTER to exit.");
                Console.ReadLine();
            }
            Core.Dispose();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RunWithNoConsole(string[] args)
        {
            ManualResetEventSlim mres = new ManualResetEventSlim(false);
            AppDomain.CurrentDomain.ProcessExit += (s, e) => mres.Set();

            initAction?.Invoke();
            ShowFullHeader();
            Core.Log.InfoBasic("Running without a Console, Capturing the ProcessExit for AppDomain to Stop.");
            Core.Log.InfoBasic("**************************************************************************************");
            Service.OnStart(null);
            mres.Wait();
            Core.Log.InfoBasic("*** Stopping Service ***");
            Service.OnStop();
            Core.Log.InfoBasic("*** Service Stopped ***");
            Core.Log.InfoBasic("**************************************************************************************");
            if (!serviceEndAfterStart)
                Core.Log.WriteEmptyLine();
            Core.Dispose();
        }
        #endregion
    }
}
