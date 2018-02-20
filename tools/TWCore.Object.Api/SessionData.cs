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

namespace TWCore.Object.Api
{
    public class SessionData
    {
        public string FilePath { get; set; }
        public object FileObject { get; set; }
        public string SourceCode { get; set; } = @"using TWCore;
using TWCore.Object.Compiler;
using TWCore.Serialization;
using System;

namespace Runtime
{
    public class RuntimeCode : IRuntimeCode
    {
        public object Execute(object value)
        {
            return value is SerializedObject obj ? obj.GetValue() : value;
        }
    }
}
";
        public object CompiledResult { get; set; }
    }
}
