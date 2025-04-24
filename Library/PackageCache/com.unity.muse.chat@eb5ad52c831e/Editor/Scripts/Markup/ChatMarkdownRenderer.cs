using System.Collections.Generic;
using System.Text;
using Markdig.Renderers;
using Markdig.Syntax;
using UnityEditor;
using UnityEngine;
using Unity.Muse.Chat;
using Unity.Muse.Chat.Commands;
using Unity.Muse.Chat.WebApi;
using Unity.Muse.Editor.Markup.Renderers;
using UnityEngine.UIElements;

namespace Unity.Muse.Editor.Markup
{
    /// <summary>
    /// Helper class to parse text (chat responses) and reformat blocks of text to improve readability
    /// of for example quoted text, code blocks, and links.
    /// </summary>
    internal class ChatMarkdownRenderer : RendererBase
    {
        internal enum VisitingState
        {
            Text,
            Code,
            SourceContent,
            SourceBoundaryEnd,
        }

        internal VisualElement m_PreviousLastElement;
        internal readonly IList<VisualElement> m_OutputTextElements;
        internal VisitingState m_VisitingState;
        private StringBuilder m_Builder = new();
        private IList<SourceBlock> m_SourceBlocks;
        internal ListBlock m_CurrentListBlock;

        internal readonly string m_CodeColor;
        private const string k_CodeColorDarkMode = "#AAAAAA";
        private const string k_CodeColorLightMode = "#555555";

        internal readonly bool k_OptionUseListIndentation = true;

        internal void AppendText(string text)
        {
            m_Builder.Append(text);
        }

        internal string ClearText()
        {
            string text = m_Builder.ToString();
            m_Builder.Clear();
            return text;
        }

        internal ListBlock SetCurrentList(ListBlock listBlock)
        {
            var previous = m_CurrentListBlock;
            m_CurrentListBlock = listBlock;
            return previous;
        }

        internal string GetCurrentListBullet(ListItemBlock listItemBlock)
        {
            if(m_CurrentListBlock == null)
                return null;

            if (k_OptionUseListIndentation)
                return m_CurrentListBlock.IsOrdered ? $"{listItemBlock.Order}. " : "- ";

            return new string('\t', listItemBlock.Column/4+1) + (m_CurrentListBlock.IsOrdered ? $"{listItemBlock.Order}. " : "- ");
        }

        internal void AddSource(string source)
        {
            var sourceBlock = JsonUtility.FromJson<SourceBlock>(source);
            m_SourceBlocks.Add(sourceBlock);

            //TODO-boris.bauer: add a count
            //AppendText($" <sprite=\"{MuseEditorUI.k_ReferenceSprite}\" index={m_SourceBlocks.Count}>");
        }

        public ChatMarkdownRenderer(IList<SourceBlock> sourceBlocks, IList<VisualElement> outTextElements, VisualElement previousLastElement, ChatCommandHandler commandHandler = null)
        {
            m_SourceBlocks = sourceBlocks;
            m_OutputTextElements = outTextElements;
            m_PreviousLastElement = previousLastElement;

            // TODO-boris.bauer: Design good dark and light colors
            m_CodeColor = EditorGUIUtility.isProSkin ? k_CodeColorDarkMode : k_CodeColorLightMode;

            // Required to output anything, blocks of text/markup
            ObjectRenderers.Add(new ParagraphRenderer());

            // Required to output any kind of text
            ObjectRenderers.Add(new LiteralInlineRenderer());

            // Optional for HTML tags, which includes rich text like "<b>"
            ObjectRenderers.Add(new HtmlInlineRenderer());
            // Optional for HTML entities, which includes e.g. "&lt;" and as transcoded becomes "<"
            ObjectRenderers.Add(new HtmlEntityInlineRenderer());

            ObjectRenderers.Add(new EmphasisInlineRenderer());
            ObjectRenderers.Add(new LineBreakInlineRenderer());
            ObjectRenderers.Add(new LinkInlineRenderer());

            // Required to output block :::plugin block
            ObjectRenderers.Add(new PluginsContainerRenderer());

            // Required to output block of code within "```csharp ... ```" for example
            ObjectRenderers.Add(new FencedCodeBlockRenderer { CustomFenceHandler = commandHandler });

            ObjectRenderers.Add(new CodeInlineBlockRenderer());

            // Required so include list start/block AND list items
            ObjectRenderers.Add(new ListBlockRenderer());
            ObjectRenderers.Add(new ListItemBlockRenderer());
        }

        public override object Render(MarkdownObject markdownObject)
        {
            Write(markdownObject);
            return null;
        }
    }
}
