using Markdig.Renderers;
using Markdig.Syntax;

namespace Unity.Muse.Editor.Markup.Renderers
{
    internal class ListItemBlockRenderer : MarkdownObjectRenderer<ChatMarkdownRenderer, ListItemBlock>
    {
        protected override void Write(ChatMarkdownRenderer renderer, ListItemBlock obj)
        {
            renderer.AppendText(renderer.GetCurrentListBullet(obj));
            renderer.WriteChildren(obj);
        }
    }
}
