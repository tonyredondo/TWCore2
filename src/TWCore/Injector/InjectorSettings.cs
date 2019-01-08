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

using System.Collections.Concurrent;
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
        private ConcurrentDictionary<string, NonInstantiable> _interfaceDefinitionCache = new ConcurrentDictionary<string, NonInstantiable>();
        private ConcurrentDictionary<string, NonInstantiable> _abstractDefinitionCache = new ConcurrentDictionary<string, NonInstantiable>();
        private ConcurrentDictionary<string, Instantiable[]> _classDefinitionCache = new ConcurrentDictionary<string, Instantiable[]>();
        private ConcurrentDictionary<(string Type, string Name), Instantiable> _interfaceInstanceDefinitionCache = new ConcurrentDictionary<(string Type, string Name), Instantiable>();
        private ConcurrentDictionary<(string Type, string Name), Instantiable> _abstractInstanceDefinitionCache = new ConcurrentDictionary<(string Type, string Name), Instantiable>();

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
            => Arguments.TryGet(name, out var value) ? value : null;
        /// <summary>
        /// Get an interface definition
        /// </summary>
        /// <param name="type">AssemblyQualifiedName of the interface type</param>
        /// <returns>NonInstantiable instance definition</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NonInstantiable GetInterfaceDefinition(string type)
        {
            if (_interfaceDefinitionCache.TryGetValue(type, out var value))
                return value;

            value = Interfaces.FirstOrDefault((i, mType) =>
            {
                if (mType.Length == i.Type.Length)
                    return mType == i.Type;

                var tArr = mType.Split(',');
                var iArr = i.Type.Split(',');
                return tArr[0] == iArr[0];
            }, type);

            if (value != null)
                _interfaceDefinitionCache.TryAdd(type, value);

            return value;
        }
        /// <summary>
        /// Get an abstract definition
        /// </summary>
        /// <param name="type">AssemblyQualifiedName of the abstract type</param>
        /// <returns>NonInstantiable instance definition</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NonInstantiable GetAbstractDefinition(string type)
        {
            if (_abstractDefinitionCache.TryGetValue(type, out var value))
                return value;

            value = Abstracts.FirstOrDefault((i, mType) =>
            {
                if (mType.Length == i.Type.Length)
                    return mType == i.Type;

                var tArr = mType.Split(',');
                var iArr = i.Type.Split(',');
                return tArr[0] == iArr[0];
            }, type);

            if (value != null)
                _abstractDefinitionCache.TryAdd(type, value);

            return value;
        }
        /// <summary>
        /// Get an instantiable class definition
        /// </summary>
        /// <param name="type">AssemblyQualifiedName of the type</param>
        /// <returns>Instantiable instance definition</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Instantiable> GetInstantiableClassDefinition(string type)
        {
            if (_classDefinitionCache.TryGetValue(type, out var value))
                return value;

            value = InstantiableClasses.Where((i, mType) =>
            {
                if (mType.Length == i.Type.Length)
                    return mType == i.Type;

                var tArr = mType.Split(',');
                var iArr = i.Type.Split(',');
                return tArr[0] == iArr[0];
            }, type).ToArray();

            if (value != null)
                _classDefinitionCache.TryAdd(type, value);

            return value;
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
            if (_interfaceInstanceDefinitionCache.TryGetValue((type, name), out var value))
                return value;

            var interfaceType = GetInterfaceDefinition(type);
            if (interfaceType is null)
                throw new KeyNotFoundException($"The Interface type: {type} couldn't be found in the settings definition. Please check if some configuration or a dal registration is missing. [Interface={type}, Name={name}]");
            var className = name ?? interfaceType.DefaultClassName ?? interfaceType.ClassDefinitions.FirstOrDefault()?.Name;
            value = interfaceType.ClassDefinitions.FirstOrDefault((i, cName) => i.Name == cName, className);

            if (value != null)
                _interfaceInstanceDefinitionCache.TryAdd((type, name), value);

            return value;
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
            if (_abstractInstanceDefinitionCache.TryGetValue((type, name), out var value))
                return value;

            var abstractType = GetAbstractDefinition(type);
            if (abstractType is null)
                throw new KeyNotFoundException($"The Abstract type: {type} couldn't be found in the settings definition. Please check if some configuration or a dal registration is missing. [Interface={type}, Name={name}]");
            var className = name ?? abstractType.DefaultClassName ?? abstractType.ClassDefinitions.FirstOrDefault()?.Name;
            value = abstractType.ClassDefinitions.FirstOrDefault((i, cName) => i.Name == cName, className);

            if (value != null)
                _abstractInstanceDefinitionCache.TryAdd((type, name), value);

            return value;
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
                    if (type.ClassDefinitions is null) continue;
                    foreach (var cDef in type.ClassDefinitions)
                        lst.Add(new TypeLoadResult(cDef.Type, Core.GetType(cDef.Type) != null));
                }
            }
            if (Abstracts != null)
            {
                foreach (var type in Abstracts)
                {
                    lst.Add(new TypeLoadResult(type.Type, Core.GetType(type.Type) != null));
                    if (type.ClassDefinitions is null) continue;
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
        /// Clear all settings cache
        /// </summary>
        public void ClearCache()
        {
            _interfaceDefinitionCache.Clear();
            _abstractDefinitionCache.Clear();
            _classDefinitionCache.Clear();
            _interfaceInstanceDefinitionCache.Clear();
            _abstractInstanceDefinitionCache.Clear();
        }

        /// <summary>
        /// Type Load Result
        /// </summary>
        public readonly struct TypeLoadResult
        {
            /// <summary>
            /// Type AssemblyQualifiedName
            /// </summary>
            public readonly string Type;
            /// <summary>
            /// Get if the Type is Loaded
            /// </summary>
            public readonly bool Loaded;
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
            /// <summary>
            /// Gets the hash code of the struct
            /// </summary>
            /// <returns>Hash code</returns>
            public override int GetHashCode()
                => Type?.GetHashCode() ?? 0 + Loaded.GetHashCode();
            /// <summary>
            /// Gets if the object instance is equal to the struct
            /// </summary>
            /// <param name="obj">Object instance</param>
            /// <returns>True if the object is equal to the current struct</returns>
            public override bool Equals(object obj)
                => obj is TypeLoadResult tRes && Type == tRes.Type && Loaded == tRes.Loaded;
        }
        #endregion
    }
}
