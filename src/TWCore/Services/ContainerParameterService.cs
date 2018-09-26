﻿/*
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

using System.Runtime.CompilerServices;

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Container Parameter Service
    /// </summary>
    public abstract class ContainerParameterService : ICoreStart
    {
        /// <summary>
        /// Parameter Name
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Parameter Description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Container Parameter Service
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ContainerParameterService(string name, string description) 
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// OnHandler Method 
        /// </summary>
        /// <param name="info">Parameter handler info</param>
        protected abstract void OnHandler(ParameterHandlerInfo info);

        void ICoreStart.BeforeInit() { }
        void ICoreStart.AfterFactoryInit(Factories factories) { }
        void ICoreStart.FinalizingInit(Factories factories)
            => ServiceContainer.RegisterParametersHandler(Name, Description, OnHandler);
    }
}
