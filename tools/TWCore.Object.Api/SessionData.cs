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
    }
}
