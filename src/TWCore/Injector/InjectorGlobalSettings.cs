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

using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable SuggestBaseTypeForParameter

namespace TWCore.Injector
{
    /// <summary>
    /// Injector global settings
    /// </summary>
    [XmlRoot("InjectorGlobalSettings"), DataContract]
    public class InjectorGlobalSettings
    {
        /// <summary>
        /// Global injector settings
        /// </summary>
        [XmlElement("Global"), DataMember]
        public InjectorSettings Global { get; set; }
        /// <summary>
        /// Injector settings by environment and application name
        /// </summary>
        [XmlElement("InjectorSettings"), DataMember]
        public KeyStringDelegatedCollection<InjectorSettingsByApp> Settings { get; set; } = new KeyStringDelegatedCollection<InjectorSettingsByApp>(k => k.EnvironmentName + k.ApplicationName);

        /// <summary>
        /// Get the injector settings for the environment and application
        /// </summary>
        /// <param name="environment">Environment</param>
        /// <param name="applicationName">Application name</param>
        /// <returns>Injector settings</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InjectorSettings GetSettings(string environment, string applicationName)
        {
            var appNames = applicationName?.SplitAndTrim(",") ?? new string[0];
            var res = new InjectorSettings();

            foreach (var appName in appNames)
            {
                var set1 = Settings?.FirstOrDefault((i, vTuple) => 
                    i.EnvironmentName?.SplitAndTrim(",").Contains(vTuple.environment) == true && i.ApplicationName?.SplitAndTrim(",").Contains(vTuple.appName) == true, (environment, appName));
                var set2 = Settings?.FirstOrDefault((i, aName) 
                    => i.EnvironmentName.IsNullOrWhitespace() && i.ApplicationName?.SplitAndTrim(",").Contains(aName) == true, appName);
                var set3 = Settings?.FirstOrDefault((i, env) 
                    => i.EnvironmentName?.SplitAndTrim(",").Contains(env) == true && i.ApplicationName.IsNullOrWhitespace(), environment);
                var set4 = Global;

                res.Arguments.AddRange(set1?.Arguments);
                res.Arguments.AddRange(set2?.Arguments);
                res.Arguments.AddRange(set3?.Arguments);
                res.Arguments.AddRange(set4?.Arguments);

                res.InstantiableClasses.AddRange(set1?.InstantiableClasses);
                res.InstantiableClasses.AddRange(set2?.InstantiableClasses);
                res.InstantiableClasses.AddRange(set3?.InstantiableClasses);
                res.InstantiableClasses.AddRange(set4?.InstantiableClasses);

                res.Interfaces.AddRange(set1?.Interfaces);
                NonIntantiableClassAppend(set2?.Interfaces, res.Interfaces);
                NonIntantiableClassAppend(set3?.Interfaces, res.Interfaces);
                NonIntantiableClassAppend(set4?.Interfaces, res.Interfaces);

                res.Abstracts.AddRange(set1?.Abstracts);
                NonIntantiableClassAppend(set2?.Abstracts, res.Abstracts);
                NonIntantiableClassAppend(set3?.Abstracts, res.Abstracts);
                NonIntantiableClassAppend(set4?.Abstracts, res.Abstracts);
            }
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void NonIntantiableClassAppend(KeyStringDelegatedCollection<NonInstantiable> nonNew, KeyStringDelegatedCollection<NonInstantiable> nonBase)
        {
            if (nonNew is null) return;
            foreach (var item in nonNew)
            {
                if (!nonBase.Contains(item.Type))
                    nonBase.Add(item);
                else
                    nonBase[item.Type].ClassDefinitions.AddRange(item.ClassDefinitions);
            }
        }
    }
}
