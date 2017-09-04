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

namespace TWCore.Diagnostics.Log.Storages
{
    /// <summary>
    /// Writes a simple log file
    /// </summary>
    public class SimpleFileLogStorage : ILogStorage
    {
        /// <summary>
        /// All file log storage writers
        /// </summary>
        static ConcurrentDictionary<string, StreamWriter> logStreams = new ConcurrentDictionary<string, StreamWriter>();
        StreamWriter sWriter;
        string currentFileName;
        int numbersOfFiles;
        volatile bool firstWrite = true;

        #region Properties
        /// <summary>
        /// File name with path
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// File creation date
        /// </summary>
        public DateTime FileDate { get; private set; }
        /// <summary>
        /// True if a new log file is created each day; otherwise, false
        /// </summary>
        public bool CreateByDay { get; private set; }
        /// <summary>
        /// True if a new log file is created when a maximum length is reached; otherwise, false.
        /// </summary>
        public bool UseMaxLength { get; private set; }
        /// <summary>
        /// Maximun length in bytes for a single file. Default value is 4Mb
        /// </summary>
        public long MaxLength { get; private set; } = 4194304L;
        #endregion

        #region .ctor
        /// <summary>
        /// Writes a simple log file
        /// </summary>
        /// <param name="fileName">File name with path</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimpleFileLogStorage(string fileName) :
            this(fileName, true)
        { }
        /// <summary>
        /// Writes a simple log file
        /// </summary>
        /// <param name="fileName">File name with path</param>
        /// <param name="createByDay">True if a new log file is created each day; otherwise, false</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimpleFileLogStorage(string fileName, bool createByDay) :
            this(fileName, createByDay, false)
        { }
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
                collection.Add("Current FileName", currentFileName);
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
            bool dayHasChange = FileDate.Date != DateTime.Today;
            long fileLength = -1L;
            bool maxLengthReached = false;
            if (UseMaxLength)
            {
                fileLength = new FileInfo(currentFileName).Length;
                if (fileLength >= MaxLength)
                    maxLengthReached = true;
            }
            if (dayHasChange || maxLengthReached)
            {
                string oldFileName = string.Empty;
                if (dayHasChange && CreateByDay)
                {
                    currentFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "_" + DateTime.Today.ToString("yyyy-MM-dd") + Path.GetExtension(fileName));
                    oldFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "_" + FileDate.Date.ToString("yyyy-MM-dd") + Path.GetExtension(fileName));
                    numbersOfFiles = 0;
                }
                else
                {
                    currentFileName = fileName;
                    oldFileName = fileName;
                }
                if (maxLengthReached)
                {
                    oldFileName = currentFileName;
                    numbersOfFiles++;
                    currentFileName = Path.Combine(Path.GetDirectoryName(currentFileName), Path.GetFileNameWithoutExtension(currentFileName) + "_" + FileDate.Date.ToString("yyyy-MM-dd") + "." + numbersOfFiles + Path.GetExtension(currentFileName));
                }

                #region Remove previous
                try
                {
                    sWriter?.Dispose();
                    sWriter = null;
                }
                catch
                {
                    // ignored
                }
                if (logStreams.TryRemove(oldFileName, out var oldWriter))
                {
                    try
                    {
                        oldWriter?.Dispose();
                        oldWriter = null;
                    }
                    catch
                    {
                        // ignored
                    }
                }
                #endregion

                #region Load Writer
                sWriter = logStreams.GetOrAdd(currentFileName, fname =>
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
                if (File.Exists(currentFileName))
                    FileDate = File.GetCreationTime(currentFileName);
                #endregion
            }
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

        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ILogItem item)
        {
            EnsureLogFile(FileName);
            if (sWriter != null)
            {
                if (firstWrite)
                {
                    lock (sWriter)
                    {
                        sWriter.WriteLine();
                        sWriter.WriteLine();
                        sWriter.WriteLine();
                        sWriter.WriteLine();
                        sWriter.WriteLine();
                        sWriter.WriteLine("-.");
                        sWriter.Flush();
                    }
                    firstWrite = false;
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
                string line = sbuilder.ToString();

                lock (sWriter)
                {
                    sWriter.WriteLine(line);
                    sWriter.Flush();
                }
            }
        }
        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEmptyLine()
        {
            lock (sWriter)
            {
                sWriter.WriteLine(string.Empty);
                sWriter.Flush();
            }
        }

        /// <summary>
        /// Dispose the current object resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            try
            {
                sWriter?.WriteLine(".-");
                sWriter?.Flush();
                sWriter?.Dispose();
                sWriter = null;
            }
            catch
            {
                // ignored
            }
            if (!logStreams.TryRemove(currentFileName, out var oldWriter)) return;
            try
            {
                oldWriter?.Dispose();
                oldWriter = null;
            }
            catch
            {
                // ignored
            }
        }
    }
}
