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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
// ReSharper disable UnusedMethodReturnValue.Global

namespace TWCore.Collections
{
    // Adds strong typing to WeakReference.Target using generics. Also,
    // the Create factory method is used in place of a constructor
    // to handle the case where target is null, but we want the 
    // reference to still appear to be alive.
    internal class WeakReference<T> : WeakReference where T : class
    {
        public static WeakReference<T> Create(T target)
        {
            return target == null ? WeakNullReference<T>.Singleton : new WeakReference<T>(target);
        }

        protected WeakReference(T target)
            : base(target, false) { }

        public new T Target => (T)base.Target;
    }
    // Provides a weak reference to a null target object, which, unlike
    // other weak references, is always considered to be alive. This 
    // facilitates handling null dictionary values, which are perfectly
    // legal.
    internal class WeakNullReference<T> : WeakReference<T> where T : class
    {
        public static readonly WeakNullReference<T> Singleton = new WeakNullReference<T>();

        private WeakNullReference() : base(null) { }

        public override bool IsAlive => true;
    }

    // Provides a weak reference to an object of the given type to be used in
    // a WeakDictionary along with the given comparer.
    internal sealed class WeakKeyReference<T> : WeakReference<T> where T : class
    {
        public readonly int HashCode;

        public WeakKeyReference(T key, IEqualityComparer<object> comparer)
            : base(key)
        {
            // retain the object’s hash code immediately so that even
            // if the target is GC’ed we will be able to find and
            // remove the dead weak reference.
            HashCode = comparer.GetHashCode(key);
        }
    }

    // Compares objects of the given type or WeakKeyReferences to them
    // for equality based on the given comparer. Note that we can only
    // implement IEqualityComparer<T> for T = object as there is no 
    // other common base between T and WeakKeyReference<T>. We need a
    // single comparer to handle both types because we don’t want to
    // allocate a new weak reference for every lookup.
    internal sealed class WeakKeyComparer<T> : IEqualityComparer<object>
        where T : class
    {

        private readonly IEqualityComparer<T> _comparer;

        internal WeakKeyComparer(IEqualityComparer<T> comparer)
        {
            if (comparer == null)
                comparer = EqualityComparer<T>.Default;

            _comparer = comparer;
        }

        public int GetHashCode(object obj)
        {
            if (obj is WeakKeyReference<T> weakKey) return weakKey.HashCode;
            return _comparer.GetHashCode((T)obj);
        }

        // Note: There are actually 9 cases to handle here.
        //
        //  Let Wa = Alive Weak Reference
        //  Let Wd = Dead Weak Reference
        //  Let S  = Strong Reference
        //  
        //  x  | y  | Equals(x,y)
        // ————————————————-
        //  Wa | Wa | comparer.Equals(x.Target, y.Target) 
        //  Wa | Wd | false
        //  Wa | S  | comparer.Equals(x.Target, y)
        //  Wd | Wa | false
        //  Wd | Wd | x == y
        //  Wd | S  | false
        //  S  | Wa | comparer.Equals(x, y.Target)
        //  S  | Wd | false
        //  S  | S  | comparer.Equals(x, y)
        // ————————————————-
        public new bool Equals(object x, object y)
        {
            var first = GetTarget(x, out bool xIsDead);
            var second = GetTarget(y, out bool yIsDead);
            if (xIsDead)
                return yIsDead && x == y;
            return !yIsDead && _comparer.Equals(first, second);
        }

        private static T GetTarget(object obj, out bool isDead)
        {
            var wref = obj as WeakKeyReference<T>;
            T target;
            if (wref != null)
            {
                target = wref.Target;
                isDead = !wref.IsAlive;
            }
            else
            {
                target = (T)obj;
                isDead = false;
            }
            return target;
        }
    }


