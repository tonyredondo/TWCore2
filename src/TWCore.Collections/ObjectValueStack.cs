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

using System;
using System.Collections.Generic;
using System.Linq;

namespace TWCore.Collections
{
    /// <summary>
    /// Stack for tracking multiple values for a object
    /// </summary>
    /// <typeparam name="T">Value Type</typeparam>
    public class ObjectValueStack<T>
    {
        #region Fields
        /// <summary>
        /// Internal Stack
        /// </summary>
        private Stack<ObjectValue<T>> _stack;
        /// <summary>
        /// The sync lock.
        /// </summary>
        protected readonly object Padlock = new object();
        #endregion

        #region Properties
        /// <summary>
        /// Default Value
        /// </summary>
        public T DefaultValue { get; set; }
        /// <summary>
        /// Action for change value when the stack change
        /// </summary>
        private Action<object, T> ChangeValueAction { get; set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Stack for tracking multiple values for a object
        /// </summary>
        /// <param name="changeValueAction">Action for change value when the stack change</param>
        /// <param name="defaultValue">Default Value</param>
        public ObjectValueStack(Action<object, T> changeValueAction, T defaultValue)
        {
            _stack = new Stack<ObjectValue<T>>();
            ChangeValueAction = changeValueAction;
            DefaultValue = defaultValue;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Register new object in the stack for tracking
        /// </summary>
        /// <param name="sender">Sender object</param>
        public void RegisterObject(object sender)
        {
            lock (Padlock)
                RegisterObject(sender, DefaultValue);
        }
        /// <summary>
        /// Register new object-value in the stack for tracking
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="value">New Value</param>
        public void RegisterObject(object sender, T value)
        {
            lock (Padlock)
            {
                var oValue = new ObjectValue<T> { Sender = sender, Value = value };
                _stack.Push(oValue);
                ChangeValueAction(oValue.Sender, oValue.Value);
            }
        }
        /// <summary>
        /// Deregister the object from the stack
        /// </summary>
        /// <param name="sender">Sender object</param>
        public void DeRegisterObject(object sender)
        {
            lock (Padlock)
            {
                if (_stack.Count <= 0) return;
                var oldPeek = _stack.Peek();
                var nStack = _stack.Where((item, mSender) => item.Sender != mSender, sender).ToArray();
                if (nStack.Any())
                    _stack = new Stack<ObjectValue<T>>(nStack.Reverse());

                if (_stack.Count > 0)
                {
                    var newPeek = _stack.Peek();
                    if (newPeek != oldPeek)
                        ChangeValueAction(newPeek.Sender, newPeek.Value);
                }
                else
                    ChangeValueAction(this, DefaultValue);
            }
        }
        /// <summary>
        /// Method to change the value for a object-value item in the stack
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="value">New Value for this object in the stack</param>
        public void SetValue(object sender, T value)
        {
            lock (Padlock)
            {
                if (_stack.Count <= 0) return;
                var objValue = _stack.FirstOrDefault(item => item.Sender == sender);
                if (objValue != null)
                    objValue.Value = value;

                if (_stack.Peek() == objValue && objValue != null)
                    ChangeValueAction(objValue.Sender, objValue.Value);
            }
        }
        /// <summary>
        /// Pop an Object-Value item from the stack and call the Change Value Action
        /// </summary>
        /// <returns>ObjectValue item instance</returns>
        public ObjectValue<T> PopObjectValue()
        {
            lock (Padlock)
            {
                var pop = _stack.Pop();
                var newPeek = _stack.Peek();
                if (newPeek != null)
                    ChangeValueAction(newPeek.Sender, newPeek.Value);
                return pop;
            }
        }
        #endregion

        #region Nested Classes
        /// <summary>
        /// Object and value element to save in the stack
        /// </summary>
        /// <typeparam name="TInner">Value object</typeparam>
        public class ObjectValue<TInner>
        {
            /// <summary>
            /// Object sender
            /// </summary>
            public object Sender { get; set; }
            /// <summary>
            /// Object value
            /// </summary>
            public TInner Value { get; set; }
        }
        #endregion
    }
}
