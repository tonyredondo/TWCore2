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

namespace TWCore
{
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
                if (_assemblies == null)
                {
                    _assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(d =>
                    {
                        if (d.IsDynamic) return false;
                        var assemblyName = d.GetName();
                        return !assemblyName.Name.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) &&
                        !assemblyName.Name.StartsWith("Libuv", StringComparison.OrdinalIgnoreCase) &&
                        !assemblyName.Name.StartsWith("NETStandard", StringComparison.OrdinalIgnoreCase) &&
                        !assemblyName.Name.StartsWith("System.", StringComparison.OrdinalIgnoreCase) &&
                        !assemblyName.Name.StartsWith("Newtonsoft.", StringComparison.OrdinalIgnoreCase) &&
                        !assemblyName.Name.StartsWith("Runtime.", StringComparison.OrdinalIgnoreCase);
                    }).DistinctBy(i => i.Location).ToArray();
                }
                return _assemblies;
            };
            GetAllAssemblies = () =>
            {
                var resolver = AssemblyResolverManager.GetAssemblyResolver();
                if (!AllAssembliesLoaded || (!_usedResolver && resolver != null))
                {
                    if (resolver != null)
                    {
                        var lst = resolver.Assemblies.AsParallel().Select(a => a.Instance).RemoveNulls().ToList();
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

            var configFile = $"{Core.ApplicationName}.config.json";
            if (File.Exists(configFile))
            {
                var jser = JsonSerializer.CreateDefault();
                var fSettings = jser.Deserialize<FactorySettings>(new JsonTextReader(new StreamReader(configFile)));

                if (fSettings != null)
                {
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
                                string env = k1[0];
                                string mac = k1.Length > 1 ? k1[1] : null;
                                string ckey = keyPair[1];
                                if (env == Core.EnvironmentName && (mac == null || mac == Core.MachineName))
                                {
                                    Core.Settings.Remove(ckey);
                                    Core.Settings.Add(ckey, item.Value);
                                }
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
                }
            }

            SetLargeObjectHeapCompactTimeout();
        }

        /// <summary>
        /// Attach to the status process
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void AttachStatus()
        {
            Core.Status.Attach(() =>
            {
                var sItem = new StatusItem { Name = "Environment Information" };
                sItem.Values.Add(nameof(Directory.GetCurrentDirectory), Directory.GetCurrentDirectory());
                sItem.Values.Add(nameof(Environment.MachineName), Environment.MachineName);
                sItem.Values.Add(nameof(Factory.PlatformType), Factory.PlatformType);
                sItem.Values.Add(nameof(Environment.ProcessorCount), Environment.ProcessorCount);
                sItem.Values.Add(nameof(Environment.TickCount), Environment.TickCount);
                sItem.Values.Add(nameof(RuntimeInformation.FrameworkDescription), RuntimeInformation.FrameworkDescription);
                sItem.Values.Add(nameof(RuntimeInformation.OSArchitecture), RuntimeInformation.OSArchitecture);
                sItem.Values.Add(nameof(RuntimeInformation.OSDescription), RuntimeInformation.OSDescription);
                sItem.Values.Add(nameof(RuntimeInformation.ProcessArchitecture), RuntimeInformation.ProcessArchitecture);
                sItem.Values.Add("Core Framework Version", Core.FrameworkVersion);
                sItem.Values.Add("Core Debug Mode", Core.DebugMode);
                return sItem;
            });
            Core.Status.Attach(() =>
            {
                var process = Process.GetCurrentProcess();
                var sItem = new StatusItem { Name = "Process Information" };
                sItem.Values.Add(nameof(process.BasePriority), process.BasePriority);
                sItem.Values.Add(nameof(process.Id), process.Id);
                sItem.Values.Add(nameof(process.MachineName), process.MachineName);
                sItem.Values.Add("MainModule." + nameof(process.MainModule.FileName), process.MainModule.FileName);
                sItem.Values.Add("MainModule." + nameof(process.MainModule.ModuleName), process.MainModule.ModuleName);
                sItem.Values.Add(nameof(process.NonpagedSystemMemorySize64), process.NonpagedSystemMemorySize64.ToReadeableBytes());
                sItem.Values.Add(nameof(process.PagedMemorySize64), process.PagedMemorySize64.ToReadeableBytes());
                sItem.Values.Add(nameof(process.PagedSystemMemorySize64), process.PagedSystemMemorySize64.ToReadeableBytes());
                sItem.Values.Add(nameof(process.PeakPagedMemorySize64), process.PeakPagedMemorySize64.ToReadeableBytes());
                sItem.Values.Add(nameof(process.PeakVirtualMemorySize64), process.PeakVirtualMemorySize64.ToReadeableBytes());
                sItem.Values.Add(nameof(process.PeakWorkingSet64), process.PeakWorkingSet64.ToReadeableBytes());
                sItem.Values.Add(nameof(process.PrivateMemorySize64), process.PrivateMemorySize64.ToReadeableBytes());
                sItem.Values.Add(nameof(process.PrivilegedProcessorTime), process.PrivilegedProcessorTime);
                sItem.Values.Add(nameof(process.ProcessName), process.ProcessName);
                sItem.Values.Add("RunningTime", Core.Now - process.StartTime);
                sItem.Values.Add(nameof(process.StartTime), process.StartTime);
                sItem.Values.Add(nameof(process.Threads) + " Count", process.Threads.Count);
                sItem.Values.Add(nameof(process.TotalProcessorTime), process.TotalProcessorTime);
                sItem.Values.Add(nameof(process.UserProcessorTime), process.UserProcessorTime);
                sItem.Values.Add(nameof(process.VirtualMemorySize64), process.VirtualMemorySize64.ToReadeableBytes());
                sItem.Values.Add(nameof(process.WorkingSet64), process.WorkingSet64.ToReadeableBytes());
                return sItem;
            });
            Core.Status.Attach(() =>
            {
                var sItem = new StatusItem { Name = "Garbage Collector" };
                var maxGen = GC.MaxGeneration;
                sItem.Values.Add("Max Generation", maxGen);
                for (var i = 0; i <= maxGen; i++)
                {
                    sItem.Values.Add("Collection Count Gen " + i, GC.CollectionCount(i));
                }
                sItem.Values.Add("Memory allocated", GC.GetTotalMemory(false).ToReadeableBytes());
                sItem.Values.Add("Is Server GC", GCSettings.IsServerGC);
                sItem.Values.Add("Latency Mode", GCSettings.LatencyMode);
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
