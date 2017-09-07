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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Log.Storages;
using TWCore.Diagnostics.Status;
using TWCore.Diagnostics.Trace;
using TWCore.Diagnostics.Trace.Storages;
using TWCore.Injector;
using TWCore.Reflection;
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
        private static readonly ConcurrentDictionary<Type, SettingsBase> SettingsCache = new ConcurrentDictionary<Type, SettingsBase>();
        private static readonly ConcurrentDictionary<string, Type> TypesCache = new ConcurrentDictionary<string, Type>();
        private static CoreSettings _globalSettings;
        [ThreadStatic] private static Dictionary<string, object> _threadData;
        [ThreadStatic] private static Dictionary<object, object> _threadObjectData;
        private static volatile bool _initialized;
        private static readonly Queue<Action> OninitActions = new Queue<Action>();

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
        public static Dictionary<string, object> Data { get; } = new Dictionary<string, object>();
        /// <summary>
        /// App global object data dictionary
        /// </summary>
        public static Dictionary<object, object> ObjectData { get; } = new Dictionary<object, object>();
        /// <summary>
        /// App Settings
        /// </summary>
        public static KeyValueCollection Settings { get; private set; } = new KeyValueCollection(false);
        /// <summary>
        /// Default Injector instance
        /// </summary>
        public static InjectorEngine Injector { get; private set; } = new InjectorEngine();
        /// <summary>
        /// Thread global data dictionary
        /// </summary>
        public static Dictionary<string, object> ThreadData => _threadData ?? (_threadData = new Dictionary<string, object>());

        /// <summary>
        /// Thread global object data dictionary
        /// </summary>
        public static Dictionary<object, object> ThreadObjectData => _threadObjectData ?? (_threadObjectData = new Dictionary<object, object>());

        /// <summary>
        /// Current Framework version
        /// </summary>
        public static string FrameworkVersion { get; } = typeof(Core).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
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
		#endregion

		#region Init
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Init(Factories factories)
        {
            if (_initialized) return;
            _initialized = true;
            UpdateLocalUtc();
            Factory.SetFactories(factories);
            Status = Factory.CreateStatusEngine();
            Log = Factory.CreateLogEngine();
            Trace = Factory.CreateTraceEngine();
            AssemblyResolverManager.RegisterDomain();
            factories.Init();
            GlobalSettings.ReloadSettings();
            DebugMode = DebugMode || GlobalSettings.DebugMode;
            if (DebugMode)
            {
                Log.InfoBasic("Core Init - Platform: {0} - OS: {1}", Factory.PlatformType, RuntimeInformation.OSDescription);
            }
            if (ServiceContainer.HasConsole)
                Log.AddConsoleStorage();

            if (Injector?.Settings != null && Injector.Settings.Interfaces.Count > 0)
            {
                //Init Log
                Log.Debug("Loading log engine configuration");
                var logStorages = Injector.GetNames<ILogStorage>();
                if (logStorages?.Any() == true)
                {
                    foreach (var name in logStorages)
                    {
                        if (!Settings[$"Core.Log.Storage.{name}.Enabled"].ParseTo(false)) continue;
                        Log.Debug("Loading log storage: {0}", name);
                        Log.Storage.Add(Injector.New<ILogStorage>(name), Settings[$"Core.Log.Storage.{name}.LogLevel"].ParseTo(LogLevel.Error | LogLevel.Warning));
                    }
                }
                var logStorage = Log.Storage.Get(typeof(ConsoleLogStorage));
                if (!Settings["Core.Log.Storage.Console.Enabled"].ParseTo(true))
                    Log.Storage.Remove(logStorage);
                Log.Storage.ChangeStorageLogLevel(logStorage, Settings["Core.Log.Storage.Console.LogLevel"].ParseTo(LogStorageCollection.AllLevels));
                Log.MaxLogLevel = (LogLevel)GlobalSettings.LogMaxLogLevel;
                Log.Enabled = GlobalSettings.LogEnabled;

                //Init Trace
                Log.Debug("Loading trace engine configuration");
                var traceStorages = Injector.GetNames<ITraceStorage>();
                if (traceStorages?.Any() == true)
                {
                    foreach (var name in traceStorages)
                    {
                        if (!Settings[$"Core.Trace.Storage.{name}.Enabled"].ParseTo(false)) continue;
                        Log.Debug("Loading trace storage: {0}", name);
                        Trace.Storage.Add(Injector.New<ITraceStorage>(name));
                    }
                }
                Trace.Enabled = GlobalSettings.TraceEnabled;

                //Init Status
                Log.Debug("Loading status engine configuration");
                var statusTransports = Injector.GetNames<IStatusTransport>();
                if (statusTransports?.Any() == true)
                {
                    foreach (var name in statusTransports)
                    {
                        if (!Settings[$"Core.Status.Transport.{name}.Enabled"].ParseTo(false)) continue;
                        Log.Debug("Loading status transport: {0}", name);
                        Status.Transports.Add(Injector.New<IStatusTransport>(name));
                    }
                }
                Status.Enabled = GlobalSettings.StatusEnabled;
            }

            try
            {
                var allAssemblies = Factory.GetAllAssemblies();
                var types = allAssemblies.SelectMany(a =>
                {
                    try
                    {
                        return a.DefinedTypes;
                    }
                    catch
                    {
                        // ignored
                    }
                    return new TypeInfo[0];
                }).Where(t => !t.IsAbstract && t.IsClass && t.ImplementedInterfaces.Contains(typeof(ICoreStart))).ToArray();
                if (types?.Any() == true)
                {
                    foreach (var type in types)
                    {
                        try
                        {
                            var instance = (ICoreStart)Activator.CreateInstance(type.AsType());
                            Log.Debug("Loading CoreStart from: {0}", instance);
                            instance.CoreInit(factories);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            Status.Attach(() =>
            {
                if (Settings == null) return null;
                var sItem = new StatusItem
                {
                    Name = "Settings"
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

            factories.Thread.Sleep(25);
            var dlog = (Log as DefaultLogEngine);
            dlog?.LogDoneTask.WaitAsync();

            if (onError)
                throw new Exception("Error initializing the application.");
        }

        /// <summary>
        /// Initialize with the default factories.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InitDefaults()
        {
            Init(new DefaultFactories());
        }
        /// <summary>
        /// Initialize with the default factories configured for AspNet.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InitAspNet()
        {
            var factories = new DefaultFactories
            {
                SetDirectoryToBaseAssembly = false
            };
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
            var service = (IService)Activator.CreateInstance<TIService>();
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
        #endregion  

        #region Time Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void UpdateLocalUtc(Task tsk = null)
        {
            LocalUtcOffset = TimeSpan.FromMinutes(Math.Round((DateTime.Now - DateTime.UtcNow).TotalMinutes));
            Task.Delay(5000).ContinueWith(UpdateLocalUtc);
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
            if (engine == null) return;
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
        /// <param name="applicationName">Application name</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadSettings(GlobalSettings settings, string environmentName = null, string applicationName = null)
            => Settings = settings?.GetItems(environmentName ?? EnvironmentName, applicationName ?? ApplicationName);
        /// <summary>
        /// Load application settings
        /// </summary>
        /// <param name="settingsFilePath">Global settings file path</param>
        /// <param name="environmentName">Environment name</param>
        /// <param name="applicationName">Application name</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadSettings(string settingsFilePath, string environmentName = null, string applicationName = null)
        {
            settingsFilePath = settingsFilePath?.Replace("{EnvironmentName}", EnvironmentName);
            settingsFilePath = settingsFilePath?.Replace("{MachineName}", MachineName);
            settingsFilePath = settingsFilePath?.Replace("{ApplicationName}", ApplicationName);
            Ensure.ExistFile(settingsFilePath);
            var serializer = SerializerManager.GetByFileExtension(Path.GetExtension(settingsFilePath));
            Ensure.ReferenceNotNull(serializer, $"A serializer for file '{settingsFilePath}' was not found");
            GlobalSettings globalSettings;
            try
            {
                globalSettings = serializer.DeserializeFromFile<GlobalSettings>(settingsFilePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"The Global settings file: {settingsFilePath} can't be deserialized.", ex);
            }
            try
            {
                LoadSettings(globalSettings, environmentName, applicationName);
                RebindSettings();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading the settings definitions.", ex);
            }
            //Checks if a reload time is set.
            if (GlobalSettings != null && GlobalSettings.SettingsReloadTimeInMinutes > 0)
            {
                Task.Delay(TimeSpan.FromMinutes(GlobalSettings.SettingsReloadTimeInMinutes)).ContinueWith(t =>
                {
                    LoadSettings(settingsFilePath, environmentName, applicationName);
                });
            }
        }
        /// <summary>
        /// Replace settings template on source string
        /// </summary>
        /// <param name="source">Source string to replace the settings template</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReplaceSettingsTemplate(string source)
        {
            StringBuilder sb = new StringBuilder(source, source.Length * 2);
            foreach (var setting in Settings)
                sb = sb.Replace(SettingsTemplateFormat.ApplyFormat(setting.Key), setting.Value);
            return sb.ToString();
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
        /// <param name="settingsFilePath">Global injector settings file path</param>
        /// <param name="environmentName">Environment name</param>
        /// <param name="applicationName">Application name</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadInjector(string settingsFilePath, string environmentName = null, string applicationName = null)
        {
            settingsFilePath = settingsFilePath?.Replace("{EnvironmentName}", EnvironmentName);
            settingsFilePath = settingsFilePath?.Replace("{MachineName}", MachineName);
            settingsFilePath = settingsFilePath?.Replace("{ApplicationName}", ApplicationName);
            Ensure.ExistFile(settingsFilePath);
            var serializer = SerializerManager.GetByFileExtension(Path.GetExtension(settingsFilePath));
            Ensure.ReferenceNotNull(serializer, $"A serializer for file '{settingsFilePath}' was not found");
            InjectorGlobalSettings globalSettings;
            try
            {
                globalSettings = serializer.DeserializeFromFile<InjectorGlobalSettings>(settingsFilePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"The Injector settings file: {settingsFilePath} can't be deserialized.", ex);
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
                    throw new ArgumentNullException("The TypeName is null");
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
            if (type == null)
                Log.Warning("The type '{0}' couldn't be found or loaded, this could lead to exceptions.", typeName);
            return type;
        }
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
        }
        #endregion
    }
}
