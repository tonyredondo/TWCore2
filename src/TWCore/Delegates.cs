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

using System;
using System.Reflection;
using System.Threading.Tasks;

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
    /// <summary>
    /// Represents the method that will handle an async event when the event provides data.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An object that contains the event data. </param>
    /// <typeparam name="TEventArgs">The type of the event data generated by the event.</typeparam>
    public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs e);
    /// <summary>
    /// Represents the method that will handle an async event when the event provides data.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An object that contains the event data. </param>
    public delegate Task AsyncEventHandler(object sender, EventArgs e);
}
