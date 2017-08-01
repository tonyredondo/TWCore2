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

namespace TWCore
{
    /// <summary>
    /// Worker where the Queue elements are actions to be executed in order
    /// </summary>
    public class ActionWorker : Worker<ActionWorker.WorkerItem>
    {
        #region .ctors
        /// <summary>
        /// Worker where all elements are actions to be executed in order
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ActionWorker() : base((item) => Try.Do(() => item.Action(item.State), item.OnExceptionCallback, true)) { }
        /// <summary>
        /// Worker where all elements are actions to be executed in order
        /// </summary>
        /// <param name="precondition">Precondition to accomplish before dequeuing an element from the queue</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ActionWorker(Func<bool> precondition) : base(precondition, item => Try.Do(() => item.Action(item.State), item.OnExceptionCallback, true)) { }
        #endregion

        #region Methods
        /// <summary>
        /// Enqueue a new Action on the queue
        /// </summary>
        /// <param name="action">Action to be executed</param>
        /// <param name="onExceptionCallback">Action executed in case of an Exception</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(Action action, Action<Exception> onExceptionCallback = null) => Enqueue(new WorkerItem(o => action(), null, onExceptionCallback));
        /// <summary>
        /// Enqueue a new Action on the queue
        /// </summary>
        /// <param name="action">Action to be executed</param>
        /// <param name="state">State object to pass to the action</param>
        /// <param name="onExceptionCallback">Action executed in case of an Exception</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue<T>(Action<T> action, T state, Action<Exception> onExceptionCallback = null) => Enqueue(new WorkerItem(o => action((T)o), state, onExceptionCallback));
        #endregion

        #region Nested classes
        /// <summary>
        /// Action Worker queue item where is stored the action to be executed and the action when an exception occurs
        /// </summary>
        public class WorkerItem
        {
            /// <summary>
            /// Action to be executed
            /// </summary>
            public Action<object> Action { get; private set; }
            /// <summary>
            /// Saves the state to be used by the action
            /// </summary>
            public object State { get; private set; }
            /// <summary>
            /// Action executed in case of an Exception
            /// </summary>
            public Action<Exception> OnExceptionCallback { get; private set; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal WorkerItem(Action<object> action, object state, Action<Exception> onExceptionCallback)
            {
                Action = action;
                State = state;
                OnExceptionCallback = onExceptionCallback;
            }
        }
        #endregion
    }
}
