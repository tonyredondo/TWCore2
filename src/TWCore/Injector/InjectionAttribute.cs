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
using System.Runtime.CompilerServices;

namespace TWCore.Injector
{
    /// <inheritdoc />
    /// <summary>
    /// Defines an instance to a noninstantiable type
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class InjectionAttribute : Attribute
    {
        /// <summary>
        /// Non instantiable object type
        /// </summary>
        public Type NonInstantiableType { get; }
        /// <summary>
        /// Instantiable object type
        /// </summary>
        public Type InstantiableType { get; }
        /// <summary>
        /// Indicates the instantiable object should be a singleton
        /// </summary>
        public bool Singleton { get; }
        /// <summary>
        /// Instance name
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        /// <summary>
        /// Defines an instance to a noninstantiable type
        /// </summary>
        /// <param name="noninstantiableType">Non instantiable object type</param>
        /// <param name="instantiableType">Instantiable object type</param>
        /// <param name="name">Instance name</param>
        /// <param name="singleton">Injection instance is a singleton instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InjectionAttribute(Type noninstantiableType, Type instantiableType, string name = null, bool singleton = false)
        {
            NonInstantiableType = noninstantiableType;
            InstantiableType = instantiableType;
            Name = name ?? InstantiableType.Name;
            Singleton = singleton;
        }
    }
}
