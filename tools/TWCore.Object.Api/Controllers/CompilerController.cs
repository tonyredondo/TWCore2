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

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Object.Compiler;
using TWCore.Object.Descriptor;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable TooWideLocalVariableScope

namespace TWCore.Object.Api.Controllers
{
    public class CompilerController : Controller
    {
        [HttpGet("api/code/get.{format}")]
        [HttpGet("api/code/get")]
        [FormatFilter]
        public string GetSource()
        {
            var sessionData = HttpContext.Session.GetSessionData();
            return sessionData.SourceCode;
        }

        [HttpPost("api/code/set.{format}")]
        [HttpPost("api/code/set")]
        [FormatFilter]
        public string SetSource([FromBody] string code)
        {
            var sessionData = HttpContext.Session.GetSessionData();
            sessionData.SourceCode = code;
            HttpContext.Session.SetSessionData(sessionData);
            return code;
        }

        [HttpGet("api/code/compile.{format}")]
        [HttpGet("api/code/compile")]
        [FormatFilter]
        public ActionResult Compile()
        {
            var sessionData = HttpContext.Session.GetSessionData();
            try
            {
                sessionData.CompiledResult = CodeCompiler.Run(sessionData.SourceCode, sessionData.FileObject);
                var descriptor = new Descriptor.Descriptor();
                return new ObjectResult(GetTreeListData(descriptor.GetDescription(sessionData.CompiledResult)));
            }
            catch(Exception ex)
            {
                Core.Log.Write(ex);
                return new ObjectResult(new SerializableException(ex));
            }
        }

        [HttpGet("api/code/results.{format}")]
        [HttpGet("api/code/results")]
        [FormatFilter]
        public TreeListCollection GetDescription()
        {
            var sessionData = HttpContext.Session.GetSessionData();
            var descriptor = new Descriptor.Descriptor();
            return GetTreeListData(descriptor.GetDescription(sessionData.CompiledResult));
        }


        public TreeListCollection GetTreeListData(ObjectDescription description)
        {
            var collection = new TreeListCollection();
            if (description == null) return collection;
            if (description.Value == null)
            {
                collection.Add(new TreeListItem {Id = 0, Name = "Value", Value = "(null)", Type = "Unknown", Member = "object" });
                return collection;
            }
            var id = 0;
            FillCollection("Value", MemberType.Field, description.Value, null);
            return collection;

            void FillCollection(string name, MemberType type, Value value, int? parentId)
            {
                id++;
                if (id > 100000)
                    return;
                collection.Add(new TreeListItem { Id = id, Name = name, Value = value?.ValueString, Type = value?.ValueType, Member = type.ToString(), ParentId = parentId });
                if (!(value?.Members?.Length > 0)) return;
                var idParent = id;
                foreach (var member in value.Members)
                    if (member != null)
                        FillCollection(member.Name, member.Type, member.Value, idParent);
            }
        }

        #region Nested Types
        [DataContract]
        public class TreeListCollection : List<TreeListItem>
        { }
        [DataContract]
        public class TreeListItem
        {
            [XmlAttribute, DataMember]
            public int Id { get; set; }
            [XmlAttribute, DataMember]
            public string Name { get; set; }
            [XmlAttribute, DataMember]
            public string Value { get; set; }
            [XmlAttribute, DataMember]
            public string Type { get; set; }
            [XmlAttribute, DataMember]
            public string Member { get; set; }
            [DataMember]
            public int? ParentId { get; set; }
        }
        #endregion
    }
}