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

namespace TWCore.Cache
{
    /// <summary>
    /// IStorage Extension
    /// </summary>
    public interface IStorageExtension : IDisposable
    {
        /// <summary>
        /// Extension name
        /// </summary>
        string ExtensionName { get; }
        /// <summary>
        /// Initialize extension
        /// </summary>
        /// <param name="storage">Storage instance</param>
        void Init(IStorage storage);
        /// <summary>
        /// Execute the extension
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <param name="args">Arguments of the command</param>
        /// <returns>Command response</returns>
        object Execute(string command, object[] args);
    }
}
