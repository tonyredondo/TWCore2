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
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace TWCore
{
    /// <summary>
	/// Generic EventArgs
	/// </summary>
	/// <typeparam name="T1">Type 1 of EventArgs</typeparam>
    [DataContract]
    public class EventArgs<T1> : EventArgs
    {
        /// <summary>
        /// Item1 Object
        /// </summary>
        [XmlElement, DataMember]
        public T1 Item1 { get; set; }

        #region .ctor
        /// <summary>
        /// Generic EventArgs
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventArgs()
        {
        }
        /// <summary>
        /// Generic EventArgs
        /// </summary>
        /// <param name="item1">Item1 object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventArgs(T1 item1)
        {
            Item1 = item1;
        }
        #endregion
    }
    /// <summary>
    /// Generic EventArgs
    /// </summary>
    /// <typeparam name="T1">Type 1 of EventArgs</typeparam>
    /// <typeparam name="T2">Type 2 of EventArgs</typeparam>
    [DataContract]
    public class EventArgs<T1, T2> : EventArgs
    {
        /// <summary>
        /// Item1 Object
        /// </summary>
        [XmlElement, DataMember]
        public T1 Item1 { get; set; }
        /// <summary>
        /// Item2 Object
        /// </summary>
        [XmlElement, DataMember]
        public T2 Item2 { get; set; }

        #region .ctor
        /// <summary>
        /// Generic EventArgs
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventArgs()
        {
        }
        /// <summary>
        /// Generic EventArgs
        /// </summary>
        /// <param name="item1">Item1 object</param>
        /// <param name="item2">Item2 object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventArgs(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
        #endregion
    }
    /// <summary>
    /// Generic EventArgs
    /// </summary>
    /// <typeparam name="T1">Type 1 of EventArgs</typeparam>
    /// <typeparam name="T2">Type 2 of EventArgs</typeparam>
    /// <typeparam name="T3">Type 3 of EventArgs</typeparam>
    [DataContract]
    public class EventArgs<T1, T2, T3> : EventArgs
    {
        /// <summary>
        /// Item1 Object
        /// </summary>
        [XmlElement, DataMember]
        public T1 Item1 { get; set; }
        /// <summary>
        /// Item2 Object
        /// </summary>
        [XmlElement, DataMember]
        public T2 Item2 { get; set; }
        /// <summary>
        /// Item3 Object
        /// </summary>
        [XmlElement, DataMember]
        public T3 Item3 { get; set; }

        #region .ctor
        /// <summary>
        /// Generic EventArgs
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventArgs()
        {
        }
        /// <summary>
        /// Generic EventArgs
        /// </summary>
        /// <param name="item1">Item1 object</param>
        /// <param name="item2">Item2 object</param>
        /// <param name="item3">Item3 object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventArgs(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }
        #endregion
    }
    /// <summary>
    /// Generic EventArgs
    /// </summary>
    /// <typeparam name="T1">Type 1 of EventArgs</typeparam>
    /// <typeparam name="T2">Type 2 of EventArgs</typeparam>
    /// <typeparam name="T3">Type 3 of EventArgs</typeparam>
    /// <typeparam name="T4">Type 4 of EventArgs</typeparam>
    [DataContract]
    public class EventArgs<T1, T2, T3, T4> : EventArgs
    {
        /// <summary>
        /// Item1 Object
        /// </summary>
        [XmlElement, DataMember]
        public T1 Item1 { get; set; }
        /// <summary>
        /// Item2 Object
        /// </summary>
        [XmlElement, DataMember]
        public T2 Item2 { get; set; }
        /// <summary>
        /// Item3 Object
        /// </summary>
        [XmlElement, DataMember]
        public T3 Item3 { get; set; }
        /// <summary>
        /// Item4 Object
        /// </summary>
        [XmlElement, DataMember]
        public T4 Item4 { get; set; }

        #region .ctor
        /// <summary>
        /// Generic EventArgs
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventArgs()
        {
        }
        /// <summary>
        /// Generic EventArgs
        /// </summary>
        /// <param name="item1">Item1 object</param>
        /// <param name="item2">Item2 object</param>
        /// <param name="item3">Item3 object</param>
        /// <param name="item4">Item4 object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventArgs(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }
        #endregion
    }
    /// <summary>
    /// Generic EventArgs
    /// </summary>
    /// <typeparam name="T1">Type 1 of EventArgs</typeparam>
    /// <typeparam name="T2">Type 2 of EventArgs</typeparam>
    /// <typeparam name="T3">Type 3 of EventArgs</typeparam>
    /// <typeparam name="T4">Type 4 of EventArgs</typeparam>
    /// <typeparam name="T5">Type 5 of EventArgs</typeparam>
    [DataContract]
    public class EventArgs<T1, T2, T3, T4, T5> : EventArgs
    {
        /// <summary>
        /// Item1 Object
        /// </summary>
        [XmlElement, DataMember]
        public T1 Item1 { get; set; }
        /// <summary>
        /// Item2 Object
        /// </summary>
        [XmlElement, DataMember]
        public T2 Item2 { get; set; }
        /// <summary>
        /// Item3 Object
        /// </summary>
        [XmlElement, DataMember]
        public T3 Item3 { get; set; }
        /// <summary>
        /// Item4 Object
        /// </summary>
        [XmlElement, DataMember]
        public T4 Item4 { get; set; }
        /// <summary>
        /// Item5 Object
        /// </summary>
        [XmlElement, DataMember]
        public T5 Item5 { get; set; }

        #region .ctor
        /// <summary>
        /// Generic EventArgs
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventArgs()
        {
        }
        /// <summary>
        /// Generic EventArgs
        /// </summary>
        /// <param name="item1">Item1 object</param>
        /// <param name="item2">Item2 object</param>
        /// <param name="item3">Item3 object</param>
        /// <param name="item4">Item4 object</param>
        /// <param name="item5">Item5 object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventArgs(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
        }
        #endregion
    }
}
