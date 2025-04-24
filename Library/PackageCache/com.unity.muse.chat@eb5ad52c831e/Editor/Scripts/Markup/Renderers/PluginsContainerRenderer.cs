using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markdig.Extensions.CustomContainers;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Newtonsoft.Json;
using Unity.Muse.Chat.UI.Components.ChatElements;

namespace Unity.Muse.Editor.Markup
{
    [System.Serializable]
    internal struct PluginCall
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("function")]
        public string Function { get; set; }

        [JsonProperty("parameters")]
        public string[] Parameters { get; set; }
    }

    class PluginsContainerRenderer : MarkdownObjectRenderer<ChatMarkdownRenderer, CustomContainer>
    {
        protected override void Write(ChatMarkdownRenderer renderer, CustomContainer obj)
        {
            // plugin contains start with :::plugin
            if (obj.Info == "plugin")
            {
                // Construct the plugin data
                IEnumerable<string> calls = obj
                    .Select(a => a)
                    .Cast<ParagraphBlock>()
                    .SelectMany(p => p.Inline)
                    .Aggregate(new StringBuilder(), (builder, inline) =>
                    {
                        if (inline is LineBreakInline)
                            builder.Append("@!#$");
                        else
                            builder.Append(inline.ToString());
                        return builder;
                    })
                    .ToString()
                    .Split("@!#$");

                // Extract the plugin calls that are valid
                List<PluginCall> parsedCalls = new();

                foreach (string call in calls)
                {
                    try
                    {
                        parsedCalls.Add(JsonConvert.DeserializeObject<PluginCall>(call));
                    }
                    catch (Exception)
                    {
                        // Ignore failures to parse PluginCall. This can happen when calls are partially streamed in
                    }
                }

                ChatElementPluginBlock pluginButton = new();
                pluginButton.Initialize();
                pluginButton.SetData(parsedCalls.ToArray());
                renderer.m_OutputTextElements.Add(pluginButton);
            }
        }
    }
}
