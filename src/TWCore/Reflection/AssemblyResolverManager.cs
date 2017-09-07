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
using System.Linq;
using System.Runtime.CompilerServices;
using TWCore.Collections;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeTypeMemberModifiers

namespace TWCore.Reflection
{
    /// <summary>
    /// Manager to handle all the assembly resolvers on domains
    /// </summary>
    public static class AssemblyResolverManager
    {
        /// <summary>
        /// Assembly resolver settings key
        /// </summary>
        const string SettingsKey = "AssemblyResolverManager.Paths";

        /// <summary>
        /// Registered domains with assemblies resolvers
        /// </summary>
        public static KeyDelegatedCollection<AppDomain, AssemblyResolver> Resolvers { get; private set; } = new KeyDelegatedCollection<AppDomain, AssemblyResolver>(r => r.Domain);

        /// <summary>
        /// Register a domain and creates a new assembly resolver on the search paths
        /// </summary>
        /// <param name="domain">Application domain to register the resolver. Current domain if the argument is null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterDomain(AppDomain domain = null)
        {
            if (!Core.Settings[SettingsKey].IsNotNullOrWhitespace()) return;
            var paths = Core.Settings[SettingsKey].SplitAndTrim(',').ToArray();
            RegisterDomain(paths, domain);
        }
        /// <summary>
        /// Register a domain and creates a new assembly resolver on the search paths
        /// </summary>
        /// <param name="paths">Search path array where the assemblies are located</param>
        /// <param name="domain">Application domain to register the resolver. Current domain if the argument is null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterDomain(string[] paths, AppDomain domain = null)
        {
            if (Resolvers.Contains(domain ?? AppDomain.CurrentDomain)) return;
            var resolver = new AssemblyResolver(domain ?? AppDomain.CurrentDomain, paths);
            resolver.LoadAssembliesInfo();
            resolver.BindToDomain();
            Resolvers.Add(resolver);
        }
        /// <summary>
        /// Unregister a domain and removes the associated assembly resolver
        /// </summary>
        /// <param name="domain">Application domain to unregister the resolver. Current domain if the argument is null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnregisterDomain(AppDomain domain = null)
        {
            if (!Resolvers.Contains(domain ?? AppDomain.CurrentDomain)) return;
            var resolver = Resolvers[domain ?? AppDomain.CurrentDomain];
            resolver.UnbindFromDomain();
            Resolvers.Remove(resolver);
        }
        /// <summary>
        /// Gets the assembly resolver associated with the domain
        /// </summary>
        /// <param name="domain">Application domain to get the resolver. Current domain if the argument is null</param>
        /// <returns>AssemblyResolver instance or null if no resolvers where found</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssemblyResolver GetAssemblyResolver(AppDomain domain = null) => 
            Resolvers.Contains(domain ?? AppDomain.CurrentDomain) ? Resolvers[domain ?? AppDomain.CurrentDomain] : null;

        /// <summary>
        /// Gets the SerializationBinder to use in the BinaryFormatter serializer.
        /// </summary>
        /// <param name="domain">Application domain to get the SerializationBinder. Current domain if the argument is null</param>
        /// <returns>Serialization binder that uses this assembly resolver, null if no resolvers where found</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssemblyResolverSerializationBinder GetSerializationBinder(AppDomain domain = null) =>
            GetAssemblyResolver(domain)?.GetSerializationBinder();
    }
}