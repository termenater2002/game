using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace Unity.Muse.Editor.Markup.Renderers
{
    internal class CodeInlineBlockRenderer : MarkdownObjectRenderer<ChatMarkdownRenderer, CodeInline>
    {
        protected override void Write(ChatMarkdownRenderer renderer, CodeInline obj)
        {
            var codeWithoutEscapes = obj.Content.Replace(@"\", @"\\");

            //TODO figure out if we want separate element to unify styling or keep mixing rich text tags and uss
            //Note: "<noparse>" ensures that quoted code containing tags is not interpreted as actual rich text tags
            renderer.AppendText($"<color={renderer.m_CodeColor}><noparse>{codeWithoutEscapes}</noparse></color>");
        }
    }

}
