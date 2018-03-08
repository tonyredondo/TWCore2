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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Status;
using TWCore.Diagnostics.Trace;
using TWCore.Reflection;
using TWCore.Threading;
// ReSharper disable ImpureMethodCallOnReadonlyValueField
// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TWCore
{
    /// <summary>
    /// Default Factories
    /// </summary>
    public abstract class Factories
    {
        private PlatformType _platformType = PlatformType.Unknown;

        #region Properties
        /// <summary>
        /// Activation helper
        /// </summary>
        public IAccessorsFactory Accessors { get; protected set; } = new DefaultAccessorsFactory();
        /// <summary>
        /// Log Item Factory
        /// </summary>
        public CreateLogItemDelegate CreateLogItem { get; protected set; } = BaseCreateLogItem;
        /// <summary>
        /// Default LogEngine factory
        /// </summary>
        public CreateLogEngineDelegate CreateLogEngine { get; protected set; } = () => new DefaultLogEngine();
        /// <summary>
        /// Trace Item Factory
        /// </summary>
        public CreateTraceItemDelegate CreateTraceItem { get; protected set; } = BaseCreateTraceItem;
        /// <summary>
        /// Default TraceEngine factory
        /// </summary>
        public CreateTraceEngineDelegate CreateTraceEngine { get; protected set; } = () => new DefaultTraceEngine();
        /// <summary>
        /// Default StatusEngine factory
        /// </summary>
        public CreateStatusEngineDelegate CreateStatusEngine { get; protected set; } = () => new DefaultStatusEngine();
        /// <summary>
        /// Gets the available assemblies loaded on the AppDomain
        /// </summary>
        /// <returns>Assemblies array</returns>
        public GetAssembliesDelegate GetAssemblies { get; protected set; } = () => throw new NullReferenceException();
        /// <summary>
        /// Gets the available assemblies on the folder
        /// </summary>
        /// <returns>Assemblies array</returns>
        public GetAssembliesDelegate GetAllAssemblies { get; protected set; } = () => throw new NullReferenceException();
        /// <summary>
        /// Gets the platform type
        /// </summary>
        public PlatformType PlatformType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_platformType != PlatformType.Unknown) return _platformType;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    _platformType = PlatformType.Linux;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    _platformType = PlatformType.Mac;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    _platformType = PlatformType.Windows;
                return _platformType;
            }
            protected set
            {
                _platformType = value;
            }
        }
        /// <summary>
        /// Sequential Guid Generator
        /// </summary>
        public GuidGeneratorDelegate SequentialGuidGenerator { get; protected set; } = BaseSequentialGuidGenerator;
        /// <summary>
        /// Get New Guid
        /// </summary>
        public GetGuidDelegate NewGuid { get; protected set; } = BaseNewGuid;
        /// <summary>
        /// Thread helper methods
        /// </summary>
        public IThread Thread { get; protected set; } = new Thread();
        #endregion

        /// <summary>
        /// Initialization Method
        /// </summary>
        public abstract void Init();

        #region IO
        /// <summary>
        /// Get the absolute path from a relative path
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        /// <param name="basePath">Base path to calculate the absolute path</param>
        /// <returns>Absolute path</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual string GetAbsolutePath(string relativePath, string basePath = null)
        {
            if (relativePath == null)
                return null;

            basePath = basePath == null ? Path.GetFullPath(".") : GetAbsolutePath(basePath);

            if (relativePath.StartsWith("~", StringComparison.Ordinal)) relativePath = relativePath.Substring(1);
            if (Factory.PlatformType == PlatformType.Windows)
            {
                if (relativePath.StartsWith("/", StringComparison.Ordinal)) relativePath = relativePath.Substring(1);
            }
            if (!Path.IsPathRooted(relativePath) || "\\".Equals(Path.GetPathRoot(relativePath)))
            {
                return Path.GetFullPath(relativePath.StartsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ?
                    Path.Combine(Path.GetPathRoot(basePath), relativePath.TrimStart(Path.DirectorySeparatorChar)) :
                    Path.Combine(basePath, relativePath));
            }
            return Path.GetFullPath(relativePath);
        }
        /// <summary>
        /// Get the relative path from a absolute path
        /// </summary>
        /// <param name="absolutePath">Absolute path</param>
        /// <param name="basePath">Base path to calculate the relative path</param>
        /// <returns>Relative path</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual string GetRelativePath(string absolutePath, string basePath = null)
        {
            var fileUri = new Uri(absolutePath);
            basePath = basePath ?? Path.GetFullPath(".");
            var referenceUri = new Uri(basePath);
            return referenceUri.MakeRelativeUri(fileUri).ToString();
        }
        #endregion

        #region Sockets
        /// <summary>
        /// Set Socket Loopback Fast Path
        /// </summary>
        /// <param name="socket">Socket instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSocketLoopbackFastPath(Socket socket)
        {
            if (PlatformType != PlatformType.Windows) return;
            try
            {
                socket.IOControl((-1744830448), BitConverter.GetBytes(1), null);
            }
            catch (Exception)
            {
                Core.Log.Warning("The Loopback FastPath can't be setted.");
            }
        }
        #endregion

        #region Default delegates implementation
        private static readonly NonBlocking.ConcurrentDictionary<MethodBase, (string AssemblyName, string TypeName)> MethodValues = new NonBlocking.ConcurrentDictionary<MethodBase, (string AssemblyName, string TypeName)>();
        private static readonly NonBlocking.ConcurrentDictionary<MethodBase, object[]> MethodAttributes = new NonBlocking.ConcurrentDictionary<MethodBase, object[]>();
        private static readonly NonBlocking.ConcurrentDictionary<Type, object[]> TypeAttributes = new NonBlocking.ConcurrentDictionary<Type, object[]>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ILogItem BaseCreateLogItem(LogLevel level, string code, string message, string groupName, Exception ex, string assemblyName, string typeName)
        {
            if (assemblyName == null || typeName == null)
            {
                var stack = new StackTrace(2, false);
                var frames = stack.GetFrames();
                foreach (var frame in frames)
                {
                    var method = frame.GetMethod();
                    if (method == null) continue;
                    if (MethodValues.TryGetValue(method, out var mValues))
                    {
                        assemblyName = mValues.AssemblyName;
                        typeName = mValues.TypeName;
                        break;
                    }

                    #region Name Attr
                    var attrs = MethodAttributes.GetOrAdd(method, i => i.GetCustomAttributes(false)); // method.GetCustomAttributes(false);
                    StackFrameLogAttribute nameAttr = null;
                    int attrsCases = 0;
                    for (var i = 0; i < attrs.Length; i++)
                    {
                        if (attrs[i] is IgnoreStackFrameLogAttribute)
                        {
                            attrsCases = 1;
                            break;
                        }
                        if (attrs[i] is StackFrameLogAttribute nA)
                        {
                            nameAttr = nA;
                            attrsCases = 2;
                            break;
                        }
                    }
                    if (attrsCases == 1) continue;
                    if (attrsCases == 2)
                    {
                        assemblyName = method.DeclaringType.Assembly.FullName;
                        typeName = nameAttr.ClassName;
                        MethodValues.TryAdd(method, (assemblyName, typeName));
                        break;
                    }
                    #endregion

                    var declarationType = method.DeclaringType;
                    if (declarationType == null) continue;

                    #region Name Type Attr
                    var typeAttrs = TypeAttributes.GetOrAdd(declarationType, i => i.GetCustomAttributes(false)); //declarationType.GetCustomAttributes(false);
                    StackFrameLogAttribute nameTypeAttr = null;
                    var typeAttrsCases = 0;
                    for (var i = 0; i < typeAttrs.Length; i++)
                    {
                        if (typeAttrs[i] is IgnoreStackFrameLogAttribute)
                        {
                            typeAttrsCases = 1;
                            break;
                        }
                        if (typeAttrs[i] is StackFrameLogAttribute nA)
                        {
                            nameTypeAttr = nA;
                            typeAttrsCases = 2;
                            break;
                        }
                    }
                    if (typeAttrsCases == 1) continue;
                    if (typeAttrsCases == 2)
                    {
                        assemblyName = declarationType.Assembly.FullName;
                        typeName = nameTypeAttr.ClassName;
                        MethodValues.TryAdd(method, (assemblyName, typeName));
                        break;
                    }
                    #endregion

                    if (method.Name.Contains("MoveNext"))
                    {
                        var actualType = declarationType.DeclaringType;

                        if (actualType.Assembly == typeof(Core).Assembly) continue;

                        #region Actual type attrs
                        var actualTypeAttrs = TypeAttributes.GetOrAdd(actualType, i => i.GetCustomAttributes(false)); //actualType.GetCustomAttributes(false);
                        StackFrameLogAttribute actualTypeNameTypeAttr = null;
                        int actualTypeCases = 0;
                        for (var i = 0; i < actualTypeAttrs.Length; i++)
                        {
                            if (actualTypeAttrs[i] is IgnoreStackFrameLogAttribute)
                            {
                                actualTypeCases = 1;
                                break;
                            }
                            if (actualTypeAttrs[i] is StackFrameLogAttribute nA)
                            {
                                actualTypeNameTypeAttr = nA;
                                actualTypeCases = 2;
                                break;
                            }
                        }
                        if (actualTypeCases == 1) continue;
                        if (actualTypeCases == 2)
                        {
                            assemblyName = actualType.Assembly.FullName;
                            typeName = actualTypeNameTypeAttr.ClassName;
                            MethodValues.TryAdd(method, (assemblyName, typeName));
                            break;
                        }
                        #endregion

                        assemblyName = actualType.Assembly.FullName;
                        typeName = actualType.Name;
                        if (actualType.ReflectedType != null && typeName.Contains("<"))
                            typeName = actualType.ReflectedType.Name;
                        MethodValues.TryAdd(method, (assemblyName, typeName));
                        break;
                    }

                    if (!method.Name.Contains("<") &&
                        !declarationType.Name.Contains("<") &&
                        !declarationType.AssemblyQualifiedName.Contains("System.Private") &&
                        !declarationType.AssemblyQualifiedName.Contains("mscorlib"))
                    {
                        if (declarationType.Name.Contains("ConcurrentDictionary"))
                            continue;
                        if (declarationType.Name.Contains("CacheCollectionBase`3"))
                            continue;
                        assemblyName = declarationType.Assembly.FullName;
                        typeName = declarationType.Name;
                        MethodValues.TryAdd(method, (assemblyName, typeName));
                        break;
                    }
                }
            }
            if (!Core.DebugMode && assemblyName == typeof(Core).Assembly.FullName && level > LogLevel.Stats)
                return null;

            var lItem = new LogItem
            {
                Id = Guid.NewGuid(),
                EnvironmentName = Core.EnvironmentName,
                MachineName = Core.MachineName,
                Timestamp = Core.Now,
                ApplicationName = Core.ApplicationName,
                Level = level,
                Code = code,
                Message = message,
                GroupName = groupName,
                AssemblyName = assemblyName,
                TypeName = typeName,
                Exception = ex != null ? new SerializableException(ex) : null
            };
            return lItem;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TraceItem BaseCreateTraceItem(string groupName, string traceName, object traceObject)
        {
            return new TraceItem
            {
                Id = Factory.NewGuid(),
                Timestamp = Core.Now,
                GroupName = groupName,
                TraceName = traceName,
                TraceObject = traceObject
            };
        }
        
        /// <summary>
        /// Compare for equality two byte arrays
        /// </summary>
        /// <returns>True if the arrays are equals; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool BytesEquals(byte[] x, byte[] y)
        {
            if (x == y)
                return true;
            if (x == null || y == null || x.Length != y.Length)
                return false;
            fixed (byte* bytes1 = x, bytes2 = y)
            {
                var len = x.Length;
                var rem = len % (sizeof(long) * 16);
                var b1 = (long*)bytes1;
                var b2 = (long*)bytes2;
                var e1 = (long*)(bytes1 + len - rem);

                while (b1 < e1)
                {
                    if (*(b1) != *(b2) || *(b1 + 1) != *(b2 + 1) ||
                        *(b1 + 2) != *(b2 + 2) || *(b1 + 3) != *(b2 + 3) ||
                        *(b1 + 4) != *(b2 + 4) || *(b1 + 5) != *(b2 + 5) ||
                        *(b1 + 6) != *(b2 + 6) || *(b1 + 7) != *(b2 + 7) ||
                        *(b1 + 8) != *(b2 + 8) || *(b1 + 9) != *(b2 + 9) ||
                        *(b1 + 10) != *(b2 + 10) || *(b1 + 11) != *(b2 + 11) ||
                        *(b1 + 12) != *(b2 + 12) || *(b1 + 13) != *(b2 + 13) ||
                        *(b1 + 14) != *(b2 + 14) || *(b1 + 15) != *(b2 + 15))
                        return false;
                    b1 += 16;
                    b2 += 16;
                }

                for (var i = 0; i < rem; i++)
                    if (x[len - 1 - i] != y[len - 1 - i])
                        return false;
                return true;
            }
        }
        /// <summary>
        /// Sequential Guid Generator
        /// </summary>[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid BaseSequentialGuidGenerator()
        {
            var guidArray = Guid.NewGuid().ToByteArray();
            var baseDate = new DateTime(1900, 1, 1);
            var now = DateTime.UtcNow;

            // Get the days and milliseconds which will be used to build the byte string 
            var days = new TimeSpan(now.Ticks - baseDate.Ticks);
            var msecs = now.TimeOfDay;

            // Convert to a byte array 
            // Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333 
            var daysArray = BitConverter.GetBytes(days.Days);
            var msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

            // Reverse the bytes to match SQL Servers ordering 
            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);

            // Copy the bytes into the guid 
            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

            return new Guid(guidArray);
        }
        /// <summary>
        /// Get New Guid
        /// </summary>[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid BaseNewGuid(bool sequential = false)
            => sequential ? Factory.SequentialGuidGenerator() : Guid.NewGuid();
        #endregion
    }
}
