using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace Unity.Muse.Editor.Markup.Renderers
{
    internal class LinkInlineRenderer : MarkdownObjectRenderer<ChatMarkdownRenderer, LinkInline>
    {
        protected override void Write(ChatMarkdownRenderer renderer, LinkInline obj)
        {
            renderer.AppendText($"<a href=\"{obj.Url}\">");
            renderer.WriteChildren(obj);
            renderer.AppendText("</a>");
        }
    }
}
