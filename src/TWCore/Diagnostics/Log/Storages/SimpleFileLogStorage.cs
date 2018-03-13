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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Net.Multicast;
using TWCore.Serialization;

// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable UnusedMember.Global
// ReSharper disable IntroduceOptionalParameters.Global

namespace TWCore.Diagnostics.Log.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// Writes a simple log file
    /// </summary>
    public class SimpleFileLogStorage : ILogStorage
    {
        /// <summary>
        /// All file log storage writers
        /// </summary>
        private static readonly NonBlocking.ConcurrentDictionary<string, StreamWriter> LogStreams = new NonBlocking.ConcurrentDictionary<string, StreamWriter>();
        private readonly StringBuilder _stringBuffer = new StringBuilder(128);
        private readonly Guid _discoveryServiceId;
        private StreamWriter _sWriter;
        private string _currentFileName;
        private int _numbersOfFiles;
        private volatile bool _firstWrite = true;

        #region Properties
        /// <summary>
        /// File name with path
        /// </summary>
        public string FileName { get; }
        /// <summary>
        /// File creation date
        /// </summary>
        public DateTime FileDate { get; private set; }
        /// <summary>
        /// True if a new log file is created each day; otherwise, false
        /// </summary>
        public bool CreateByDay { get; }
        /// <summary>
        /// True if a new log file is created when a maximum length is reached; otherwise, false.
        /// </summary>
        public bool UseMaxLength { get; }
        /// <summary>
        /// Maximun length in bytes for a single file. Default value is 4Mb
        /// </summary>
        public long MaxLength { get; }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Writes a simple log file
        /// </summary>
        /// <param name="fileName">File name with path</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimpleFileLogStorage(string fileName) :
            this(fileName, true)
        { }
        /// <inheritdoc />
        /// <summary>
        /// Writes a simple log file
        /// </summary>
        /// <param name="fileName">File name with path</param>
        /// <param name="createByDay">True if a new log file is created each day; otherwise, false</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimpleFileLogStorage(string fileName, bool createByDay) :
            this(fileName, createByDay, false)
        { }
        /// <inheritdoc />
        /// <summary>
        /// Writes a simple log file
        /// </summary>
        /// <param name="fileName">File name with path</param>
        /// <param name="createByDay">True if a new log file is created each day; otherwise, false</param>
        /// <param name="useMaxLength">True if a new log file is created when a maximum length is reached; otherwise, false.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimpleFileLogStorage(string fileName, bool createByDay, bool useMaxLength) :
            this(fileName, createByDay, useMaxLength, 4194304L)
        { }
        /// <summary>
        /// Writes a simple log file
        /// </summary>
        /// <param name="fileName">File name with path</param>
        /// <param name="createByDay">True if a new log file is created each day; otherwise, false</param>
        /// <param name="useMaxLength">True if a new log file is created when a maximum length is reached; otherwise, false.</param>
        /// <param name="maxLength">Maximun length in bytes for a single file. Default value is 4Mb</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimpleFileLogStorage(string fileName, bool createByDay, bool useMaxLength, long maxLength)
        {
            FileName = fileName;
            CreateByDay = createByDay;
            UseMaxLength = useMaxLength;
            MaxLength = maxLength;
            EnsureLogFile(fileName);
            if (!string.IsNullOrWhiteSpace(fileName))
                _discoveryServiceId = DiscoveryService.RegisterService(DiscoveryService.FrameworkCategory, "LOG.FILE", "This is the File Log base path", new SerializedObject(Path.GetDirectoryName(Path.GetFullPath(fileName))));
            Core.Status.Attach(collection =>
            {
                collection.Add("Configuration",
                    new StatusItemValueItem(nameof(FileName), FileName),
                    new StatusItemValueItem(nameof(CreateByDay), CreateByDay),
                    new StatusItemValueItem(nameof(UseMaxLength), UseMaxLength),
                    new StatusItemValueItem(nameof(MaxLength) + " (MB)", MaxLength.ToMegabytes())
                );
                collection.Add("Current FileDate", FileDate);
                collection.Add("Current FileName", _currentFileName);
            });
        }

        /// <summary>
        /// Destructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~SimpleFileLogStorage()
        {
            DiscoveryService.UnregisterService(_discoveryServiceId);
            Core.Status.DeAttachObject(this);
            Dispose();
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureLogFile(string fileName)
        {
            var dayHasChange = FileDate.Date != DateTime.Today;
            var maxLengthReached = false;
            if (UseMaxLength && _currentFileName.IsNotNullOrWhitespace() && File.Exists(_currentFileName))
            {
                var fileLength = new FileInfo(_currentFileName).Length;
                if (fileLength >= MaxLength)
                    maxLengthReached = true;
            }
            if (!dayHasChange && !maxLengthReached) return;

            string oldFileName;
            if (dayHasChange && CreateByDay)
            {
                _currentFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "_" + DateTime.Today.ToString("yyyy-MM-dd") + Path.GetExtension(fileName));
                oldFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "_" + FileDate.Date.ToString("yyyy-MM-dd") + Path.GetExtension(fileName));
                _numbersOfFiles = 0;
            }
            else
            {
                _currentFileName = fileName;
                oldFileName = fileName;
            }
            if (maxLengthReached)
            {
                oldFileName = _currentFileName;
                _numbersOfFiles++;
                _currentFileName = Path.Combine(Path.GetDirectoryName(_currentFileName), Path.GetFileNameWithoutExtension(_currentFileName) + "_" + FileDate.Date.ToString("yyyy-MM-dd") + "." + _numbersOfFiles + Path.GetExtension(_currentFileName));
            }

            #region Remove previous
            try
            {
                _sWriter?.Dispose();
                _sWriter = null;
            }
            catch
            {
                // ignored
            }
            if (LogStreams.TryRemove(oldFileName, out var oldWriter))
            {
                try
                {
                    oldWriter?.Dispose();
                }
                catch
                {
                    // ignored
                }
            }
            #endregion

            #region Load Writer
            _sWriter = LogStreams.GetOrAdd(_currentFileName, fname =>
            {
                var folder = Path.GetDirectoryName(fname);
                if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                var sw = new StreamWriter(new FileStream(fname, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 2048, true))
                {
                    AutoFlush = true
                };
                return sw;
            });
            if (File.Exists(_currentFileName))
                FileDate = File.GetCreationTime(_currentFileName);
            #endregion
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetExceptionDescription(SerializableException itemEx, StringBuilder sbuilder)
        {
            while (true)
            {
                sbuilder.AppendFormat("\tType: {0}\r\n\tMessage: {1}\r\n\tStack: {2}\r\n\r\n", itemEx.ExceptionType, itemEx.Message.Replace("\r", "\\r").Replace("\n", "\\n"), itemEx.StackTrace);
                if (itemEx.InnerException == null) break;
                itemEx = itemEx.InnerException;
            }
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task WriteAsync(ILogItem item)
        {
            EnsureLogFile(FileName);
            if (_sWriter == null) return;
            string buffer;
            lock (_sWriter)
            {
                if (_firstWrite)
                {
                    _stringBuffer.AppendLine();
                    _stringBuffer.AppendLine();
                    _stringBuffer.AppendLine();
                    _stringBuffer.AppendLine();
                    _stringBuffer.AppendLine();
                    _stringBuffer.AppendLine("-.");
                    _firstWrite = false;
                }
                _stringBuffer.Append(item.Timestamp.GetTimeSpanFormat());
                _stringBuffer.AppendFormat("{0, 11}: ", item.Level);

                if (!string.IsNullOrEmpty(item.GroupName))
                    _stringBuffer.Append(item.GroupName + " - ");

                if (item.LineNumber > 0)
                    _stringBuffer.AppendFormat("<{0};{1:000}> ", string.IsNullOrEmpty(item.TypeName) ? string.Empty : item.TypeName, item.LineNumber);
                else if (!string.IsNullOrEmpty(item.TypeName))
                    _stringBuffer.Append("<" + item.TypeName + "> ");

                if (!string.IsNullOrEmpty(item.Code))
                    _stringBuffer.Append("[" + item.Code + "] ");

                _stringBuffer.Append(item.Message);

                if (item.Exception != null)
                {
                    _stringBuffer.Append("\r\nExceptions:\r\n");
                    GetExceptionDescription(item.Exception, _stringBuffer);
                }
                buffer = _stringBuffer.ToString();
                _stringBuffer.Clear();
            }
            await _sWriter.WriteLineAsync(buffer).ConfigureAwait(false);
            await _sWriter.FlushAsync().ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task WriteEmptyLineAsync()
        {
            await _sWriter.WriteLineAsync(string.Empty).ConfigureAwait(false);
            await _sWriter.FlushAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        /// <summary>
        /// Dispose the current object resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            try
            {
                _sWriter?.WriteLine(".-");
                _sWriter?.Flush();
                _sWriter?.Dispose();
                _sWriter = null;
            }
            catch
            {
                // ignored
            }
            if (!LogStreams.TryRemove(_currentFileName, out var oldWriter)) return;
            try
            {
                oldWriter?.Dispose();
            }
            catch
            {
                // ignored
            }
        }
    }
}
