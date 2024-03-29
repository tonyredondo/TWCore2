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
using TWCore.Diagnostics.Counters;
using TWCore.Diagnostics.Counters.Storages;
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
        private static readonly SymmetricKeyProvider SymmetricProvider = new();
        private static readonly Regex EnvironmentTemplateFormatRegex = new(@"{Env:([A-Za-z0-9_ |+-\\*/_!""$% &\(\) = '?¡¿.:,;<>]*)}", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex EncriptionTemplateFormatRegex = new(@"{Encripted:([A-Za-z0-9_ |+-\\*/_!""$% &\(\) = '?¡¿.:,;<>]*)}", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly ConcurrentDictionary<string, Type> TypesCache = new();
        private static readonly AsyncLocal<string> _contextGroupName = new();
        private static CoreSettings _globalSettings;
        private static Dictionary<string, object> _data;
        private static Dictionary<object, object> _objectData;
        private static AsyncLocal<Dictionary<string, object>> _taskData;
        private static AsyncLocal<Dictionary<object, object>> _taskObjectData;
        private static int _initialized;
        private static readonly Queue<Action> OninitActions = new();
        private static Timer _updateLocalUtcTimer;
        private static TimeSpan _localUtcOffset;
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
        /// Default Counters engine instance
        /// </summary>
        public static ICountersEngine Counters { get; private set; }
        /// <summary>
        /// App global data dictionary
        /// </summary>
        public static Dictionary<string, object> Data => _data ??= new Dictionary<string, object>();
        /// <summary>
        /// App global object data dictionary
        /// </summary>
        public static Dictionary<object, object> ObjectData => _objectData ??= new Dictionary<object, object>();
        /// <summary>
        /// App Settings
        /// </summary>
        public static KeyValueCollection Settings { get; private set; } = new(false);
        /// <summary>
        /// Default Injector instance
        /// </summary>
        public static InjectorEngine Injector { get; private set; } = new();
        /// <summary>
        /// Task global data dictionary
        /// </summary>
        public static Dictionary<string, object> TaskData
        {
            get
            {
                _taskData ??= new AsyncLocal<Dictionary<string, object>>
                {
                    Value = new Dictionary<string, object>()
                };
                return _taskData.Value;
            }
        }
        /// <summary>
        /// Task global object data dictionary
        /// </summary>
        public static Dictionary<object, object> TaskObjectData
        {
            get
            {
                _taskObjectData ??= new AsyncLocal<Dictionary<object, object>>
                {
                    Value = new Dictionary<object, object>()
                };

                return _taskObjectData.Value;
            }
        }
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
        public static CoreServices Services { get; } = new();
        /// <summary>
        /// Faster DateTime.Now
        /// </summary>
        public static DateTime Now => DateTime.SpecifyKind(DateTime.UtcNow.Add(_localUtcOffset), DateTimeKind.Local);
        /// <summary>
        /// Local UTC Offset
        /// </summary>
        public static TimeSpan LocalUtcOffset => _localUtcOffset;
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
        /// Instance identifier string
        /// </summary>
        public static string InstanceIdString { get; private set; }
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
        /// <summary>
        /// Process id
        /// </summary>
        public static int ProcessId { get; } = System.Diagnostics.Process.GetCurrentProcess().Id;
        /// <summary>
        /// Context group name
        /// </summary>
        public static string ContextGroupName
        {
            get => _contextGroupName.Value;
            set => _contextGroupName.Value = value;
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
            if (Interlocked.CompareExchange(ref _initialized, 1, 0) == 1) return;
            InstanceIdString = InstanceId.ToString();
            _updateLocalUtcTimer = new Timer(UpdateLocalUtc, null, 0, 5000);
            Factory.SetFactories(factories);
            var (coreInits, coreInitsExceptions) = GetCoreInits();

            #region CoreStart.BeforeInit
            foreach(var ci in coreInits)
            {
                try
                {
                    ci.BeforeInit();
                }
                catch
                {
                    //
                }
            }
            #endregion

            Status = Factory.CreateStatusEngine();
            Log = Factory.CreateLogEngine();
            Trace = Factory.CreateTraceEngine();
            Counters = Factory.CreateCountersEngine();
            factories.Init();
            GlobalSettings.ReloadSettings();
            DebugMode = DebugMode || GlobalSettings.DebugMode;
            if (DebugMode)
            {
                Log.InfoBasic("Core Init - Platform: {0} - OS: {1}", Factory.PlatformType, RuntimeInformation.OSDescription);
                Log.InfoBasic("Directory: {0}", Directory.GetCurrentDirectory());
            }
            AssemblyResolverManager.RegisterDomain();

            #region CoreStart.AfterFactoryInit
            foreach (var ci in coreInits)
            {
                try
                {
                    Log.LibDebug("CoreStart AfterFactoryInit from: {0}", ci);
                    ci.AfterFactoryInit(factories);
                }
                catch
                {
                    //
                }
            }
            #endregion

            if (ServiceContainer.HasConsole)
                Log.AddConsoleStorage();

            #region Log, Trace and Status Injector Load
            if (Injector?.Settings?.Interfaces?.Count > 0)
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
                        if (Injector.New<ILogStorage>(name) is { } lStorage)
                        {
                            if (lStorage is ConsoleLogStorage)
                            {
                                Log.LibDebug("Console log storage already added, ignoring.");
                                continue;
                            }
                         
                            Log.Storages.Add(lStorage, Settings[$"Core.Log.Storage.{name}.LogLevel"].ParseTo(LogLevel.Error | LogLevel.Warning));
                        }
                        else
                        {
                            Log.Warning("The Injection for \"{0}\" with name \"{1}\" is null.", nameof(ILogStorage), name);
                        }
                    }
                }
                var logStorage = Log.Storages.Get(typeof(ConsoleLogStorage));
                if (!Settings["Core.Log.Storage.Console.Enabled"].ParseTo(true))
                    Log.Storages.Remove(logStorage);
                Log.Storages.ChangeStorageLogLevel(logStorage, Settings["Core.Log.Storage.Console.LogLevel"].ParseTo(LogStorageCollection.AllLevels));
                Log.MaxLogLevel = (LogLevel)GlobalSettings.LogMaxLogLevel;
                Log.Enabled = GlobalSettings.LogEnabled;

                //Init Trace
                Log.LibDebug("Loading trace engine configuration");
                var traceStorages = Injector.GetNames<ITraceStorage>();
                if (traceStorages?.Length > 0)
                {
                    foreach (var name in traceStorages)
                    {
                        if (!Settings[$"Core.Trace.Storage.{name}.Enabled"].ParseTo(false)) continue;
                        Log.LibDebug("Loading trace storage: {0}", name);
                        if (Injector.New<ITraceStorage>(name) is { } traceStorage)
                        {
                            Trace.Storages.Add(traceStorage);
                        }
                        else
                        {
                            Log.Warning("The Injection for \"{0}\" with name \"{1}\" is null.", nameof(ITraceStorage), name);
                        }
                    }
                }
                Trace.Enabled = GlobalSettings.TraceEnabled;

                //Init Status
                Log.LibDebug("Loading status engine configuration");
                var statusTransports = Injector.GetNames<IStatusTransport>();
                if (statusTransports?.Length > 0)
                {
                    foreach (var name in statusTransports)
                    {
                        if (!Settings[$"Core.Status.Transport.{name}.Enabled"].ParseTo(false)) continue;
                        Log.LibDebug("Loading status transport: {0}", name);
                        if (Injector.New<IStatusTransport>(name) is { } statusTransport)
                        {
                            Status.Transports.Add(statusTransport);
                        }
                        else
                        {
                            Log.Warning("The Injection for \"{0}\" with name \"{1}\" is null.", nameof(IStatusTransport), name);
                        }
                    }
                }
                Status.Enabled = GlobalSettings.StatusEnabled;

                //Init Counters
                Log.LibDebug("Loading counters engine configuration");
                var countersStorages = Injector.GetNames<ICountersStorage>();
                if (countersStorages?.Length > 0)
                {
                    foreach(var name in countersStorages)
                    {
                        if (!Settings[$"Core.Counters.Storage.{name}.Enabled"].ParseTo(false)) continue;
                        Log.LibDebug("Loading counter storage: {0}", name);
                        if (Injector.New<ICountersStorage>(name) is { } countersStorage)
                        {
                            Counters.Storages.Add(countersStorage);
                        }
                        else
                        {
                            Log.Warning("The Injection for \"{0}\" with name \"{1}\" is null.", nameof(ICountersStorage), name);
                        }
                    }
                }
            }
            #endregion
            
            #region CoreStart.FinalizingInit
            foreach (var ci in coreInits)
            {
                try
                {
                    Log.LibDebug("CoreStart FinalizingInit from: {0}", ci);
                    ci.FinalizingInit(factories);
                }
                catch
                {
                    //
                }
            }
            #endregion

            #region CoreStart Exceptions
            foreach (var ex in coreInitsExceptions)
                Log.Write(LogLevel.Warning, ex);
            #endregion
            
            Task.Run(() =>
            {
                Status.Attach(() =>
                {
                    if (Settings is null) return null;
                    var sItem = new StatusItem
                    {
                        Name = "Application Information\\Settings"
                    };
                    foreach (var i in Settings.OrderBy(i => i.Key))
                        sItem.Values.Add(i.Key, i.Value);
                    return sItem;
                });
            });

            #region Run On Init Actions
            var onError = false;
            var lstExceptions = new List<Exception>();
            lock (OninitActions)
            {
#if NETCOREAPP3_1_OR_GREATER
                while (OninitActions.TryDequeue(out var action))
                {
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        lstExceptions.Add(ex);
                        onError = true;
                    }
                }
