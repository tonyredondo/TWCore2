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
using System.Diagnostics;
using System.Runtime.CompilerServices;
// ReSharper disable InconsistentNaming

namespace TWCore.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// Collection with a fixed capacity and LRU 2Q replacement logic
    /// </summary>
    /// <typeparam name="TKey">Collection Key</typeparam>
    /// <typeparam name="TValue">Collection Value</typeparam>
    public class LRU2QCollection<TKey, TValue> : CacheCollectionBase<TKey, TValue, LRU2QCollection<TKey, TValue>.ValueNode>
    {
        #region Nested Type
        /// <inheritdoc />
        /// <summary>
        /// LRU Collection Value Node
        /// </summary>
        public sealed class ValueNode : CacheCollectionValueNode<TValue>
        {
            /// <summary>
            /// Slot number
            /// </summary>
            public readonly int Slot;
            /// <summary>
            /// Am List node
            /// </summary>
            public LinkedListNode<TKey> ListNodeAm;
            /// <summary>
            /// A1In List node
            /// </summary>
            public LinkedListNode<TKey> ListNodeA1In;
            /// <summary>
            /// A1Out List node
            /// </summary>
            public LinkedListNode<TKey> ListNodeA1Out;

            #region .ctor
            /// <summary>
            /// LRU Collection Value Node
            /// </summary>
            /// <param name="value">Value node value</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueNode(TValue value) : base(value) { }
            /// <summary>
            /// LRU Collection Value Node
            /// </summary>
            /// <param name="value">Value node value</param>
            /// <param name="slot">Slot number</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueNode(TValue value, int slot) : base(value)
            {
                Slot = slot;
            }
            #endregion
        }
        #endregion

        #region Fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private const int K_IN_PART = 4; //recommended parameters
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private const int K_OUT_PART = 2;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Dictionary<int, TKey> _slots;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Queue<int> _availableSlots;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly LinkedList<TKey> _amList;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly LinkedList<TKey> _a1InList;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly LinkedList<TKey> _a1OutList;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly int _kin;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly int _kout;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _currentSlot;
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
        /// <inheritdoc />
        /// <summary>
        /// Collection with a fixed capacity and LRU replacement logic with a capacity of ushort.MaxValue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LRU2QCollection() : this(CoreSettings.Instance.LRU2QCollectionDefaultCapacity) { }
        /// <inheritdoc />
        /// <summary>
        /// Collection with a fixed capacity and LRU replacement logic
        /// </summary>
        /// <param name="capacity">Total items count allowed in the collection</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LRU2QCollection(int capacity) : this(capacity, capacity / K_IN_PART, capacity / K_OUT_PART) { }
        /// <inheritdoc />
        /// <summary>
        /// Collection with a fixed capacity and LRU replacement logic
        /// </summary>
        /// <param name="capacity">Total items count allowed in the collection</param>
        /// <param name="kInPart">Kin collection part</param>
        /// <param name="kOutPart">Kout collection part</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LRU2QCollection(int capacity, int kInPart, int kOutPart) : base(capacity)
        {
            Ensure.GreaterThan(kInPart, 0, "KInPart should be greater than zero");
            Ensure.GreaterThan(kOutPart, 0, "KOutPart should be greater than zero");
            _kin = kInPart;
            _kout = kOutPart;
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
        private void ReclaimSpace()
        {
            if (ValueStorage.Count <= Capacity) return;

            if (_a1InList.Count > Kin)
            {
                var y = _a1InList.Last;
                _a1InList.RemoveLast();
                if (ValueStorage.TryGetValue(y.Value, out var oValue))
                {
                    ValueStorage.Remove(y.Value);
                    _slots.Remove(oValue.Slot);
                    _availableSlots.Enqueue(oValue.Slot);
                    CleanLists(oValue);
                    ReportDelete(y.Value, oValue.Value);
                }
                _a1OutList.AddFirst(y.Value);
                if (_a1OutList.Count >= Kout)
                    _a1OutList.RemoveLast();
            }
            else if (_amList.Last != null)
            {
                var y = _amList.Last;
                _amList.RemoveLast();
                if (ValueStorage.TryGetValue(y.Value, out var oValue))
                {
                    ValueStorage.Remove(y.Value);
                    _slots.Remove(oValue.Slot);
                    _availableSlots.Enqueue(oValue.Slot);
                    CleanLists(oValue);
                    ReportDelete(y.Value, oValue.Value);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CleanLists(ValueNode value)
        {
            value.ListNodeA1Out = null;
            value.ListNodeA1In = null;
            value.ListNodeAm = null;
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Update the internal list
        /// </summary>
        /// <param name="key">Key value</param>
        /// <param name="node">Value node value</param>
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
        /// <summary>
        /// Create a value node
        /// </summary>
        /// <param name="key">Key value</param>
        /// <param name="value">Value</param>
        /// <returns>ValueNode instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ValueNode CreateNode(TKey key, TValue value)
        {
            var nValue = new ValueNode(value, _availableSlots.Count > 0 ? _availableSlots.Dequeue() : _currentSlot++);
            _slots[nValue.Slot] = key;
            return nValue;
        }
        /// <summary>
        /// Handles when Clean is called
        /// </summary>
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
        /// <summary>
        /// Handles when a node is removed
        /// </summary>
        /// <param name="node">ValueNode instance to remove</param>
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
        /// <summary>
        /// On get the index of a key
        /// </summary>
        /// <param name="key">Key value</param>
        /// <param name="index">Index of that key in the collection</param>
        /// <returns>True if the key was found in the collection; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnGetIndex(TKey key, out int index)
        {
            if (ValueStorage.TryGetValue(key, out var node))
            {
                index = node.Slot;
                return true;
            }
            index = -1;
            return false;
        }
        /// <summary>
        /// On get the key of an index
        /// </summary>
        /// <param name="index">Index value</param>
        /// <param name="key">Key of the index</param>
        /// <returns>True if the key was found in the collection; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnGetKey(int index, out TKey key)
        {
            return _slots.TryGetValue(index, out key);
        }
        #endregion
    }
}
