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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using TWCore.Collections;
using TWCore.Diagnostics.Log;
using TWCore.Text;
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Injector
{
    /// <summary>
    /// Injector delegate for type instance resolver
    /// </summary>
    /// <param name="type">Type to resolve</param>
    /// <param name="name">Name to resolve</param>
    /// <param name="value">Output instance value</param>
    /// <returns>True if the resolve was successful; otherwise, false.</returns>
    public delegate bool InjectorResolveDelegate(Type type, string name, out object value);

    /// <inheritdoc />
    /// <summary>
    /// Inject instances engine
    /// </summary>
    public class NewInjectorEngine : IDisposable
    {
        private ConcurrentDictionary<(Type, string), InjectorTypeNameInfo> _injectorData = new ConcurrentDictionary<(Type, string), InjectorTypeNameInfo>();
        private InjectorSettings _settings;
        private bool _attributesRegistered;
        private bool _useOnlyLoadedAssemblies = true;

        #region Delegates
        /// <summary>
        /// Delgate when a new instance is requested
        /// </summary>
        public InjectorResolveDelegate OnTypeInjectorResolve;
        #endregion

        #region Properties
        /// <summary>
        /// Injector settings
        /// </summary>
        public InjectorSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                _attributesRegistered = false;
            }
        }
        /// <summary>
        /// Use only assemblies loaded, false if uses all assemblies in the folder
        /// </summary>
        public bool UseOnlyLoadedAssemblies
        {
            get => _useOnlyLoadedAssemblies;
            set
            {
                _useOnlyLoadedAssemblies = value;
                _attributesRegistered = false;
            }
        }
        /// <summary>
        /// Throw exception on instance creation error
        /// </summary>
        public bool ThrowExceptionOnInstanceCreationError { get; set; } = true;
        #endregion

        #region .ctor
        /// <summary>
        /// Inject instances for non instantiable class
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NewInjectorEngine()
        {
            UseOnlyLoadedAssemblies = Core.GlobalSettings.InjectorUseOnlyLoadedAssemblies;
            Settings = new InjectorSettings();
        }
        /// <summary>
        /// Inject instances for non instantiable class
        /// </summary>
        /// <param name="settings">Injector settings</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NewInjectorEngine(InjectorSettings settings)
        {
            UseOnlyLoadedAssemblies = Core.GlobalSettings.InjectorUseOnlyLoadedAssemblies;
            Settings = settings;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~NewInjectorEngine()
        {
            Dispose();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Create a new instance for a Interface, Abstract, or setted class
        /// </summary>
        /// <typeparam name="T">Type of object to create</typeparam>
        /// <param name="name">Name of the instance, if is null the default one</param>
        /// <returns>A new instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T New<T>(string name = null)
            => (T)New(typeof(T), name);
        /// <summary>
        /// Create a new instance for a Interface, Abstract, or setted class
        /// </summary>
        /// <param name="type">Type of object to create</param>
        /// <param name="name">Name of the instance, if is null the default one</param>
        /// <returns>A new instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object New(Type type, string name = null)
        {
            if (OnTypeInjectorResolve != null && OnTypeInjectorResolve(type, name, out var result))
                return result;

            var injectorTypeInfo = _injectorData.GetOrAdd((type, name), CreateInjectorTypeNameInfo);


            return null;
        }
        #endregion


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private InjectorTypeNameInfo CreateInjectorTypeNameInfo((Type Type, string Name) value)
            => new InjectorTypeNameInfo(value.Type, value.Name, Settings);

        /// <inheritdoc />
        /// <summary>
        /// Dispose resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
        }


        private class InjectorTypeNameInfo
        {
            public readonly Type Type;
            public readonly Type InstantiableType;
            public readonly string InstantiableName;
            public bool Singleton;
            public object SingletonValue;
            public Func<object> Activator;
            private Instantiable Definition;

            public InjectorTypeNameInfo(Type type, string name, InjectorSettings settings)
            {
                Type = type;
                InstantiableName = name;

                NonInstantiable nonInstantiable = null;
                if (type.IsInterface)
                {
                    if (!settings.Interfaces.TryGet(type.AssemblyQualifiedName, out nonInstantiable))
                        settings.Interfaces.TryGet(type.GetTypeName(), out nonInstantiable);
                }
                else if (type.IsAbstract)
                {
                    if (!settings.Abstracts.TryGet(type.AssemblyQualifiedName, out nonInstantiable))
                        settings.Abstracts.TryGet(type.GetTypeName(), out nonInstantiable);
                }
                else
                {
                    InstantiableType = type;
                }

                if (nonInstantiable != null)
                    nonInstantiable.ClassDefinitions.TryGet(name, out Definition);
                else
                    settings.InstantiableClasses.TryGet(name, out Definition);


                if (Definition != null)
                {

                }
                

            }

            public InjectorTypeNameInfo(Type type, Instantiable definition, NewInjectorEngine engine)
            {
                Type = type;
                InstantiableType = Core.GetType(definition.Type, engine.ThrowExceptionOnInstanceCreationError);
                InstantiableName = definition.Name;
                Singleton = definition.Singleton;

                if (InstantiableType is null) return;


                var ctors = InstantiableType.GetConstructors();

            }

            public InjectorTypeNameInfo(Type type, Type instantiableType, bool singleton, string name, object[] parameters)
            {

            }

            public InjectorTypeNameInfo(Type type, Func<object> activator, bool singleton, string name)
            {

            }
        }
    }
}
