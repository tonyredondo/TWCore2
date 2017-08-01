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
using System.Reflection;

namespace TWCore
{
    /// <summary>
    /// Sequential Guid Generator delegate
    /// </summary>
    /// <returns>Guid value</returns>
    public delegate Guid GuidGeneratorDelegate();
    /// <summary>
    /// Get Guid delegate
    /// </summary>
    /// <param name="sequential">True for a sequiential Guid, otherwise, false.</param>
    /// <returns>Guid value</returns>
    public delegate Guid GetGuidDelegate(bool sequential = false);
    /// <summary>
    /// Get assemblies delegate
    /// </summary>
    /// <returns>Assemblies</returns>
    public delegate Assembly[] GetAssembliesDelegate();
    /// <summary>
    /// Equals delegate
    /// </summary>
    /// <param name="a">Byte array</param>
    /// <param name="b">Byte array</param>
    /// <returns>True if is equal, otherwise, false.</returns>
    public delegate bool EqualsBytesDelegate(byte[] a, byte[] b);
}
