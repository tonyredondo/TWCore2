/*
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
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
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
        private static readonly ConcurrentDictionary<string, StreamWriter> LogStreams = new ConcurrentDictionary<string, StreamWriter>();

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

            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(FileName), FileName);
                collection.Add(nameof(CreateByDay), CreateByDay);
                collection.Add(nameof(FileDate), FileDate);
                collection.Add(nameof(UseMaxLength), UseMaxLength);
                collection.Add(nameof(MaxLength), MaxLength);
                collection.Add("Current FileName", _currentFileName);
            });
        }

        /// <summary>
        /// Destructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~SimpleFileLogStorage()
        {
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
                var sw = new StreamWriter(new FileStream(fname, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
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
        private static string GetExceptionDescription(SerializableException itemEx)
        {
            var desc = string.Format("\tType: {0}\r\n\tMessage: {1}\r\n\tStack: {2}\r\n", itemEx.ExceptionType, itemEx.Message.Replace("\r", "\\r").Replace("\n", "\\n"), itemEx.StackTrace);
            if (itemEx.InnerException != null)
                desc += GetExceptionDescription(itemEx.InnerException);
            return desc;
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ILogItem item)
        {
            EnsureLogFile(FileName);
            if (_sWriter == null) return;
            if (_firstWrite)
            {
                lock (_sWriter)
                {
                    _sWriter.WriteLine();
                    _sWriter.WriteLine();
                    _sWriter.WriteLine();
                    _sWriter.WriteLine();
                    _sWriter.WriteLine();
                    _sWriter.WriteLine("-.");
                    _sWriter.Flush();
                }
                _firstWrite = false;
            }
            var sbuilder = new StringBuilder(128);
            sbuilder.Append(item.Timestamp.GetTimeSpanFormat());
            sbuilder.AppendFormat(" ({0:000}) ", item.ThreadId);
            sbuilder.AppendFormat("{0, 10}: ", item.Level);

            if (!string.IsNullOrEmpty(item.GroupName))
                sbuilder.Append(item.GroupName + " ");

            if (item.LineNumber > 0)
                sbuilder.AppendFormat("<{0};{1:000}> ", string.IsNullOrEmpty(item.TypeName) ? string.Empty : item.TypeName, item.LineNumber);
            else if (!string.IsNullOrEmpty(item.TypeName))
                sbuilder.Append("<" + item.TypeName + "> ");

            if (!string.IsNullOrEmpty(item.Code))
                sbuilder.Append("[" + item.Code + "] ");

            sbuilder.Append(item.Message);

            if (item.Exception != null)
            {
                sbuilder.Append("\r\nExceptions:\r\n");
                sbuilder.Append(GetExceptionDescription(item.Exception));
            }
            var line = sbuilder.ToString();

            lock (_sWriter)
            {
                _sWriter.WriteLine(line);
                _sWriter.Flush();
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEmptyLine()
        {
            lock (_sWriter)
            {
                _sWriter.WriteLine(string.Empty);
                _sWriter.Flush();
            }
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
