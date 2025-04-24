using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace Unity.Muse.Editor.Markup.Renderers
{
    internal class HtmlInlineRenderer : MarkdownObjectRenderer<ChatMarkdownRenderer, HtmlInline>
    {
        protected override void Write(ChatMarkdownRenderer renderer, HtmlInline obj)
        {
            renderer.AppendText(obj.Tag);
        }
    }
}
