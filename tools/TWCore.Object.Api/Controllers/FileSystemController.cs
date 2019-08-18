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

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TWCore.Collections;
using TWCore.Compression;
using TWCore.Net.Multicast;
using TWCore.Reflection;
using TWCore.Serialization;
using TWCore.Settings;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable SuggestBaseTypeForParameter

namespace TWCore.Object.Api.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileSystemController : Controller
    {
        private static readonly TimeoutDictionary<bool, Dictionary<string, PathEntry>> LocalServicesCache = new TimeoutDictionary<bool, Dictionary<string, PathEntry>>();
        private static readonly FileSystemSettings Settings = Core.GetSettings<FileSystemSettings>();
        private static readonly PathEntry[] PathEmpty = Array.Empty<PathEntry>();
        private static readonly string[] Extensions;
        private static readonly string[] TextExtensions = { ".xml", ".js", ".txt", ".log", ".json", ".ini", ".srt", ".htm", ".html" };

        static FileSystemController()
        {
            var serExtensions = SerializerManager.Serializers.Select(s => s.Extensions).SelectMany(i => i).ToArray();
            var comExtensions = CompressorManager.Compressors.Select(s => s.Value.FileExtension).ToArray();

            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var serExt in serExtensions)
            {
                extensions.Add(serExt);
                foreach (var comExt in comExtensions)
                {
                    extensions.Add(serExt + comExt);
                }
            }
            foreach (var i in TextExtensions)
                extensions.Add(i);
            Extensions = extensions.ToArray();
        }

        [HttpGet("list.{format}")]
        [HttpGet("list")]
        [FormatFilter]
        public ActionResult Get()
        {
            try
            {
                string virtualPath = null;
                if (Request.Query.TryGetValue("path", out var folderValues))
                    virtualPath = folderValues;

                Request.Query.TryGetValue("filter", out var filter);
                var withFilters = !string.IsNullOrWhiteSpace(filter);

                if (string.IsNullOrWhiteSpace(virtualPath))
                    return new ObjectResult(GetRootEntryCollection());

                var services = GetLocalServices(true);
                if (virtualPath.StartsWith("SRV:", StringComparison.OrdinalIgnoreCase))
                {
                    if (services.TryGetValue(virtualPath, out var entry))
                        return new ObjectResult(new PathEntryCollection
                        {
                            Current = virtualPath,
                            Entries = entry.Entries?.ToArray()
                        });
                }
                
                var path = Path.GetFullPath(virtualPath);
                if (!Directory.Exists(path))
                    return new ObjectResult(new PathEntryCollection
                    {
                        Current = virtualPath,
                        Entries = PathEmpty,
                        Error = "Directory doesn't exist."
                    });

                if (GetRootEntryCollection().Entries.Concat(services.Values.SelectMany(v => v.Entries))
                    .All((rp, aPath) => !aPath.StartsWith(rp.Path, StringComparison.OrdinalIgnoreCase), path))
                    return new ObjectResult(new PathEntryCollection
                    {
                        Current = virtualPath,
                        Entries = PathEmpty,
                        Error = "The path is outside of allowed folders."
                    });

                var pathEntries = Directory.EnumerateDirectories(path)
                    .Where(d =>
                    {
                        if (Path.GetFileName(d)[0] == '.') return false;
                        return Try.Do(dArg => Directory.EnumerateDirectories(dArg).Any(), d, false) ||
                               Try.Do(dArg => Directory.EnumerateFiles(dArg).Any(), d, false);
                    })
                    .Select((d, mPath) => new PathEntry
                    {
                        Name = Path.GetFullPath(d).Replace(mPath, string.Empty, StringComparison.OrdinalIgnoreCase).Replace("/", string.Empty).Replace("\\", string.Empty),
                        Path = Path.GetFullPath(d),
                        Type = PathEntryType.Directory
                    }, path)
                    .Concat(Directory.EnumerateFiles(path)
                        .Where((file, iWithFilters, iFilter, iExtensions) =>
                        {
                            if (Path.GetFileName(file)[0] == '.') return false;
                            if (iWithFilters)
                            {
                                if (file.IndexOf(iFilter, StringComparison.OrdinalIgnoreCase) < 0) return false;
                            }
                            foreach (var ext in iExtensions)
                            {
                                if (file.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                                    return true;
                            }
                            return false;
                        }, withFilters, filter, Extensions)
                        .Select((d, mPath) => new PathEntry
                        {
                            Name = Path.GetFullPath(d).Replace(mPath, string.Empty, StringComparison.OrdinalIgnoreCase).Replace("/", string.Empty).Replace("\\", string.Empty),
                            Path = Path.GetFullPath(d),
                            Type = PathEntryType.File,
                            IsBinary = !TextExtensions.Contains(Path.GetExtension(d), StringComparer.OrdinalIgnoreCase)
                        }, path))
                        .OrderBy(d => d.Name)
                    .ToArray();
                return new ObjectResult(new PathEntryCollection { Current = virtualPath, Entries = pathEntries });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new SerializableException(ex));
            }
        }

        [HttpGet("load.{format}")]
        [HttpGet("load")]
        [FormatFilter]
        public async Task<ActionResult> LoadFile()
        {
            try
            {
                object obj;
                SessionData sessionData;
                Type fileType = null;

                if (Request.Query.TryGetValue("type", out var type))
                    fileType = Core.GetType(type);
                if (!Request.Query.TryGetValue("path", out var path))
                    return new ObjectResult(new FileLoadedStatus { Loaded = false });
                if (string.IsNullOrWhiteSpace(path))
                    return new ObjectResult(new FileLoadedStatus { Loaded = false });
                path = Path.GetFullPath(path);
                if (!System.IO.File.Exists(path))
                    return new ObjectResult(new FileLoadedStatus { FilePath = path, Loaded = false });

                var isSerializedObject = ((string)path).IndexOf(SerializedObject.FileExtension, StringComparison.OrdinalIgnoreCase) > -1;
                
                var serializer = SerializerManager.GetByFileName(path);
                if (isSerializedObject || (serializer != null && (serializer.SerializerType == SerializerType.Binary || fileType != null)))
                {
                    try
                    {
                        if (isSerializedObject)
                        {
                            Core.Log.InfoBasic("SerializedObject found, deserializing...");
                            obj = await SerializedObject.FromFileAsync(path);
                        }
                        else
                        {
                            Core.Log.InfoBasic("Serializer {0} found, deserializing...", serializer.ToString());
                            obj = serializer.DeserializeFromFile(fileType, path);
                        }
                        sessionData = HttpContext.Session.GetSessionData();
                        sessionData.FilePath = path;
                        sessionData.FileObject = obj;
                        HttpContext.Session.SetSessionData(sessionData);
                        Core.Log.InfoBasic("File {0} was loaded.", path);
                        return new ObjectResult(new FileLoadedStatus
                        {
                            FilePath = sessionData.FilePath,
                            ObjectType = sessionData.FileObject?.GetType().Name
                        });
                    }
                    catch(Exception ex)
                    {
                        Core.Log.Write(ex);
                        if (!isSerializedObject)
                        {
                            Core.Log.InfoBasic("Trying to deserialize from a continous stream...");
                            using (var fs = System.IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                var lstObj = new List<object>();
                                while (fs.Position != fs.Length)
                                {
                                    var item = serializer.Deserialize<object>(fs);
                                    if (item != null)
                                        lstObj.Add(item);
                                }
                                obj = lstObj;
                                sessionData = HttpContext.Session.GetSessionData();
                                sessionData.FilePath = path;
                                sessionData.FileObject = obj;
                                HttpContext.Session.SetSessionData(sessionData);
                                Core.Log.InfoBasic("File {0} was loaded.", path);
                                return new ObjectResult(new FileLoadedStatus
                                {
                                    FilePath = sessionData.FilePath,
                                    ObjectType = sessionData.FileObject?.GetType().Name
                                });
                            }
                        }
                    }
                }

                Core.Log.Warning("The serializer for file {0} wasn't found.", path);
                var extension = Path.GetExtension(path);

                if (string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".html", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".htm", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".js", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".log", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".ini", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".srt", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".css", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".ts", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".vue", StringComparison.OrdinalIgnoreCase) ||
                    serializer?.SerializerType == SerializerType.Text)
                {

                    using (var fs = System.IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs))
                        obj = sr.ReadToEnd();
                    sessionData = HttpContext.Session.GetSessionData();
                    sessionData.FilePath = path;
                    sessionData.FileObject = obj;
                    HttpContext.Session.SetSessionData(sessionData);
                    Core.Log.InfoBasic("File {0} was loaded as text data.", path);
                    return new ObjectResult(new FileLoadedStatus
                    {
                        FilePath = sessionData.FilePath,
                        ObjectType = sessionData.FileObject?.GetType().Name
                    });
                }

                using (var fs = System.IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new BinaryReader(fs))
                    obj = sr.ReadBytes((int)fs.Length);
                sessionData = HttpContext.Session.GetSessionData();
                sessionData.FilePath = path;
                sessionData.FileObject = obj;
                HttpContext.Session.SetSessionData(sessionData);
                Core.Log.InfoBasic("File {0} was loaded as bytes data.", path);
                return new ObjectResult(new FileLoadedStatus
                {
                    FilePath = sessionData.FilePath,
                    ObjectType = sessionData.FileObject?.GetType().Name
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new SerializableException(ex));
            }
        }

        [HttpGet("unload.{format}")]
        [HttpGet("unload")]
        [FormatFilter]
        public bool UnloadFile()
        {
            var sessionData = HttpContext.Session.GetSessionData();
            if (sessionData.FilePath is null && sessionData.FileObject is null) return false;
            var oldFile = sessionData.FilePath;
            sessionData.FilePath = null;
            sessionData.FileObject = null;
            HttpContext.Session.SetSessionData(sessionData);
            Core.Log.InfoBasic("File {0} was unloaded.", oldFile);
            return true;
        }

        [HttpGet("status.{format}")]
        [HttpGet("status")]
        [FormatFilter]
        public FileLoadedStatus LoadStatus()
        {
            var sessionData = HttpContext.Session.GetSessionData();
            return new FileLoadedStatus
            {
                FilePath = sessionData.FilePath,
                ObjectType = sessionData.FileObject?.GetType().Name,
                Loaded = sessionData.FileObject != null
            };
        }

        [HttpPost("upload.{format}")]
        [HttpPost("upload")]
        [FormatFilter]
        public async Task<ActionResult> UploadFile()
        {
            try
            {
                if (!Request.Query.TryGetValue("name", out var name))
                    throw new Exception("You must specify a name of the file.");

                if (Request.ContentLength.HasValue && Request.ContentLength.Value > 2097152)
                    throw new Exception("The file is too large.");

                object obj;
                SessionData sessionData;

                var isSerializedObject = ((string)name).IndexOf(SerializedObject.FileExtension, StringComparison.OrdinalIgnoreCase) > -1;

                var serializer = SerializerManager.GetByFileName(name);
                if (isSerializedObject || (serializer != null && serializer.SerializerType == SerializerType.Binary))
                {
                    if (isSerializedObject)
                    {
                        Core.Log.InfoBasic("SerializedObject found, deserializing...");
                        obj = SerializedObject.FromStream(Request.Body);
                    }
                    else
                    {
                        Core.Log.InfoBasic("Serializer {0} found, deserializing...", serializer.ToString());
                        obj = serializer.Deserialize(Request.Body, typeof(object));
                    }
                    sessionData = HttpContext.Session.GetSessionData();
                    sessionData.FilePath = name;
                    sessionData.FileObject = obj;
                    HttpContext.Session.SetSessionData(sessionData);
                    Core.Log.InfoBasic("File {0} was loaded.", name);
                    return new ObjectResult(new FileLoadedStatus
                    {
                        FilePath = sessionData.FilePath,
                        ObjectType = sessionData.FileObject?.GetType().Name
                    });
                }

                Core.Log.Warning("The serializer for file {0} wasn't found.", name);
                var extension = Path.GetExtension(name);

                if (string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".html", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".htm", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".js", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".ini", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".srt", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".css", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".ts", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".vue", StringComparison.OrdinalIgnoreCase) ||
                    serializer?.SerializerType == SerializerType.Text)
                {
                    obj = await Request.Body.TextReadToEndAsync();
                    sessionData = HttpContext.Session.GetSessionData();
                    sessionData.FilePath = name;
                    sessionData.FileObject = obj;
                    HttpContext.Session.SetSessionData(sessionData);
                    Core.Log.InfoBasic("File {0} was loaded as text data.", name);
                    return new ObjectResult(new FileLoadedStatus
                    {
                        FilePath = sessionData.FilePath,
                        ObjectType = sessionData.FileObject?.GetType().Name
                    });
                }

                obj = (await Request.Body.ReadAllBytesAsync()).AsArray();
                sessionData = HttpContext.Session.GetSessionData();
                sessionData.FilePath = name;
                sessionData.FileObject = obj;
                HttpContext.Session.SetSessionData(sessionData);
                Core.Log.InfoBasic("File {0} was loaded as bytes data.", name);
                return new ObjectResult(new FileLoadedStatus
                {
                    FilePath = sessionData.FilePath,
                    ObjectType = sessionData.FileObject?.GetType().Name
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new SerializableException(ex));
            }
        }

        private static PathEntryCollection GetRootEntryCollection()
        {
            var servicesPathEntries = GetLocalServices(false);

            var notemptyPaths = Settings.RootPaths
                .Select(p => new PathEntry {Name = p, Path = p})
                .Where(ls => !string.IsNullOrWhiteSpace(ls.Path))
                .ToArray();

            var rootPaths = new List<PathEntry>();
            foreach (var item in notemptyPaths)
            {
                item.Path = Path.GetFullPath(item.Path);
                if (Directory.Exists(item.Path))
                    rootPaths.Add(item);
            }

            var entries = rootPaths.Concat(servicesPathEntries.Values)
                //rootPaths.Concat(logFilesPaths).Concat(logHttpPaths).Concat(traceFilesPath)
                .DistinctBy(p => p.Path)
                .OrderBy(i => i.Name)
                .ToArray();
            
            return new PathEntryCollection
            {
                Current = string.Empty,
                Entries = entries,
                Error = entries.Length == 0 ? "There isn't any valid folder." : null
            };
        }
        private static Dictionary<string, PathEntry> GetLocalServices(bool withChildren)
        {
            return LocalServicesCache.GetOrAdd(withChildren, wChildren =>
            {
                var logFilesServices = DiscoveryService.GetLocalRegisteredServices("LOG.FILE");
                var logHttpServices = DiscoveryService.GetLocalRegisteredServices("LOG.HTTP");
                var traceFilesServices = DiscoveryService.GetLocalRegisteredServices("TRACE.FILE");
                var statusHttpServices = DiscoveryService.GetRegisteredServices("STATUS.HTTP");
                var statusFilesServices = DiscoveryService.GetLocalRegisteredServices("STATUS.FILE");
                
                var appNames = traceFilesServices.Select(ts => ts.ApplicationName).ToArray();

                if (Core.Settings["ImportRemoteAssemblies"].ParseTo(false) && Core.Settings["ImportRemoteAssembliesPath"] != null)
                {
                    var folderServices = DiscoveryService.GetLocalRegisteredServices("FOLDERS");
                    var folderServicesData = folderServices
                        .Where((s, names) => names.Contains(s.ApplicationName) && s.ApplicationName != Core.ApplicationName, appNames)
                        .Select(s => s.Data.GetValue() is string[] strArray ? strArray[0] : null)
                        .ToArray();

                    var destinationPath = Core.Settings["ImportRemoteAssembliesPath"];
                    foreach (var path in folderServicesData)
                    {
                        var localExeFiles = Directory.EnumerateFiles(path, "*.exe", SearchOption.AllDirectories);
                        var localDllFiles = Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories);
                        var localFiles = localExeFiles.Concat(localDllFiles);
                        foreach (var file in localFiles)
                        {
                            try
                            {
                                var name = AssemblyName.GetAssemblyName(file);
                                if (AssemblyResolver.IsExcludedAssembly(name.Name)) continue;
                                var destinationFile = Path.Combine(destinationPath, Path.GetFileName(file));
                                if (!System.IO.File.Exists(destinationFile))
                                    System.IO.File.Copy(file, destinationFile);
                            }
                            catch (BadImageFormatException)
                            {
                                //
                            }
                            catch (Exception)
                            {
                                //
                            }
                        }
                    }

                    var asmResolver = AssemblyResolverManager.GetAssemblyResolver();
                    asmResolver?.AppendPath(destinationPath);
                }

                var servicesPathEntries = new Dictionary<string, PathEntry>();
    
                foreach (var srv in logFilesServices)
                {
                    var srvPe = EnsureServiceEntry(servicesPathEntries, srv);
                    if (!wChildren) continue;
                    var path = srv.Data.GetValue() as string;
                    if (string.IsNullOrWhiteSpace(path)) continue;
                    path = Path.GetFullPath(path);
                    if (!Directory.Exists(path)) continue;
                    if (srvPe.Entries.All((c, aPath) => c.Path != aPath, path))
                        srvPe.Entries.Add(new PathEntry
                        {
                            Name = "LOGS",
                            Path = Path.GetFullPath(path),
                            Type = PathEntryType.Directory
                        });
                }
                foreach (var srv in logHttpServices)
                {
                    var srvPe = EnsureServiceEntry(servicesPathEntries, srv);
                    if (!wChildren) continue;
                    var path = srv.Data.GetValue() as string;
                    if (string.IsNullOrWhiteSpace(path)) continue;
                    path = Path.GetFullPath(path);
                    if (!Directory.Exists(path)) continue;
                    if (srvPe.Entries.All((c, aPath) => c.Path != aPath, path))
                        srvPe.Entries.Add(new PathEntry
                        {
                            Name = "HTTP LOGS",
                            Path = Path.GetFullPath(path),
                            Type = PathEntryType.Directory
                        });
                }
                foreach (var srv in traceFilesServices)
                {
                    var srvPe = EnsureServiceEntry(servicesPathEntries, srv);
                    if (!wChildren) continue;
                    var path = srv.Data.GetValue() as string;
                    if (string.IsNullOrWhiteSpace(path)) continue;
                    path = Path.GetFullPath(path);
                    if (!Directory.Exists(path)) continue;
                    if (srvPe.Entries.All((c, aPath) => c.Path != aPath, path))
                        srvPe.Entries.Add(new PathEntry
                        {
                            Name = "TRACES",
                            Path = Path.GetFullPath(path),
                            Type = PathEntryType.Directory
                        });
                }
                foreach (var srv in statusFilesServices)
                {
                    var srvPe = EnsureServiceEntry(servicesPathEntries, srv);
                    if (!wChildren) continue;
                    var path = srv.Data.GetValue() as string;
                    if (string.IsNullOrWhiteSpace(path)) continue;
                    path = Path.GetFullPath(path);
                    if (!Directory.Exists(path)) continue;
                    if (srvPe.Entries.All((c, aPath) => c.Path != aPath, path))
                        srvPe.Entries.Add(new PathEntry
                        {
                            Name = "STATUSES",
                            Path = Path.GetFullPath(path),
                            Type = PathEntryType.Directory
                        });
                }
                foreach(var srv in statusHttpServices)
                {
                    var srvPe = EnsureServiceEntry(servicesPathEntries, srv);
                    if (!wChildren) continue;
                    if (!(srv.Data.GetValue() is Dictionary<string, object> data)) continue;
                    if (!data.TryGetValue("Port", out var port)) continue;
                    if (srv.Addresses is null || srv.Addresses.Length == 0) continue;
                    var ipAddress = srv.Addresses[0];
                    var strPort = ipAddress + "/"+ port.ToString();
                    if (srvPe.Entries.All((c, aPort) => c.Path != aPort, strPort))
                        srvPe.Entries.Add(new PathEntry
                        {
                            Name = $"CURRENT STATUS ({ipAddress})",
                            Path = strPort,
                            Type = PathEntryType.Status
                        });
                }

                return (servicesPathEntries, TimeSpan.FromSeconds(20));
            });
        }
        private static PathEntry EnsureServiceEntry(Dictionary<string, PathEntry> servicesPathEntries, DiscoveryService.ReceivedService srv)
        {
            var path = "SRV:" + srv.ApplicationName;
            if (servicesPathEntries.TryGetValue(path, out var srvPe)) return srvPe;
            srvPe = new PathEntry
            {
                Name = srv.ApplicationName,
                Path = path,
                Type = PathEntryType.Service,
                Entries = new List<PathEntry>()
            };
            servicesPathEntries.Add(path, srvPe);
            return srvPe;
        }

        #region Nested types
        private class FileSystemSettings : SettingsBase
        {
            [SettingsArray(';')]
            public string[] RootPaths { get; set; } = Array.Empty<string>();
        }
        public enum PathEntryType
        {
            Directory,
            File,
            Url,
            Status,
            Service
        }
        [DataContract]
        public class PathEntry
        {
            [XmlAttribute, DataMember]
            public string Name { get; set; }
            [XmlAttribute, DataMember]
            public string Path { get; set; }
            [XmlAttribute, DataMember]
            public PathEntryType Type { get; set; }
            [XmlAttribute, DataMember]
            public bool IsBinary { get; set; }
            [XmlIgnore]
            public List<PathEntry> Entries { get; set; }
        }
        [DataContract]
        public class PathEntryCollection
        {
            [XmlAttribute, DataMember]
            public string Current { get; set; }
            [XmlElement("Entry"), DataMember]
            public PathEntry[] Entries { get; set; }
            [XmlAttribute, DataMember]
            public string Error { get; set; }
        }
        [DataContract]
        public class FileLoadedStatus
        {
            [XmlAttribute, DataMember]
            public string FilePath { get; set; }
            [XmlAttribute, DataMember]
            public string ObjectType { get; set; }
            [XmlAttribute, DataMember]
            public bool Loaded { get; set; } = true;
        }
        #endregion
    }
}
