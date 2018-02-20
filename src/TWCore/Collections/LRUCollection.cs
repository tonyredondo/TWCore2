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
    /// Collection with a fixed capacity and LRU replacement logic
    /// </summary>
    /// <typeparam name="TKey">Collection Key</typeparam>
    /// <typeparam name="TValue">Collection Value</typeparam>
    public class LRUCollection<TKey, TValue> : CacheCollectionBase<TKey, TValue, LRUCollection<TKey, TValue>.ValueNode>
    {
        #region Nested Type
        /// <inheritdoc />
        /// <summary>
        /// LRU Collection Value Node
        /// </summary>
        public sealed class ValueNode : CacheCollectionValueNode<TValue>
        {
            public readonly int Slot;
            public LinkedListNode<TKey> ListNode;
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
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly LinkedList<TKey> _list;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Dictionary<int, TKey> _slots;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Queue<int> _availableSlots;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _currentSlot;
        #endregion

        #region .ctors
        /// <inheritdoc />
        /// <summary>
        /// Collection with a fixed capacity and LRU replacement logic with a capacity of ushort.MaxValue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LRUCollection() : this(CoreSettings.Instance.LRUCollectionDefaultCapacity) { }
        /// <inheritdoc />
        /// <summary>
        /// Collection with a fixed capacity and LRU replacement logic
        /// </summary>
        /// <param name="capacity">Total items count allowed in the collection</param>
        /// <param name="comparer">Equality comparer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LRUCollection(int capacity, IEqualityComparer<TKey> comparer = null) : base(capacity, comparer ?? EqualityComparer<TKey>.Default)
        {
            _list = new LinkedList<TKey>();
            _slots = new Dictionary<int, TKey>();
            _availableSlots = new Queue<int>();
            _currentSlot = 0;
        }
        #endregion
        
        #region Overrides
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void UpdateList(TKey key, ValueNode node)
        {
            var value = node.Value;
            var lNode = node.ListNode;
			if (lNode != null)
			{
				_list.Remove(lNode);
				ReportHit(key, value);
			}
			else
			{
				if (ValueStorage.Count > Capacity)
				{
					var lastNode = _list.Last;
					_list.RemoveLast();
					if (ValueStorage.TryGetValue(lastNode.Value, out var oldNode))
					{
						ValueStorage.Remove(lastNode.Value);
						_slots.Remove(oldNode.Slot);
						_availableSlots.Enqueue(oldNode.Slot);
						ReportDelete(lastNode.Value, oldNode.Value);
					}
				}
				ReportInsert(key, value);
			}
            node.ListNode = _list.AddFirst(key);
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
            _list.Clear();
            _slots.Clear();
            _availableSlots.Clear();
            _currentSlot = 0;
        }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnNodeRemove(ValueNode node)
        {
            _list.Remove(node.ListNode);
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