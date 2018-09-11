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
// ReSharper disable UnusedMember.Global

namespace TWCore.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// Least frequently used cache algorithm collection
    /// </summary>
    /// <typeparam name="TKey">Key Type</typeparam>
    /// <typeparam name="TValue">Value Type</typeparam>
    public class LFUCollection<TKey, TValue> : CacheCollectionBase<TKey, TValue, LFUCollection<TKey, TValue>.ValueNode>
    {
        #region Nested Type
        /// <inheritdoc />
        /// <summary>
        /// LFU Collection Value Node
        /// </summary>
        public sealed class ValueNode : CacheCollectionValueNode<TValue>
        {
            /// <summary>
            /// Slot number
            /// </summary>
            public readonly int Slot;
            /// <summary>
            /// CountList Node
            /// </summary>
            public LinkedListNode<CountNode> CountListNode;
            /// <summary>
            /// KeyList Node
            /// </summary>
            public LinkedListNode<KeyNode> KeyListNode;

            #region .ctor
            /// <summary>
            /// LFU Collection Value Node
            /// </summary>
            /// <param name="value">Value instance</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueNode(TValue value) : base(value) { }
            /// <summary>
            /// LFU Collection Value Node
            /// </summary>
            /// <param name="value">Value instance</param>
            /// <param name="slot">Slot number</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueNode(TValue value, int slot) : base(value) => Slot = slot;
            #endregion

            /// <summary>
            /// Count Node
            /// </summary>
            public sealed class CountNode
            {
                /// <summary>
                /// Count
                /// </summary>
                public int Count;
                /// <summary>
                /// KeyNode List
                /// </summary>
                public LinkedList<KeyNode> List = new LinkedList<KeyNode>();
                /// <summary>
                /// Count Node
                /// </summary>
                /// <param name="count">Count value</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public CountNode(int count) => Count = count;
            }
            /// <summary>
            /// Key Node
            /// </summary>
            public sealed class KeyNode
            {
                /// <summary>
                /// Key value
                /// </summary>
                public readonly TKey Key;
                /// <summary>
                /// Key Node
                /// </summary>
                /// <param name="key">Key value</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public KeyNode(TKey key) => Key = key;
            }
        }
        #endregion

        #region Fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly int _agePolicy;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly LinkedList<ValueNode.CountNode> _list;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Dictionary<int, TKey> _slots;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Queue<int> _availableSlots;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly int _insertionCount;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _age;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _currentSlot;
        #endregion

        #region Properties
        /// <summary>
        /// Current Age
        /// </summary>
        public int Age => _age;
        /// <summary>
        /// Aging policy in use
        /// </summary>
        public int AgePolicy => _agePolicy;
        #endregion

        #region .ctors
        /// <inheritdoc />
        /// <summary>
        /// Least frequently used cache algorithm collection with a capacity of ushort.MaxValue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LFUCollection() : this(CoreSettings.Instance.LFUCollectionDefaultCapacity) { }
        /// <inheritdoc />
        /// <summary>
        /// Least frequently used cache algorithm collection
        /// </summary>
        /// <param name="capacity">Total items count allowed in the collection</param>
        /// <param name="agePolicy">Aging policy for the algorithm</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LFUCollection(int capacity, int agePolicy = -1) : base(capacity)
        {
            _agePolicy = (agePolicy < 0) ? CoreSettings.Instance.LFUCollectionDefaultAgePolicy : agePolicy;
            _list = new LinkedList<ValueNode.CountNode>();
            _slots = new Dictionary<int, TKey>();
            _availableSlots = new Queue<int>();
            _currentSlot = 0;
            _insertionCount = 1;
            _insertionPoint = _list.AddLast(new ValueNode.CountNode(_insertionCount));
        }
        #endregion

        #region Overrides
        private LinkedListNode<ValueNode.CountNode> _insertionPoint;

        /// <summary>
        /// Update the internal list
        /// </summary>
        /// <param name="key">Key value</param>
        /// <param name="node">Value node value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void UpdateList(TKey key, ValueNode node)
        {
            var value = node.Value;
            var countNode = node.CountListNode;
            var countValue = countNode?.Value;
            var keyNode = node.KeyListNode;
            var keyValue = keyNode?.Value;
            var nCount = _insertionCount;
            if (keyNode != null)
            {
                if (++_age > _agePolicy)
                {
                    _age = 0;
                    var mid = _list.First;
                    var insPointValue = _insertionCount;
                    var insPointSetted = false;
                    do
                    {
                        mid.Value.Count--;
                        if (!insPointSetted)
                        {
                            var midCount = mid.Value.Count;
                            if (midCount == insPointValue)
                            {
                                _insertionPoint = mid;
                                insPointSetted = true;
                            }
                            else if (midCount < insPointValue)
                            {
                                _insertionPoint = _list.AddBefore(mid, new ValueNode.CountNode(1));
                                insPointSetted = true;
                            }
                        }
                        mid = mid.Next;
                    } while (mid != null);
                }
                nCount = countValue?.Count + 1 ?? 1;
                countValue?.List.Remove(keyNode);
                ReportHit(key, value);
            }
            else
            {
                if (ValueStorage.Count > Capacity)
                {
                    var lastCountNode = _list.Last;
                    while (true)
                    {
                        var lstCount = lastCountNode.Value.List.Count;
                        if (lstCount > 0)
                        {
                            var lastKeyNode = lastCountNode.Value.List.Last;
                            if (ValueStorage.TryGetValue(lastKeyNode.Value.Key, out var oldNode))
                            {
                                lastCountNode.Value.List.RemoveLast();
                                ValueStorage.Remove(lastKeyNode.Value.Key);
                                _slots.Remove(oldNode.Slot);
                                _availableSlots.Enqueue(oldNode.Slot);
                                ReportDelete(lastKeyNode.Value.Key, oldNode.Value);
                                break;
                            }
                        }
                        if (lstCount == 0 && lastCountNode != _insertionPoint)
                        {
                            lastCountNode.Value.List = null;
                            _list.Remove(lastCountNode);
                            lastCountNode = _list.Last;
                        }
                        else
                            lastCountNode = lastCountNode.Previous;
                        if (lastCountNode == null)
                            break;
                    }
                }
                keyValue = new ValueNode.KeyNode(key);
                ReportInsert(key, value);
            }

            LinkedListNode<ValueNode.CountNode> countInsertion = null;
            var countNodePrevious = countNode?.Previous;

            if (nCount == _insertionCount)
            {
                countInsertion = _insertionPoint;
            }
            else if (countNodePrevious != null)
            {
                countInsertion = countNodePrevious;
            }
            else if (_list.Count > 0)
            {
                var mid = _list.First;
                do
                {
                    if (nCount >= mid.Value.Count)
                    {
                        countInsertion = mid;
                        break;
                    }
                    mid = mid.Next;
                } while (mid != null);
            }

            if (countInsertion != null)
            {
                var vCount = countInsertion.Value.Count;

                if (vCount == nCount)
                    node.CountListNode = countInsertion;
                else if (vCount > nCount)
                {
                    node.CountListNode = _list.AddAfter(countInsertion, new ValueNode.CountNode(nCount));
                }
                else
                {
                    node.CountListNode = _list.AddBefore(countInsertion, new ValueNode.CountNode(nCount));
                    /*
					if (nCount == 1)
						Core.Log.Warning("Adding Count 1, the countInsertion is: {0}, and insertionpoint {1}", countInsertion.Value.Count, _insertionPoint.Value.Count);

					var lst = _list.Select(i => i.Count.ToString()).ToArray();
					Core.Log.InfoBasic("Adding Count {0} before {1}: {2}", nCount, countInsertion.Value.Count, string.Join(",", lst));
					if (lst.Distinct().Count() != lst.Length)
						throw new InvalidProgramException();*/
                }
            }
            else
            {
                node.CountListNode = _list.AddLast(new ValueNode.CountNode(nCount));
            }
            node.KeyListNode = node.CountListNode.Value.List.AddFirst(keyValue);
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
            _list.Clear();
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
            if (node.KeyListNode != null)
            {
                node.CountListNode.Value.List.Remove(node.KeyListNode);
                if (node.CountListNode.Value.List.Count == 0)
                {
                    node.CountListNode.Value.List = null;
                    _list.Remove(node.CountListNode);
                }
            }
            _slots.Remove(node.Slot);
            _availableSlots.Enqueue(node.Slot);
            node.CountListNode = null;
            node.KeyListNode = null;
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
