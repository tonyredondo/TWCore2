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
    public class LRU2QSimpleCollection<TKey, TValue> : CacheCollectionBase<TKey, TValue, LRU2QSimpleCollection<TKey, TValue>.ValueNode>
    {
        #region Nested Type
        /// <inheritdoc />
        /// <summary>
        /// LRU Collection Value Node
        /// </summary>
        public sealed class ValueNode : CacheCollectionValueNode<TValue>
        {
            public readonly int Slot;
            public LinkedListNode<TKey> ListNodeAm;
            public LinkedListNode<TKey> ListNodeA1;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueNode(TValue value) : base(value) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueNode(TValue value, int slot) : base(value) => Slot = slot;
        }
        #endregion

        #region Fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly LinkedList<TKey> _amList;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly LinkedList<TKey> _a1List;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly int _threshold;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Dictionary<int, TKey> _slots;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Queue<int> _availableSlots;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _currentSlot;
        #endregion

        #region Properties
        /// <summary>
        /// Maximum capacity of the A1 Linked List
        /// </summary>
        public int Threshold => _threshold;
        #endregion

        #region .ctors
        /// <inheritdoc />
        /// <summary>
        /// Collection with a fixed capacity and LRU replacement logic with a capacity of ushort.MaxValue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LRU2QSimpleCollection() : this(CoreSettings.Instance.LRU2QSimpleCollectionDefaultCapacity) { }
        /// <inheritdoc />
        /// <summary>
        /// Collection with a fixed capacity and LRU replacement logic
        /// </summary>
        /// <param name="capacity">Total items count allowed in the collection</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LRU2QSimpleCollection(int capacity) : this(capacity, capacity / 4) { }
        /// <inheritdoc />
        /// <summary>
        /// Collection with a fixed capacity and LRU replacement logic
        /// </summary>
        /// <param name="capacity">Total items count allowed in the collection</param>
        /// <param name="threshold">Collection threshold</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LRU2QSimpleCollection(int capacity, int threshold) : base(capacity)
        {
            Ensure.GreaterThan(capacity, 0, "Capacity should be greater than zero");
            Ensure.GreaterThan(threshold, 0, "Threshold should be greater than zero");
            _threshold = threshold;
            _amList = new LinkedList<TKey>();
            _a1List = new LinkedList<TKey>();
            _slots = new Dictionary<int, TKey>();
            _availableSlots = new Queue<int>();
            _currentSlot = 0;
        }
        #endregion

        #region Overrides
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void UpdateList(TKey key, ValueNode node)
        {
            var isInAm = node.ListNodeAm != null;
            var isInA1 = node.ListNodeA1 != null;
            var value = node.Value;
            if (isInAm)
            {
                _amList.Remove(node.ListNodeAm);
                ReportHit(key, value);
                node.ListNodeAm = _amList.AddFirst(key);
            }
            else if (isInA1)
            {
                _a1List.Remove(node.ListNodeA1);
                node.ListNodeA1 = null;
                ReportHit(key, value);
                node.ListNodeAm = _amList.AddFirst(key);
            }
            else
            {
                if (ValueStorage.Count > Capacity)
                {
                    var list = _a1List.Count >= _threshold ? _a1List : _amList;
                    var oNode = list.Last;
                    if (ValueStorage.TryGetValue(oNode.Value, out var oValue))
                    {
                        ValueStorage.Remove(oNode.Value);
                        list.Remove(oNode);
                        _slots.Remove(oValue.Slot);
                        _availableSlots.Enqueue(oValue.Slot);
                        ReportDelete(oNode.Value, oValue.Value);
                    }
                }
                node.ListNodeA1 = _a1List.AddFirst(key);
                ReportInsert(key, value);
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
            _a1List.Clear();
            _amList.Clear();
            _slots.Clear();
            _availableSlots.Clear();
            _currentSlot = 0;
        }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnNodeRemove(ValueNode node)
        {
            if (node.ListNodeAm != null)
            {
                _amList.Remove(node.ListNodeAm);
                node.ListNodeAm = null;
            }
            if (node.ListNodeA1 != null)
            {
                _a1List.Remove(node.ListNodeA1);
                node.ListNodeA1 = null;
            }
            _slots.Remove(node.Slot);
            _availableSlots.Enqueue(node.Slot);
        }
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnGetKey(int index, out TKey key)
        {
            return _slots.TryGetValue(index, out key);
        }
        #endregion
    }
}
