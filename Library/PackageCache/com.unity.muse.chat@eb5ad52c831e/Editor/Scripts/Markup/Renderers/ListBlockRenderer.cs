using Markdig.Renderers;
using Markdig.Syntax;

namespace Unity.Muse.Editor.Markup.Renderers
{
    internal class ListBlockRenderer : MarkdownObjectRenderer<ChatMarkdownRenderer, ListBlock>
    {
        protected override void Write(ChatMarkdownRenderer renderer, ListBlock obj)
        {
            var previous = renderer.SetCurrentList(obj);
            renderer.WriteChildren(obj);
            renderer.SetCurrentList(previous);
        }
    }
}
