﻿/*
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

using System.Net.Sockets;
using System.Runtime.CompilerServices;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Status;
using TWCore.Diagnostics.Trace;
using TWCore.Numerics;
using TWCore.Reflection;
using TWCore.Threading;

namespace TWCore
{
    /// <summary>
    /// Factory
    /// </summary>
    public static class Factory
    {
        static Factories _factories;

        #region Properties
        /// <summary>
        /// Activation helper
        /// </summary>
        public static IAccessorsFactory Accessors => _factories?.Accessors;
        /// <summary>
        /// Log Item Factory
        /// </summary>
        public static CreateLogItemDelegate CreateLogItem => _factories?.CreateLogItem;
        /// <summary>
        /// Default LogEngine factory
        /// </summary>
        public static CreateLogEngineDelegate CreateLogEngine => _factories?.CreateLogEngine;
        /// <summary>
        /// Trace Item Factory
        /// </summary>
        public static CreateTraceItemDelegate CreateTraceItem => _factories?.CreateTraceItem;
        /// <summary>
        /// Default TraceEngine factory
        /// </summary>
        public static CreateTraceEngineDelegate CreateTraceEngine => _factories?.CreateTraceEngine;
        /// <summary>
        /// Default StatusEngine factory
        /// </summary>
        public static CreateStatusEngineDelegate CreateStatusEngine => _factories?.CreateStatusEngine;
        /// <summary>
        /// Gets the available assemblies loaded on the AppDomain
        /// </summary>
        /// <returns>Assemblies array</returns>
        public static GetAssembliesDelegate GetAssemblies => _factories?.GetAssemblies;
        /// <summary>
        /// Gets the available assemblies on the folder
        /// </summary>
        /// <returns>Assemblies array</returns>
        public static GetAssembliesDelegate GetAllAssemblies => _factories?.GetAllAssemblies;
        /// <summary>
        /// Compare for equality two byte arrays
        /// </summary>
        /// <param name="a">Array a</param>
        /// <param name="b">Array b</param>
        /// <returns>True if the arrays are equals; otherwise, false.</returns>
        public static EqualsBytesDelegate BytesEquals => _factories?.BytesEquals;
        /// <summary>
        /// Gets the platform type
        /// </summary>
        public static PlatformType PlatformType => _factories?.PlatformType ?? PlatformType.Unknown;
        /// <summary>
        /// Sequential Guid Generator
        /// </summary>
        public static GuidGeneratorDelegate SequentialGuidGenerator => _factories?.SequentialGuidGenerator;
        /// <summary>
        /// Get New Guid
        /// </summary>
        public static GetGuidDelegate NewGuid => _factories?.NewGuid;
        /// <summary>
        /// Thread helper methods
        /// </summary>
        public static IThread Thread => _factories?.Thread;
        /// <summary>
        /// Converter factory
        /// </summary>
        public static IConverterFactory Converter => _factories?.Converter;
        #endregion

        #region Methods
        /// <summary>
        /// Set the factories methods instance
        /// </summary>
        /// <param name="factories">Factories object instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetFactories(Factories factories)
        { 
            _factories = factories;
        }
        #endregion

        #region IO
        /// <summary>
        /// Get the absolute path from a relative path
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        /// <param name="basePath">Base path to calculate the absolute path</param>
        /// <returns>Absolute path</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetAbsolutePath(string relativePath, string basePath = null) => _factories?.GetAbsolutePath(relativePath, basePath);
        /// <summary>
        /// Get the relative path from a absolute path
        /// </summary>
        /// <param name="absolutePath">Absolute path</param>
        /// <param name="basePath">Base path to calculate the relative path</param>
        /// <returns>Relative path</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetRelativePath(string absolutePath, string basePath = null) => _factories?.GetRelativePath(absolutePath, basePath);
        #endregion

        #region Sockets
        /// <summary>
        /// Set Socket Loopback Fast Path
        /// </summary>
        /// <param name="socket">Socket instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSocketLoopbackFastPath(Socket socket) => _factories?.SetSocketLoopbackFastPath(socket);
        #endregion
    }
}
