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

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Services container parameters
    /// </summary>
    public class ServicesContainerParameters : ICoreStart
    {
        /// <summary>
        /// Before Init
        /// </summary>
        public void BeforeInit()
        {
        }

        /// <summary>
        /// After Factory Init
        /// </summary>
        /// <param name="factories">Factory instance</param>
        public void AfterFactoryInit(Factories factories)
        {
        }

        /// <summary>
        /// Finalizing Init
        /// </summary>
        /// <param name="factories">Factory instance</param>
        public void FinalizingInit(Factories factories)
        {
            switch (factories.PlatformType)
            {
                case PlatformType.Windows:
                    Core.ServiceContainerFactory = (service, action) => new WindowsServiceContainer(service, action);
                    break;
                case PlatformType.Linux:
                    Core.ServiceContainerFactory = (service, action) => new LinuxServiceContainer(service, action);
                    break;
                case PlatformType.Mac:
                    break;
                case PlatformType.Android:
                    break;
                case PlatformType.iOS:
                    break;
            }
        }
    }
}