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
            if (injectorTypeInfo == null)
                return null;
            return injectorTypeInfo.Singleton ? injectorTypeInfo.SingletonValue : injectorTypeInfo.Activator();
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private InjectorTypeNameInfo CreateInjectorTypeNameInfo((Type Type, string Name) value)
        {
            if (ThrowExceptionOnInstanceCreationError)
                return new InjectorTypeNameInfo(value.Type, value.Name, Settings);
            try
            {
                return new InjectorTypeNameInfo(value.Type, value.Name, Settings);
            }
            catch (Exception ex)
            {
                Core.Log.Write(LogLevel.Warning, ex);
                return null;
            }
        }

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
            public readonly Instantiable Definition;
            public bool Singleton;
            public object SingletonValue;
            public LambdaExpression ActivatorExpression;
            public Func<object> Activator;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public InjectorTypeNameInfo(Type type, string name, InjectorSettings settings)
            {
                Type = type;
                InstantiableName = name;
                Instantiable definition = null;

                #region Definitions
                if (type.IsInterface)
                    definition = settings.GetInterfaceInstanceDefinition(type.AssemblyQualifiedName, name);
                else if (type.IsAbstract)
                    definition = settings.GetAbstractInstanceDefinition(type.AssemblyQualifiedName, name);
                else if (type.IsClass)
                {
                    definition = settings.GetInstantiableClassDefinition(type.AssemblyQualifiedName).FirstOrDefault(i => i.Name == name);
                    if (definition == null)
                        definition = new Instantiable { Type = type.AssemblyQualifiedName, Name = name };
                }
                Definition = definition;
                #endregion

                Singleton = definition.Singleton;
                var defType = Core.GetType(definition.Type, true);
                if (defType is null) return;
                InstantiableType = defType;

                ConstructorInfo selectedCtor = null;

                #region Constructor Selector
                var ctors = defType.GetConstructors();
                if (definition.Parameters == null || definition.Parameters.Count == 0)
                {
                    #region .ctor Selector
                    var lstParams = new List<string>();
                    var ctorsOrdersByParamLength = ctors.OrderByDescending(i => i.GetParameters().Length);
                    foreach (var ctor in ctorsOrdersByParamLength)
                    {
                        selectedCtor = ctor;
                        var ctorParameters = ctor.GetParameters();
                        foreach (var ctorParam in ctorParameters)
                        {
                            var paramType = ctorParam.ParameterType;
                            var paramTypeAQName = paramType.AssemblyQualifiedName;

                            if (ctorParam.HasDefaultValue) continue;

                            if (paramType.IsInterface && settings.GetInterfaceDefinition(paramTypeAQName) == null)
                            {
                                selectedCtor = null;
                                lstParams.Add($"A definition for interface: '{paramTypeAQName}' wasn't found.");
                                break;
                            }
                            else if (paramType.IsAbstract && settings.GetAbstractDefinition(paramTypeAQName) == null)
                            {
                                selectedCtor = null;
                                lstParams.Add($"A definition for abstract: '{paramTypeAQName}' wasn't found.");
                                break;
                            }
                            else if (paramType.IsClass && !settings.GetInstantiableClassDefinition(paramTypeAQName).Any())
                            {
                                selectedCtor = null;
                                lstParams.Add($"A definition for class: '{paramTypeAQName}' wasn't found.");
                                break;
                            }
                        }
                        if (selectedCtor != null)
                            break;
                    }
                    #endregion

                    if (selectedCtor == null)
                        throw new Exception($"A valid .ctor wasn't found in the type: {defType.AssemblyQualifiedName}.\r\n{lstParams.Join("\r\n")}");
                }
                else
                {
                    selectedCtor = ctors.FirstOrDefault(c =>
                    {
                        var cParams = c.GetParameters();
                        var definitionParameters = definition.Parameters.ToDictionary(k => k.Name);
                        foreach (var cParam in cParams)
                        {
                            if (!definitionParameters.Remove(cParam.Name))
                                if (!cParam.HasDefaultValue)
                                    return false;

                        }
                        return definitionParameters.Count == 0;
                    });

                    if (selectedCtor == null)
                        throw new Exception($"A .ctor with {definition.Parameters.Count} parameters wasn't found in the type: {defType.AssemblyQualifiedName}.");
                }
                #endregion

                #region Create Expression
                var serExpressions = new List<Expression>();
                var varExpressions = new List<ParameterExpression>();
                var value = Expression.Parameter(defType, "value");
                varExpressions.Add(value);
                var returnTarget = Expression.Label(typeof(object), "ReturnTarget");

                #region New Call
                var paramExpressions = new List<Expression>();
                var cParameters = selectedCtor.GetParameters();
                var defParameters = definition.Parameters.ToDictionary(k => k.Name);
                foreach (var cParam in cParameters)
                {
                    if (defParameters.TryGetValue(cParam.Name, out var defParam))
                    {
                        Argument defArgument = defParam;
                        if (!string.IsNullOrWhiteSpace(defParam.ArgumentName))
                            settings.Arguments.TryGet(defParam.ArgumentName, out defArgument);

                        switch (defArgument.Type)
                        {
                            case ArgumentType.Abstract:
                            case ArgumentType.Instance:
                            case ArgumentType.Interface:
                                paramExpressions.Add(
                                    Expression.Convert(
                                        Expression.Call(
                                            Expression.Property(null, typeof(Core), "Injector"),
                                            "New",
                                            null,
                                            Expression.Constant(Core.GetType(defArgument.Value), typeof(Type)),
                                            Expression.Constant(defArgument.ClassName, typeof(string))),
                                        cParam.ParameterType)
                                );

                                break;
                            case ArgumentType.Raw:
                                var rawValue = defArgument.Value.ParseTo(cParam.ParameterType, cParam.ParameterType.IsValueType ? System.Activator.CreateInstance(cParam.ParameterType) : null);
                                paramExpressions.Add(Expression.Constant(rawValue, cParam.ParameterType));
                                break;
                            case ArgumentType.Settings:
                                if (Core.Settings.TryGet(defArgument.Value, out var setKeyValue))
                                {
                                    var setValue = setKeyValue.Value.ParseTo(cParam.ParameterType, cParam.ParameterType.IsValueType ? System.Activator.CreateInstance(cParam.ParameterType) : null);
                                    paramExpressions.Add(Expression.Constant(setValue, cParam.ParameterType));
                                }
                                else
                                    paramExpressions.Add(Expression.Constant(cParam.ParameterType.IsValueType ? System.Activator.CreateInstance(cParam.ParameterType) : null, cParam.ParameterType));
                                break;
                        }
                    }
                    else if (cParam.HasDefaultValue)
                    {
                        paramExpressions.Add(Expression.Constant(cParam.RawDefaultValue, cParam.ParameterType));
                    }
                }
                if (paramExpressions.Count == 0)
                    serExpressions.Add(Expression.Assign(value, Expression.New(selectedCtor)));
                else
                    serExpressions.Add(Expression.Assign(value, Expression.New(selectedCtor, paramExpressions)));
                #endregion

                #region Properties Set
                if (definition.PropertiesSets?.Any() == true)
                {
                    foreach (var set in definition.PropertiesSets)
                    {
                        Argument argSet = set;
                        if (!string.IsNullOrWhiteSpace(set.ArgumentName))
                            settings.Arguments.TryGet(set.ArgumentName, out argSet);

                        var property = defType.GetProperty(set.Name, BindingFlags.Instance | BindingFlags.Public);
                        if (property == null)
                            throw new Exception($"The Property '{set.Name}' can't be found in the object type '{definition.Type}'");

                        var propertyExpression = Expression.Property(value, property);

                        switch (argSet.Type)
                        {
                            case ArgumentType.Abstract:
                            case ArgumentType.Instance:
                            case ArgumentType.Interface:
                                serExpressions.Add(
                                    Expression.Assign(propertyExpression,
                                        Expression.Convert(
                                            Expression.Call(
                                                Expression.Property(null, typeof(Core), "Injector"),
                                                "New",
                                                null,
                                                Expression.Constant(Core.GetType(argSet.Value), typeof(Type)),
                                                Expression.Constant(argSet.ClassName, typeof(string))),
                                            property.PropertyType)
                                   )
                                );
                                break;
                            case ArgumentType.Raw:
                                var rawValue = argSet.Value.ParseTo(property.PropertyType, property.PropertyType.IsValueType ? System.Activator.CreateInstance(property.PropertyType) : null);
                                serExpressions.Add(Expression.Assign(propertyExpression, Expression.Constant(rawValue, property.PropertyType)));
                                break;
                            case ArgumentType.Settings:
                                if (Core.Settings.TryGet(argSet.Value, out var setKeyValue))
                                {
                                    var setValue = setKeyValue.Value.ParseTo(property.PropertyType, property.PropertyType.IsValueType ? System.Activator.CreateInstance(property.PropertyType) : null);
                                    serExpressions.Add(Expression.Assign(propertyExpression, Expression.Constant(setValue, property.PropertyType)));
                                }
                                else
                                    serExpressions.Add(Expression.Assign(propertyExpression, Expression.Constant(property.PropertyType.IsValueType ? System.Activator.CreateInstance(property.PropertyType) : null, property.PropertyType)));
                                break;
                        }
                    }
                }
                #endregion

                serExpressions.Add(Expression.Return(returnTarget, value, typeof(object)));
                serExpressions.Add(Expression.Label(returnTarget, value));
                var block = Expression.Block(varExpressions, serExpressions).Reduce();
                var lambda = Expression.Lambda<Func<object>>(block, type.Name + "_Activator", null);
                ActivatorExpression = lambda;
                Activator = lambda.Compile();
                #endregion

                if (Singleton)
                    SingletonValue = Activator();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public InjectorTypeNameInfo(Type type, Type instantiableType, bool singleton, string name, object[] parameters)
            {

            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public InjectorTypeNameInfo(Type type, Func<object> activator, bool singleton, string name)
            {

            }
        }
    }
}
