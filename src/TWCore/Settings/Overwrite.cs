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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace TWCore.Settings
{
    /// <inheritdoc />
    /// <summary>
    /// Overwrites
    /// </summary>
    [DataContract]
    public class Overwrite : INameItem
    {
        private string _name;
        private string _environmentName;
        private string _machineName;
        private bool _isDirty = true;

        /// <inheritdoc />
        /// <summary>
        /// Environment name
        /// </summary>
        [XmlAttribute, DataMember]
        public string Name
        {
            get
            {
                if (!_isDirty) return _name;

                var environments = EnvironmentName?.SplitAndTrim(",");
                var machines = MachineName?.SplitAndTrim(",");
                var result = new List<string>();
                if (environments != null && environments.Length > 0)
                {
                    var hasMachines = machines != null && machines.Length > 0;
                    foreach (var env in environments)
                    {
                        if (hasMachines)
                            result.AddRange(machines.Select(machine => env + ">" + machine));
                        else
                            result.Add(env);
                    }
                }
                else if (machines != null)
                {
                    result.AddRange(machines.Select(machine => ">" + machine));
                }
                _name = string.Join(",", result);
                _isDirty = false;
                return _name;
            }
            set { }
        }

        /// <summary>
        /// Environment Name
        /// </summary>
        [XmlAttribute, DataMember]
        public string EnvironmentName
        {
            get => _environmentName;
            set
            {
                _environmentName = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Machine Name
        /// </summary>
        [XmlAttribute, DataMember]
        public string MachineName
        {
            get => _machineName;
            set
            {
                _machineName = value;
                _isDirty = true;
            }
        }
        /// <summary>
        /// Items overwrites
        /// </summary>
        [XmlElement("Item"), DataMember]
        public KeyValueCollection Items { get; set; } = new KeyValueCollection();
    }
}
