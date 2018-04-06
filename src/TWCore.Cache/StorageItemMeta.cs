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
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml.Serialization;
using TWCore.Serialization;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace TWCore.Cache
{
    /// <inheritdoc />
    /// <summary>
    /// Storage item Metadata
    /// </summary>
    [DataContract, Serializable]
    public class StorageItemMeta : IDisposable
    {
        #region Static Timer
        private static readonly object LockPad = new object();
        private static readonly HashSet<StorageItemMeta> AllMetas = new HashSet<StorageItemMeta>();
        private static volatile int _registeredCount;
        private static volatile int _currentCount;
        private static volatile bool _expirationTimerSetted;
        private static volatile bool _runningTimer;
        private static Timer _globalExpirationTimer;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetExpirationTimer()
        {
            if (_expirationTimerSetted) return;
            _expirationTimerSetted = true;
            _globalExpirationTimer?.Dispose();
            _globalExpirationTimer = new Timer(i =>
            {
                if (_runningTimer) return;
                try
                {
                    _runningTimer = true;
                    StorageItemMeta[] metasToRemove;
                    lock (LockPad)
                    {
                        metasToRemove = AllMetas.Where(m => m.IsExpired).ToArray();
                        foreach (var meta in metasToRemove)
                            if (AllMetas.Remove(meta))
                                _currentCount--;
                    }
                    foreach (var meta in metasToRemove)
                    {
                        try
                        {
                            meta?.FireOnExpire();
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Write(ex);
                        }
                    }
                    if (metasToRemove.Length > 0 && _registeredCount > 0 && (double)_currentCount / _registeredCount < 0.6)
                    {
                        lock (LockPad)
                            AllMetas.TrimExcess();
                        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                        GC.Collect();
                        _registeredCount = _currentCount;
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
                finally
                {
                    _runningTimer = false;
                }
            }, null, 5000, 5000);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RegisterMeta(StorageItemMeta meta)
        {
            lock (LockPad)
            {
                if (!_expirationTimerSetted) SetExpirationTimer();
                if (!AllMetas.Add(meta)) return;
                _registeredCount++;
                _currentCount++;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DeregisterMeta(StorageItemMeta meta)
        {
            lock (LockPad)
            {
                if (!AllMetas.Remove(meta)) return;
                _currentCount--;
            }
            if (_registeredCount > 0 && (double)_currentCount / _registeredCount < 0.6)
            {
                lock (LockPad)
                    AllMetas.TrimExcess();
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();
                _registeredCount = _currentCount;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Item Key
        /// </summary>
        [XmlAttribute, DataMember]
        public string Key { get; set; }
        /// <summary>
        /// Item creation date
        /// </summary>
        [XmlAttribute, DataMember]
        public DateTime CreationDate { get; set; }
        /// <summary>
        /// Item expiration date
        /// </summary>
        [DataMember]
        public DateTime? ExpirationDate { get; set; }
        /// <summary>
        /// true if the item has already expired; otherwise, false.
        /// </summary>
        [XmlIgnore, NonSerialize]
        public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < Core.Now;
        /// <summary>
        /// Item tags
        /// </summary>
        [XmlArray("Tags"), XmlArrayItem("Item"), DataMember]
        public List<string> Tags { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// Event fired when the storage item has expired.
        /// </summary>
		[field: NonSerialized]
        public event EventHandler OnExpire;
        #endregion

        #region .ctor
        /// <summary>
        /// Storage item Metadata
        /// </summary>
        public StorageItemMeta()
        {
            RegisterMeta(this);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~StorageItemMeta()
        {
            Dispose();
        }
        private volatile bool _disposed;
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            DeregisterMeta(this);
            OnExpire = null;
        }
        #endregion
        
        #region Static Methods
        /// <summary>
        /// Creates a new StorageItemMeta instance
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="expirationDate">Expiration date</param>
        /// <param name="tags">Tags</param>
        /// <returns>StorageItemMeta instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StorageItemMeta Create(string key, DateTime? expirationDate, string[] tags = null)
        {
            if (string.IsNullOrEmpty(key)) return null;
            var dateNow = Core.Now;
            if (expirationDate < dateNow) return null;
            return new StorageItemMeta
            {
                CreationDate = dateNow,
                ExpirationDate = expirationDate,
                Key = key,
                Tags = tags?.Distinct().ToList()
            };
        }
        #endregion

        #region Protected Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void FireOnExpire()
        {
            Core.Log.LibVerbose("On Expire timer for key: {0}", Key);
            OnExpire?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
