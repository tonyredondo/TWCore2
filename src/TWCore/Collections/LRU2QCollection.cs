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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TWCore.Collections
{
    /// <summary>
    /// Collection with a fixed capacity and LRU 2Q replacement logic
    /// </summary>
    /// <typeparam name="TKey">Collection Key</typeparam>
    /// <typeparam name="TValue">Collection Value</typeparam>
    public class LRU2QCollection<TKey, TValue> : CacheCollectionBase<TKey, TValue, LRU2QCollection<TKey, TValue>.ValueNode>
    {
        static CoreSettings _defaultSettings = Core.GetSettings<CoreSettings>();

        #region Nested Type
        /// <summary>
        /// LRU Collection Value Node
        /// </summary>
        /// <typeparam name="TKey">Collection Key</typeparam>
        /// <typeparam name="TValue">Collection Value</typeparam>
        public sealed class ValueNode : CacheCollectionValueNode<TValue>
        {
            public int Slot;
            public LinkedListNode<TKey> ListNodeAm;
            public LinkedListNode<TKey> ListNodeA1In;
            public LinkedListNode<TKey> ListNodeA1Out;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueNode(TValue value) : base(value) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueNode(TValue value, int slot) : base(value)
            {
                Slot = slot;
            }
        }
        #endregion

        #region Fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly Dictionary<int, TKey> _slots;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly Queue<int> _availableSlots;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int _currentSlot;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly LinkedList<TKey> _amList;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly LinkedList<TKey> _a1InList;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly LinkedList<TKey> _a1OutList;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly int _kin;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly int _kout;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const int K_IN_PART = 4; //recommended parameters
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const int K_OUT_PART = 2;
        #endregion

        #region Properties
        /// <summary>
        /// Maximum capacity of the A1In list depending of parameters
        /// </summary>
        public int Kin => _kin;
        /// <summary>
        /// Maximum capacity of the A1Out list depending of parameters
        /// </summary>
        public int Kout => _kout;
        #endregion

        #region .ctors
        /// <summary>
        /// Collection with a fixed capacity and LRU replacement logic with a capacity of ushort.MaxValue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LRU2QCollection() : this(_defaultSettings.LRU2QCollectionDefaultCapacity) { }
        /// <summary>
        /// Collection with a fixed capacity and LRU replacement logic
        /// </summary>
        /// <param name="capacity">Total items count allowed in the collection</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LRU2QCollection(int capacity) : this(capacity, capacity / K_IN_PART, capacity / K_OUT_PART) { }
        /// <summary>
        /// Collection with a fixed capacity and LRU replacement logic
        /// </summary>
        /// <param name="capacity">Total items count allowed in the collection</param>
        /// <param name="k_in_part">Kin collection part</param>
        /// <param name="k_out_part">Kout collection part</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LRU2QCollection(int capacity, int k_in_part, int k_out_part) : base(capacity)
        {
            Ensure.GreaterThan(k_in_part, 0, "KInPart should be greater than zero");
            Ensure.GreaterThan(k_out_part, 0, "KOutPart should be greater than zero");
            _kin = k_in_part;
            _kout = k_out_part;
            _amList = new LinkedList<TKey>();
            _a1InList = new LinkedList<TKey>();
            _a1OutList = new LinkedList<TKey>();
            _slots = new Dictionary<int, TKey>();
            _availableSlots = new Queue<int>();
            _currentSlot = 0;
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ReclaimSpace()
        {
            if (_valueStorage.Count > _capacity)
            {
                if (_a1InList.Count > Kin)
                {
                    var Y = _a1InList.Last;
                    _a1InList.RemoveLast();
                    if (_valueStorage.TryGetValue(Y.Value, out var oValue))
                    {
                        _valueStorage.Remove(Y.Value);
                        _slots.Remove(oValue.Slot);
                        _availableSlots.Enqueue(oValue.Slot);
                        CleanLists(oValue);
                        ReportDelete(Y.Value, oValue.Value);
                    }
                    _a1OutList.AddFirst(Y.Value);
                    if (_a1OutList.Count >= Kout)
                        _a1OutList.RemoveLast();
                }
                else if (_amList.Last != null)
                {
                    var Y = _amList.Last;
                    _amList.RemoveLast();
                    if (_valueStorage.TryGetValue(Y.Value, out var oValue))
                    {
                        _valueStorage.Remove(Y.Value);
                        _slots.Remove(oValue.Slot);
                        _availableSlots.Enqueue(oValue.Slot);
                        CleanLists(oValue);
                        ReportDelete(Y.Value, oValue.Value);
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CleanLists(ValueNode value)
        {
            value.ListNodeA1Out = null;
            value.ListNodeA1In = null;
            value.ListNodeAm = null;
        }
        #endregion
        
        #region Overrides
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void UpdateList(TKey key, ValueNode node)
        {
            if (node.ListNodeAm != null)
            {
                _amList.Remove(node.ListNodeAm);
                CleanLists(node);
                node.ListNodeAm = _amList.AddFirst(key);
                ReportHit(key, node.Value);
            }
            else if (node.ListNodeA1Out != null)
            {
                _a1OutList.Remove(node.ListNodeA1Out);
                ReclaimSpace();
                node.ListNodeAm = _amList.AddFirst(key);
                ReportHit(key, node.Value);
            }
            else if (node.ListNodeA1In != null)
            {
                ReportHit(key, node.Value);
            }
            else
            {
                CleanLists(node);
                ReclaimSpace();
                node.ListNodeA1In = _a1InList.AddFirst(key);
                ReportInsert(key, node.Value);
            }
        }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ValueNode CreateNode(TKey key, TValue value)
        {
            var nValue = new ValueNode(value, _availableSlots.Count > 0 ? _availableSlots.Dequeue() : _currentSlot++);
            _slots[nValue.Slot] = key;
            return nValue;
        }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnClean()
        {
            _a1InList.Clear();
            _a1OutList.Clear();
            _amList.Clear();
            _slots.Clear();
            _availableSlots.Clear();
            _currentSlot = 0;
        }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnNodeRemove(ValueNode node)
        {
            _slots.Remove(node.Slot);
            _availableSlots.Enqueue(node.Slot);
            if (node.ListNodeAm != null)
            {
                _amList.Remove(node.ListNodeAm);
                node.ListNodeAm = null;
            }
            if (node.ListNodeA1In != null)
            {
                _a1InList.Remove(node.ListNodeA1In);
                node.ListNodeA1In = null;
            }
            if (node.ListNodeA1Out != null)
            {
                _a1OutList.Remove(node.ListNodeA1Out);
                node.ListNodeA1Out = null;
            }
        }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnGetIndex(TKey key, out int index)
        {
            if (_valueStorage.TryGetValue(key, out var node))
            {
                index = node.Slot;
                return true;
            }
            index = -1;
            return false;
        }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnGetKey(int index, out TKey key)
        {
            return _slots.TryGetValue(index, out key);
        }
        #endregion
    }
}
