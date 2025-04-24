using Markdig.Renderers;
using Markdig.Syntax;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Editor.Markup.Renderers
{
    internal class ParagraphRenderer : MarkdownObjectRenderer<ChatMarkdownRenderer, ParagraphBlock>
    {
        protected override void Write(ChatMarkdownRenderer renderer, ParagraphBlock obj)
        {
            renderer.m_VisitingState = ChatMarkdownRenderer.VisitingState.Text;
            if(obj.Inline != null)
                renderer.WriteChildren(obj.Inline);
            string text = renderer.ClearText();
            // case with multiple contiguous source blocks - because of the newline in the response text, it looks like multiple markdown paragraphs when it should be one
            if (text.StartsWith(" <sprite") && renderer.m_PreviousLastElement != null)
            {
                var textElement = renderer.m_PreviousLastElement as TextElement;

                if (textElement != null)
                    textElement.text += text;
                else
                    Debug.LogError($"Expected a Text element, not {renderer.m_PreviousLastElement.GetType()}. Cannot append text to the previous element.");
            }
            else
            {
                var textElement = new TextElement { text = text };
                textElement.selection.isSelectable = true;

                textElement.style.marginBottom = new Length(12);

                // Use actual indentation for lists (not tabs or spaces)
                if (renderer.k_OptionUseListIndentation && renderer.m_CurrentListBlock != null)
                {
                    textElement.style.marginLeft = new Length(14);
                }

                renderer.m_OutputTextElements.Add(textElement);
            }
        }
    }
}
