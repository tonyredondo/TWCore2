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
        private static readonly HashSet<StorageItemMeta> AllMetas = new HashSet<StorageItemMeta>();
        private static bool _expirationTimerSetted;
        private static Timer _globalExpirationTimer;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetExpirationTimer()
        {
            if (_expirationTimerSetted) return;
            _expirationTimerSetted = true;
            _globalExpirationTimer?.Dispose();
            _globalExpirationTimer = new Timer(i =>
            {
                lock (AllMetas)
                    AllMetas.Where(m => m.IsExpired).Each(m => Try.Do(m.FireOnExpire));
            }, null, 5000, 5000);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RegisterMeta(StorageItemMeta meta)
        {
            lock (AllMetas)
            {
                if (!_expirationTimerSetted) SetExpirationTimer();
                AllMetas.Add(meta);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DeregisterMeta(StorageItemMeta meta)
        {
            lock (AllMetas)
                AllMetas.Remove(meta);
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
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Dispose()
        {
            DeregisterMeta(this);
            OnExpire = null;
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
