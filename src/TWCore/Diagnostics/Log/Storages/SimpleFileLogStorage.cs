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
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
    [StatusName("Simple File Log")]
    public class SimpleFileLogStorage : ILogStorage
    {
        /// <summary>
        /// All file log storage writers
        /// </summary>
        private static readonly ConcurrentDictionary<string, StreamWriter> LogStreams = new ConcurrentDictionary<string, StreamWriter>();
        private static readonly ConcurrentStack<StringBuilder> StringBuilderPool = new ConcurrentStack<StringBuilder>();
        private readonly Guid _discoveryServiceId;
        private StreamWriter _sWriter;
        private string _currentFileName;
        private int _numbersOfFiles;
        private volatile bool _firstWrite = true;
        private Timer _flushTimer;
        private int _shouldFlush;
        
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
            fileName = Factory.ResolveLowLowPath(fileName);
            FileName = fileName;
            CreateByDay = createByDay;
            UseMaxLength = useMaxLength;
            MaxLength = maxLength;
            EnsureLogFile(fileName);
            _flushTimer = new Timer(obj =>
            {
                if (_sWriter is null) return;
                try
                {
                    if (Interlocked.CompareExchange(ref _shouldFlush, 0, 1) == 1)
                        _sWriter.Flush();
                }
                catch
                {
                    //
                }
            }, this, 1500, 1500);
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
                var sw = new StreamWriter(new FileStream(fname, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 4096, true));
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
                if (itemEx.Data == null || itemEx.Data.Count == 0)
                    sbuilder.AppendFormat("\tType: {0}\r\n\tMessage: {1}\r\n\tStack: {2}\r\n\r\n", itemEx.ExceptionType, itemEx.Message.Replace("\r", "\\r").Replace("\n", "\\n"), itemEx.StackTrace);
                else
                {
                    sbuilder.AppendFormat("\tType: {0}\r\n\tMessage: {1}\r\n\tData:\r\n",
                        itemEx.ExceptionType,
                        itemEx.Message.Replace("\r", "\\r").Replace("\n", "\\n"));

                    foreach (var dataItem in itemEx.Data)
                        sbuilder.AppendFormat("\t\t{0}: {1}\r\n", dataItem.Key, dataItem.Value);

                    sbuilder.AppendFormat("\tStack: {0}\r\n\r\n",
                        itemEx.StackTrace);
                }
                if (itemEx.InnerException is null) break;
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
            if (_sWriter is null) return;
            if (!StringBuilderPool.TryPop(out var strBuffer))
                strBuffer = new StringBuilder();
            if (_firstWrite)
            {
                strBuffer.AppendLine();
                strBuffer.AppendLine();
                strBuffer.AppendLine();
                strBuffer.AppendLine();
                strBuffer.AppendLine();
                strBuffer.AppendLine("-.");
                _firstWrite = false;
            }
            strBuffer.Append(item.Timestamp.GetTimeSpanFormat());
            strBuffer.AppendFormat("{0, 11}: ", item.Level);

            if (!string.IsNullOrEmpty(item.GroupName))
                strBuffer.Append(item.GroupName + " - ");

            if (!string.IsNullOrEmpty(item.TypeName))
                strBuffer.Append("<" + item.TypeName + "> ");

            if (!string.IsNullOrEmpty(item.Code))
                strBuffer.Append("[" + item.Code + "] ");

            strBuffer.Append(item.Message);

            if (item.Exception != null)
            {
                strBuffer.Append("\r\nExceptions:\r\n");
                GetExceptionDescription(item.Exception, strBuffer);
            }
            var buffer = strBuffer.ToString();
            strBuffer.Clear();      
            StringBuilderPool.Push(strBuffer);
            await _sWriter.WriteLineAsync(buffer).ConfigureAwait(false);
            Interlocked.Exchange(ref _shouldFlush, 1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task WriteEmptyLineAsync()
        {
            await _sWriter.WriteLineAsync(string.Empty).ConfigureAwait(false);
            Interlocked.Exchange(ref _shouldFlush, 1);
        }
        /// <summary>
        /// Writes a group metadata item to the storage
        /// </summary>
        /// <param name="item">Group metadata item</param>
        /// <returns>Task process</returns>
        public async Task WriteAsync(IGroupMetadata item)
        {
            EnsureLogFile(FileName);
            if (_sWriter is null) return;
            if (!StringBuilderPool.TryPop(out var strBuffer))
                strBuffer = new StringBuilder();
            if (_firstWrite)
            {
                strBuffer.AppendLine();
                strBuffer.AppendLine();
                strBuffer.AppendLine();
                strBuffer.AppendLine();
                strBuffer.AppendLine();
                strBuffer.AppendLine("-.");
                _firstWrite = false;
            }
            if (item == null || string.IsNullOrWhiteSpace(item.GroupName)) return;

            strBuffer.Append(item.Timestamp.GetTimeSpanFormat());
            strBuffer.AppendFormat("{0, 11}: ", "GroupData");
            strBuffer.Append(item.GroupName + " | ");
            if (item.Items != null)
            {
                strBuffer.Append(" [");
                var count = item.Items.Length;
                for (var i = 0; i < count; i++)
                {
                    var keyValue = item.Items[i];
                    strBuffer.AppendFormat("{0} = {1}", keyValue.Key, keyValue.Value);
                    if (i < count - 1)
                        strBuffer.Append(", ");
                }
                strBuffer.Append("] ");
            }
            strBuffer.AppendLine();
            var message = strBuffer.ToString();
            strBuffer.Clear();
            StringBuilderPool.Push(strBuffer);

            await _sWriter.WriteLineAsync(message).ConfigureAwait(false);
            Interlocked.Exchange(ref _shouldFlush, 1);
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
                _flushTimer?.Dispose();
                _flushTimer = null;
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
