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
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TWCore.Diagnostics.Counters;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Status;
using TWCore.Diagnostics.Trace;
using TWCore.Reflection;
// ReSharper disable ImpureMethodCallOnReadonlyValueField
// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

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
        public IAccessorsFactory Accessors { get; set; } = new DefaultAccessorsFactory();
        /// <summary>
        /// Default LogEngine factory
        /// </summary>
        public CreateLogEngineDelegate CreateLogEngine { get; set; } = () => new DefaultLogEngine();
        /// <summary>
        /// Default TraceEngine factory
        /// </summary>
        public CreateTraceEngineDelegate CreateTraceEngine { get; set; } = () => new DefaultTraceEngine();
        /// <summary>
        /// Default StatusEngine factory
        /// </summary>
        public CreateStatusEngineDelegate CreateStatusEngine { get; set; } = () => new DefaultStatusEngine();
        /// <summary>
        /// Default CountersEngine factory
        /// </summary>
        public CreateCountersEngineDelegate CreateCountersEngine { get; set; } = () => new DefaultCountersEngine();
        /// <summary>
        /// Gets the available assemblies loaded on the AppDomain
        /// </summary>
        /// <returns>Assemblies array</returns>
        public GetAssembliesDelegate GetAssemblies { get; set; } = () => throw new NullReferenceException();
        /// <summary>
        /// Gets the available assemblies on the folder
        /// </summary>
        /// <returns>Assemblies array</returns>
        public GetAssembliesDelegate GetAllAssemblies { get; set; } = () => throw new NullReferenceException();
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
                {
                    _platformType = PlatformType.Linux;

                    if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
                        RunningAsContainer = true;
                    else
                    {
                        //Tries to detect a docker container.
                        try
                        {
                            if (File.Exists("/proc/1/cgroup"))
                            {
                                var fileContent = File.ReadAllText("/proc/1/cgroup");
                                if (fileContent.Contains("/docker/"))
                                    RunningAsContainer = true;
                            }
                        }
                        catch
                        {
                            //
                        }
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    _platformType = PlatformType.Mac;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    _platformType = PlatformType.Windows;
                return _platformType;
            }
            protected set => _platformType = value;
        }
        /// <summary>
        /// Gets if the process is running inside a container
        /// </summary>
        public bool RunningAsContainer { get; set; }
        /// <summary>
        /// Sequential Guid Generator
        /// </summary>
        public GuidGeneratorDelegate SequentialGuidGenerator { get; set; } = BaseSequentialGuidGenerator;
        /// <summary>
        /// Get New Guid
        /// </summary>
        public GetGuidDelegate NewGuid { get; set; } = BaseNewGuid;
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
            if (relativePath is null)
                return null;

            basePath = basePath is null ? Path.GetFullPath(".") : GetAbsolutePath(basePath);

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
        /// <summary>
        /// Get the absolute file path from a low low filepath
        /// </summary>
        /// <param name="lowlowPath">Low low file path</param>
        /// <returns>Absolute file path</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual string ResolveLowLowFilePath(string lowlowPath)
        {
            // "<</" or "<<(Name)/"
            if (lowlowPath is null) return null;
            if (lowlowPath.StartsWith("<</", StringComparison.Ordinal))
            {
                var lPath = "." + lowlowPath.Substring(2);
                var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
                do
                {
                    var nPath = Path.Combine(currentDirectory.FullName, lPath);
                    if (File.Exists(nPath))
                        return Path.GetFullPath(nPath);
                    currentDirectory = currentDirectory.Parent;
                } while (currentDirectory != null);
                return null;
            }
            if (lowlowPath.StartsWith("<<(", StringComparison.Ordinal))
            {
                var lstIdx = lowlowPath.IndexOf(')');
                if (lstIdx == -1 || lstIdx < 3) return null;
                var name = lowlowPath.SubstringIndex(3, lstIdx);
                var lPath = "." + lowlowPath.Substring(lstIdx + 1);
                var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
                do
                {
                    if (currentDirectory.Name == name)
                        break;
                    currentDirectory = currentDirectory.Parent;
                } while (currentDirectory != null);
                if (currentDirectory != null)
                {
                    var nPath = Path.Combine(currentDirectory.FullName, lPath);
                    return Path.GetFullPath(nPath);
                }
                return null;
            }
            return lowlowPath;
        }
        /// <summary>
        /// Get the absolute path from a low low path
        /// </summary>
        /// <param name="lowlowPath">Low low path</param>
        /// <returns>Absolute path</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual string ResolveLowLowPath(string lowlowPath)
        {
            // "<</" or "<<(Name)/"
            if (lowlowPath is null) return null;
            if (lowlowPath.StartsWith("<</", StringComparison.Ordinal))
            {
                var lPath = "." + lowlowPath.Substring(2);
                var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
                do
                {
                    var nPath = Path.Combine(currentDirectory.FullName, lPath);
                    var nfolder = Path.GetDirectoryName(nPath);
                    if (Directory.Exists(nfolder))
                        return Path.GetFullPath(nPath);
                    currentDirectory = currentDirectory.Parent;
                } while (currentDirectory != null);
                Core.Log.Warning("The Path: {0} wasn't found.", lowlowPath);
                return null;
            }
            if (lowlowPath.StartsWith("<<(", StringComparison.Ordinal))
            {
                var lstIdx = lowlowPath.IndexOf(')');
                if (lstIdx == -1 || lstIdx < 3) return null;
                var name = lowlowPath.SubstringIndex(3, lstIdx);
                var lPath = "." + lowlowPath.Substring(lstIdx + 1);
                var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
                do
                {
                    if (currentDirectory.Name == name)
                        break;
                    currentDirectory = currentDirectory.Parent;
                } while (currentDirectory != null);
                if (currentDirectory != null)
                {
                    var nPath = Path.Combine(currentDirectory.FullName, lPath);
                    return Path.GetFullPath(nPath);
                }
                Core.Log.Warning("The Path: {0} wasn't found.", lowlowPath);
                return null;
            }
            return lowlowPath;
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
        /// <summary>
        /// Compare for equality two byte arrays
        /// </summary>
        /// <returns>True if the arrays are equals; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool BytesEquals(byte[] x, byte[] y)
        {
            if (x == y)
                return true;
            if (x is null || y is null || x.Length != y.Length)
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
