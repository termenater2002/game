using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Unity.Muse.Chat
{
    internal class FixMissingParenthesis : CSharpFixProvider
    {
        static readonly string[] k_DiagnosticIds = { "CS1026" };

        public override bool CanFix(Diagnostic diagnostic)
        {
            return k_DiagnosticIds.Contains(diagnostic.Id);
        }

        public override SyntaxTree ApplyFix(SyntaxTree tree, Diagnostic diagnostic)
        {
            return tree.InsertAtLocation(diagnostic.Location, ")");
        }
    }
}