#else
                while (OninitActions.Count > 0)
                {
                    try
                    {
                        OninitActions.Dequeue()();
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        lstExceptions.Add(ex);
                        onError = true;
                    }
                }
#endif
            }
            #endregion

            Counters.Start();
            Log.Start();

            if (onError)
                throw new AggregateException("Error initializing the application.", lstExceptions);

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

        #region Private Methods
        private static (List<ICoreStart>, List<Exception>) GetCoreInits()
        {
            var lst = new List<ICoreStart>();
            var exs = new List<Exception>();
            try
            {
                var allAssemblies = Factory.GetAllAssemblies();
                foreach (var asm in allAssemblies)
                {
                    try
                    {
                        if (asm.IsDynamic) continue;
                        if (asm.ReflectionOnly) continue;
                        foreach (var type in asm.ExportedTypes.AsParallel())
                        {
                            if (type.IsAbstract || !type.IsClass || !type.IsPublic || !type.IsVisible) continue;
                            var cStart = type.GetInterface(nameof(ICoreStart));
                            if (cStart == null) continue;
                            try
                            {
                                lst.Add((ICoreStart)Activator.CreateInstance(type));
                            }
                            catch (Exception ex)
                            {
                                exs.Add(ex);
                            }
                        }
                    }
                    catch
                    {
                        //
                    }
                }
            }
            catch (Exception ex)
            {
                exs.Add(ex);
            }
            return (lst, exs);
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
            if (Interlocked.CompareExchange(ref _initialized, 1, 1) == 1)
            {
                action();
            }
            else
            {
                lock (OninitActions)
                {
                    OninitActions.Enqueue(action);
                }
            }
        }
        /// <summary>
        /// Enqueue actions on Init
        /// </summary>
        /// <param name="taskFunc">Func to execute when the core is initialized</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunOnInit(Func<Task> taskFunc)
        {
            if (Interlocked.CompareExchange(ref _initialized, 1, 1) == 1)
            {
                SynchronizationContext context = null;
                try
                {
                    context = SynchronizationContext.Current;
                    taskFunc?.Invoke().GetAwaiter().GetResult();
                }
                finally
                {
                    if (context is not null)
                    {
                        SynchronizationContext.SetSynchronizationContext(context);
                    }
                }
            }
            else
            {
                lock (OninitActions)
                {
                    OninitActions.Enqueue(() =>
                    {
                        SynchronizationContext context = null;
                        try
                        {
                            context = SynchronizationContext.Current;
                            taskFunc?.Invoke().GetAwaiter().GetResult();
                        }
                        finally
                        {
                            if (context is not null)
                            {
                                SynchronizationContext.SetSynchronizationContext(context);
                            }
                        }
                    });
                }
            }
        }
        #endregion  

        #region Time Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UpdateLocalUtc(object state)
        {
            _localUtcOffset = TimeSpan.FromMinutes(Math.Round((DateTime.Now - DateTime.UtcNow).TotalMinutes));
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
            Log = engine;
        }
        /// <summary>
        /// Sets a new Trace Engine
        /// </summary>
        /// <param name="engine">Trace engine instance, if is null then is ignored</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTraceEngine(ITraceEngine engine)
        {
            if (engine is null) return;
            Trace = engine;
        }
        /// <summary>
        /// Sets a new Status Engine
        /// </summary>
        /// <param name="engine">Status engine instance, if is null then is ignored</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetStatusEngine(IStatusEngine engine)
        {
            if (engine is null) return;
            Status = engine;
        }
        /// <summary>
        /// Sets a new Counters Engine
        /// </summary>
        /// <param name="engine">Counters engine instance, if is null then is ignored</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCountersEngine(ICountersEngine engine)
        {
            if (engine is null) return;
            Counters = engine;
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

            environmentName ??= EnvironmentName;
            machineName ??= MachineName;
            applicationName ??= ApplicationName;

            settingsFilePath = settingsFilePath.Replace("{EnvironmentName}", environmentName);
            settingsFilePath = settingsFilePath.Replace("{MachineName}", machineName);
            settingsFilePath = settingsFilePath.Replace("{ApplicationName}", applicationName);
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
            if (GlobalSettings?.SettingsReloadTimeInMinutes > 0)
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
            foreach (var setting in InternalSettingsCache.Values)
            {
                setting.ReloadSettings();
            }
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

            environmentName ??= EnvironmentName;
            machineName ??= MachineName;
            applicationName ??= ApplicationName;

            injectorFilePath = injectorFilePath.Replace("{EnvironmentName}", environmentName);
            injectorFilePath = injectorFilePath.Replace("{MachineName}", machineName);
            injectorFilePath = injectorFilePath.Replace("{ApplicationName}", applicationName);
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
            => InternalSettingsCache<T>.Instance;
        /// <summary>
        /// Apply Settings on object
        /// </summary>
        /// <returns>Settings object type instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ApplySettings(object instance)
            => SettingsEngine.ApplySettings(instance);
        
        private static class InternalSettingsCache
        {
            public static ConcurrentBag<SettingsBase> Values { get; } = new();
        }

        private static class InternalSettingsCache<T>
            where T: SettingsBase
        {
            public static T Instance { get; }

            static InternalSettingsCache()
            {
                Instance = (T)Activator.CreateInstance(typeof(T));
                InternalSettingsCache.Values.Add(Instance);
            }
        }

        #endregion

        #region Misc Methods
        /// <summary>
        /// Sets the context groupname if is null or empty
        /// </summary>
        /// <param name="contextGroupName">Context group name</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetContextGroupNameIfIsNullOrEmpty(string contextGroupName)
        {
            if (string.IsNullOrEmpty(_contextGroupName.Value))
                _contextGroupName.Value = contextGroupName;
        }
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
            Counters?.Dispose();
            Data?.Clear();
            Settings?.Clear();
            Injector?.Dispose();
            _updateLocalUtcTimer?.Dispose();
        }
        #endregion
    }
}
