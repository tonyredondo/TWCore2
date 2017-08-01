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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace TWCore
{
	/// <summary>
	/// Sub array for each delegate
	/// </summary>
	public delegate void SubArrayForEach<T>(ref T value);
	/// <summary>
	/// Sub array for each delegate
	/// </summary>
	public delegate void SubArrayForEach<T, A1>(ref T value, ref A1 arg1);
    /// <summary>
	/// Sub array for each delegate
	/// </summary>
	public delegate void SubArrayForEach<T, A1, A2>(ref T value, ref A1 arg1, ref A2 arg2);

    /// <summary>
    /// Provides an SubArray from an array without copying buffer.
    /// </summary>
    /// <typeparam name="T">Type of element</typeparam>
    [DataContract]
    [Serializable]
    public struct SubArray<T> : IEquatable<SubArray<T>>
    {
        /// <summary>
        /// Empty SubArray instance
        /// </summary>
        public static SubArray<T> Empty = new SubArray<T>();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        T[] _array;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int _offset;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int _count;

        #region Properties
        /// <summary>
        /// Get the current array
        /// </summary>
        [DataMember]
        public T[] Array => _array;
        /// <summary>
        /// Offset
        /// </summary>
        [DataMember, XmlAttribute]
        public int Offset => _offset;
        /// <summary>
        /// Count
        /// </summary>
        [DataMember, XmlAttribute]
        public int Count => _count;
        #endregion

        #region .ctor
        /// <summary>
        /// Provides an SubArray from an array without copying buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SubArray(T[] array)
        {
            _array = array;
            _offset = 0;
            _count = array?.Length ?? 0;
        }
        /// <summary>
        /// Provides an SubArray from an array without copying buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SubArray(T[] array, int offset, int count)
        {
            _array = array ?? throw new ArgumentNullException("array");
            Ensure.GreaterEqualThan(offset, 0, "The offset should be a positive number.");
            Ensure.GreaterEqualThan(count, 0, "The count should be a positive number.");
            if (array.Length - offset < count)
                throw new ArgumentOutOfRangeException("The count is invalid.");
            _offset = offset;
            _count = count;
        }
        /// <summary>
        /// Provides an SubArray from an array without copying buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SubArray(ArraySegment<T> array)
        {
            _array = array.Array;
            _offset = array.Offset;
            _count = array.Count;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Reference to an element inside the array
        /// </summary>
        /// <param name="index">Item index</param>
        /// <returns>Reference to the element of the index</returns>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _array[_offset + index];
        }
        /// <summary>
        /// Unsafe pointer to the array offset
        /// </summary>
        public unsafe void* GetPointer() => Marshal.UnsafeAddrOfPinnedArrayElement(_array, _offset).ToPointer();
        /// <summary>
        /// Gets the index of an item
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns>Index of the item inside the SubArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item)
        {
            int index = System.Array.IndexOf(_array, item, _offset, _count);
            return index >= 0 ? index - _offset : -1;
        }
        /// <summary>
        /// Slice the SubArray
        /// </summary>
        /// <param name="index">Index from the slice begins</param>
        /// <returns>New SubArray instance</returns>
        public SubArray<T> Slice(int index)
            => Slice(index, _count - index);
        /// <summary>
        /// Slice the SubArray
        /// </summary>
        /// <param name="index">Index from the slice begins</param>
        /// <param name="count">Number of element of the slice</param>
        /// <returns>New SubArray instance</returns>
        public SubArray<T> Slice(int index, int count)
        {
            Ensure.GreaterEqualThan(index, 0, "Index should be a possitive number.");
            Ensure.GreaterEqualThan(count, 0, "Count should be a possitive number.");
            if (index > _count)
                throw new ArgumentOutOfRangeException("The index should be lower than the total Array Count");
            if (_count - index < count)
                throw new ArgumentOutOfRangeException("The count is invalid");
            return new SubArray<T>(_array, _offset + index, count);
        }
        /// <summary>
        /// Gets if the SubArray contains the item
        /// </summary>
        /// <param name="item">Item to look in the SubArray instance</param>
        /// <returns>true if the SubArray contains the element; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item) => System.Array.IndexOf(_array, item, _offset, _count) >= 0;
        /// <summary>
        /// Gets the SubArray HashCode
        /// </summary>
        /// <returns>HashCode</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => _array == null ? 0 : _array.GetHashCode() ^ _offset ^ _count;
        /// <summary>
        /// Gets if the SubArray is equal to another object
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>true if the object is equal; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(Object obj)
        {
            if (obj is SubArray<T> sobj)
                return Equals(sobj);
            else
                return false;
        }
        /// <summary>
        /// Gets if the SubArray is equal to another SubArray
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>true if the object is equal; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(SubArray<T> obj) => obj._array == _array && obj._offset == _offset && obj._count == _count;
        /// <summary>
        /// Gets if the SubArray is equal to another SubArray
        /// </summary>
        /// <param name="a">First SubArray instance</param>
        /// <param name="b">Second SubArray instance</param>
        /// <returns>true if the object is equal; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SubArray<T> a, SubArray<T> b) => a.Equals(b);
        /// <summary>
        /// Gets if the SubArray is different to another SubArray
        /// </summary>
        /// <param name="a">First SubArray instance</param>
        /// <param name="b">Second SubArray instance</param>
        /// <returns>true if the object is different; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SubArray<T> a, SubArray<T> b) => !(a == b);
        /// <summary>
        /// Copy the SubArray content to an Array 
        /// </summary>
        /// <param name="array">Array object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array) => System.Array.Copy(_array, _offset, array, 0, _count);
        /// <summary>
        /// Copy the SubArray content to an Array 
        /// </summary>
        /// <param name="array">Array object</param>
        /// <param name="arrayIndex">Starting offset in the destination array</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex) => System.Array.Copy(_array, _offset, array, arrayIndex, _count);
        /// <summary>
        /// Gets an Array from the SubArray
        /// </summary>
        /// <returns>Array instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            var temp = new T[_count];
            System.Array.Copy(_array, _offset, temp, 0, _count);
            return temp;
        }
        /// <summary>
        /// Get the String representation of the instance.
        /// </summary>
        /// <returns>String value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe string ToString()
        {
            if (typeof(T) == typeof(char))
                return new string((char*)Marshal.UnsafeAddrOfPinnedArrayElement(_array, _offset).ToPointer(), 0, _count);
            return "[SubArray: " + typeof(T).Name + " - Offset: " + _offset + " - Count: " + _count + "]";
        }

		/// <summary>
		/// For each method in the inner array by reference
		/// </summary>
		/// <param name="delegate">ForEach delegate</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ForEach(SubArrayForEach<T> @delegate)
		{
			for(var i = _offset; i < (_offset + _count); i++)
				@delegate(ref _array[i]);
		}
		/// <summary>
		/// For each method in the inner array by reference
		/// </summary>
		/// <param name="delegate">ForEach delegate</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ForEach<A1>(SubArrayForEach<T, A1> @delegate, ref A1 arg1)
		{
			for(var i = _offset; i < (_offset + _count); i++)
				@delegate(ref _array[i], ref arg1);
		}
        /// <summary>
		/// For each method in the inner array by reference
		/// </summary>
		/// <param name="delegate">ForEach delegate</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<A1, A2>(SubArrayForEach<T, A1, A2> @delegate, ref A1 arg1, ref A2 arg2)
        {
            for (var i = _offset; i < (_offset + _count); i++)
                @delegate(ref _array[i], ref arg1, ref arg2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator T[] (SubArray<T> segment)
        {
            if (segment._offset == 0 && segment._count == segment._array.Length)
                return segment._array;
            else
                return segment.ToArray();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SubArray<T>(T[] array) => new SubArray<T>(array);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SubArray<T>(ArraySegment<T> arraySegment) => new SubArray<T>(arraySegment);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ArraySegment<T>(SubArray<T> subArray) => new ArraySegment<T>(subArray.Array, subArray.Offset, subArray.Count);
		#endregion
	}
}
