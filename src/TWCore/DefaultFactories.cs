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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TWCore.Diagnostics.Status;
using TWCore.Reflection;
using TWCore.Services;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore
{
    /// <inheritdoc />
    /// <summary>
    /// Default Factories
    /// </summary>
    public sealed class DefaultFactories : Factories
    {
        private const string EnvironmentVariableName = "TWCORE_ENVIRONMENT";
        private const string MachineVariableName = "TWCORE_MACHINE";
        private const string ForceEnvironmentVariableName = "TWCORE_FORCE_ENVIRONMENT";
        private const string ForceMachineVariableName = "TWCORE_FORCE_MACHINE";
        private Assembly[] _assemblies;
        private bool _usedResolver;
        private static CpuUsage _cpuUsage;

        [DllImport("rpcrt4.dll", SetLastError = true)]
        private static extern int UuidCreateSequential(out Guid guid);

        #region Properties
        /// <summary>
        /// Sets the current directory to the base assembly location
        /// </summary>
        public bool SetDirectoryToBaseAssembly { get; set; } = true;
        /// <summary>
        /// Sets the current directory to the configuration file path
        /// </summary>
        public bool SetDirectoryToConfigurationFilePath { get; set; } = false;
        /// <summary>
        /// Gets or Sets the configuration file
        /// </summary>
        public string ConfigurationFile { get; set; }
        /// <summary>
        /// Bool indicating if all assemblies were loaded.
        /// </summary>
        private bool AllAssembliesLoaded { get; set; }
        #endregion

        #region .ctor
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DefaultFactories()
        {
            Accessors = new CompleteAccessorsFactory();
            GetAssemblies = DefaultGetAssemblies;
            GetAllAssemblies = DefaultGetAllAssemblies;
            if (File.Exists($"{Core.ApplicationName}.config.json"))
                ConfigurationFile = $"{Core.ApplicationName}.config.json";
            else if (File.Exists($"{Core.ApplicationName}.json"))
                ConfigurationFile = $"{Core.ApplicationName}.json";
            else if (File.Exists(Path.Combine(AppContext.BaseDirectory, $"{Core.ApplicationName}.config.json")))
                ConfigurationFile = Path.Combine(AppContext.BaseDirectory, $"{Core.ApplicationName}.config.json");
            else if (File.Exists(Path.Combine(AppContext.BaseDirectory, $"{Core.ApplicationName}.json")))
                ConfigurationFile = Path.Combine(AppContext.BaseDirectory, $"{Core.ApplicationName}.json");
        }
        #endregion

        #region Public Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Init()
        {
            var line = Environment.CommandLine;
            var asmLocation = Path.GetFullPath(Assembly.GetEntryAssembly().Location);
            var argumentLine = line.Replace(asmLocation, string.Empty);
            var cleanArguments = GetArguments(argumentLine);

            if (SetDirectoryToBaseAssembly || cleanArguments.Contains("service-run", StringComparer.OrdinalIgnoreCase))
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            if (SetDirectoryToConfigurationFilePath && (Path.IsPathRooted(ConfigurationFile) || !string.IsNullOrEmpty(Path.GetDirectoryName(ConfigurationFile))))
                Directory.SetCurrentDirectory(Path.GetDirectoryName(ConfigurationFile));
            
            if (Factory.PlatformType == PlatformType.Windows)
                SequentialGuidGenerator = GetSequentialGuid;

            Core.Log.ItemFactory = Factory.CreateLogItem;
            Core.Trace.ItemFactory = Factory.CreateTraceItem;
            Core.MachineName = GetValueFromEnvironment(MachineVariableName) ?? Environment.MachineName;
            Core.ApplicationName = Assembly.GetEntryAssembly().GetName().Name;
            Core.ApplicationDisplayName = Core.ApplicationName;
            Core.EnvironmentName = GetValueFromEnvironment(EnvironmentVariableName);

            Task.Run(() => AttachStatus());

            ServiceContainer.RegisterParametersHandler("configfile=[Path]",
                                                       "Load the application using other configuration file.",
                                                       obj => { });

            
            var argConfigFile = cleanArguments.FirstOrDefault(a => a.StartsWith("configfile=", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(argConfigFile))
            {
                argConfigFile = argConfigFile.Substring(11).Replace("'", string.Empty).Trim();
                if (!LoadConfigFile(argConfigFile, cleanArguments))
                    throw new FileNotFoundException($"Configuration file: '{argConfigFile}' couldn't be loaded. CommandLine: {argumentLine}");
            }
            else if (ConfigurationFile != null)
            {
                if (!LoadConfigFile(ConfigurationFile, cleanArguments))
                    throw new FileNotFoundException($"Configuration file: '{ConfigurationFile}' couldn't be loaded.");
            }
            else if (!LoadConfigFile($"{Core.ApplicationName}.config.json", cleanArguments))
            {
                LoadConfigFile($"{Core.ApplicationName}.json", cleanArguments);
            }
            SetLargeObjectHeapCompactTimeout();
        }
        #endregion


        #region Private factory methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Assembly[] DefaultGetAssemblies()
        {
            if (_assemblies != null) return _assemblies;
            _assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(d =>
            {
                if (d.IsDynamic) return false;
                var assemblyName = d.GetName();
                return !IsExcludedAssembly(assemblyName.Name);
            }).DistinctBy(i => i.Location).ToArray();
            return _assemblies;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Assembly[] DefaultGetAllAssemblies()
        {
            var resolver = AssemblyResolverManager.GetAssemblyResolver();
            if (!AllAssembliesLoaded || (!_usedResolver && resolver != null))
            {
                if (resolver != null)
                {
                    _usedResolver = true;
                }
                else
                {
                    var loaded = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var file in Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            var name = AssemblyName.GetAssemblyName(file);
                            if (IsExcludedAssembly(name.Name)) continue;
                            if (loaded.All((l, fullName) => l.FullName != fullName, name.FullName))
                                AppDomain.CurrentDomain.Load(name);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                AllAssembliesLoaded = true;
                _assemblies = null;
                return DefaultGetAssemblies();
            }
            return _assemblies;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Guid GetSequentialGuid()
        {
            //https://blogs.msdn.microsoft.com/dbrowne/2012/07/03/how-to-generate-sequential-guids-for-sql-server-in-net/
            UuidCreateSequential(out var guid);
            var s = guid.ToByteArray();
            var t = new byte[16];
            t[3] = s[0];
            t[2] = s[1];
            t[1] = s[2];
            t[0] = s[3];
            t[5] = s[4];
            t[4] = s[5];
            t[7] = s[6];
            t[6] = s[7];
            t[8] = s[8];
            t[9] = s[9];
            t[10] = s[10];
            t[11] = s[11];
            t[12] = s[12];
            t[13] = s[13];
            t[14] = s[14];
            t[15] = s[15];
            return new Guid(t);
        }

        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<string> GetArguments(string argumentLine)
        {
            var originalArguments = argumentLine.Split(new[] { '/' }, StringSplitOptions.None).Skip(1);
            var cleanArguments = new List<string>();
            var keepIdx = false;
            foreach (var arg in originalArguments)
            {
                if (keepIdx)
                    cleanArguments[cleanArguments.Count - 1] += "/" + arg;
                else
                    cleanArguments.Add(arg);

                if (arg.IndexOf("'", StringComparison.Ordinal) > -1)
                    keepIdx = !keepIdx;
            }

            for (var i = 0; i < cleanArguments.Count; i++)
                cleanArguments[i] = cleanArguments[i]?.Trim();

            return cleanArguments;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsExcludedAssembly(string assemblyName)
        {
            return assemblyName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("Libuv", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.Equals("NETStandard", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.Equals("System", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("SOS.NETCore", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("Newtonsoft.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("SQLitePCLRaw.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("StackExchange.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("RabbitMQ.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.Equals("mscorlib", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("Remotion.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("Runtime.", StringComparison.OrdinalIgnoreCase);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool LoadConfigFile(string configFile, List<string> args)
        {
            ServiceContainer.RegisterParametersHandler("environment=[Environment]",
                "Force an environment to run the application.",
                obj => { obj.ShouldEndExecution = false; });
            ServiceContainer.RegisterParametersHandler("machinename=[MachineName]",
                "Force a machine name to run the application.",
                obj => { obj.ShouldEndExecution = false; });
            ServiceContainer.RegisterParametersHandler("noconsole",
                "Remove the console output/input.",
                obj => { obj.ShouldEndExecution = false; });

            var envConfigFile = args?.FirstOrDefault(a => a.StartsWith("environment=", StringComparison.OrdinalIgnoreCase));
            envConfigFile = envConfigFile?.Substring(12).Replace("'", string.Empty).Trim();

            var mnameConfigFile = args?.FirstOrDefault(a => a.StartsWith("machinename=", StringComparison.OrdinalIgnoreCase));
            mnameConfigFile = mnameConfigFile?.Substring(12).Replace("'", string.Empty).Trim();

            if (args?.Any(a => string.Equals(a, "noconsole", StringComparison.Ordinal)) == true)
                ServiceContainer.HasConsole = false;

            if (envConfigFile.IsNotNullOrWhitespace())
                Core.EnvironmentName = envConfigFile;
            if (mnameConfigFile.IsNotNullOrWhitespace())
                Core.MachineName = mnameConfigFile;

            if (!File.Exists(configFile))
            {
                return false;
            }
            try
            {
                var jser = JsonSerializer.CreateDefault();
                var fSettings = jser.Deserialize<FactorySettings>(new JsonTextReader(new StreamReader(configFile)));
                if (fSettings == null) return true;

                if (fSettings.Core != null)
                {
                    if (fSettings.Core.EnvironmentName.IsNotNullOrWhitespace() && envConfigFile.IsNullOrWhitespace())
                        Core.EnvironmentName = fSettings.Core.EnvironmentName;
                    if (fSettings.Core.MachineName.IsNotNullOrWhitespace() && mnameConfigFile.IsNullOrWhitespace())
                        Core.MachineName = fSettings.Core.MachineName;
                    if (fSettings.Core.ApplicationName.IsNotNullOrWhitespace())
                        Core.ApplicationName = fSettings.Core.ApplicationName;
                    if (fSettings.Core.ApplicationDisplayName.IsNotNullOrWhitespace())
                        Core.ApplicationDisplayName = fSettings.Core.ApplicationDisplayName;
                }

                var forcedEnvironmentVariable = GetValueFromEnvironment(ForceEnvironmentVariableName);
                if (forcedEnvironmentVariable != null && envConfigFile.IsNullOrWhitespace())
                {
					Core.Log.Warning("Environment name forced by EnvironmentVariable, previous value: {0}, new value: {1}", Core.EnvironmentName ?? "(null)", forcedEnvironmentVariable);
                    Core.EnvironmentName = forcedEnvironmentVariable;
                }
                var forcedMachineVariable = GetValueFromEnvironment(ForceMachineVariableName);
                if (forcedMachineVariable != null && mnameConfigFile.IsNullOrWhitespace())
                {
					Core.Log.Warning("Machine name forced by EnvironmentVariable, previous value: {0}, new value: {1}", Core.MachineName ?? "(null)", forcedMachineVariable);
                    Core.MachineName = forcedMachineVariable;
                }

                if (fSettings.Core != null && fSettings.Core.SettingsFile.IsNotNullOrWhitespace())
                {
                    var settingsFile = ResolveLowLowFilePath(fSettings.Core.SettingsFile);
                    if (settingsFile == null)
                        throw new FileNotFoundException("The settings file: " + fSettings.Core.SettingsFile + " was not found.");
                    Core.LoadSettings(settingsFile);
                }
                
                if (fSettings.AppSettings != null)
                {
                    foreach (var item in fSettings.AppSettings)
                    {
                        if (item.Key.IndexOf(">", StringComparison.Ordinal) > -1)
                        {
                            var keyPair = item.Key.SplitAndTrim(">");
                            var k1 = keyPair[0].SplitAndTrim(".");
                            var env = k1[0];
                            var mac = k1.Length > 1 ? k1[1] : null;
                            var ckey = keyPair[1];
                            if (env != Core.EnvironmentName || (mac != null && mac != Core.MachineName)) continue;
                            Core.Settings.Remove(ckey);
                            Core.Settings.Add(ckey, item.Value);
                        }
                        else
                        {
                            if (!Core.Settings.Contains(item.Key))
                                Core.Settings.Add(item.Key, item.Value);
                        }
                    }
                }

                Core.RebindSettings();

                if (fSettings.Core != null && fSettings.Core.InjectorFile.IsNotNullOrWhitespace())
                {
                    var injectorFile = ResolveLowLowFilePath(fSettings.Core.InjectorFile);
                    if (injectorFile == null)
                        throw new FileNotFoundException("The injector file: " + fSettings.Core.InjectorFile + " was not found.");
                    Core.LoadInjector(injectorFile);
                }

                return true;
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                return false;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetValueFromEnvironment(string environmentName)
        {
            var environmentFile = ResolveLowLowFilePath("<</" + environmentName);
            if (environmentFile != null)
            {
                try
                {
                    var environmentValue = File.ReadAllText(environmentFile).Trim();
                    return environmentValue;
                }
                catch (Exception ex)
                {
                    Core.Log.Warning(ex.Message);
                }
            }
            return Environment.GetEnvironmentVariable(environmentName);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AttachStatus()
        {
            _cpuUsage = new CpuUsage(Process.GetCurrentProcess());
            Core.Status.Attach(() =>
            {
                using (var process = Process.GetCurrentProcess())
                {
                    var sItem = new StatusItem { Name = "Application Information" };
                    sItem.Values.Add("Command Line", Environment.CommandLine);
                    sItem.Values.Add("Current Directory", Directory.GetCurrentDirectory());

                    sItem.Values.Add("Operating System",
                        new StatusItemValueItem(nameof(Factory.PlatformType), Factory.PlatformType),
                        new StatusItemValueItem(nameof(Environment.ProcessorCount), Environment.ProcessorCount),
                        new StatusItemValueItem(nameof(Environment.OSVersion), Environment.OSVersion),
                        new StatusItemValueItem(nameof(RuntimeInformation.OSArchitecture),
                            RuntimeInformation.OSArchitecture),
                        new StatusItemValueItem(nameof(RuntimeInformation.OSDescription),
                            RuntimeInformation.OSDescription)
                    );
                    sItem.Values.Add("User",
                        new StatusItemValueItem(nameof(Environment.MachineName), Environment.MachineName),
                        new StatusItemValueItem(nameof(Environment.UserDomainName), Environment.UserDomainName),
                        new StatusItemValueItem(nameof(Environment.UserInteractive), Environment.UserInteractive),
                        new StatusItemValueItem(nameof(Environment.UserName), Environment.UserName)
                    );
                    sItem.Values.Add("Process Information",
                        new StatusItemValueItem(nameof(process.Id), process.Id),
                        new StatusItemValueItem(nameof(RuntimeInformation.ProcessArchitecture),
                            RuntimeInformation.ProcessArchitecture),
                        new StatusItemValueItem(nameof(process.ProcessName), process.ProcessName),
                        new StatusItemValueItem(nameof(process.Threads), process.Threads.Count, true),
                        new StatusItemValueItem("Handles", process.HandleCount, true),
                        new StatusItemValueItem(nameof(RuntimeInformation.FrameworkDescription),
                            RuntimeInformation.FrameworkDescription)
                    );
                    sItem.Values.Add("Process Times",
                        new StatusItemValueItem(nameof(process.StartTime), process.StartTime),
                        new StatusItemValueItem("RunningTime", Core.Now - process.StartTime),
                        new StatusItemValueItem(nameof(process.UserProcessorTime), process.UserProcessorTime),
                        new StatusItemValueItem(nameof(process.PrivilegedProcessorTime),
                            process.PrivilegedProcessorTime),
                        new StatusItemValueItem(nameof(process.TotalProcessorTime), process.TotalProcessorTime)
                    );

                    sItem.Values.Add("Process Usages",
                        new StatusItemValueItem("30 Seconds Usage in %", _cpuUsage.Last30Seconds, true),
                        new StatusItemValueItem("1 Minute Usage in %", _cpuUsage.LastMinute, true),
                        new StatusItemValueItem("10 Minutes Usage in %", _cpuUsage.Last10Minutes, true),
                        new StatusItemValueItem("30 Minutes Usage in %", _cpuUsage.Last30Minutes, true));

                    sItem.Values.Add("Process Memory",
                        new StatusItemValueItem(nameof(Environment.WorkingSet) + " (MB)",
                            Environment.WorkingSet.ToMegabytes(), true),
                        new StatusItemValueItem(nameof(process.PrivateMemorySize64) + " (MB)",
                            process.PrivateMemorySize64.ToMegabytes(), true),
                        new StatusItemValueItem(nameof(process.PagedMemorySize64) + " (MB)",
                            process.PagedMemorySize64.ToMegabytes(), true),
                        new StatusItemValueItem(nameof(process.NonpagedSystemMemorySize64) + " (MB)",
                            process.NonpagedSystemMemorySize64.ToMegabytes(), true),
                        new StatusItemValueItem(nameof(process.VirtualMemorySize64) + " (MB)",
                            process.VirtualMemorySize64.ToMegabytes(), true)
                    );

                    var maxGen = GC.MaxGeneration;
                    var lstGc = new List<StatusItemValueItem>();
                    for (var i = 0; i <= maxGen; i++)
                        lstGc.Add(new StatusItemValueItem("Collection Count Gen " + i, GC.CollectionCount(i), true));
                    lstGc.Add(new StatusItemValueItem("Memory allocated (MB)", GC.GetTotalMemory(false).ToMegabytes(),
                        true));
                    lstGc.Add(new StatusItemValueItem("Is Server GC", GCSettings.IsServerGC));
                    lstGc.Add(new StatusItemValueItem("Latency Mode", GCSettings.LatencyMode));
                    sItem.Values.Add("Garbage Collector", lstGc.ToArray());

                    sItem.Values.Add("Core Framework",
                        new StatusItemValueItem("Version", Core.FrameworkVersion),
                        new StatusItemValueItem("Debug Mode", Core.DebugMode),
                        new StatusItemValueItem("Environment", Core.EnvironmentName),
                        new StatusItemValueItem("MachineName", Core.MachineName),
                        new StatusItemValueItem("ApplicationName", Core.ApplicationName),
                        new StatusItemValueItem("ApplicationDisplayName", Core.ApplicationDisplayName)
                    );
                    return sItem;

                }
            });
        }
        #endregion

        #region LargeObjectHeap Compact Timeout Method
        private static int _lastValue;
        private static Timer _largeObjectTimer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetLargeObjectHeapCompactTimeout()
        {
            Core.GlobalSettings.OnSettingsReload += Settings_OnSettingsReload;
            Settings_OnSettingsReload(null, EventArgs.Empty);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Settings_OnSettingsReload(object sender, EventArgs e)
        {
            if (Core.GlobalSettings.LargeObjectHeapCompactTimeoutInMinutes == _lastValue) return;
            if (_largeObjectTimer != null)
            {
                try
                {
                    _largeObjectTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _largeObjectTimer.Dispose();
                    _largeObjectTimer = null;
                }
                catch
                {
                    // ignored
                }
            }
            if (Core.GlobalSettings.LargeObjectHeapCompactTimeoutInMinutes > 0)
            {
                if (Core.GlobalSettings.LargeObjectHeapCompactTimeoutInMinutes != 60)
                    Core.Log.InfoBasic("Setting the Large Object Heap Compact timeout every {0} minutes.", Core.GlobalSettings.LargeObjectHeapCompactTimeoutInMinutes);
                var time = TimeSpan.FromMinutes(Core.GlobalSettings.LargeObjectHeapCompactTimeoutInMinutes);
                _largeObjectTimer = new Timer(obj =>
                {
                    Core.Log.Warning("Setting the Compaction on the Large Object Heap and forcing the garbage collector collect...");
                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                    GC.Collect();
                }, null, time, time);
            }
            _lastValue = Core.GlobalSettings.LargeObjectHeapCompactTimeoutInMinutes;
        }
        #endregion

        #region FactorySettings
        [DataContract]
        internal class FactorySettings
        {
            [DataMember]
            public Dictionary<string, string> AppSettings { get; set; }
            [DataMember]
            public InnerSettings Core { get; set; }

            [DataContract]
            public class InnerSettings
            {
                [DataMember]
                public string EnvironmentName { get; set; }
                [DataMember]
                public string MachineName { get; set; }
                [DataMember]
                public string ApplicationName { get; set; }
                [DataMember]
                public string ApplicationDisplayName { get; set; }
                [DataMember]
                public string SettingsFile { get; set; }
                [DataMember]
                public string InjectorFile { get; set; }
            }
        }
        #endregion

        #region CPU Usage
        private class CpuUsage
        {
            private readonly CpuUsageTimer _cpuLast30Seconds;
            private readonly CpuUsageTimer _cpuLastMinute;
            private readonly CpuUsageTimer _cpuLast10Minutes;
            private readonly CpuUsageTimer _cpuLast30Minutes;

            public double Last30Seconds => _cpuLast30Seconds.Percentage;
            public double LastMinute => _cpuLastMinute.Percentage;
            public double Last10Minutes => _cpuLast10Minutes.Percentage;
            public double Last30Minutes => _cpuLast30Minutes.Percentage;

            public CpuUsage(Process process)
            {
                _cpuLast30Seconds = new CpuUsageTimer(process, TimeSpan.FromSeconds(30));
                _cpuLastMinute = new CpuUsageTimer(process, TimeSpan.FromMinutes(1));
                _cpuLast10Minutes = new CpuUsageTimer(process, TimeSpan.FromMinutes(10));
                _cpuLast30Minutes = new CpuUsageTimer(process, TimeSpan.FromMinutes(30));
            }
            
            private class CpuUsageTimer
            {
                private readonly Process _process;
                private readonly TimeSpan _startCpuTime;
                private Timer _timer;
                private DateTime _monitorTime;
                private TimeSpan _cpuTime;

                public double Percentage { get; private set; }

                public CpuUsageTimer(Process process, TimeSpan frequency)
                {
                    _process = process;
                    _startCpuTime = process.TotalProcessorTime;
                    _monitorTime = DateTime.UtcNow;
                    _timer = new Timer(TimerCallback, this, frequency, frequency);
                }

                private void TimerCallback(object state)
                {
                    var newCpuTime = _process.TotalProcessorTime - _startCpuTime;
                    var usage = ((double)(newCpuTime - _cpuTime).Ticks) /
                                ((double)Environment.ProcessorCount * DateTime.UtcNow.Subtract(_monitorTime).Ticks);
                    _cpuTime = newCpuTime;
                    _monitorTime = DateTime.UtcNow;
                    Percentage = Math.Round((double)usage * 100, 2);
                }
            }
        }
        #endregion
    }
}
