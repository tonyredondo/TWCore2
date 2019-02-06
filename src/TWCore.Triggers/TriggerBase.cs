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
// ReSharper disable MemberCanBeProtected.Global

namespace TWCore.Triggers
{
    /// <summary>
	/// Update Trigger base class for updatable instance
	/// </summary>
	public abstract class TriggerBase
    {
        /// <summary>
        /// On Update event trigger
        /// </summary>
        public event Action<TriggerBase> OnTriggered;
        /// <summary>
        /// Get if the trigger has been initiated
        /// </summary>
        public bool Initiated { get; private set; }

        #region Constructor
        /// <summary>
        /// Update Trigger base class for updatable instance
        /// </summary>
        protected TriggerBase()
        {
            Initiated = false;

            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(Initiated), Initiated);
            });
        }
        #endregion

        #region Methods
        /// <summary>
        /// Init Method
        /// </summary>
        public void Init()
        {
            if (Initiated) return;
            OnInit();
            Initiated = true;
        }
        /// <summary>
        /// Finalize Method
        /// </summary>
        public void Dispose()
        {
            if (!Initiated) return;
            OnFinalize();
            Initiated = false;
        }
        /// <summary>
        /// Call the update trigger
        /// </summary>
        public void Trigger()
        {
            OnTriggered?.Invoke(this);
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On trigger init
        /// </summary>
        protected abstract void OnInit();
        /// <summary>
        /// On trigger finalize
        /// </summary>
        protected abstract void OnFinalize();
        #endregion
    }
}
