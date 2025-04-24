using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace Unity.Muse.Editor.Markup.Renderers
{
    internal class HtmlEntityInlineRenderer : MarkdownObjectRenderer<ChatMarkdownRenderer, HtmlEntityInline>
    {
        protected override void Write(ChatMarkdownRenderer renderer, HtmlEntityInline obj)
        {
            renderer.AppendText(obj.Original.ToString());
        }
    }
}
