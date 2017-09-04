﻿/*
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
using System.Collections.Generic;
using System.Linq;

namespace TWCore.Collections
{
    /// <summary>
    /// Stack for multiple EventHandlers on event
    /// </summary>
    public class EventHandlerStack
    {
        #region Private Fields
        /// <summary>
        /// EventHandler stack
        /// </summary>
        Stack<EventHandler> _eventHandlersStack;
        /// <summary>
        /// EventHandler internal delegate
        /// </summary>
        public EventHandler EventHandlerDelegate;
        /// <summary>
        /// The sync lock.
        /// </summary>
        protected object _padlock = new object();
        #endregion

        #region Properties
        /// <summary>
        /// Use Bubbling in the stack
        /// </summary>
        public bool UseBubbling { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Stack for multiple EventHandlers on event
        /// </summary>
        public EventHandlerStack()
        {
            _eventHandlersStack = new Stack<EventHandler>();
            UseBubbling = false;
            EventHandlerDelegate = new EventHandler(CallEventHandler);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Internal method for the internal event handler
        /// </summary>
        private void CallEventHandler(object sender, EventArgs e)
        {
            lock (_padlock)
            {
                if (_eventHandlersStack.Count > 0)
                {
                    if (!UseBubbling)
                        _eventHandlersStack.Peek().Invoke(sender, e);
                    else
                        _eventHandlersStack.Each(item => item.Invoke(sender, e));
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Register new EventHandler in the stack
        /// </summary>
        /// <param name="handler">EventHandler</param>
        public void RegisterHandler(EventHandler handler)
        {
            lock (_padlock)
                _eventHandlersStack.Push(handler);
        }
        /// <summary>
        /// Deregister an EventHadler of the stack
        /// </summary>
        /// <param name="handler">EventHandler</param>
        public void DeRegisterHandler(EventHandler handler)
        {
            lock (_padlock)
            {
                var nStack = _eventHandlersStack.Where(item => item != handler);
                if (nStack != null && nStack.Any())
                    _eventHandlersStack = new Stack<EventHandler>(nStack.Reverse());
            }
        }
        /// <summary>
        /// Pop EventHandler from the stack
        /// </summary>
        /// <returns>EventHandler</returns>
        public EventHandler PopHandler()
        {
            lock (_padlock)
                return _eventHandlersStack.Pop();
        }
        #endregion
    }

    /// <summary>
    /// Stack for multiple EventHandlers on event
    /// </summary>
    /// <typeparam name="T">Type of EventHandler</typeparam>
    public class EventHandlerStack<T>
    {
        #region Private Fields
        /// <summary>
        /// EventHandler stack
        /// </summary>
        Stack<EventHandler<T>> _eventHandlersStack;
        /// <summary>
        /// EventHandler internal delegate
        /// </summary>
        public EventHandler<T> EventHandlerDelegate;
        /// <summary>
        /// The sync lock.
        /// </summary>
        protected object _padlock = new object();
        #endregion

        #region Properties
        /// <summary>
        /// Use Bubbling in the stack
        /// </summary>
        public bool UseBubbling { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Stack for multiple EventHandlers on event
        /// </summary>
        public EventHandlerStack()
        {
            _eventHandlersStack = new Stack<EventHandler<T>>();
            UseBubbling = false;
            EventHandlerDelegate = new EventHandler<T>(CallEventHandler);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Internal method for the internal event handler
        /// </summary>
        private void CallEventHandler(object sender, T e)
        {
            lock (_padlock)
            {
                if (_eventHandlersStack.Count > 0)
                {
                    if (!UseBubbling)
                        _eventHandlersStack.Peek().Invoke(sender, e);
                    else
                        _eventHandlersStack.Each(item => item.Invoke(sender, e));
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Register new EventHandler in the stack
        /// </summary>
        /// <param name="handler">EventHandler</param>
        public void RegisterHandler(EventHandler<T> handler)
        {
            lock (_padlock)
                _eventHandlersStack.Push(handler);
        }
        /// <summary>
        /// Deregister an EventHadler of the stack
        /// </summary>
        /// <param name="handler">EventHandler</param>
        public void DeRegisterHandler(EventHandler<T> handler)
        {
            lock (_padlock)
            {
                var nStack = _eventHandlersStack.Where(item => item != handler);
                if (nStack != null && nStack.Any())
                    _eventHandlersStack = new Stack<EventHandler<T>>(nStack.Reverse());
            }
        }
        /// <summary>
        /// Pop EventHandler from the stack
        /// </summary>
        /// <returns>EventHandler</returns>
        public EventHandler<T> PopHandler()
        {
            lock (_padlock)
                return _eventHandlersStack.Pop();
        }
        #endregion
    }
}
