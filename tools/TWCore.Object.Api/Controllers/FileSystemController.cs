using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TWCore.Compression;
using TWCore.Net.Multicast;
using TWCore.Serialization;
using TWCore.Settings;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace TWCore.Object.Api.Controllers
{
    public class FileSystemController : Controller
    {
        private static readonly FileSystemSettings Settings = Core.GetSettings<FileSystemSettings>();
        private static readonly PathEntry[] PathEmpty = new PathEntry[0];
        private static readonly string[] Extensions;
        private static readonly string[] TextExtensions = new[] { ".xml", ".js", ".txt", ".log", ".json", ".ini", ".srt" };

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
            TextExtensions.Each(i => extensions.Add(i));
            Extensions = extensions.ToArray();
        }

        [HttpGet("api/files/list.{format}")]
        [HttpGet("api/files/list")]
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

                var path = Path.GetFullPath(virtualPath);
                if (!Directory.Exists(path))
                    return new ObjectResult(new PathEntryCollection
                    {
                        Current = virtualPath,
                        Entries = PathEmpty,
                        Error = "Directory doesn't exist."
                    });

                if (Settings.RootPaths.All(rp =>
                    !path.StartsWith(Path.GetFullPath(rp), StringComparison.OrdinalIgnoreCase)))
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
                        return Try.Do(() => Directory.EnumerateDirectories(d).Any(), false) ||
                               Try.Do(() => Directory.EnumerateFiles(d).Any(), false);
                    })
                    .Select(d => new PathEntry
                    {
                        Name = Path.GetFullPath(d).Replace(path, string.Empty, StringComparison.OrdinalIgnoreCase),
                        Type = PathEntryType.Directory
                    })
                    .Concat(Directory.EnumerateFiles(path)
                        .Where(file =>
                        {
                            if (Path.GetFileName(file)[0] == '.') return false;
                            if (withFilters)
                            {
                                if (file.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) return false;
                            }
                            foreach (var ext in Extensions)
                            {
                                if (file.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                                    return true;
                            }
                            return false;
                        })
                        .Select(d => new PathEntry
                        {
                            Name = Path.GetFullPath(d).Replace(path, string.Empty, StringComparison.OrdinalIgnoreCase),
                            Type = PathEntryType.File,
                            IsBinary = !TextExtensions.Contains(Path.GetExtension(d), StringComparer.OrdinalIgnoreCase)
                        }))
                    .ToArray();
                return new ObjectResult(new PathEntryCollection { Current = virtualPath, Entries = pathEntries });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new SerializableException(ex));
            }
        }

        [HttpGet("api/files/load.{format}")]
        [HttpGet("api/files/load")]
        [FormatFilter]
        public ActionResult LoadFile()
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

                var serializer = SerializerManager.GetByFileName(path);
                if (serializer != null && (serializer.SerializerType == SerializerType.Binary || fileType != null))
                {
                    Core.Log.InfoBasic("Serializer {0} found, deserializing...", serializer.ToString());
                    obj = serializer.DeserializeFromFile(fileType, path);
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

                Core.Log.Warning("The serializer for file {0} wasn't found.", path);
                var extension = Path.GetExtension(path);

                if (string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".html", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".htm", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".js", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".log", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".ini", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".srt", StringComparison.OrdinalIgnoreCase) ||
                    serializer?.SerializerType == SerializerType.Text)
                {
                    obj = System.IO.File.ReadAllText(path);
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

                obj = System.IO.File.ReadAllBytes(path);
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

        [HttpGet("api/files/unload.{format}")]
        [HttpGet("api/files/unload")]
        [FormatFilter]
        public bool UnloadFile()
        {
            var sessionData = HttpContext.Session.GetSessionData();
            if (sessionData.FilePath == null && sessionData.FileObject == null) return false;
            var oldFile = sessionData.FilePath;
            sessionData.FilePath = null;
            sessionData.FileObject = null;
            HttpContext.Session.SetSessionData(sessionData);
            Core.Log.InfoBasic("File {0} was unloaded.", oldFile);
            return true;
        }

        [HttpGet("api/files/status.{format}")]
        [HttpGet("api/files/status")]
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

        [HttpPost("api/files/upload.{format}")]
        [HttpPost("api/files/upload")]
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


                var serializer = SerializerManager.GetByFileName(name);
                if (serializer != null && serializer.SerializerType == SerializerType.Binary)
                {
                    Core.Log.InfoBasic("Serializer {0} found, deserializing...", serializer.ToString());
                    obj = serializer.Deserialize(Request.Body, typeof(object));
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
                    string.Equals(extension, ".log", StringComparison.OrdinalIgnoreCase) ||
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

                obj = (byte[]) await Request.Body.ReadBytesAsync();
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
            var logFilesServices = DiscoveryService.GetLocalRegisteredServices("LOG.FILE");
            var logHttpServices = DiscoveryService.GetLocalRegisteredServices("LOG.HTTP");
            var traceFilesServices = DiscoveryService.GetLocalRegisteredServices("TRACE.FILE");

            var logFilesPaths = logFilesServices.Select(ls => ls.Data.GetValue() as string).RemoveNulls().ToArray();
            var logHttpPaths = logHttpServices.Select(ls => ls.Data.GetValue() as string).RemoveNulls().ToArray();
            var traceFilesPath = traceFilesServices.Select(ls => ls.Data.GetValue() as string).RemoveNulls().ToArray();

            var paths = Settings.RootPaths.Concat(logFilesPaths).Concat(logHttpPaths).Concat(traceFilesPath)
                .Select(Path.GetFullPath).Distinct().ToArray();
            
            var entries = paths.Select(p => new PathEntry
            {
                Name = p,
                Type = PathEntryType.Directory
            }).Where(p => Directory.Exists(p.Name)).ToArray();

            return new PathEntryCollection
            {
                Current = string.Empty,
                Entries = entries,
                Error = entries.Length == 0 ? "There isn't any valid folder." : null
            };
        }

        #region Nested types
        private class FileSystemSettings : SettingsBase
        {
            [SettingsArray(';')]
            public string[] RootPaths { get; set; } = new string[0];
        }
        public enum PathEntryType
        {
            Directory,
            File
        }
        [DataContract]
        public class PathEntry
        {
            [XmlAttribute, DataMember]
            public string Name { get; set; }
            [XmlAttribute, DataMember]
            public PathEntryType Type { get; set; }
            [XmlAttribute, DataMember]
            public bool IsBinary { get; set; }
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
