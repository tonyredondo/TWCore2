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
using System.Runtime.CompilerServices;

namespace TWCore.Settings
{
    /// <summary>
    /// Setting base class for automatic settings parser
    /// </summary>
    public abstract class SettingsBase
    {
        /// <summary>
        /// On settings reload event
        /// </summary>
        public event EventHandler OnSettingsReload;

        /// <summary>
        /// Setting base class for automatic settings parser
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected SettingsBase()
        {
            LoadSettings();
        }

        /// <summary>
        /// Reload Settings
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReloadSettings()
        {
            LoadSettings();
        }

        /// <summary>
        /// Load Settings
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LoadSettings()
        {
            SettingsEngine.ApplySettings(this);
            OnSettingsReload?.Invoke(this, EventArgs.Empty);
        }
    }
}
