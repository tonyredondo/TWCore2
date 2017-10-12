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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TWCore.Collections;

// ReSharper disable ReturnTypeCanBeEnumerable.Local
// ReSharper disable UnusedMember.Global

namespace TWCore.Object.Compiler
{
    /// <summary>
    /// CSharp Code compiler class
    /// </summary>
    public static class CodeCompiler
    {
        private static readonly LRU2QCollection<string, IRuntimeCode> CachedCompilations = new LRU2QCollection<string, IRuntimeCode>(1024);
        private static MetadataReference[] _references;

        /// <summary>
        /// Get Metadata references
        /// </summary>
        private static MetadataReference[] References
        {
            get
            {
                if (_references != null) return _references;
                var locationReferences = new HashSet<string>
                {
                    typeof(object).Assembly.Location,
                    typeof(Enumerable).Assembly.Location,
                    typeof(CodeCompiler).Assembly.Location,
                    typeof(Core).Assembly.Location,
                    typeof(System.Diagnostics.Process).Assembly.Location,
                    typeof(IDisposable).Assembly.Location,
                    typeof(Type).Assembly.Location,
                    typeof(Console).Assembly.Location
                };
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(asm => !asm.IsDynamic && !string.IsNullOrWhiteSpace(asm.Location));
                foreach (var asm in assemblies)
                    locationReferences.Add(asm.Location);
                _references = locationReferences.Select(location => Try.Do(() => (MetadataReference)MetadataReference.CreateFromFile(location))).RemoveNulls().ToArray();
                return _references;
            }
        }


        /// <summary>
        /// Run CSharp code and execute it using the IRuntimeCode interface
        /// </summary>
        /// <param name="code">CSharp code</param>
        /// <param name="value">Object value instance to inject to the IRuntimeCode interface</param>
        /// <returns>IRuntimeCode response</returns>
        public static object Run(string code, object value)
        {
            var instance = CachedCompilations.GetOrAdd(code, mCode =>
            {
                Core.Log.InfoBasic("Compiling code...");
                var syntax = CSharpSyntaxTree.ParseText(code);
                var assemblyName = Path.GetRandomFileName();
                var csCompilation = CSharpCompilation.Create(assemblyName,
                    syntaxTrees: new[] { syntax },
                    references: References,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using (var compiledCode = new MemoryStream())
                using (var compiledPdb = new MemoryStream())
                {
                    var compilerResult = csCompilation.Emit(compiledCode, compiledPdb);

                    if (!compilerResult.Success)
                    {
                        Core.Log.Warning("Error when compiling the code.");
                        var sbErrors = new StringBuilder();
                        sbErrors.AppendLine("Compilation error:");
                        var failures = compilerResult.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
                        foreach (var diagnostic in failures)
                            sbErrors.AppendLine(diagnostic.Id + " (" + diagnostic.Severity + ") : " + diagnostic.GetMessage());
                        throw new Exception(sbErrors.ToString());
                    }

                    Core.Log.InfoBasic("Compilation Success.");
                    compiledCode.Position = 0;
                    compiledPdb.Position = 0;

                    var assembly = Assembly.Load(compiledCode.ToArray(), compiledPdb.ToArray());


                    var runType = assembly.GetTypes().FirstOrDefault(type => type.GetInterface(typeof(IRuntimeCode).FullName) != null);
                    if (runType == null)
                        throw new Exception("An implementation of the IRuntimeCode interface can't be found.");

                    var runCode = (IRuntimeCode)Activator.CreateInstance(runType);
                    return runCode;
                    
                }
            });
            return instance.Execute(value);
        }
    }
}
