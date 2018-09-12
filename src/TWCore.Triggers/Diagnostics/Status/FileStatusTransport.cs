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
using TWCore.Net.Multicast;
using TWCore.Serialization;
using TWCore.Triggers;
using SPath = System.IO.Path;
// ReSharper disable CheckNamespace
// ReSharper disable InheritdocConsiderUsage

namespace TWCore.Diagnostics.Status.Transports
{
    /// <summary>
    /// File Status Transport
    /// </summary>
    public class FileStatusTransport : TriggeredActionBase, IStatusTransport
    {
        private readonly Guid _discoveryServiceId;

        #region Events
        /// <summary>
        /// Handles when a fetch status event has been received
        /// </summary>
        public event FetchStatusDelegate OnFetchStatus;
        #endregion

        #region Properties
        /// <summary>
        /// Filename format
        /// </summary>
        public string FileNameFormat { get; set; } = "Status-{yyyy}-{MM}-{dd}_{HH}-{mm}";
        /// <summary>
        /// File path
        /// </summary>
        public string Path { get; set; } = AppContext.BaseDirectory;
        /// <summary>
        /// File Serializer
        /// </summary>
        public ISerializer Serializer { get; set; } = new XmlTextSerializer();
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// File Status Transport
        /// </summary>
        public FileStatusTransport()
        {
            if (!string.IsNullOrWhiteSpace(Path))
                _discoveryServiceId = DiscoveryService.RegisterService(DiscoveryService.FrameworkCategory, "STATUS.FILE", "This is the File Log base path", new SerializedObject(SPath.GetFullPath(Path)));
        }
        /// <inheritdoc />
        /// <summary>
        /// File Status Transport
        /// </summary>
        /// <param name="triggers">Triggers</param>
        public FileStatusTransport(params TriggerBase[] triggers)
        {
            triggers?.Each(AddTrigger);
            if (!string.IsNullOrWhiteSpace(Path))
                _discoveryServiceId = DiscoveryService.RegisterService(DiscoveryService.FrameworkCategory, "STATUS.FILE", "This is the File Log base path", new SerializedObject(SPath.GetFullPath(Path)));
        }
        /// <inheritdoc />
        /// <summary>
        /// File Status Transport
        /// </summary>
        /// <param name="filenameFormat">Filename format</param>
        /// <param name="path">File path</param>
        /// <param name="serializer">File serializer</param>
        /// <param name="triggers">Triggers</param>
        public FileStatusTransport(string filenameFormat, string path, ISerializer serializer, params TriggerBase[] triggers) : this(triggers)
        {
            path = Factory.ResolveLowLowPath(path);
            FileNameFormat = filenameFormat;
            Path = path;
            Serializer = serializer;

            if (!string.IsNullOrWhiteSpace(Path))
                _discoveryServiceId = DiscoveryService.RegisterService(DiscoveryService.FrameworkCategory, "STATUS.FILE", "This is the File Log base path", new SerializedObject(SPath.GetFullPath(Path)));
        }
        /// <summary>
        /// FileStatusTransport finalizer
        /// </summary>
        ~FileStatusTransport()
        {
            DiscoveryService.UnregisterService(_discoveryServiceId);
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Action to execute when the trigger occurs
        /// </summary>
        protected override void OnAction()
        {
            var status = OnFetchStatus?.Invoke();
            try
            {
                if (status is null) return;
                var now = Core.Now;
                var filename = FileNameFormat
                    .Replace("{yyyy}", now.ToString("yyyy"))
                    .Replace("{MM}", now.ToString("MM"))
                    .Replace("{dd}", now.ToString("dd"))
                    .Replace("{HH}", now.ToString("HH"))
                    .Replace("{mm}", now.ToString("mm"));

                var filePath = SPath.Combine(AppContext.BaseDirectory, Path, filename);
                var folder = SPath.GetDirectoryName(filePath);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                Serializer.SerializeToFile(status, filePath);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
    }
}
