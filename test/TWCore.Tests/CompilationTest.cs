using System;
using TWCore.Object.Compiler;
using TWCore.Services;
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    /// <inheritdoc />
    public class CompilationTest : ContainerParameterService
    {
        public CompilationTest() : base("compilertest", "Compiler Test") { }

        private const string TestCode = @"
using TWCore;
using TWCore.Object.Compiler;
using System;

namespace Runtime 
{
    public class RuntimeCode : IRuntimeCode 
    {
        public object Execute(object value)
        {
            Core.Log.Warning(""This is the dynamic code!!!!"");
            return null;
        }
    }
}
";
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            CodeCompiler.Run(TestCode, null);
        }
    }
}