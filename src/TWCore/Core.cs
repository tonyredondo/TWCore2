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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Log.Storages;
using TWCore.Diagnostics.Status;
using TWCore.Diagnostics.Trace;
using TWCore.Diagnostics.Trace.Storages;
using TWCore.Injector;
using TWCore.Reflection;
using TWCore.Security;
using TWCore.Serialization;
using TWCore.Services;
using TWCore.Settings;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace TWCore
{
	/// <summary>
	/// Service container factory delegate
	/// </summary>
	public delegate IServiceContainer ServiceContainerFactoryDelegate(IService service, Action initAction);

    /// <summary>
    /// CORE App Static
    /// </summary>
    public static class Core
    {
        private const string SettingsTemplateFormat = "{{Settings:{0}}}";
        private static readonly SymmetricKeyProvider SymmetricProvider = new SymmetricKeyProvider();
        private static readonly Regex EnvironmentTemplateFormatRegex = new Regex(@"{Env:([A-Za-z0-9_ |+-\\*/_!""$% &\(\) = '?¡¿.:,;<>]*)}", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex EncriptionTemplateFormatRegex = new Regex(@"{Encripted:([A-Za-z0-9_ |+-\\*/_!""$% &\(\) = '?¡¿.:,;<>]*)}", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly ConcurrentDictionary<Type, SettingsBase> SettingsCache = new ConcurrentDictionary<Type, SettingsBase>();
        private static readonly ConcurrentDictionary<string, Type> TypesCache = new ConcurrentDictionary<string, Type>();
        private static CoreSettings _globalSettings;
        private static Dictionary<string, object> _data;
        private static Dictionary<object, object> _objectData;
        private static AsyncLocal<Dictionary<string, object>> _taskData;
        private static AsyncLocal<Dictionary<object, object>> _taskObjectData;
        private static volatile bool _initialized;
        private static readonly Queue<Action> OninitActions = new Queue<Action>();
        private static Timer UpdateLocalUtcTimer;
        internal static Dictionary<string, string> DefaultEnvironmentVariables = null;
        internal static string EncryptionKey = null;

        #region Properties
        /// <summary>
        /// Gets or Sets the Application display Name
        /// </summary>
        public static string ApplicationDisplayName { get; set; }
        /// <summary>
        /// Gets or Sets the Application Name
        /// </summary>
        public static string ApplicationName { get; set; }
        /// <summary>
        /// Gets or Sets the Application Environment Name
        /// </summary>
        public static string EnvironmentName { get; set; }
        /// <summary>
        /// Gets or Sets the Machine Name
        /// </summary>
        public static string MachineName { get; set; }
        /// <summary>
        /// Default Log engine instance
        /// </summary>
        public static ILogEngine Log { get; private set; }
        /// <summary>
        /// Default Trace engine instance
        /// </summary>
        public static ITraceEngine Trace { get; private set; }
        /// <summary>
        /// Default Status engine instance
        /// </summary>
        public static IStatusEngine Status { get; private set; }
        /// <summary>
        /// App global data dictionary
        /// </summary>
        public static Dictionary<string, object> Data => _data ?? (_data = new Dictionary<string, object>());
        /// <summary>
        /// App global object data dictionary
        /// </summary>
        public static Dictionary<object, object> ObjectData => _objectData ?? (_objectData = new Dictionary<object, object>());
        /// <summary>
        /// App Settings
        /// </summary>
        public static KeyValueCollection Settings { get; private set; } = new KeyValueCollection(false);
        /// <summary>
        /// Default Injector instance
        /// </summary>
        public static InjectorEngine Injector { get; private set; } = new InjectorEngine();
        /// <summary>
        /// Task global data dictionary
        /// </summary>
        public static Dictionary<string, object> TaskData => _taskData?.Value ?? (_taskData = new AsyncLocal<Dictionary<string, object>>()).Value;
        /// <summary>
        /// Task global object data dictionary
        /// </summary>
        public static Dictionary<object, object> TaskObjectData => _taskObjectData?.Value ?? (_taskObjectData = new AsyncLocal<Dictionary<object, object>>()).Value;
        /// <summary>
        /// Current Framework version
        /// </summary>
        public static string FrameworkVersion { get; } = typeof(Core).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        /// <summary>
        /// Global TApp settings
        /// </summary>
        public static CoreSettings GlobalSettings
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_globalSettings != null) return _globalSettings;
                _globalSettings = GetSettings<CoreSettings>();
                _globalSettings.OnSettingsReload += (s, e) =>
                {
                    Status.Enabled = GlobalSettings.StatusEnabled;
                    Log.MaxLogLevel = (LogLevel)GlobalSettings.LogMaxLogLevel;
                    Log.Enabled = GlobalSettings.LogEnabled;
                    Trace.Enabled = GlobalSettings.TraceEnabled;
                    DebugMode = GlobalSettings.DebugMode;
                };
                return _globalSettings;
            }
        }
        /// <summary>
        /// Core services
        /// </summary>
        public static CoreServices Services { get; } = new CoreServices();
        /// <summary>
        /// Faster DateTime.Now
        /// </summary>
        public static DateTime Now => DateTime.SpecifyKind(DateTime.UtcNow + LocalUtcOffset, DateTimeKind.Local);
        /// <summary>
        /// Local UTC Offset
        /// </summary>
        public static TimeSpan LocalUtcOffset { get; private set; }
        /// <summary>
        /// Gets or Sets if the Library is in Debug mode
        /// </summary>
        public static bool DebugMode { get; set; }
		/// <summary>
		/// Gets or sets the service container factory.
		/// </summary>
		/// <value>The service container factory.</value>
		public static ServiceContainerFactoryDelegate ServiceContainerFactory { get; set; } = (service, initAction) => new ServiceContainer(service, initAction);
        /// <summary>
        /// Instance identifier
        /// </summary>
        public static Guid InstanceId { get; } = Guid.NewGuid();
        /// <summary>
        /// Get if the optimized version is loaded
        /// </summary>
        public static bool IsOptimizedVersion
        {
            get
            {
#if COMPATIBILITY
                return false;
#else
                return true;
#endif
            }
        }
		#endregion

		#region Init
		/// <summary>
		/// Initialize core framework
		/// </summary>
		/// <returns>The init.</returns>
		/// <param name="factories">Factories.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Init(Factories factories)
        {
            if (_initialized) return;
            _initialized = true;
            UpdateLocalUtcTimer = new Timer(UpdateLocalUtc, null, 0, 5000);
            Factory.SetFactories(factories);
            Status = Factory.CreateStatusEngine();
            Log = Factory.CreateLogEngine();
            Trace = Factory.CreateTraceEngine();
            factories.Init();
            GlobalSettings.ReloadSettings();
            DebugMode = DebugMode || GlobalSettings.DebugMode;
            if (DebugMode)
            {
                Log.InfoBasic("Core Init - Platform: {0} - OS: {1}", Factory.PlatformType, RuntimeInformation.OSDescription);
                Log.InfoBasic("Directory: {0}", Directory.GetCurrentDirectory());
            }
            AssemblyResolverManager.RegisterDomain();
            if (ServiceContainer.HasConsole)
                Log.AddConsoleStorage();

            if (Injector?.Settings != null && Injector.Settings.Interfaces.Count > 0)
            {
                //Init Log
                Log.LibDebug("Loading log engine configuration");
                var logStorages = Injector.GetNames<ILogStorage>();
                if (logStorages?.Any() == true)
                {
                    foreach (var name in logStorages)
                    {
                        if (!Settings[$"Core.Log.Storage.{name}.Enabled"].ParseTo(false)) continue;
                        Log.LibDebug("Loading log storage: {0}", name);
                        var lSto = Injector.New<ILogStorage>(name);
                        if (lSto is null)
                        {
                            Log.Warning("The Injection for \"{0}\" with name \"{1}\" is null.", typeof(ILogStorage).Name, name);
                            continue;
                        }
                        if (lSto.GetType() == typeof(ConsoleLogStorage))
                        {
                            Log.LibDebug("Console log storage already added, ignoring.");
                            continue;
                        }
                        Log.Storage.Add(lSto, Settings[$"Core.Log.Storage.{name}.LogLevel"].ParseTo(LogLevel.Error | LogLevel.Warning));
                    }
                }
                var logStorage = Log.Storage.Get(typeof(ConsoleLogStorage));
                if (!Settings["Core.Log.Storage.Console.Enabled"].ParseTo(true))
                    Log.Storage.Remove(logStorage);
                Log.Storage.ChangeStorageLogLevel(logStorage, Settings["Core.Log.Storage.Console.LogLevel"].ParseTo(LogStorageCollection.AllLevels));
                Log.MaxLogLevel = (LogLevel)GlobalSettings.LogMaxLogLevel;
                Log.Enabled = GlobalSettings.LogEnabled;

                //Init Trace
                Log.LibDebug("Loading trace engine configuration");
                var traceStorages = Injector.GetNames<ITraceStorage>();
                if (traceStorages?.Any() == true)
                {
                    foreach (var name in traceStorages)
                    {
                        if (!Settings[$"Core.Trace.Storage.{name}.Enabled"].ParseTo(false)) continue;
                        Log.LibDebug("Loading trace storage: {0}", name);
                        var lTrace = Injector.New<ITraceStorage>(name);
                        if (lTrace is null)
                        {
                            Log.Warning("The Injection for \"{0}\" with name \"{1}\" is null.", typeof(ITraceStorage).Name, name);
                            continue;
                        }
                        Trace.Storage.Add(lTrace);
                    }
                }
                Trace.Enabled = GlobalSettings.TraceEnabled;

                //Init Status
                Log.LibDebug("Loading status engine configuration");
                var statusTransports = Injector.GetNames<IStatusTransport>();
                if (statusTransports?.Any() == true)
                {
                    foreach (var name in statusTransports)
                    {
                        if (!Settings[$"Core.Status.Transport.{name}.Enabled"].ParseTo(false)) continue;
                        Log.LibDebug("Loading status transport: {0}", name);
                        var sTransport = Injector.New<IStatusTransport>(name);
                        if (sTransport is null)
                        {
                            Log.Warning("The Injection for \"{0}\" with name \"{1}\" is null.", typeof(IStatusTransport).Name, name);
                            continue;
                        }
                        Status.Transports.Add(sTransport);
                    }
                }
                Status.Enabled = GlobalSettings.StatusEnabled;
            }

            try
            {
                var allAssemblies = Factory.GetAllAssemblies();
                foreach(var asm in allAssemblies)
                {
                    try
                    {
                        if (asm.IsDynamic) continue;
                        if (asm.ReflectionOnly) continue;
                        foreach(var type in asm.ExportedTypes.AsParallel())
                        {
                            if (type.IsAbstract) continue;
                            if (!type.IsClass) continue;
                            if (!type.IsPublic) continue;
                            if (!type.IsVisible) continue;
                            var cStart = type.GetInterface(nameof(ICoreStart));
                            if (cStart == null) continue;
                            try
                            {
                                var instance = (ICoreStart)Activator.CreateInstance(type);
                                Log.LibDebug("Loading CoreStart from: {0}", instance);
                                instance.CoreInit(factories);
                            }
                            catch (Exception ex)
                            {
                                Log.Write(ex);
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            Status.Attach(() =>
            {
                if (Settings is null) return null;
                var sItem = new StatusItem
                {
                    Name = "Application Information\\Settings"
                };
                Settings.OrderBy(i => i.Key).Each(i => sItem.Values.Add(i.Key, i.Value));
                return sItem;
            });

            var onError = false;
            lock (OninitActions)
            {
                while (OninitActions.Count > 0)
                {
                    try
                    {
                        OninitActions.Dequeue()();
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        onError = true;
                    }
                }
            }
            Log.Start();

            if (onError)
                throw new Exception("Error initializing the application.");

            Log.LibDebug("Core has been initialized.");
        }
        /// <summary>
        /// Initialize with the default factories.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InitDefaults(bool setDirectoryToBaseAssembly = true)
        {
            var factories = new DefaultFactories
            {
                SetDirectoryToBaseAssembly = setDirectoryToBaseAssembly
            };
            Init(factories);
        }
        /// <summary>
        /// Initialize with the default factories.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InitDefaults(string configurationFile, bool setDirectoryToConfigurationFilePath = true)
        {
            var factories = new DefaultFactories
            {
                SetDirectoryToConfigurationFilePath = setDirectoryToConfigurationFilePath,
                ConfigurationFile = configurationFile
            };
            Init(factories);
        }
        /// <summary>
        /// Initialize with the default factories.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InitDefaults(Action<DefaultFactories> defaultFactoryAction)
        {
            var factories = new DefaultFactories();
            defaultFactoryAction(factories);
            Init(factories);
        }
        #endregion

        #region Run Service
        /// <summary>
        /// Starts the default container with the arguments
        /// </summary>
        /// <param name="args">Service arguments</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartContainer(string[] args)
        {
            InitDefaults();
			ServiceContainerFactory(null, null).Run(args);
        }
        /// <summary>
        /// Starts the default container with the arguments
        /// </summary>
        /// <param name="args">Service arguments</param>
        /// <param name="factories">Factories instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartContainer(string[] args, Factories factories)
        {
            Init(factories);
            ServiceContainerFactory(null, null).Run(args);
        }
        /// <summary>
        /// Run IService with the default container
        /// </summary>
        /// <param name="initAction">Action for initialize</param>
        /// <param name="serviceFunc">Func to get the IService instance</param>
        /// <param name="args">Service arguments</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunService(Action initAction, Func<IService> serviceFunc, string[] args)
        {
			ServiceContainerFactory(serviceFunc(), initAction).Run(args);
		}
        /// <summary>
        /// Run IService with the default container
        /// </summary>
        /// <param name="args">Service arguments</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunService<TIService>(string[] args) where TIService : class, new()
        {
            InitDefaults();
            var service = (IService)Injector.New<TIService>();
			ServiceContainerFactory(service, null).Run(args);
        }
        /// <summary>
        /// Run IService with the default container
        /// </summary>
        /// <param name="args">Service arguments</param>
        /// <param name="factories">Factories instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunService<TIService>(string[] args, Factories factories) where TIService : class, new()
        {
            Init(factories);
            var service = (IService)Injector.New<TIService>();
            ServiceContainerFactory(service, null).Run(args);
        }
        /// <summary>
        /// Run IService with the default container
        /// </summary>
        /// <param name="serviceFunc">Func to get the IService instance</param>
        /// <param name="args">Service arguments</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunService(Func<IService> serviceFunc, string[] args)
        {
            InitDefaults();
			ServiceContainerFactory(serviceFunc(), null).Run(args);
        }
        /// <summary>
        /// Run IService with the default container
        /// </summary>
        /// <param name="serviceFunc">Func to get the IService instance</param>
        /// <param name="args">Service arguments</param>
        /// <param name="factories">Factories instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunService(Func<IService> serviceFunc, string[] args, Factories factories)
        {
            Init(factories);
			ServiceContainerFactory(serviceFunc(), null).Run(args);
        }
        /// <summary>
        /// Run IService with the default container
        /// </summary>
        /// <param name="initAction">Action for initialize</param>
        /// <param name="servicesFunc">Func to get the IServices instances</param>
        /// <param name="args">Service arguments</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunService(Action initAction, Func<IService[]> servicesFunc, string[] args)
        {
			ServiceContainerFactory(new ServiceList(servicesFunc()), initAction).Run(args);
		}
        /// <summary>
        /// Run IService with the default container
        /// </summary>
        /// <param name="servicesFunc">Func to get the IServices instances</param>
        /// <param name="args">Service arguments</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunService(Func<IService[]> servicesFunc, string[] args)
        {
            InitDefaults();
			ServiceContainerFactory(new ServiceList(servicesFunc()), null).Run(args);
        }
        /// <summary>
        /// Run IService with the default container
        /// </summary>
        /// <param name="servicesFunc">Func to get the IServices instances</param>
        /// <param name="args">Service arguments</param>
        /// <param name="factories">Factories instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunService(Func<IService[]> servicesFunc, string[] args, Factories factories)
        {
            Init(factories);
			ServiceContainerFactory(new ServiceList(servicesFunc()), null).Run(args);
        }
        /// <summary>
        /// Run defined IService with the default container from Injector configuration 
        /// </summary>
        /// <param name="args"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunServicesFromInjector(string[] args)
        {
            InitDefaults();
            var services = Injector.GetAllInstances<IService>();
            var service = new ServiceList(services);
            ServiceContainerFactory(service, null).Run(args);
        }
        #endregion

        #region OnInitActions Queue
        /// <summary>
        /// Enqueue actions on Init
        /// </summary>
        /// <param name="action">Action to execute when the core is initialized</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunOnInit(Action action)
        {
            if (_initialized)
                action();
            else
                lock (OninitActions)
                    OninitActions.Enqueue(action);
        }
        /// <summary>
        /// Enqueue actions on Init
        /// </summary>
        /// <param name="taskFunc">Func to execute when the core is initialized</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunOnInit(Func<Task> taskFunc)
        {
            if (_initialized)
                taskFunc().WaitAsync();
            else
                lock (OninitActions)
                    OninitActions.Enqueue(() => taskFunc().WaitAsync());
        }
        #endregion  

        #region Time Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UpdateLocalUtc(object state)
        {
            LocalUtcOffset = TimeSpan.FromMinutes(Math.Round((DateTime.Now - DateTime.UtcNow).TotalMinutes));
        }
        #endregion

        #region Set Engine
        /// <summary>
        /// Sets a new Log Engine
        /// </summary>
        /// <param name="engine">Log engine instance, if is null then is ignored</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLogEngine(ILogEngine engine)
        {
            if (engine is null) return;
            var prevItems = Log?.GetPendingItems();
            Log = engine;
            if (prevItems != null)
                engine.EnqueueItemsArray(prevItems);
        }
        /// <summary>
        /// Sets a new Trace Engine
        /// </summary>
        /// <param name="engine">Trace engine intance, if is null then is ignored</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTraceEngine(ITraceEngine engine)
        {
            if (engine != null)
                Trace = engine;
        }
        /// <summary>
        /// Sets a new Status Engine
        /// </summary>
        /// <param name="engine">Status engine intance, if is null then is ignored</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetStatusEngine(IStatusEngine engine)
        {
            if (engine != null)
                Status = engine;
        }
        #endregion

        #region Load Settings
        /// <summary>
        /// Load application settings
        /// </summary>
        /// <param name="settings">Global settings object</param>
        /// <param name="environmentName">Environment name</param>
        /// <param name="machineName">Machine name</param>
        /// <param name="applicationName">Application name</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadSettings(GlobalSettings settings, string environmentName = null, string machineName = null, string applicationName = null)
            => Settings = settings?.GetItems(environmentName ?? EnvironmentName, machineName ?? MachineName, applicationName ?? ApplicationName);
        /// <summary>
        /// Load application settings
        /// </summary>
        /// <param name="settingsFilePath">Global settings file path</param>
        /// <param name="environmentName">Environment name</param>
        /// <param name="machineName">Machine name</param>
        /// <param name="applicationName">Application name</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadSettings(string settingsFilePath, string environmentName = null, string machineName = null, string applicationName = null)
        {
            if (settingsFilePath is null)
                throw new NullReferenceException("The settings file path is null.");

            environmentName = environmentName ?? EnvironmentName;
            machineName = machineName ?? MachineName;
            applicationName = applicationName ?? ApplicationName;

            settingsFilePath = settingsFilePath?.Replace("{EnvironmentName}", environmentName);
            settingsFilePath = settingsFilePath?.Replace("{MachineName}", machineName);
            settingsFilePath = settingsFilePath?.Replace("{ApplicationName}", applicationName);
            Ensure.ExistFile(settingsFilePath);
            var serializer = SerializerManager.GetByFileExtension(Path.GetExtension(settingsFilePath));
            Ensure.ReferenceNotNull(serializer, $"A serializer for file '{settingsFilePath}' was not found");
            GlobalSettings globalSettings;
            try
            {
                if (serializer is ITextSerializer txtSerializer)
                {
                    var fileContent = File.ReadAllText(settingsFilePath);
                    fileContent = ReplaceEnvironmentTemplate(fileContent);
                    globalSettings = txtSerializer.DeserializeFromString<GlobalSettings>(fileContent);
                }
                else
                {
                    globalSettings = serializer.DeserializeFromFile<GlobalSettings>(settingsFilePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"The Global settings file: {settingsFilePath} can't be deserialized.", ex);
            }
            try
            {
                LoadSettings(globalSettings, environmentName, machineName, applicationName);
                RebindSettings();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading the settings definitions.", ex);
            }
            //Checks if a reload time is set.
            if (GlobalSettings != null && GlobalSettings.SettingsReloadTimeInMinutes > 0)
            {
                Task.Delay(GlobalSettings.SettingsReloadTimeInMinutes * 60 * 1000).ContinueWith(t =>
                {
                    LoadSettings(settingsFilePath, environmentName, machineName, applicationName);
                });
            }
        }
        /// <summary>
        /// Rebind loaded application Settings
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RebindSettings()
        {
            GlobalSettings?.ReloadSettings();
            SettingsCache?.Values?.Each(v => v.ReloadSettings());
        }
        #endregion

        #region Load Injector
        /// <summary>
        /// Load injector
        /// </summary>
        /// <param name="settings">Global injector settings</param>
        /// <param name="environmentName">Environment name</param>
        /// <param name="applicationName">Application name</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadInjector(InjectorGlobalSettings settings, string environmentName = null, string applicationName = null)
            => Injector = new InjectorEngine(settings.GetSettings(environmentName ?? EnvironmentName, applicationName ?? ApplicationName));
        /// <summary>
        /// Load injector
        /// </summary>
        /// <param name="injectorFilePath">Global injector settings file path</param>
        /// <param name="environmentName">Environment name</param>
        /// <param name="machineName">Machine name</param>
        /// <param name="applicationName">Application name</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadInjector(string injectorFilePath, string environmentName = null, string machineName = null, string applicationName = null)
        {
            if (injectorFilePath is null)
                throw new NullReferenceException("The injector file path is null.");

            environmentName = environmentName ?? EnvironmentName;
            machineName = machineName ?? MachineName;
            applicationName = applicationName ?? ApplicationName;

            injectorFilePath = injectorFilePath?.Replace("{EnvironmentName}", environmentName);
            injectorFilePath = injectorFilePath?.Replace("{MachineName}", machineName);
            injectorFilePath = injectorFilePath?.Replace("{ApplicationName}", applicationName);
            Ensure.ExistFile(injectorFilePath);
            var serializer = SerializerManager.GetByFileExtension(Path.GetExtension(injectorFilePath));
            Ensure.ReferenceNotNull(serializer, $"A serializer for file '{injectorFilePath}' was not found");
            InjectorGlobalSettings globalSettings;
            try
            {
                if (serializer is ITextSerializer txtSerializer)
                {
                    var fileContent = File.ReadAllText(injectorFilePath);
                    fileContent = ReplaceEnvironmentTemplate(fileContent);
                    globalSettings = txtSerializer.DeserializeFromString<InjectorGlobalSettings>(fileContent);
                }
                else
                {
                    globalSettings = serializer.DeserializeFromFile<InjectorGlobalSettings>(injectorFilePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"The Injector settings file: {injectorFilePath} can't be deserialized.", ex);
            }
            try
            {
                LoadInjector(globalSettings, environmentName, applicationName);
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading the injector definitions.", ex);
            }
        }
        #endregion

        #region Settings
        /// <summary>
        /// Get and handles caches for Settings instances
        /// </summary>
        /// <typeparam name="T">Settings object type</typeparam>
        /// <returns>Settings object type instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetSettings<T>() where T : SettingsBase
            => (T)SettingsCache.GetOrAdd(typeof(T), t => (SettingsBase)Activator.CreateInstance(t));
        /// <summary>
        /// Apply Settings on object
        /// </summary>
        /// <returns>Settings object type instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ApplySettings(object instance)
            => SettingsEngine.ApplySettings(instance);
        #endregion

        #region Misc Methods
        /// <summary>
        /// One line if method
        /// </summary>
        /// <param name="conditionResult">Condition result</param>
        /// <param name="trueAction">Action to execute if the condition result is true</param>
        /// <param name="falseAction">Action to execute if the condition result is false</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Iif(this bool conditionResult, Action trueAction, Action falseAction = null)
        {
            if (conditionResult)
                trueAction?.Invoke();
            else
                falseAction?.Invoke();
        }
        /// <summary>
        /// One line if method
        /// </summary>
        /// <param name="condition">Condition function</param>
        /// <param name="trueAction">Action to execute if the condition result is true</param>
        /// <param name="falseAction">Action to execute if the condition result is false</param>
        /// <returns>Condition result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Iif(Func<bool> condition, Action trueAction, Action falseAction = null)
        {
            var res = condition();
            if (res)
                trueAction?.Invoke();
            else
                falseAction?.Invoke();
            return res;
        }
        /// <summary>
        /// One line if method
        /// </summary>
        /// <typeparam name="T">Func return type</typeparam>
        /// <param name="conditionResult">Condition result</param>
        /// <param name="trueAction">Func to execute if the condition result is true</param>
        /// <param name="falseAction">Func to execute if the condition result is false</param>
        /// <returns>Return value of one of the executed Func</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Iif<T>(this bool conditionResult, Func<T> trueAction, Func<T> falseAction = null) where T : class
            => conditionResult ? trueAction?.Invoke() : falseAction?.Invoke();
        /// <summary>
        /// One line if method
        /// </summary>
        /// <typeparam name="T">Func return type</typeparam>
        /// <param name="condition">Condition function</param>
        /// <param name="trueAction">Action to execute if the condition result is true</param>
        /// <param name="falseAction">Action to execute if the condition result is false</param>
        /// <returns>Return value of one of the executed Func</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Iif<T>(Func<bool> condition, Func<T> trueAction, Func<T> falseAction = null) where T : class
            => condition() ? trueAction?.Invoke() : falseAction?.Invoke();
        /// <summary>
        /// Get type from a type name, using internal cache for faster get.
        /// </summary>
        /// <param name="typeName">The assembly-qualified name of the type to get. See System.Type.AssemblyQualifiedName</param>
        /// <param name="throwOnError">true to throw an exception if the type cannot be found; false to return null.</param>
        /// <returns>The type with the specified name. If the type is not found, the throwOnError parameter specifies whether null is returned or an exception is thrown</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type GetType(string typeName, bool throwOnError = false)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                if (throwOnError)
                    throw new ArgumentNullException(nameof(typeName));
                return null;
            }
            var type = TypesCache.GetOrAdd(typeName, tName =>
            {
                try
                {
                    return Type.GetType(tName, throwOnError);
                }
                catch (Exception)
                {
                    if (throwOnError)
                        throw;
                    return null;
                }
            });
            if (type is null)
                Log.Warning("The type '{0}' couldn't be found or loaded, this could lead to exceptions.", typeName);
            return type;
        }
        /// <summary>
        /// Replace settings template on source string
        /// </summary>
        /// <param name="source">Source string to replace the settings template</param>
        /// <returns>Result of the replacement</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReplaceSettingsTemplate(string source)
        {
            var sb = new StringBuilder(source, source.Length * 2);
            foreach (var setting in Settings)
                sb = sb.Replace(SettingsTemplateFormat.ApplyFormat(setting.Key), setting.Value);
            return sb.ToString();
        }
        /// <summary>
        /// Replace environment template on source string
        /// </summary>
        /// <param name="source">Source string to replace the environment template</param>
        /// <returns>Result of the replacement</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReplaceEnvironmentTemplate(string source)
        {
            if (source is null) return null;
            if (source.IndexOf("{Env:", StringComparison.Ordinal) == -1 
                && source.IndexOf("{MACHINENAME}", StringComparison.Ordinal) == -1) return source;
            source = source.Replace("{MACHINENAME}", MachineName);
            var result = EnvironmentTemplateFormatRegex.Replace(source, match =>
            {
                if (match.Groups.Count < 2) return match.Value;
                var key = match.Groups[1].Value;
                var value = Environment.GetEnvironmentVariable(key);
                if (string.IsNullOrEmpty(value))
                {
                    string defaultValue = null;
                    DefaultEnvironmentVariables?.TryGetValue(key, out defaultValue);
                    if (defaultValue != null)
                        return defaultValue;
                }
                return value;
            });
            return result;
        }
        /// <summary>
        /// Replace encription template on source string
        /// </summary>
        /// <param name="source">Source string to replace the environment template</param>
        /// <returns>Result of the replacement</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReplaceEncriptionTemplate(string source)
        {
            if (source is null) return null;
            if (source.IndexOf("{Encripted:", StringComparison.Ordinal) == -1) return source;
            var result = EncriptionTemplateFormatRegex.Replace(source, match =>
            {
                if (match.Groups.Count < 2) return match.Value;
                var encValue = match.Groups[1].Value;
                return SymmetricProvider.Decrypt(encValue, EncryptionKey);
            });
            return result;
        }

        ///// <summary>
        ///// Encrypt a value with the default encryption key
        ///// </summary>
        ///// <param name="value">Value to encrypt</param>
        ///// <returns>Encrypted value</returns>
        //public static string EncryptValue(string value)
        //    => SymmetricProvider.Encrypt(value, EncryptionKey);
        ///// <summary>
        ///// Decrypt a value with the default encryption key
        ///// </summary>
        ///// <param name="value">Value to decrypt</param>
        ///// <returns>Encrypted value</returns>
        //public static string DecryptValue(string value)
        //    => SymmetricProvider.Decrypt(value, EncryptionKey);
        #endregion

        #region Dispose
        /// <summary>
        /// Dispose all engines
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Dispose()
        {
            Log?.Dispose();
            Trace?.Dispose();
            Status?.Dispose();
            Data?.Clear();
            Settings?.Clear();
            Injector?.Dispose();
            UpdateLocalUtcTimer?.Dispose();
        }
        #endregion
    }
}
