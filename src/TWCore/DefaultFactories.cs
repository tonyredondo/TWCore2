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
        private Assembly[] _assemblies;
        private bool _usedResolver;

        [DllImport("rpcrt4.dll", SetLastError = true)]
        private static extern int UuidCreateSequential(out Guid guid);

        /// <summary>
        /// Sets the current directory to the base assembly location
        /// </summary>
        public bool SetDirectoryToBaseAssembly { get; set; } = true;
        /// <summary>
        /// Bool indicating if all assemblies were loaded.
        /// </summary>
        private bool AllAssembliesLoaded { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DefaultFactories()
        {
            Accessors = new CompleteAccessorsFactory();
            GetAssemblies = () =>
            {
                if (_assemblies != null) return _assemblies;
                _assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(d =>
                {
                    if (d.IsDynamic) return false;
                    var assemblyName = d.GetName();
                    return !IsExcludedAssembly(assemblyName.Name);
                }).DistinctBy(i => i.Location).ToArray();
                return _assemblies;
            };
            GetAllAssemblies = () =>
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
                                if (loaded.All(l => l.FullName != name.FullName))
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
                    return GetAssemblies();
                }
                return _assemblies;
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Init()
        {
            if (SetDirectoryToBaseAssembly)
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            if (Factory.PlatformType == PlatformType.Windows)
            {
                SequentialGuidGenerator = () =>
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
                };
            }

            Core.Log.ItemFactory = Factory.CreateLogItem;
            Core.Trace.ItemFactory = Factory.CreateTraceItem;
            Core.MachineName = Environment.MachineName;
            Core.ApplicationName = Assembly.GetEntryAssembly().GetName().Name;
            Core.ApplicationDisplayName = Core.ApplicationName;
            Core.EnvironmentName = Environment.GetEnvironmentVariable(EnvironmentVariableName);

            AttachStatus();

            ServiceContainer.RegisterParametersHandler("configfile=[Path]", "Load the application using other configuration file.",
                obj => { });
            
            var line = Environment.CommandLine;
            var asmLocation = Path.GetFullPath(Assembly.GetEntryAssembly().Location);
            var arguments = line.Replace(asmLocation, string.Empty).Split(new[] { '/' }, StringSplitOptions.None)
                .Skip(1).ToArray();
            var argConfigFile = arguments.FirstOrDefault(a => a.StartsWith("configfile=", StringComparison.OrdinalIgnoreCase));
            
            if (!string.IsNullOrWhiteSpace(argConfigFile))
            {
                argConfigFile = argConfigFile.Substring(11).Trim();
                if (!LoadConfigFile(argConfigFile))
                    throw new FileNotFoundException(string.Format("Configuration file: '{0}' couldn't be loaded.", argConfigFile));
            }
            else
            {
                if (!LoadConfigFile($"{Core.ApplicationName}.config.json"))
                    LoadConfigFile($"{Core.ApplicationName}.json");
            }
            SetLargeObjectHeapCompactTimeout();
        }

        private static bool IsExcludedAssembly(string assemblyName)
        {
            return assemblyName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("Libuv", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("NETStandard", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("Newtonsoft.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("SQLitePCLRaw.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("StackExchange.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("RabbitMQ.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("Remotion.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("Runtime.", StringComparison.OrdinalIgnoreCase);
        }

        private static bool LoadConfigFile(string configFile)
        {
            if (!File.Exists(configFile)) return false;
            try
            {
                var jser = JsonSerializer.CreateDefault();
                var fSettings = jser.Deserialize<FactorySettings>(new JsonTextReader(new StreamReader(configFile)));
                if (fSettings == null) return true;

                if (fSettings.Core != null)
                {
                    if (fSettings.Core.EnvironmentName.IsNotNullOrWhitespace())
                        Core.EnvironmentName = fSettings.Core.EnvironmentName;
                    if (fSettings.Core.MachineName.IsNotNullOrWhitespace())
                        Core.MachineName = fSettings.Core.MachineName;
                    if (fSettings.Core.ApplicationName.IsNotNullOrWhitespace())
                        Core.ApplicationName = fSettings.Core.ApplicationName;
                    if (fSettings.Core.ApplicationDisplayName.IsNotNullOrWhitespace())
                        Core.ApplicationDisplayName = fSettings.Core.ApplicationDisplayName;
                    if (fSettings.Core.SettingsFile.IsNotNullOrWhitespace())
                        Core.LoadSettings(fSettings.Core.SettingsFile);
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

                if (fSettings.Core != null)
                    if (fSettings.Core.InjectorFile.IsNotNullOrWhitespace())
                        Core.LoadInjector(fSettings.Core.InjectorFile);

                return true;
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                return false;
            }
        }

        /// <summary>
        /// Attach to the status process
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AttachStatus()
        {
            Core.Status.Attach(() =>
            {
                var process = Process.GetCurrentProcess();

                var sItem = new StatusItem { Name = "Application Information" };
                sItem.Values.Add(nameof(Environment.CommandLine), Environment.CommandLine);
                sItem.Values.Add(nameof(Directory.GetCurrentDirectory), Directory.GetCurrentDirectory());

                sItem.Values.Add("Operating System",
                    new StatusItemValueItem(nameof(Factory.PlatformType), Factory.PlatformType),
                    new StatusItemValueItem(nameof(Environment.ProcessorCount), Environment.ProcessorCount),
                    new StatusItemValueItem(nameof(Environment.OSVersion), Environment.OSVersion),
                    new StatusItemValueItem(nameof(RuntimeInformation.OSArchitecture), RuntimeInformation.OSArchitecture),
                    new StatusItemValueItem(nameof(RuntimeInformation.OSDescription), RuntimeInformation.OSDescription)
                );
                sItem.Values.Add("User",
                    new StatusItemValueItem(nameof(Environment.MachineName), Environment.MachineName),
                    new StatusItemValueItem(nameof(Environment.UserDomainName), Environment.UserDomainName),
                    new StatusItemValueItem(nameof(Environment.UserInteractive), Environment.UserInteractive),
                    new StatusItemValueItem(nameof(Environment.UserName), Environment.UserName)
                );
                sItem.Values.Add("Process Information", 
                    new StatusItemValueItem(nameof(process.Id), process.Id),
                    new StatusItemValueItem(nameof(RuntimeInformation.ProcessArchitecture), RuntimeInformation.ProcessArchitecture),
                    new StatusItemValueItem(nameof(process.ProcessName), process.ProcessName),
                    new StatusItemValueItem(nameof(process.Threads), process.Threads.Count, true),
                    new StatusItemValueItem("Handles", process.HandleCount, true),
                    new StatusItemValueItem(nameof(RuntimeInformation.FrameworkDescription), RuntimeInformation.FrameworkDescription)
                );
                sItem.Values.Add("Process Times",
                    new StatusItemValueItem(nameof(process.StartTime), process.StartTime),
                    new StatusItemValueItem("RunningTime", Core.Now - process.StartTime),
                    new StatusItemValueItem(nameof(process.UserProcessorTime), process.UserProcessorTime),
                    new StatusItemValueItem(nameof(process.PrivilegedProcessorTime), process.PrivilegedProcessorTime),
                    new StatusItemValueItem(nameof(process.TotalProcessorTime), process.TotalProcessorTime)
                );
                sItem.Values.Add("Process Memory",
                    new StatusItemValueItem(nameof(Environment.WorkingSet) + " (MB)", Environment.WorkingSet.ToMegabytes(), true),
                    new StatusItemValueItem(nameof(process.PrivateMemorySize64) + " (MB)", process.PrivateMemorySize64.ToMegabytes(), true),
                    new StatusItemValueItem(nameof(process.PagedMemorySize64) + " (MB)", process.PagedMemorySize64.ToMegabytes(), true),
                    new StatusItemValueItem(nameof(process.NonpagedSystemMemorySize64) + " (MB)", process.NonpagedSystemMemorySize64.ToMegabytes(), true),
                    new StatusItemValueItem(nameof(process.VirtualMemorySize64) + " (MB)", process.VirtualMemorySize64.ToMegabytes(), true)
                );

                var maxGen = GC.MaxGeneration;
                var lstGc = new List<StatusItemValueItem> { new StatusItemValueItem("Max Generation", maxGen) };
                for (var i = 0; i <= maxGen; i++)
                    lstGc.Add(new StatusItemValueItem("Collection Count Gen " + i, GC.CollectionCount(i), true));
                lstGc.Add(new StatusItemValueItem("Memory allocated (MB)", GC.GetTotalMemory(false).ToMegabytes(), true));
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
            });
        }

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
    }
}
