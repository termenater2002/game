using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace Unity.Muse.Editor.Markup.Renderers
{
    internal class LineBreakInlineRenderer : MarkdownObjectRenderer<ChatMarkdownRenderer, LineBreakInline>
    {
        protected override void Write(ChatMarkdownRenderer renderer, LineBreakInline obj)
        {
        }
    }
}
