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

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Status;
using TWCore.Diagnostics.Trace;
using TWCore.Numerics;
using TWCore.Reflection;
using TWCore.Threading;

namespace TWCore
{
    /// <summary>
    /// Default Factories
    /// </summary>
    public abstract class Factories
    {
        /// <summary>
        /// Activation helper
        /// </summary>
        public virtual IAccessorsFactory Accessors { get; protected set; }
        /// <summary>
        /// Log Item Factory
        /// </summary>
        public virtual CreateLogItemDelegate CreateLogItem { get; protected set; }
        /// <summary>
        /// Default LogEngine factory
        /// </summary>
        public virtual CreateLogEngineDelegate CreateLogEngine { get; protected set; }
        /// <summary>
        /// Trace Item Factory
        /// </summary>
        public virtual CreateTraceItemDelegate CreateTraceItem { get; protected set; }
        /// <summary>
        /// Default TraceEngine factory
        /// </summary>
        public virtual CreateTraceEngineDelegate CreateTraceEngine { get; protected set; }
        /// <summary>
        /// Default StatusEngine factory
        /// </summary>
        public virtual CreateStatusEngineDelegate CreateStatusEngine { get; protected set; }
        /// <summary>
        /// Gets the available assemblies loaded on the AppDomain
        /// </summary>
        /// <returns>Assemblies array</returns>
        public virtual GetAssembliesDelegate GetAssemblies { get; protected set; }
        /// <summary>
        /// Gets the available assemblies on the folder
        /// </summary>
        /// <returns>Assemblies array</returns>
        public virtual GetAssembliesDelegate GetAllAssemblies { get; protected set; }
        /// <summary>
        /// Compare for equality two byte arrays
        /// </summary>
        /// <param name="a">Array a</param>
        /// <param name="b">Array b</param>
        /// <returns>True if the arrays are equals; otherwise, false.</returns>
        public virtual EqualsBytesDelegate BytesEquals { get; protected set; }
        PlatformType _platformType = PlatformType.Unknown;
        /// <summary>
        /// Gets the platform type
        /// </summary>
        public virtual PlatformType PlatformType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_platformType == PlatformType.Unknown)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        _platformType = PlatformType.Linux;
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        _platformType = PlatformType.Mac;
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        _platformType = PlatformType.Windows;
                }
                return _platformType;
            }
        }
        /// <summary>
        /// Sequential Guid Generator
        /// </summary>
        public virtual GuidGeneratorDelegate SequentialGuidGenerator { get; protected set; }
        /// <summary>
        /// Get New Guid
        /// </summary>
        public virtual GetGuidDelegate NewGuid { get; protected set; }
        /// <summary>
        /// Thread helper methods
        /// </summary>
        public virtual IThread Thread { get; protected set; }
        /// <summary>
        /// Converter factory
        /// </summary>
        public virtual IConverterFactory Converter { get; protected set; }

        #region .ctor
        /// <summary>
        /// Default factories
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Factories()
        {
            Accessors = new DefaultAccessorsFactory();
            CreateLogItem = (LogLevel level, string code, string message, string groupName, Exception ex, string assemblyName, string typeName) =>
            {
                if (assemblyName == null || typeName == null)
                {
                    var stack = new StackTrace(2, false);
                    var frames = stack.GetFrames();
                    foreach (var frame in frames)
                    {
                        var method = frame.GetMethod();
                        var attrs = method.GetCustomAttributes(false);
                        if (attrs.Any(a => a is IgnoreStackFrameLogAttribute)) continue;
                        if (attrs.FirstOrDefault(a => a is StackFrameLogAttribute) is StackFrameLogAttribute nameAttr)
                        {
                            assemblyName = method.DeclaringType.Assembly.FullName;
                            typeName = nameAttr.ClassName;
                            break;
                        }
                        var typeAttrs = method.DeclaringType.GetCustomAttributes(false);
                        if (typeAttrs.Any(a => a is IgnoreStackFrameLogAttribute)) continue;
                        if (typeAttrs.FirstOrDefault(a => a is StackFrameLogAttribute) is StackFrameLogAttribute nameTypeAttr)
                        {
                            assemblyName = method.DeclaringType.Assembly.FullName;
                            typeName = nameTypeAttr.ClassName;
                            break;
                        }
                        if (method.Name.Contains("MoveNext"))
                        {
                            var asyncType = method.DeclaringType;
                            var actualType = asyncType.DeclaringType;
                            var actualTypeAttrs = actualType.GetCustomAttributes(false);
                            if (actualTypeAttrs.Any(a => a is IgnoreStackFrameLogAttribute)) continue;
                            if (actualTypeAttrs.FirstOrDefault(a => a is StackFrameLogAttribute) is StackFrameLogAttribute actualTypeNameTypeAttr)
                            {
                                assemblyName = actualType.Assembly.FullName;
                                typeName = actualTypeNameTypeAttr.ClassName;
                                break;
                            }
                            assemblyName = actualType.Assembly.FullName;
                            typeName = actualType.Name;
                            break;
                        }
                        if (!method.Name.Contains("<") && !method.DeclaringType.Name.Contains("<") && !method.DeclaringType.AssemblyQualifiedName.Contains("System.Private"))
                        {
                            assemblyName = method.DeclaringType.Assembly.FullName;
                            typeName = method.DeclaringType.Name;
                            break;
                        }
                    }
                }
                if (!Core.DebugMode && assemblyName == typeof(Core).Assembly.FullName && level != LogLevel.Error)
                    return null;

                var lItem = new LogItem
                {
                    Id = Factory.NewGuid(),
                    EnvironmentName = Core.EnvironmentName,
                    MachineName = Core.MachineName,
                    Timestamp = Core.Now,
                    ThreadId = Environment.CurrentManagedThreadId,
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
            };
            CreateLogEngine = () => new DefaultLogEngine();
            CreateTraceItem = (string groupName, string traceName, object traceObject) =>
            {
                var tItem = new TraceItem
                {
                    Id = NewGuid(),
                    Timestamp = Core.Now,
                    GroupName = groupName,
                    TraceName = traceName,
                    TraceObject = traceObject
                };
                return tItem;
            };
            CreateTraceEngine = () => new DefaultTraceEngine();
            CreateStatusEngine = () => new DefaultStatusEngine();

            BytesEquals = (x, y) =>
            {
                unsafe
                {
                    if (x == y)
                        return true;
                    if (x == null || y == null || x.Length != y.Length)
                        return false;
                    fixed (byte* bytes1 = x, bytes2 = y)
                    {
                        int len = x.Length;
                        int rem = len % (sizeof(long) * 16);
                        long* b1 = (long*)bytes1;
                        long* b2 = (long*)bytes2;
                        long* e1 = (long*)(bytes1 + len - rem);

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

                        for (int i = 0; i < rem; i++)
                            if (x[len - 1 - i] != y[len - 1 - i])
                                return false;

                        return true;
                    }
                }
            };
            SequentialGuidGenerator = () =>
            {
                byte[] guidArray = Guid.NewGuid().ToByteArray();
                var baseDate = new DateTime(1900, 1, 1);
                var now = DateTime.UtcNow;

                // Get the days and milliseconds which will be used to build the byte string 
                TimeSpan days = new TimeSpan(now.Ticks - baseDate.Ticks);
                TimeSpan msecs = now.TimeOfDay;

                // Convert to a byte array 
                // Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333 
                byte[] daysArray = BitConverter.GetBytes(days.Days);
                byte[] msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

                // Reverse the bytes to match SQL Servers ordering 
                Array.Reverse(daysArray);
                Array.Reverse(msecsArray);

                // Copy the bytes into the guid 
                Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
                Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

                return new Guid(guidArray);
            };
            NewGuid = seq => seq ? SequentialGuidGenerator() : Guid.NewGuid();
            Thread = new Thread();
            Converter = new DefaultConverterFactory();
        }
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
            if (basePath == null)
                basePath = Path.GetFullPath(".");
            else
                basePath = GetAbsolutePath(basePath, null);

            if (relativePath.StartsWith("~")) relativePath = relativePath.Substring(1);
            if (Factory.PlatformType == PlatformType.Windows)
            {
                if (relativePath.StartsWith("/")) relativePath = relativePath.Substring(1);
            }
            if (!Path.IsPathRooted(relativePath) || "\\".Equals(Path.GetPathRoot(relativePath)))
            {
                if (relativePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
                    return Path.GetFullPath(Path.Combine(Path.GetPathRoot(basePath), relativePath.TrimStart(Path.DirectorySeparatorChar)));
                else
                    return Path.GetFullPath(Path.Combine(basePath, relativePath));
            }
            else
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
        const int SIO_LOOPBACK_FAST_PATH = (-1744830448);
        /// <summary>
        /// Set Socket Loopback Fast Path
        /// </summary>
        /// <param name="socket">Socket instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetSocketLoopbackFastPath(Socket socket)
        {
            try
            {
                var optionInValue = BitConverter.GetBytes(1);
                socket.IOControl(SIO_LOOPBACK_FAST_PATH, optionInValue, null);
            }
            catch (Exception)
            {
            }
        }
        #endregion
    }
}
