using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Compression;
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
        private static readonly string[] TextExtensions = new[] {".xml", ".js", ".txt", ".log", ".json"};

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
        public PathEntryCollection Get()
        {
            string virtualPath = null;
            if (Request.Query.TryGetValue("path", out var folderValues))
                virtualPath = folderValues;

            if (string.IsNullOrWhiteSpace(virtualPath))
                return GetRootEntryCollection();
            
            var path = Path.GetFullPath(virtualPath);
            if (!Directory.Exists(path))
                return new PathEntryCollection { Current = virtualPath, Entries =  PathEmpty, Error = "Directory doesn't exist." };
            if (Settings.RootPaths.All(rp => !path.StartsWith(Path.GetFullPath(rp), StringComparison.OrdinalIgnoreCase)))
                return new PathEntryCollection { Current = virtualPath, Entries =  PathEmpty, Error = "The path is outside of allowed folders." };

            var pathEntries = Directory.EnumerateDirectories(path)
                .Select(d => new PathEntry
                {
                    Name  = Path.GetFullPath(d).Replace(path, string.Empty, StringComparison.OrdinalIgnoreCase),
                    Type =  PathEntryType.Directory
                })
                .Concat(Directory.EnumerateFiles(path)
                .Where(file =>
                {
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
            return new PathEntryCollection { Current = virtualPath, Entries = pathEntries };
        }

        [HttpGet("api/files/load.{format}")]
        [HttpGet("api/files/load")]
        [FormatFilter]
        public FileLoadedStatus LoadFile()
        {
            object obj;
            SessionData sessionData;
            Type fileType = null;

            if (Request.Query.TryGetValue("type", out var type))
                fileType = Core.GetType(type);
            if (!Request.Query.TryGetValue("path", out var path))
                return new FileLoadedStatus { Loaded = false };
            if (string.IsNullOrWhiteSpace(path))
                return new FileLoadedStatus { Loaded = false };
            path = Path.GetFullPath(path);
            if (!System.IO.File.Exists(path))
                return new FileLoadedStatus { FilePath = path, Loaded = false };

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
                return new FileLoadedStatus
                {
                    FilePath = sessionData.FilePath,
                    ObjectType = sessionData.FileObject?.GetType().Name
                };
            }

            Core.Log.Warning("The serializer for file {0} wasn't found.", path);
            var extension = Path.GetExtension(path);

            if (string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(extension, ".html", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(extension, ".htm", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".js", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".log", StringComparison.OrdinalIgnoreCase) ||
                serializer?.SerializerType == SerializerType.Text)
            {
                obj = System.IO.File.ReadAllText(path);
                sessionData = HttpContext.Session.GetSessionData();
                sessionData.FilePath = path;
                sessionData.FileObject = obj;
                HttpContext.Session.SetSessionData(sessionData);
                Core.Log.InfoBasic("File {0} was loaded as text data.", path);
                return new FileLoadedStatus
                {
                    FilePath = sessionData.FilePath,
                    ObjectType = sessionData.FileObject?.GetType().Name
                };
            }

            obj = System.IO.File.ReadAllBytes(path);
            sessionData = HttpContext.Session.GetSessionData();
            sessionData.FilePath = path;
            sessionData.FileObject = obj;
            HttpContext.Session.SetSessionData(sessionData);
            Core.Log.InfoBasic("File {0} was loaded as bytes data.", path);
            return new FileLoadedStatus
            {
                FilePath = sessionData.FilePath,
                ObjectType = sessionData.FileObject?.GetType().Name
            };
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

        private static PathEntryCollection GetRootEntryCollection()
        {
            var entries = Settings.RootPaths.Select(p => new PathEntry
            {
                Name = Path.GetFullPath(p),
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