    /// <inheritdoc />
    /// <summary>
    /// A generic dictionary, which allows both its keys and values 
    /// to be garbage collected if there are no other references
    /// to them than from the dictionary itself.
    /// </summary>
    /// <remarks>
    /// If either the key or value of a particular entry in the dictionary
    /// has been collected, then both the key and value become effectively
    /// unreachable. However, left-over WeakReference objects for the key
    /// and value will physically remain in the dictionary until
    /// RemoveCollectedEntries is called. This will lead to a discrepancy
    /// between the Count property and the number of iterations required
    /// to visit all of the elements of the dictionary using its
    /// enumerator or those of the Keys and Values collections. Similarly,
    /// CopyTo will copy fewer than Count elements in this situation.
    /// </remarks>
    public sealed class WeakDictionary<TKey, TValue> : BaseDictionary<TKey, TValue>
        where TKey : class
        where TValue : class
    {
        private readonly ConcurrentDictionary<object, WeakReference<TValue>> _dictionary;
        private readonly WeakKeyComparer<TKey> _comparer;
        private readonly Action _removeWeakReferencesInNull;

        public WeakDictionary()
            : this(null) { }
        
        public WeakDictionary(IEqualityComparer<TKey> comparer)
        {
            _comparer = new WeakKeyComparer<TKey>(comparer);
            _dictionary = new ConcurrentDictionary<object, WeakReference<TValue>>(_comparer);
            _removeWeakReferencesInNull = new Action(RemoveCollectedEntries).CreateThrottledAction(CoreSettings.Instance.WeakDictionaryRemoveReferenceThrottledTimeInMs);
        }

        // WARNING: The count returned here may include entries for which
        // either the key or value objects have already been garbage
        // collected. Call RemoveCollectedEntries to weed out collected
        // entries and update the count accordingly.
        public override int Count => _dictionary.Count;

        public override void Add(TKey key, TValue value)
        {
            TryAdd(key, value);
        }
        public bool TryAdd(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException("key");
            var weakKey = new WeakKeyReference<TKey>(key, _comparer);
            var weakValue = WeakReference<TValue>.Create(value);
            _removeWeakReferencesInNull();
            return _dictionary.TryAdd(weakKey, weakValue);
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException("key");
            WeakReference<TKey> weakKey = new WeakKeyReference<TKey>(key, _comparer);
            _removeWeakReferencesInNull();
            var res = _dictionary.GetOrAdd(weakKey, mKey => WeakReference<TValue>.Create(value));
            return res.Target;
        }
        public TValue GetOrAdd(TKey key, Func<object, TValue> valueFactory)
        {
            if (key == null) throw new ArgumentNullException("key");
            WeakReference<TKey> weakKey = new WeakKeyReference<TKey>(key, _comparer);
            _removeWeakReferencesInNull();
            var res = _dictionary.GetOrAdd(weakKey, mKey => WeakReference<TValue>.Create(valueFactory(mKey)));
            return res.Target;
        }

        public override bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public override bool Remove(TKey key)
        {
            return _dictionary.TryRemove(key, out var _);
        }
        public bool TryRemove(TKey key, out TValue value)
        {
            var res = _dictionary.TryRemove(key, out var weakValue);
            value = weakValue.IsAlive ? weakValue.Target : default(TValue);
            return res;
        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            if (_dictionary.TryGetValue(key, out var weakValue))
            {
                value = weakValue.Target;
                return weakValue.IsAlive;
            }
            value = null;
            return false;
        }

        protected override void SetValue(TKey key, TValue value)
        {
            WeakReference<TKey> weakKey = new WeakKeyReference<TKey>(key, _comparer);
            _dictionary[weakKey] = WeakReference<TValue>.Create(value);
        }

        public override void Clear()
        {
            _dictionary.Clear();
        }

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var kvp in _dictionary)
            {
                var weakKey = (WeakReference<TKey>)(kvp.Key);
                var weakValue = kvp.Value;
                var key = weakKey.Target;
                var value = weakValue.Target;
                if (weakKey.IsAlive && weakValue.IsAlive)
                    yield return new KeyValuePair<TKey, TValue>(key, value);
            }
        }

        // Removes the left-over weak references for entries in the dictionary
        // whose key or value has already been reclaimed by the garbage
        // collector. This will reduce the dictionary’s Count by the number
        // of dead key-value pairs that were eliminated.
        public void RemoveCollectedEntries()
        {
            List<object> toRemove = null;
            var dct = _dictionary.ToArray();
            foreach (var pair in dct)
            {
                var weakKey = (WeakReference<TKey>)(pair.Key);
                var weakValue = pair.Value;
                if (weakKey.IsAlive && weakValue.IsAlive) continue;
                if (toRemove == null)
                    toRemove = new List<object>();
                toRemove.Add(weakKey);
            }

            if (toRemove == null) return;
            foreach (var key in toRemove)
                _dictionary.TryRemove(key, out var _);
        }
    }
}
