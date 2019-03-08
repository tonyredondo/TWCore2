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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Diagnostics.Status;

namespace TWCore.Diagnostics.Api.Models.Status
{
    [DataContract]
	public class NodeStatusItem : NodeInfo
    {
        [XmlAttribute, DataMember]
        public string ApplicationDisplayName { get; set; }
        [XmlAttribute, DataMember]
        public double ElapsedMilliseconds { get; set; }
        [XmlAttribute, DataMember]
        public DateTime Date { get; set; }
        [XmlAttribute, DataMember]
        public DateTime StartTime { get; set; }
        [XmlElement("Value"), DataMember]
        public List<NodeStatusItemValue> Values { get; set; }


        public static NodeStatusItem Create(StatusItemCollection collection)
        {
            if (collection is null) return null;
            var newStatus = new NodeStatusItem
            {
                Environment = collection.EnvironmentName,
                Machine = collection.MachineName,
                Application = collection.ApplicationName,
                InstanceId = collection.InstanceId,
                ApplicationDisplayName = collection.ApplicationDisplayName,
                ElapsedMilliseconds = collection.ElapsedMilliseconds,
                Date = collection.Timestamp.Date,
                StartTime = collection.StartTime,
                Timestamp = collection.Timestamp,
            };
            newStatus.FillValues(collection);
            return newStatus;
        }

        public void FillValues(StatusItemCollection collection)
        {
            if (collection is null) return;
            if (Values is null)
                Values = new List<NodeStatusItemValue>();
            else
                Values.Clear();

            InnerFillValues(null, collection.Items);

            void InnerFillValues(string keyPrefix, List<StatusItem> children)
            {
                if (children is null) return;
                foreach (var item in children)
                {
                    var key = string.IsNullOrEmpty(keyPrefix) ? item.Name : keyPrefix + "\\" + item.Name;
                    if (item.Values != null)
                    {
                        foreach (var itemValue in item.Values)
                        {
                            var vKey = key + "\\" + itemValue.Key;

                            if (itemValue.RawValue != null)
                            {
                                Values.Add(new NodeStatusItemValue
                                {
                                    Key = vKey,
                                    Type = itemValue.Type,
                                    Value = itemValue.RawValue
                                });
                            }
                            if (itemValue.Values != null && itemValue.Values.Length > 0)
                            {
                                foreach(var itemValueValue in itemValue.Values)
                                {
                                    var vvKey = vKey + "\\" + itemValueValue.Name;
                                    Values.Add(new NodeStatusItemValue
                                    {
                                        Key = vvKey,
                                        Type = itemValueValue.Type,
                                        Value = itemValueValue.RawValue
                                    });
                                }
                            }
                        }
                    }
                    if (item.Children != null)
                        InnerFillValues(key, item.Children);
                }
            }
        }
    }
}