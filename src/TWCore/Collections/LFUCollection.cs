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

namespace TWCore.Collections
{
    /// <summary>
    /// Least frequently used cache algorithm collection
    /// </summary>
    /// <typeparam name="TKey">Key Type</typeparam>
    /// <typeparam name="TValue">Value Type</typeparam>
    public class LFUCollection<TKey, TValue> : CacheCollectionBase<TKey, TValue, LFUCollection<TKey, TValue>.ValueNode>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        static CoreSettings _defaultSettings = Core.GetSettings<CoreSettings>();

        #region Nested Type
        /// <summary>
        /// LFU Collection Value Node
        /// </summary>
        public sealed class ValueNode : CacheCollectionValueNode<TValue>
        {
            public int Slot;
            public LinkedListNode<CountNode> CountListNode;
            public LinkedListNode<KeyNode> KeyListNode;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueNode(TValue value) : base(value) { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueNode(TValue value, int slot) : base(value) => Slot = slot;

            public sealed class CountNode
            {
                public int Count;
                public LinkedList<KeyNode> List = new LinkedList<KeyNode>();
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public CountNode(int count) => Count = count;
            }
            public sealed class KeyNode
            {
                public TKey Key;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public KeyNode(TKey key) => Key = key;
            }
        }
        #endregion

        #region Fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int _age;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int _agePolicy;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly LinkedList<ValueNode.CountNode> _list;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly Dictionary<int, TKey> _slots;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly Queue<int> _availableSlots;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int _currentSlot;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int _insertionCount;
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
        /// <summary>
        /// Least frequently used cache algorithm collection with a capacity of ushort.MaxValue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LFUCollection() : this(_defaultSettings.LFUCollectionDefaultCapacity) { }
        /// <summary>
        /// Least frequently used cache algorithm collection
        /// </summary>
        /// <param name="capacity">Total items count allowed in the collection</param>
        /// <param name="agePolicy">Aging policy for the algorithm</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LFUCollection(int capacity, int agePolicy = -1) : base(capacity)
        {
            _agePolicy = (agePolicy < 0) ? _defaultSettings.LFUCollectionDefaultAgePolicy : agePolicy;
            _list = new LinkedList<ValueNode.CountNode>();
            _slots = new Dictionary<int, TKey>();
            _availableSlots = new Queue<int>();
            _currentSlot = 0;
            _insertionCount = 1;
            _insertionPoint = _list.AddLast(new ValueNode.CountNode(_insertionCount));
        }
        #endregion

        #region Overrides
        LinkedListNode<ValueNode.CountNode> _insertionPoint;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void UpdateList(TKey key, ValueNode node)
        {
            var value = node.Value;
            var countNode = node.CountListNode;
            var countValue = countNode?.Value;
            var keyNode = node.KeyListNode;
            var keyValue = keyNode?.Value;
            int nCount = _insertionCount;
            if (keyNode != null)
            {
                if (++_age > _agePolicy)
                {
                    _age = 0;
                    var mid = _list.First;
                    var insPointValue = _insertionCount;
                    bool insPointSetted = false;
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
                if (_valueStorage.Count > _capacity)
                {
                    var lastCountNode = _list.Last;
                    while (true)
                    {
                        var lstCount = lastCountNode.Value.List.Count;
                        if (lstCount > 0)
                        {
                            var lastKeyNode = lastCountNode.Value.List.Last;
                            if (_valueStorage.TryGetValue(lastKeyNode.Value.Key, out var oldNode))
                            {
                                lastCountNode.Value.List.RemoveLast();
                                _valueStorage.Remove(lastKeyNode.Value.Key);
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
