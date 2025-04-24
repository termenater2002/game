using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Unity.Muse.Chat
{
    internal class FixMissingImports : CSharpFixProvider
    {
        static readonly string[] k_DiagnosticIds = { "CS0246", "CS1061" };

        private readonly Dictionary<string, string[]> k_NamespaceKeywords = new ()
        {
            { "System.Linq", new[] { "Where", "Select", "OrderBy", "Concat", "Any" } },
            { "System.Collections.Generic", new[] { "List<>", "Dictionary<>" } },
            { "Unity.AI.Navigation", new[] { "NavMeshSurface" } },
        };

        public override bool CanFix(Diagnostic diagnostic)
        {
            if (!k_DiagnosticIds.Contains(diagnostic.Id))
                return false;

            var message = diagnostic.GetMessage();
            foreach (var keywords in k_NamespaceKeywords.Values)
            {
                if (keywords.Any(keyword => message.Contains($"'{keyword}'")))
                    return true;
            }

            return false;
        }

        public override SyntaxTree ApplyFix(SyntaxTree tree, Diagnostic diagnostic)
        {
            foreach (var namespaceKeywords in k_NamespaceKeywords)
            {
                var keywords = namespaceKeywords.Value;
                if (keywords.Any(keyword => diagnostic.GetMessage().Contains($"'{keyword}'")))
                {
                    return tree.AddUsingDirective(namespaceKeywords.Key);
                }
            }

            return tree;
        }
    }
}
