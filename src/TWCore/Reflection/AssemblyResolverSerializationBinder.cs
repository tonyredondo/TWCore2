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
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace TWCore.Reflection
{
    /// <summary>
    /// Serialization binder for the BinaryFormatter serializer that uses the AssemblyResolver
    /// </summary>
    public class AssemblyResolverSerializationBinder : SerializationBinder
    {
        /// <summary>
        /// Assembly resolver associated with the binder.
        /// </summary>
        public AssemblyResolver AssemblyResolver { get; private set; }
        /// <summary>
        /// Serialization binder for the BinaryFormatter serializer that uses the AssemblyResolver
        /// </summary>
        /// <param name="assemblyResolver">Assembly resolver to bind on the serialization binder</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AssemblyResolverSerializationBinder(AssemblyResolver assemblyResolver)
        {
            AssemblyResolver = assemblyResolver;
        }

        /// <summary>
        /// When overridden in a derived class, controls the binding of a serialized object to a type.
        /// </summary>
        /// <param name="assemblyName">Specifies the System.Reflection.Assembly name of the serialized object.</param>
        /// <param name="typeName">Specifies the System.Type name of the serialized object.</param>
        /// <returns>The type of the object the formatter creates a new instance of.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (AssemblyResolver.Assemblies.Contains(assemblyName))
            {
                var assemblyInfo = AssemblyResolver.Assemblies[assemblyName];
                var type = assemblyInfo.Instance.GetType(typeName);
                if (type != null)
                    return type;
            }
            return Core.GetType(typeName);
        }
    }
}