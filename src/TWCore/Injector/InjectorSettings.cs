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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMember.Global

namespace TWCore.Injector
{
    /// <summary>
    /// Injector settings
    /// </summary>
    [XmlRoot("InjectorSettings"), DataContract]
    public class InjectorSettings
    {
        /// <summary>
        /// Collection of argument values
        /// </summary>
        [XmlArray("Arguments"), XmlArrayItem("Argument"), DataMember]
        public NameCollection<Argument> Arguments { get; set; } = new NameCollection<Argument>(false);
        /// <summary>
        /// Collection of interface definitions
        /// </summary>
        [XmlArray("Interfaces"), XmlArrayItem("Interface"), DataMember]
        public KeyStringDelegatedCollection<NonInstantiable> Interfaces { get; set; } = new KeyStringDelegatedCollection<NonInstantiable>(k => k.Type, false);
        /// <summary>
        /// Collection of abstracts definitions
        /// </summary>
        [XmlArray("Abstracts"), XmlArrayItem("Abstract"), DataMember]
        public KeyStringDelegatedCollection<NonInstantiable> Abstracts { get; set; } = new KeyStringDelegatedCollection<NonInstantiable>(k => k.Type, false);
        /// <summary>
        /// Collection of instantiable classes
        /// </summary>
        [XmlArray("InstantiableClasses"), XmlArrayItem("Class"), DataMember]
        public NameCollection<Instantiable> InstantiableClasses { get; set; } = new NameCollection<Instantiable>(false);

        #region Public Methods
        /// <summary>
        /// Get an Argument
        /// </summary>
        /// <param name="name">Name of argument</param>
        /// <returns>Argument instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Argument GetArgument(string name)
            => Arguments.Contains(name) ? Arguments[name] : null;
        /// <summary>
        /// Get an interface definition
        /// </summary>
        /// <param name="type">AssemblyQualifiedName of the interface type</param>
        /// <returns>NonInstantiable instance definition</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NonInstantiable GetInterfaceDefinition(string type)
        {
            return Interfaces.FirstOrDefault(i =>
            {
                if (type.Length == i.Type.Length)
                    return type == i.Type;
                
                var tArr = type.Split(',');
                var iArr = i.Type.Split(',');
                return tArr[0] == iArr[0];
            });
        }
        /// <summary>
        /// Get an abstract definition
        /// </summary>
        /// <param name="type">AssemblyQualifiedName of the abstract type</param>
        /// <returns>NonInstantiable instance definition</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NonInstantiable GetAbstractDefinition(string type)
        {
            return Abstracts.FirstOrDefault(i =>
            {
                if (type.Length == i.Type.Length)
                    return type == i.Type;
                
                var tArr = type.Split(',');
                var iArr = i.Type.Split(',');
                return tArr[0] == iArr[0];
            });
        }
        /// <summary>
        /// Get an instantiable class definition
        /// </summary>
        /// <param name="type">AssemblyQualifiedName of the type</param>
        /// <returns>Instantiable instance definition</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Instantiable> GetInstantiableClassDefinition(string type)
        {
            return InstantiableClasses.Where((i, mType) =>
            {
                if (mType.Length == i.Type.Length)
                    return mType == i.Type;
                
                var tArr = mType.Split(',');
                var iArr = i.Type.Split(',');
                return tArr[0] == iArr[0];
            }, type);
        }
        /// <summary>
        /// Get an interface implementation definition
        /// </summary>
        /// <param name="type">AssemblyQualifiedName of the interface type</param>
        /// <param name="name">Name of the class</param>
        /// <returns>Instantiable instance definition</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Instantiable GetInterfaceInstanceDefinition(string type, string name = null)
        {
            var interfaceType = GetInterfaceDefinition(type);
            if (interfaceType == null)
                throw new KeyNotFoundException($"The Interface type: {type} couldn't be found in the settings definition. Please check if some configuration or a dal registration is missing. [Interface={type}, Name={name}]");
            var className = name ?? interfaceType.DefaultClassName ?? interfaceType.ClassDefinitions.FirstOrDefault()?.Name;
            return interfaceType.ClassDefinitions.FirstOrDefault(i => i.Name == className);
        }
        /// <summary>
        /// Get an abstract implementation definition
        /// </summary>
        /// <param name="type">AssemblyQualifiedName of the abstract type</param>
        /// <param name="name">Name of the class</param>
        /// <returns>Instantiable instance definition</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Instantiable GetAbstractInstanceDefinition(string type, string name = null)
        {
            var abstractType = GetAbstractDefinition(type);
            if (abstractType == null)
                throw new KeyNotFoundException($"The Abstract type: {type} couldn't be found in the settings definition. Please check if some configuration or a dal registration is missing. [Interface={type}, Name={name}]");
            var className = name ?? abstractType.DefaultClassName ?? abstractType.ClassDefinitions.FirstOrDefault()?.Name;
            return abstractType.ClassDefinitions.FirstOrDefault(i => i.Name == className);
        }
        /// <summary>
        /// Preload all declared types
        /// </summary>
        /// <returns>Types tuples array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyCollection<TypeLoadResult> PreloadAllTypes()
        {
            var lst = new List<TypeLoadResult>();
            if (Interfaces != null)
            {
                foreach (var type in Interfaces)
                {
                    lst.Add(new TypeLoadResult(type.Type, Core.GetType(type.Type) != null));
                    if (type.ClassDefinitions == null) continue;
                    foreach (var cDef in type.ClassDefinitions)
                        lst.Add(new TypeLoadResult(cDef.Type, Core.GetType(cDef.Type) != null));
                }
            }
            if (Abstracts != null)
            {
                foreach (var type in Abstracts)
                {
                    lst.Add(new TypeLoadResult(type.Type, Core.GetType(type.Type) != null));
                    if (type.ClassDefinitions == null) continue;
                    foreach (var cDef in type.ClassDefinitions)
                        lst.Add(new TypeLoadResult(cDef.Type, Core.GetType(cDef.Type) != null));
                }
            }
            if (InstantiableClasses != null)
            {
                foreach (var type in InstantiableClasses)
                    lst.Add(new TypeLoadResult(type.Type, Core.GetType(type.Type) != null));
            }
            return lst.AsReadOnly();
        }
        /// <summary>
        /// Type Load Result
        /// </summary>
        public struct TypeLoadResult
        {
            /// <summary>
            /// Type AssemblyQualifiedName
            /// </summary>
            public string Type { get; }
            /// <summary>
            /// Get if the Type is Loaded
            /// </summary>
            public bool Loaded { get; }
            /// <summary>
            /// Type Load Result
            /// </summary>
            /// <param name="type">Type AssemblyQualifiedName</param>
            /// <param name="loaded">True if the type is loaded</param>
            public TypeLoadResult(string type, bool loaded)
            {
                Type = type;
                Loaded = loaded;
            }
        }
        #endregion
    }
}
