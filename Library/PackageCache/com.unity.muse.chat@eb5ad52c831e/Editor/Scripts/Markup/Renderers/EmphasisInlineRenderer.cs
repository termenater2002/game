using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace Unity.Muse.Editor.Markup.Renderers
{
    internal class EmphasisInlineRenderer : MarkdownObjectRenderer<ChatMarkdownRenderer, EmphasisInline>
    {
        protected override void Write(ChatMarkdownRenderer renderer, EmphasisInline obj)
        {
            renderer.AppendText($"<b>");
            renderer.WriteChildren(obj);
            renderer.AppendText($"</b>");
        }
    }
}
