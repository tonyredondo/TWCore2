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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TWCore.Collections;
using TWCore.Diagnostics.Log;

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
                .Where(a => !a.IsDynamic && !a.FullName.Contains("mscor"))
                .Select(a => new AssemblyInfo { FilePath = a.Location, AssemblyName = a.GetName() });
            Assemblies.AddRange(domainAssembliesInfo);

            var searchPaths = new List<string> { Domain.BaseDirectory };
            searchPaths.AddRange(Paths);
            searchPaths = searchPaths.Distinct().Where(Directory.Exists).ToList();

            var basePath = Domain.BaseDirectory;
            searchPaths.ParallelEach(sPath =>
            {
                var localPath = Path.Combine(basePath, sPath?.Trim());
                var localExeFiles = Directory.EnumerateFiles(localPath, "*.exe", SearchOption.AllDirectories);
                var localDllFiles = Directory.EnumerateFiles(localPath, "*.dll", SearchOption.AllDirectories);
                var localFiles = localExeFiles.Concat(localDllFiles);
                var localAssembliesInfo = localFiles.AsParallel().Select(file =>
                {
                    try
                    {
                        var assemblyName = AssemblyName.GetAssemblyName(file);
                        return assemblyName.FullName.Contains("mscor") ? null : new AssemblyInfo { FilePath = file, AssemblyName = assemblyName };
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(LogLevel.Warning, ex);
                    }
                    return null;
                }).RemoveNulls();
                Assemblies.AddRange(localAssembliesInfo);
            });
            _assembliesInfoLoaded = true;
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
            Domain.ReflectionOnlyAssemblyResolve += ReflectionOnlyAssemblyResolveEvent;
            Core.Log.LibDebug($"Assembly resolver was registered on domain '{Domain.FriendlyName}' for paths: {Paths.Join(", ")}");
        }
        /// <summary>
        /// Unbind the resolver from the Domain
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnbindFromDomain()
        {
            Domain.AssemblyResolve -= AssemblyResolveEvent;
            Domain.ReflectionOnlyAssemblyResolve -= ReflectionOnlyAssemblyResolveEvent;
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
            => Assemblies.Contains(args.Name) ? Assemblies[args.Name].Instance : Assemblies.FirstOrDefault(a => a.Name == args.Name)?.Instance;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Assembly ReflectionOnlyAssemblyResolveEvent(object sender, ResolveEventArgs args)
            => Assemblies.Contains(args.Name) ? Assemblies[args.Name].ReflectionOnlyInstance : Assemblies.FirstOrDefault(a => a.Name == args.Name)?.ReflectionOnlyInstance;
        #endregion

        #region Nested Classes
        /// <summary>
        /// Contains the assembly information in order to load an instance
        /// </summary>
        public class AssemblyInfo
        {
            /// <summary>
            /// AssemblyName object from the assembly
            /// </summary>
            public AssemblyName AssemblyName { get; set; }
            /// <summary>
            /// Assembly file location
            /// </summary>
            public string FilePath { get; set; }
            /// <summary>
            /// Friendly name of the assembly
            /// </summary>
            public string Name => AssemblyName?.Name;
            /// <summary>
            /// Assembly full name
            /// </summary>
            public string FullName => AssemblyName?.FullName;

            Assembly _instance;
            /// <summary>
            /// Assembly instance
            /// </summary>
            public Assembly Instance
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    try
                    {
                        lock (AssemblyName)
                            if (_instance == null)
                                _instance = Assembly.Load(AssemblyName);
                        return _instance;
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                    return null;
                }
            }
            Assembly _reflectionOnlyInstance;
            /// <summary>
            /// Assembly instance in the reflection only context
            /// </summary>
            public Assembly ReflectionOnlyInstance
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    try
                    {
                        lock (AssemblyName)
                            if (_reflectionOnlyInstance == null)
                                _reflectionOnlyInstance = Assembly.ReflectionOnlyLoad(FullName);
                        return _reflectionOnlyInstance;
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                    return null;
                }
            }
        }
        #endregion
    }
}