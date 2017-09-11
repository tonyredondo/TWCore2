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

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TWCore.Data.Cache
{
    public abstract class CachedDal<TEntity, TDal> : ICachedDal
    {
        protected ConcurrentDictionary<string, IEnumerable<TEntity>> Cache { get; } = new ConcurrentDictionary<string, IEnumerable<TEntity>>();
        protected TDal Dal { get; } = Core.Injector.New<TDal>();

        public virtual void Clear() => Cache.Clear();
        public abstract void Load();
    }
}