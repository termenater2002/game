using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using UnityEngine;

namespace Unity.Muse.Chat
{
    class DynamicAssemblyBuilder
    {
        const string k_CompilationSuccessfulMessage = "Compilation successful";
        static readonly string[] k_CuratedAssemblyPrefixes = { "Assembly-CSharp", "UnityEngine", "UnityEditor", "Unity.", "netstandard" };
        static Dictionary<string, ReportDiagnostic> k_SupressedDiagnosticOptions = new()
        {
            { "CS0168", ReportDiagnostic.Suppress }, // The variable is declared but never used
            { "CS8321", ReportDiagnostic.Suppress }, // The local function is declared but never used
            { "CS0219", ReportDiagnostic.Suppress } // The variable is assigned but its value is never used
        };

        List<MetadataReference> m_References = new();
        List<CSharpFixProvider> m_FixProviders = new()
        {
            new FixMissingImports(),
            new FixMissingParenthesis(),
            new FixMissingBrace(),
            new FixMissingSquareBracket(),
            new FixMissingSemicolon(),
            new FixAmbiguousReference()
        };

        string m_AssemblyName;
        string m_DefaultNamespace;

        public DynamicAssemblyBuilder(string assemblyName, string defaultNamespace = null)
        {
            m_AssemblyName = assemblyName;
            m_DefaultNamespace = defaultNamespace;

            InitializeReferences();
        }

        public void AddReferences(List<string> additionalReferences)
        {
            foreach (var referencePath in additionalReferences)
            {
                m_References.Add(MetadataReference.CreateFromFile(referencePath));
            }
        }

        void InitializeReferences()
        {
            m_References.Clear();

            m_References.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location)); // mscorlib
            m_References.Add(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)); // System.Core
            m_References.Add(MetadataReference.CreateFromFile(typeof(Application).Assembly.Location)); // UnityEngine.CoreModule
            m_References.Add(MetadataReference.CreateFromFile(typeof(UnityEditor.Editor).Assembly.Location)); // UnityEditor.CoreModule

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
                {
                    if (k_CuratedAssemblyPrefixes.Any(prefix => assembly.FullName.StartsWith(prefix)))
                        m_References.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }
        }

        public Assembly CompileAndLoadAssembly(string code, out string compilationLogs, out string updatedCode)
        {
            var compilation = Compile(code, out var tree);
            var diagnostics = compilation.GetDiagnostics();

            var updatedTree = tree;
            // Try to repair the tree if errors are detected
            bool hasError = diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
            if (hasError)
            {
                foreach (var diagnostic in diagnostics)
                {
                    if (diagnostic.Severity != DiagnosticSeverity.Error)
                        continue;

                    foreach (var fix in m_FixProviders)
                    {
                        if (fix.CanFix(diagnostic))
                        {
                            updatedTree = fix.ApplyFix(updatedTree, diagnostic);

                            InternalLog.Log($"{fix.GetType().Name} was applied:\n{updatedTree.GetText()}");
                        }
                    }
                }

                if (updatedTree != tree)
                {
                    compilation = compilation.ReplaceSyntaxTree(tree, updatedTree);
                }
            }

            updatedCode = updatedTree.GetText().ToString();

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);
            if (!result.Success)
            {
                compilationLogs = GetCompilationLogs(result);
                return null;
            }

            ms.Seek(0, SeekOrigin.Begin);

            compilationLogs = k_CompilationSuccessfulMessage;
            return Assembly.Load(ms.ToArray());
        }

        public CSharpCompilation Compile(string code, out SyntaxTree tree)
        {
            tree = SyntaxFactory.ParseSyntaxTree(code);
            if (m_DefaultNamespace != null)
                tree = tree.MoveToNamespace(m_DefaultNamespace);

            var compilation = CSharpCompilation.Create(m_AssemblyName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(m_References)
                .AddSyntaxTrees(tree);

            return compilation;
        }

        static string GetCompilationLogs(EmitResult result)
        {
            var diagnosticLogs = new StringBuilder();

            foreach (var diagnostic in result.Diagnostics)
            {
                if (diagnostic.Severity != DiagnosticSeverity.Error)
                    continue;
                var location = diagnostic.Location;
                if (location.IsInSource)
                {
                    var lineSpan = location.GetLineSpan();
                    diagnosticLogs.AppendLine($"- Error {diagnostic.Id}: {diagnostic.GetMessage()} (Line: {lineSpan.StartLinePosition.Line + 1}, Column: {lineSpan.StartLinePosition.Character + 1})");
                }
                else
                {
                    diagnosticLogs.AppendLine($"- Error {diagnostic.Id}: {diagnostic.GetMessage()}");
                }
            }

            return diagnosticLogs.ToString();
        }
    }
}
