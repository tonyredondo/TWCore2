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
using System.Threading;
using System.Threading.Tasks;
using TWCore.Compression;
using TWCore.Net.Multicast;
using TWCore.Serialization;
// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable UnusedMember.Global
// ReSharper disable IntroduceOptionalParameters.Global

namespace TWCore.Diagnostics.Trace.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// Writes a simple trace file
    /// </summary>
    public class SimpleFileTraceStorage : ITraceStorage
    {
        private static readonly NonBlocking.ConcurrentDictionary<string, StreamWriter> LogStreams = new NonBlocking.ConcurrentDictionary<string, StreamWriter>();
        private NonBlockingTraceWriter _traceWriter;
        private readonly Guid _discoveryServiceId;
        private StreamWriter _sWriter;
        private string _currentFileName;
        private Timer _flushTimer;
        private int _shouldFlush;

        #region Properties
        /// <summary>
        /// Serializer
        /// </summary>
        public ISerializer Serializer { get; }
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
        /// Base path
        /// </summary>
        public string BasePath { get; }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Writes a simple trace file
        /// </summary>
        /// <param name="basePath">Base path</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimpleFileTraceStorage(string basePath) : this(
            basePath, null, true)
        {
        }
        /// <inheritdoc />
        /// <summary>
        /// Writes a simple trace file
        /// </summary>
        /// <param name="basePath">Base path</param>
        /// <param name="serializer">Serializer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimpleFileTraceStorage(string basePath, ISerializer serializer) : this(
            basePath, serializer, true)
        {
        }
        /// <summary>
        /// Writes a simple trace file
        /// </summary>
        /// <param name="basePath">Base path</param>
        /// <param name="serializer">Serializer</param>
        /// <param name="createByDay">True if a new trace file is created each day; otherwise, false</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimpleFileTraceStorage(string basePath, ISerializer serializer, bool createByDay)
        {
            BasePath = basePath;
            FileName = Path.Combine(basePath, "Trace.txt");
            Serializer = serializer;
            if (Serializer == null)
            {
                Serializer = (ISerializer)Activator.CreateInstance(Core.GlobalSettings.TraceSerializerType);
                if (Serializer != null)
                {
                    var compressorType = Core.GlobalSettings.TraceCompressorType;
                    if (compressorType != null)
                        Serializer.Compressor = (ICompressor)Activator.CreateInstance(compressorType);
                }
            }
            CreateByDay = createByDay;
            _traceWriter = new NonBlockingTraceWriter(this);
            _flushTimer = new Timer(obj =>
            {
                if (_sWriter == null) return;
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
            EnsureTraceFile(FileName);
            _discoveryServiceId = DiscoveryService.RegisterService(DiscoveryService.FrameworkCategory, "TRACE.FILE", "This is the File Trace base path", new SerializedObject(Path.GetFullPath(BasePath)));
            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(FileName), FileName);
                collection.Add(nameof(CreateByDay), CreateByDay);
                collection.Add(nameof(FileDate), FileDate);
                collection.Add(nameof(Serializer), Serializer);
                collection.Add("Current FileName", _currentFileName);
            });
        }
        /// <summary>
        /// Destructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~SimpleFileTraceStorage()
        {
            DiscoveryService.UnregisterService(_discoveryServiceId);
            Dispose();
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureTraceFile(string fileName)
        {
            if (FileDate.Date == DateTime.Today) return;
            
            string oldFileName;
            if (CreateByDay)
            {
                _currentFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "_" + DateTime.Today.ToString("yyyy-MM-dd") + Path.GetExtension(fileName));
                oldFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "_" + FileDate.Date.ToString("yyyy-MM-dd") + Path.GetExtension(fileName));
            }
            else
            {
                _currentFileName = fileName;
                oldFileName = fileName;
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
                if (!Directory.Exists(folder))
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
        private string GetObjectFilePath(TraceItem item)
        {
            var pattern = DateTime.Today.ToString("yyyy-MM-dd");

            if (pattern.StartsWith("\\"))
                pattern = pattern.Substring(1);
            if (BasePath.IsNullOrEmpty())
                Core.Log.Warning("BasePath parameter is null!");
            else
            {
                if (!Directory.Exists(BasePath))
                    Directory.CreateDirectory(BasePath);
            }

            var myPath = Path.Combine(BasePath, pattern);
            var path = Factory.GetAbsolutePath(myPath);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (item.GroupName.IsNotNullOrEmpty())
            {
                path = Path.Combine(path, item.GroupName);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            var fileName = string.Format("{0} ({1})", item.TraceName ?? item.TraceObject?.GetType().Name ?? "Null Type", item.Id);
            path = path.RemovePathInvalidChars();
            fileName = fileName.RemoveFileNameInvalidChars();
            var filePath = Path.Combine(path, fileName);
            return filePath;
        }
        #endregion

        #region Nested Types
        
        private class NonBlockingTraceWriter : IDisposable
        {
            private readonly BlockingCollection<(string FilePath, object Data)> _queue = new BlockingCollection<(string FilePath, object Data)>();
            private readonly SimpleFileTraceStorage _storage;
            private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
            
            #region .ctor
            public NonBlockingTraceWriter(SimpleFileTraceStorage storage)
            {
                _storage = storage;
                var thread = new Thread(WriteTraceThread)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Lowest,
                    Name = "Trace Thread"
                };
                thread.Start();
            }
            #endregion
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void WriteTrace(string filePath, object data) 
                => _queue.Add((filePath, data));

            private void WriteTraceThread()
            {
                var token = _tokenSource.Token;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        (var filePath, var data) = _queue.Take(token);
                        if (data != null)
                        {
                            try
                            {
                                var serializer = _storage.Serializer ?? SerializerManager.Serializers[0];
                                serializer.SerializeToFile(data, filePath);
                            }
                            catch (Exception ex)
                            {
                                File.WriteAllText(filePath + ".txt", ex.Message + Environment.NewLine + ex.StackTrace);
                            }
                        }
                        else
                            File.WriteAllText(filePath + ".txt", "Object is Null");
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (ObjectDisposedException)
                    {
                        return;
                    }
                    catch(Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                }
            }

            public void Dispose()
            {
                if (!_tokenSource.IsCancellationRequested)
                    _tokenSource?.Cancel();
                _tokenSource.Dispose();
                _queue.Dispose();
            }
        }
        
        #endregion
        
        /// <inheritdoc />
        /// <summary>
        /// Writes a trace item to the storage
        /// </summary>
        /// <param name="item">Trace item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task WriteAsync(TraceItem item)
        {
            EnsureTraceFile(FileName);
            if (_sWriter == null) return;
            var traceFilePath = GetObjectFilePath(item);
            _traceWriter?.WriteTrace(traceFilePath, item.TraceObject);
            var line = string.Format("{0} ({1, 15}) {2}: {3}", 
                item.Timestamp.GetTimeSpanFormat(), 
                string.IsNullOrEmpty(item.GroupName) ? "NO GROUP" : item.GroupName, 
                item.TraceName, 
                traceFilePath);
            await _sWriter.WriteLineAsync(line).ConfigureAwait(false);
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
                _traceWriter?.Dispose();
                _traceWriter = null;
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
