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
using System.Collections.ObjectModel;
// ReSharper disable UnusedMemberInSuper.Global

namespace TWCore.Diagnostics.Status
{
    /// <inheritdoc />
    /// <summary>
    /// Interface for Status Engine
    /// </summary>
    public interface IStatusEngine : IDisposable
    {
        /// <summary>
        /// Current status engine transport
        /// </summary>
        ObservableCollection<IStatusTransport> Transports { get; }
        /// <summary>
        /// Attach a status item delegate 
        /// </summary>
        /// <param name="statusItemDelegate">Status Item delegate</param>
        /// <param name="objectToAttach">Object to attach, if is null is extracted from the delegate</param>
        void Attach(Func<StatusItem> statusItemDelegate, object objectToAttach = null);
        /// <summary>
        /// Attach a values filler delegate
        /// </summary>
        /// <param name="valuesFillerDelegate">Values filler delegate</param>
        /// <param name="objectToAttach">Object to attach, if is null is extracted from the delegate</param>
        void Attach(Action<StatusItemValuesCollection> valuesFillerDelegate, object objectToAttach = null);
        /// <summary>
        /// Attach an object without values
        /// </summary>
        /// <param name="objectToAttach">Object to attach, if is null is extracted from the delegate</param>
        void AttachObject(object objectToAttach);
        /// <summary>
        /// Attach a child object
        /// </summary>
        /// <param name="objectToAttach">Object to attach</param>
        /// <param name="parent">Parent object</param>
        void AttachChild(object objectToAttach, object parent);
        /// <summary>
        /// DeAttach all handlers for an object
        /// </summary>
        /// <param name="objectToDetach">Object to detach</param>
        void DeAttachObject(object objectToDetach);
        /// <summary>
        /// Enable or Disable the Trace engine
        /// </summary>
        bool Enabled { get; set; }
    }
}
