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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TWCore.Text;

namespace TWCore.Settings
{
    internal static class SettingsEngine
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GetSettingObject(Type type)
        {
            var instance = Activator.CreateInstance(type);
            ApplySettings(instance);
            return instance;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetSettingObject<T>()
        {
            var instance = Activator.CreateInstance<T>();
            ApplySettings(instance);
            return instance;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ApplySettings(object instance)
        {
            var type = instance.GetType();
            var containerAttribute = instance.GetAttribute<SettingsContainerAttribute>();
            var container = containerAttribute?.Name ?? type.Name;
            string shortContainer = null;
            if (container.EndsWith("Settings"))
                shortContainer = container.Substring(0, container.LastIndexOf("Settings", StringComparison.Ordinal));
            var props = type.GetProperties().Where(p => p.CanRead && p.CanWrite && !p.IsSpecialName);
            var provider = System.Globalization.CultureInfo.InvariantCulture;
            props.Each(p =>
            {
                var sKeys = p.GetAttribute<SettingsKeyAttribute>();
                var attribute = p.GetAttribute<SettingsDataFormatAttribute>();
                var arrayAttribute = p.GetAttribute<SettingsArrayAttribute>();
                var dictionaryAttribute = p.GetAttribute<SettingsDictionaryAttribute>();

                string dataFormat = attribute?.DataFormat;
                string settingValue;
                if (sKeys != null)
                {
                    settingValue = sKeys.UseContainerName ?
                                    Core.Settings[string.Format("{0}.{1}", container, sKeys.SettingsKey)] ?? (shortContainer.IsNotNullOrWhitespace() ? Core.Settings[string.Format("{0}.{1}", shortContainer, sKeys.SettingsKey)] : null) :
                                    Core.Settings[sKeys.SettingsKey];
                }
                else
                    settingValue = Core.Settings[string.Format("{0}.{1}", container, p.Name)] ?? (shortContainer.IsNotNullOrWhitespace() ? Core.Settings[string.Format("{0}.{1}", shortContainer, p.Name)] : null);

                if (arrayAttribute == null && dictionaryAttribute == null)
                {
                    //No Array
                    var settingValueObject = Try.Do(() => StringParser.Parse(settingValue, p.PropertyType, p.GetValue(instance), dataFormat, provider), false);
                    p.SetValue(instance, settingValueObject);
                }
                else if (dictionaryAttribute?.ItemSeparator != null && p.PropertyType.GetTypeInfo().ImplementedInterfaces.Any(i => i == typeof(IDictionary) || (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>))))
                {
                    var settingsValueItems = settingValue?.SplitAndTrim(dictionaryAttribute.ItemSeparator);

                    Type innerKeyType = typeof(object);
                    Type innerValueType = typeof(object);

                    var gargs = p.PropertyType.GenericTypeArguments;
                    if (gargs.Length > 0)
                        innerKeyType = gargs[0];
                    if (gargs.Length > 1)
                        innerValueType = gargs[1];

                    if (settingsValueItems != null)
                    {
                        var mDictionary = (IDictionary)Activator.CreateInstance(p.PropertyType);
                        foreach (var item in settingsValueItems)
                        {
                            var kvArray = item.SplitAndTrim(dictionaryAttribute.KeyValueSeparator).ToArray();
                            if (kvArray.Length != 2)
                                continue;
                            var key = Try.Do(() => StringParser.Parse(kvArray[0], innerKeyType, innerKeyType.GetTypeInfo().IsValueType ? Activator.CreateInstance(innerKeyType) : null, null, provider), false);
                            var value = Try.Do(() => StringParser.Parse(kvArray[1], innerValueType, innerValueType.GetTypeInfo().IsValueType ? Activator.CreateInstance(innerValueType) : null, null, provider), false);
                            mDictionary.Add(key, value);
                        }
                        p.SetValue(instance, mDictionary);
                    }
                    else
                        p.SetValue(instance, p.GetValue(instance));
                }
                else if (arrayAttribute?.Separators != null && p.PropertyType.IsArray)
                {
                    //Array
                    var settingsValueArray = settingValue?.SplitAndTrim(arrayAttribute.Separators);
                    var eType = p.PropertyType.GetElementType();
                    var defaultValue = eType.GetTypeInfo().IsValueType ? Activator.CreateInstance(eType) : null;
                    bool isDefaultValue = true;


                    var valueArray = settingsValueArray?.Select(val => Try.Do(() => StringParser.Parse(val, eType, defaultValue, dataFormat, provider), false)).ToArray();
                    Array myValueArray = null;

                    if (valueArray != null)
                    {
                        myValueArray = (Array)Activator.CreateInstance(eType.MakeArrayType(), valueArray.Length);
                        for (var i = 0; i < valueArray.Length; i++)
                        {
                            isDefaultValue &= valueArray[i] == defaultValue;
                            myValueArray.SetValue(valueArray[i], i);
                        }
                    }

                    p.SetValue(instance, isDefaultValue ? p.GetValue(instance) : myValueArray);
                }
            });
        }
    }
}
