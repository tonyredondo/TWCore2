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
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Diagnostics.Log;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Reflection
{
    /// <summary>
    /// Expands the default assembly resolver and let you load assemblies from differents folders
    /// </summary>
    public class AssemblyResolver
    {
        private bool _assembliesInfoLoaded;

        #region Properties
        /// <summary>
        /// Application domain where the resolver is bound to.
        /// </summary>
        public AppDomain Domain { get; }
        /// <summary>
        /// Paths array where the assemblies are located
        /// </summary>
        public string[] Paths { get; }
        /// <summary>
        /// Loaded AssembliesInfo available collection
        /// </summary>
        public KeyStringDelegatedCollection<AssemblyInfo> Assemblies { get; } = new KeyStringDelegatedCollection<AssemblyInfo>(a => a.FullName, false);
        #endregion

        #region .ctor
        /// <summary>
        /// Expands the default assembly resolver and let you load assemblies from differents folders
        /// </summary>
        /// <param name="domain">Application domain where the resolver is going to be bound</param>
        /// <param name="paths">Assemblies path where the resolver is going to search</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AssemblyResolver(AppDomain domain, string[] paths)
        {
            Domain = domain;
            Paths = paths;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Loads the AssembliesInfo collection cache from the search Paths
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LoadAssembliesInfo()
        {
            var domainAssemblies = Domain.GetAssemblies();
            var domainAssembliesInfo = domainAssemblies.AsParallel()
                .Where(a => !a.IsDynamic && !IsExcludedAssembly(a.GetName().Name))
                .Select(a => new AssemblyInfo(a.Location, a.GetName()));
            Assemblies.AddRange(domainAssembliesInfo);

            var searchPaths = new List<string> { Domain.BaseDirectory };
            searchPaths.AddRange(Paths);
            searchPaths = searchPaths.Distinct().Where(Directory.Exists).ToList();

            var basePath = Domain.BaseDirectory;
            var cBag = new ConcurrentBag<AssemblyInfo>();
            foreach(var sPath in searchPaths)
            {
                var localPath = Path.Combine(basePath, sPath?.Trim());
                var localExeFiles = Directory.EnumerateFiles(localPath, "*.exe", SearchOption.AllDirectories);
                var localDllFiles = Directory.EnumerateFiles(localPath, "*.dll", SearchOption.AllDirectories);
                var localFiles = localExeFiles.Concat(localDllFiles);
                Parallel.ForEach(localFiles, file =>
                {
                    try
                    {
                        var name = AssemblyName.GetAssemblyName(file);
                        if (IsExcludedAssembly(name.Name)) return;
                        if (domainAssemblies.All(l => l.FullName != name.FullName))
                            cBag.Add(new AssemblyInfo(file, name));
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(LogLevel.Warning, ex);
                    }
                });
            };
            Assemblies.AddRange(cBag);
            _assembliesInfoLoaded = true;
        }
        /// <summary>
        /// Append an assembly path
        /// </summary>
        /// <param name="paths">Assemblies path where the resolver is going to search</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendPath(params string[] paths)
        {
            if (paths == null || paths.Length == 0) return;
            var basePath = Domain.BaseDirectory;
            var localAssembliesInfo = new List<AssemblyInfo>();
            foreach (var path in paths)
            {
                var localPath = Path.Combine(basePath, path?.Trim());
                var localExeFiles = Directory.EnumerateFiles(localPath, "*.exe", SearchOption.AllDirectories);
                var localDllFiles = Directory.EnumerateFiles(localPath, "*.dll", SearchOption.AllDirectories);
                var localFiles = localExeFiles.Concat(localDllFiles);
                foreach(var file in localFiles)
                {
                    try
                    {
                        var name = AssemblyName.GetAssemblyName(file);
                        if (IsExcludedAssembly(name.Name)) continue;
                        if (!Assemblies.Contains(name.FullName))
                            localAssembliesInfo.Add(new AssemblyInfo(file, name));
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(LogLevel.Warning, ex);
                    }
                }
            }
            Assemblies.AddRange(localAssembliesInfo);
        }
        /// <summary>
        /// Bind the resolver to the Domain
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindToDomain()
        {
            if (!_assembliesInfoLoaded)
                LoadAssembliesInfo();
            Domain.AssemblyResolve += AssemblyResolveEvent;
            Domain.ReflectionOnlyAssemblyResolve += ReflectionOnlyAssemblyResolve;
            Core.Log.LibDebug($"Assembly resolver was registered on domain '{Domain.FriendlyName}' for paths: {Paths.Join(", ")}");
        }

        /// <summary>
        /// Unbind the resolver from the Domain
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnbindFromDomain()
        {
            Domain.AssemblyResolve -= AssemblyResolveEvent;
            Domain.ReflectionOnlyAssemblyResolve -= ReflectionOnlyAssemblyResolve;
            Core.Log.LibDebug($"Assembly resolver was unregistered from domain '{Domain.FriendlyName}'");
        }
        /// <summary>
        /// Gets the SerializationBinder to use in the BinaryFormatter serializer.
        /// </summary>
        /// <returns>Serialization binder that uses this assembly resolver</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AssemblyResolverSerializationBinder GetSerializationBinder() => new AssemblyResolverSerializationBinder(this);
        #endregion

        #region Events Handlers

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Assembly AssemblyResolveEvent(object sender, ResolveEventArgs args)
        {
            Core.Log.LibDebug("Resolving assembly for: {0}", args.Name);
            if (Assemblies.Contains(args.Name))
            {
                Core.Log.LibDebug("Assembly {0} found.", args.Name);
                try
                {
                    return Assemblies[args.Name].Instance;
                }
                catch(Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }

            var asmName = new AssemblyName(args.Name);
            
            var asmInstance =
                Assemblies.FirstOrDefault(a => a.Name == asmName.Name && a.AssemblyName.Version == asmName.Version && a.AssemblyName.CultureName == asmName.CultureName) ??
                Assemblies.FirstOrDefault(a => a.Name == asmName.Name && a.AssemblyName.Version == asmName.Version) ??
                Assemblies.FirstOrDefault(a => a.Name == asmName.Name);

            if (asmInstance == null)
            {
                Core.Log.LibDebug("Assembly {0} not found!", asmName);
                return null;
            }
            Core.Log.LibDebug("Assembly {0} found.", args.Name);
            try
            {
                return asmInstance.Instance;
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Assembly ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            Core.Log.LibDebug("Resolving assembly for: {0}", args.Name);
            if (Assemblies.Contains(args.Name))
            {
                Core.Log.LibDebug("Assembly {0} found.", args.Name);
                try
                {
                    return Assemblies[args.Name].Instance;
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }

            var asmName = new AssemblyName(args.Name);

            var asmInstance =
                Assemblies.FirstOrDefault(a => a.Name == asmName.Name && a.AssemblyName.Version == asmName.Version && a.AssemblyName.CultureName == asmName.CultureName) ??
                Assemblies.FirstOrDefault(a => a.Name == asmName.Name && a.AssemblyName.Version == asmName.Version) ??
                Assemblies.FirstOrDefault(a => a.Name == asmName.Name);

            if (asmInstance == null)
            {
                Core.Log.LibDebug("Assembly {0} not found!", asmName);
                return null;
            }
            Core.Log.LibDebug("Assembly {0} found.", args.Name);
            try
            {
                return asmInstance.Instance;
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            return null;
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
        #endregion

        #region Nested Classes
        /// <summary>
        /// Contains the assembly information in order to load an instance
        /// </summary>
        public class AssemblyInfo
        {
            Lazy<Assembly> _lazyInstance;

            /// <summary>
            /// AssemblyName object from the assembly
            /// </summary>
            public AssemblyName AssemblyName { get; }
            /// <summary>
            /// Assembly file location
            /// </summary>
            public string FilePath { get; }
            /// <summary>
            /// Friendly name of the assembly
            /// </summary>
            public string Name => AssemblyName?.Name;
            /// <summary>
            /// Assembly full name
            /// </summary>
            public string FullName => AssemblyName?.FullName;
            /// <summary>
            /// Assembly instance
            /// </summary>
            public Assembly Instance => _lazyInstance.Value;

            public AssemblyInfo(string filePath, AssemblyName assemblyName)
            {
                FilePath = filePath;
                AssemblyName = assemblyName;
                _lazyInstance = new Lazy<Assembly>(() => Assembly.LoadFile(FilePath));
            }
        }
        #endregion
    }
}