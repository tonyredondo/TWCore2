using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TWCore.Serialization;

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
