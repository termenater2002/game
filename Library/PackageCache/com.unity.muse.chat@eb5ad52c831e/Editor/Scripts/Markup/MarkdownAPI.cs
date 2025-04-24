using System.Collections.Generic;
using Markdig;
using Markdig.Parsers.Inlines;
using Unity.Muse.Chat.WebApi;
using Unity.Muse.Chat.Commands;
using UnityEngine.UIElements;

namespace Unity.Muse.Editor.Markup
{
    internal class MarkdownAPI
    {
        private static readonly MarkdownPipeline k_Pipeline;

        static MarkdownAPI()
        {
            var pipelineBuilder = new MarkdownPipelineBuilder();
            pipelineBuilder.InlineParsers.TryRemove<EscapeInlineParser>();
            pipelineBuilder.InlineParsers.TryRemove<LinkInlineParser>();
            pipelineBuilder.InlineParsers.AddIfNotAlready<ChatLinkInlineParser>();
            pipelineBuilder.UseCustomContainers();

            k_Pipeline = pipelineBuilder.Build();
        }

        internal static void MarkupText(string text, IList<SourceBlock> sourceBlocks, IList<VisualElement> newTextElements, VisualElement previousLastElement, ChatCommandHandler commandHandler = null)
        {
            var ourRenderer = new ChatMarkdownRenderer(sourceBlocks, newTextElements, previousLastElement, commandHandler);
            k_Pipeline.Setup(ourRenderer);

            Markdown.Convert(text, ourRenderer, k_Pipeline);
        }
    }
}
