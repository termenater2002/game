using Microsoft.CodeAnalysis;

namespace Unity.Muse.Chat
{
    internal abstract class CSharpFixProvider
    {
        public abstract bool CanFix(Diagnostic diagnostic);
        public abstract SyntaxTree ApplyFix(SyntaxTree tree, Diagnostic diagnostic);
    }
}
