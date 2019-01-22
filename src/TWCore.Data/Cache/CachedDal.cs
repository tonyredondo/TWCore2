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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TWCore.Data.Cache
{
    /// <summary>
    /// Cached dal base class
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TDal">Dal Type</typeparam>
    public abstract class CachedDal<TKey, TEntity, TDal> : ICachedDal
    {
        /// <summary>
        /// All entities
        /// </summary>
        protected TEntity[] All { get; set; }
        /// <summary>
        /// Entities cache
        /// </summary>
        protected ConcurrentDictionary<TKey, TEntity[]> Cache { get; } = new ConcurrentDictionary<TKey, TEntity[]>();
        /// <summary>
        /// Current Dal
        /// </summary>
        protected TDal Dal { get; } = Core.Injector.New<TDal>();
        
        /// <summary>
        /// Load cache
        /// </summary>
        public abstract void Load();
        /// <summary>
        /// Clear cache
        /// </summary>
        public virtual void Clear()
        {
            Cache.Clear();
            All = null;
        }
    }

    /// <summary>
    /// Async Cached dal base class
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TDal">Dal Type</typeparam>
    public abstract class CachedDalAsync<TKey, TEntity, TDal> : ICachedDalAsync
    {
        /// <summary>
        /// All entities
        /// </summary>
        protected TEntity[] All { get; set; }
        /// <summary>
        /// Entities cache
        /// </summary>
        protected ConcurrentDictionary<TKey, TEntity[]> Cache { get; } = new ConcurrentDictionary<TKey, TEntity[]>();
        /// <summary>
        /// Current Dal
        /// </summary>
        protected TDal Dal { get; } = Core.Injector.New<TDal>();

        /// <summary>
        /// Load cache
        /// </summary>
        /// <returns>Load task</returns>
        public abstract Task LoadAsync();
        /// <summary>
        /// Clear cache
        /// </summary>
        public virtual void Clear()
        {
            Cache.Clear();
            All = null;
        }
    }


    /// <summary>
    /// Cached dal base class
    /// </summary>
    /// <typeparam name="TKey">Key 1 type</typeparam>
    /// <typeparam name="TKey2">Key 2 type</typeparam>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TDal">Dal Type</typeparam>
    public abstract class CachedDal<TKey, TKey2, TEntity, TDal> : ICachedDal
    {
        /// <summary>
        /// All entities
        /// </summary>
        protected TEntity[] All { get; set; }
        /// <summary>
        /// Entities cache 1
        /// </summary>
        protected ConcurrentDictionary<TKey, TEntity[]> Cache { get; } = new ConcurrentDictionary<TKey, TEntity[]>();
        /// <summary>
        /// Entities cache 2
        /// </summary>
        protected ConcurrentDictionary<TKey2, TEntity[]> Cache2 { get; } = new ConcurrentDictionary<TKey2, TEntity[]>();
        /// <summary>
        /// Current Dal
        /// </summary>
        protected TDal Dal { get; } = Core.Injector.New<TDal>();

        /// <summary>
        /// Load cache
        /// </summary>
        public abstract void Load();
        /// <summary>
        /// Clear cache
        /// </summary>
        public virtual void Clear()
        {
            Cache.Clear();
            Cache2.Clear();
            All = null;
        }
    }

    /// <summary>
    /// Async Cached dal base class
    /// </summary>
    /// <typeparam name="TKey">Key 1 type</typeparam>
    /// <typeparam name="TKey2">Key 2 type</typeparam>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TDal">Dal Type</typeparam>
    public abstract class CachedDalAsync<TKey, TKey2, TEntity, TDal> : ICachedDalAsync
    {
        /// <summary>
        /// All entities
        /// </summary>
        protected TEntity[] All { get; set; }
        /// <summary>
        /// Entities cache 1
        /// </summary>
        protected ConcurrentDictionary<TKey, TEntity[]> Cache { get; } = new ConcurrentDictionary<TKey, TEntity[]>();
        /// <summary>
        /// Entities cache 2
        /// </summary>
        protected ConcurrentDictionary<TKey2, TEntity[]> Cache2 { get; } = new ConcurrentDictionary<TKey2, TEntity[]>();
        /// <summary>
        /// Current Dal
        /// </summary>
        protected TDal Dal { get; } = Core.Injector.New<TDal>();

        /// <summary>
        /// Load cache
        /// </summary>
        /// <returns>Load task</returns>
        public abstract Task LoadAsync();
        /// <summary>
        /// Clear cache
        /// </summary>
        public virtual void Clear()
        {
            Cache.Clear();
            Cache2.Clear();
            All = null;
        }
    }
}