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
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Net.Multicast;
using TWCore.Serialization;
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable IntroduceOptionalParameters.Global
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Service container
    /// </summary>
    public class ServiceContainer : IServiceContainer
    {
        #region Statics
        /// <summary>
        /// Parameters handlers
        /// </summary>
        protected static readonly ConcurrentDictionary<string, ContainerParameterHandler> ParametersHandlers = new ConcurrentDictionary<string, ContainerParameterHandler>();
        private static string[] _currentArgs;
        private static volatile bool _serviceEndAfterStart;
        private static bool _hasConsole = true;

        /// <summary>
        /// Bar Text Separation
        /// </summary>
        public static string BarSeparation = new string('═', 118);
        /// <summary>
        /// Gets if the Console is available
        /// </summary>
        public static bool HasConsole
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _hasConsole;
            internal set => _hasConsole = value;
        }
        /// <summary>
        /// Banner text
        /// </summary>
        public static string BannerText { get; set; }
        #endregion

        private IService _service;
        /// <summary>
        /// Init action
        /// </summary>
        protected internal readonly Action InitAction;

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Service to execute
        /// </summary>
        public IService Service
        {
            get => _service;
            internal set
            {
                switch (value)
                {
                    case null:
                        return;
                    case ServiceWrapper sWrap:
                        _service = sWrap;
                        break;
                    default:
                        _service = new ServiceWrapper(value, ServiceName);
                        break;
                }
            }
        }
        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName { get; }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Default service container
        /// </summary>
        /// <param name="service">Service instance</param>
        /// <param name="initAction">Init Action</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceContainer(IService service, Action initAction) : this(Core.ApplicationName, service, initAction)
        {
        }
        /// <inheritdoc />
        /// <summary>
        /// Default service container
        /// </summary>
        /// <param name="service">Service instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceContainer(IService service) : this(Core.ApplicationName, service, null)
        {
        }
        /// <inheritdoc />
        /// <summary>
        /// Default service container
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="service">Service instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceContainer(string serviceName, IService service) : this(serviceName, service, null)
        {
        }
        /// <summary>
        /// Default service container
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="service">Service instance</param>
        /// <param name="initAction">Init Action</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceContainer(string serviceName, IService service, Action initAction)
        {
            InitAction = initAction;
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
            => _serviceEndAfterStart = true;

        /// <inheritdoc />
        /// <summary>
        /// Execute the service
        /// </summary>
        /// <param name="args">Arguments</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Run(string[] args)
        {
            _currentArgs = args ?? new string[0];

            if (_currentArgs.Any(a => !a.Trim().StartsWith("/", StringComparison.OrdinalIgnoreCase)))
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

            var exec = false;
            foreach (var param in _currentArgs)
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
                            RegisterDiscovery();
                            var paramHandlerInfo = new ParameterHandlerInfo(this, args, value);
                            paramHandler?.Handler?.Invoke(paramHandlerInfo);
                            exec = true;
                            if (paramHandlerInfo.ShouldEndExecution)
                            {
                                UnregisterDiscovery();
                                Core.Dispose();
                                return;
                            }
                        }
                        break;
                }
            }
            if (Service != null)
            {
                RegisterDiscovery();
                InternalRun(args);
                UnregisterDiscovery();
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
            if (paramHandler?.Name is null) return;
            paramHandler.Name = paramHandler.Name.ToLowerInvariant();
            ParametersHandlers.TryAdd(paramHandler.Name, paramHandler);
        }
        #endregion

        #region Virtual Methods
        /// <summary>
        /// Show header
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal virtual void ShowHeader()
        {
            var strArgs = _currentArgs.Join(" ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(string.Format(" Service Name: {0}", ServiceName).PadRight(54) + "      " + string.Format("Environment Name: {0}", Core.EnvironmentName).PadRight(55));
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(string.Format(" Application Name: {0}", Core.ApplicationName).PadRight(54) + "      " + string.Format("Application Display: {0}", Core.ApplicationDisplayName).PadRight(55));
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(string.Format(" Machine Name: {0}", Core.MachineName).PadRight(54) + "      " + string.Format("TWCore Version: {0}", Core.FrameworkVersion).PadRight(55));
            Console.WriteLine();
        }
        /// <summary>
        /// Show full header
        /// </summary>
        /// <param name="showSettings">Show current settings</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal virtual void ShowFullHeader(bool showSettings = true)
        {
            var strArgs = _currentArgs.Join(" ");
            Core.Log.InfoBasic(BarSeparation);
            Core.Log.InfoBasic(" Arguments: {0}", strArgs);
            Core.Log.InfoBasic(string.Format(" Service Name: {0}", ServiceName).PadRight(54) + "      " + string.Format("Environment Name: {0}", Core.EnvironmentName).PadRight(55));
            Core.Log.InfoBasic(string.Format(" Application Name: {0}", Core.ApplicationName).PadRight(54) + "      " + string.Format("Application Display: {0}", Core.ApplicationDisplayName).PadRight(55));
            Core.Log.InfoBasic(string.Format(" Machine Name: {0}", Core.MachineName).PadRight(54) + "      " + string.Format("TWCore Version: {0}", Core.FrameworkVersion).PadRight(55));
            Core.Log.InfoBasic(BarSeparation);

            if (showSettings && Core.Settings?.Any() == true)
            {
                Core.Log.InfoBasic(" Settings for this app:");
                Core.Log.InfoBasic(" ─────────────────────");
                foreach (var setting in Core.Settings.OrderBy(i => i.Key))
                    Core.Log.InfoBasic("\t » {0} = {1}", setting.Key, setting.Value);
            }
        }
        /// <summary>
        /// Show help
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal virtual void ShowHelp()
        {
            ShowHeader();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Parameters:");
            Console.WriteLine("  {0,-30}  : {1}", "/help | /?", "This Help.");
            Console.WriteLine("  {0,-30}  : {1}", "/showsettings | /w", "Show all loaded settings from app.config and settings.xml file.");
            Console.WriteLine("  {0,-30}  : {1}", "/version", "Show all loaded assemblies version.");
            if (ParametersHandlers?.Any() == true)
            {
                foreach (var par in ParametersHandlers.OrderBy(p => p.Key))
                    Console.WriteLine("  {0,-30}  : {1}", "/" + par.Key, par.Value.Description);
            }
            Console.WriteLine();
        }
        /// <summary>
        /// Show assemblies versions
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal virtual void ShowVersion()
        {
            ShowHeader();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("All assemblies versions:");
            Console.WriteLine("───────────────────────");
            var assemblies = Factory.GetAllAssemblies();
            foreach (var assembly in assemblies)
            {
                var name = assembly.GetName()?.Name;
                var version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
                Console.WriteLine("  » {0} = {1}", name, version);
            }
        }
        /// <summary>
        /// Show settings of the app
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal virtual void ShowSettings()
        {
            ShowHeader();
            Console.ForegroundColor = ConsoleColor.Gray;
            if (Core.Settings?.Any() == true)
            {
                Console.WriteLine("Settings for this app:");
                Console.WriteLine("─────────────────────");
                foreach (var setting in Core.Settings)
                    Console.WriteLine("  » {0} = {1}", setting.Key, setting.Value);
            }
            else
            {
                Console.WriteLine("There aren't any settings configured for this app.");
            }
        }
        /// <summary>
        /// Internal Run command
        /// </summary>
        /// <param name="args">Run arguments</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal virtual void InternalRun(string[] args)
        {
            Core.Log.LibDebug("««« Starting Service »»»");
            InitAction?.Invoke();
            if (!string.IsNullOrWhiteSpace(BannerText))
            {
                var bannerText = BannerText.Replace("\r", string.Empty).Split("\n");
                foreach (var line in bannerText)
                    Core.Log.InfoBasic(line);
            }
            ShowFullHeader();

			var hasStdIn = !Console.IsInputRedirected;
			if (hasStdIn)
			{
				try
				{
					var keyAvailble = Console.KeyAvailable;
				}
				catch
				{
					hasStdIn = false;
				}
			}

            if (HasConsole && hasStdIn)
                ServiceStartWithConsole();
            else
                ServiceStartNoConsole();

            Core.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ServiceStartWithConsole()
        {
            Core.Log.InfoBasic(string.Empty);
            Core.Log.InfoBasic(" Available Console Commands:");
            Core.Log.InfoBasic(" ──────────────────────────");
            Core.Log.InfoBasic("\t » ESC = Stop the service.");
            if (Service.CanPauseAndContinue)
            {
                Core.Log.InfoBasic("\t » P = Pause a running service.");
                Core.Log.InfoBasic("\t » C = Continue a paused service.");
            }
            Core.Log.InfoBasic(BarSeparation);
            if (_discovery)
                Core.Log.InfoDetail("Registering Discovery Services (ReceiveThread={0})", !Core.GlobalSettings.DiscoveryDisableReceive);
            
            Service.OnStart(null);

            try
            {
                var paused = false;
                while (!_serviceEndAfterStart)
                {
                    var key = Console.ReadKey().Key;
                    if (Service.CanPauseAndContinue)
                    {
                        switch (key)
                        {
                            case ConsoleKey.P when !paused:
                                Core.Log.InfoBasic("««« Pausing Service »»»");
                                Service.OnPause();
                                paused = true;
                                Core.Log.InfoBasic("««« Service Paused »»»");
                                break;
                            case ConsoleKey.C when paused:
                                Core.Log.InfoBasic("««« Restoring Service »»»");
                                Service.OnContinue();
                                paused = false;
                                Core.Log.InfoBasic("««« Service Restored »»»");
                                break;
                        }
                    }

                    if (key == ConsoleKey.Escape)
                        break;
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                Core.Log.Warning("Error when trying to use Console input. Changing to ProcessExit detection.");
                var mres = new ManualResetEventSlim(false);
                AppDomain.CurrentDomain.ProcessExit += (s, e) => mres.Set();
                Core.Log.InfoBasic("Running without a Console, Capturing the ProcessExit for AppDomain to Stop.");
                Core.Log.InfoBasic(BarSeparation);
                mres.Wait();
            }

            Core.Log.InfoBasic("««« Stopping Service »»»");
            Service.OnStop();
            Core.Log.InfoBasic("««« Service Stopped »»»");
            Core.Log.InfoBasic(BarSeparation);
            if (!_serviceEndAfterStart)
            {
                Core.Log.WriteEmptyLine();
                Core.Log.InfoBasic("Press ENTER to exit.");
                Console.ReadLine();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ServiceStartNoConsole()
        {
            var mres = new ManualResetEventSlim(false);
            AppDomain.CurrentDomain.ProcessExit += (s, e) => mres.Set();
            if (Factory.RunningAsContainer)
                Core.Log.InfoBasic("Running inside a Container, Capturing the ProcessExit event to Stop.");
            else
                Core.Log.InfoBasic("Running without a Console, Capturing the ProcessExit event to Stop.");
            Core.Log.InfoBasic(BarSeparation);
            Service.OnStart(null);
            mres.Wait();
            Core.Log.InfoBasic("««« Stopping Service »»»");
            Service.OnStop();
            Core.Log.InfoBasic("««« Service Stopped »»»");
            Core.Log.InfoBasic(BarSeparation);
            if (!_serviceEndAfterStart)
                Core.Log.WriteEmptyLine();
        }
        #endregion

        #region Protected Methods
        private bool _discovery;
        /// <summary>
        /// Register discovery service
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void RegisterDiscovery()
        {
            if (_discovery) return;
            if (!Core.GlobalSettings.EnableDiscovery) return;
            Task.Run(() =>
            {
                _discovery = true;
                var serializer = SerializerManager.DefaultBinarySerializer;
                if (!string.IsNullOrWhiteSpace(Core.GlobalSettings.DiscoverySerializerMimeType))
                {
                    serializer = SerializerManager.GetByMimeType(Core.GlobalSettings.DiscoverySerializerMimeType);
                    if (serializer is null)
                    {
                        Core.Log.Warning("Discovery.SerializerMimeType can't be loaded, using default binary serializer.");
                        serializer = SerializerManager.DefaultBinarySerializer;
                    }
                }
                try
                {
                    DiscoveryService.Serializer = serializer;
                    DiscoveryService.Connect(Core.GlobalSettings.DiscoveryMulticastIp, Core.GlobalSettings.DiscoveryPort, !Core.GlobalSettings.DiscoveryDisableReceive);
                    DiscoveryService.RegisterService(DiscoveryService.FrameworkCategory, "FOLDERS", "Folders for the services.", new SerializedObject(new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() }));
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            });
        }
        /// <summary>
        /// Unregister discovery service
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void UnregisterDiscovery()
        {
            if (!_discovery) return;
            if (!Core.GlobalSettings.EnableDiscovery) return;
            Core.Log.LibDebug("Unregistering Discovery services.");
            _discovery = false;
            try
            {
                DiscoveryService.Disconnect();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        #endregion
    }
}
