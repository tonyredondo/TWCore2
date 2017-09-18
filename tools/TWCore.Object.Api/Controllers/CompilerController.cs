using System;
using Microsoft.AspNetCore.Mvc;
using TWCore.Object.Compiler;
using TWCore.Object.Descriptor;

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
                return new ObjectResult(descriptor.GetDescription(sessionData.CompiledResult));
            }
            catch(Exception ex)
            {
                return new ObjectResult(new SerializableException(ex));
            }
        }

        [HttpGet("api/code/results.{format}")]
        [HttpGet("api/code/results")]
        [FormatFilter]
        public ObjectDescription GetDescription()
        {
            var sessionData = HttpContext.Session.GetSessionData();
            var descriptor = new Descriptor.Descriptor();
            return descriptor.GetDescription(sessionData.CompiledResult);
        }
    }
}