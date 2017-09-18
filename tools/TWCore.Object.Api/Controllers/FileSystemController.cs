using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
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
        private static readonly string[] Extensions = SerializerManager.Serializers.Select(s => s.Extensions).SelectMany(i => i).Distinct().Concat(".txt", ".html", ".htm").ToArray();

        // GET api/values
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
                .Where(file => Extensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                .Select(d => new PathEntry
                {
                    Name = Path.GetFullPath(d).Replace(path, string.Empty, StringComparison.OrdinalIgnoreCase),
                    Type = PathEntryType.File
                }))
                .ToArray();
            return new PathEntryCollection { Current = virtualPath, Entries = pathEntries };
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
        #endregion
    }
}
