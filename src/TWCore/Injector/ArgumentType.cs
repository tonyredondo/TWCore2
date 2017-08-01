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


namespace TWCore.Injector
{
    /// <summary>
    /// Argument type enum
    /// </summary>
    public enum ArgumentType
    {
        /// <summary>
        /// A raw value
        /// </summary>
        Raw,
        /// <summary>
        /// Key to look the value in the Core.Settings collection
        /// </summary>
        Settings,
        /// <summary>
        /// Interface definition name
        /// </summary>
        Interface,
        /// <summary>
        /// Abstract definition name
        /// </summary>
        Abstract,
        /// <summary>
        /// Instance definition name
        /// </summary>
        Instance
    }
}
