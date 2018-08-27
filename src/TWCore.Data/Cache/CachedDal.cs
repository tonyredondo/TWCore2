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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace TWCore.Data.Cache
{
    public abstract class CachedDal<TKey, TEntity, TDal> : ICachedDal
    {
        protected IEnumerable<TEntity> All { get; set; }
        protected NonBlocking.ConcurrentDictionary<TKey, IEnumerable<TEntity>> Cache { get; } = new NonBlocking.ConcurrentDictionary<TKey, IEnumerable<TEntity>>();
        protected TDal Dal { get; } = Core.Injector.New<TDal>();

        public virtual void Clear()
        {
            Cache.Clear();
            All = null;
        }
        public abstract void Load();
    }

    public abstract class CachedDalAsync<TKey, TEntity, TDal> : ICachedDalAsync
    {
        protected IEnumerable<TEntity> All { get; set; }
        protected NonBlocking.ConcurrentDictionary<TKey, IEnumerable<TEntity>> Cache { get; } = new NonBlocking.ConcurrentDictionary<TKey, IEnumerable<TEntity>>();
        protected TDal Dal { get; } = Core.Injector.New<TDal>();

        public virtual void Clear()
        {
            Cache.Clear();
            All = null;
        }
        public abstract Task LoadAsync();
    }

    public abstract class CachedDal<TKey, TKey2, TEntity, TDal> : ICachedDal
    {
        protected IEnumerable<TEntity> All { get; set; }
        protected NonBlocking.ConcurrentDictionary<TKey, IEnumerable<TEntity>> Cache { get; } = new NonBlocking.ConcurrentDictionary<TKey, IEnumerable<TEntity>>();
        protected NonBlocking.ConcurrentDictionary<TKey2, IEnumerable<TEntity>> Cache2 { get; } = new NonBlocking.ConcurrentDictionary<TKey2, IEnumerable<TEntity>>();
        protected TDal Dal { get; } = Core.Injector.New<TDal>();

        public virtual void Clear()
        {
            Cache.Clear();
            Cache2.Clear();
            All = null;
        }
        public abstract void Load();
    }

    public abstract class CachedDalAsync<TKey, TKey2, TEntity, TDal> : ICachedDalAsync
    {
        protected IEnumerable<TEntity> All { get; set; }
        protected NonBlocking.ConcurrentDictionary<TKey, IEnumerable<TEntity>> Cache { get; } = new NonBlocking.ConcurrentDictionary<TKey, IEnumerable<TEntity>>();
        protected NonBlocking.ConcurrentDictionary<TKey2, IEnumerable<TEntity>> Cache2 { get; } = new NonBlocking.ConcurrentDictionary<TKey2, IEnumerable<TEntity>>();
        protected TDal Dal { get; } = Core.Injector.New<TDal>();

        public virtual void Clear()
        {
            Cache.Clear();
            Cache2.Clear();
            All = null;
        }
        public abstract Task LoadAsync();
    }
}