using System;
using System.Text;
using System.Text.RegularExpressions;
using Markdig.Renderers;
using Markdig.Syntax;
using Unity.Muse.Chat.Commands;
using Unity.Muse.Chat.UI.Components;
using Unity.Muse.Chat.UI.Components.ChatElements;
using Unity.Muse.Chat.BackendApi.Model;
using UnityEditor;
using UnityEngine;
using Unity.Muse.Chat;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace Unity.Muse.Editor.Markup.Renderers
{
    internal class FencedCodeBlockRenderer : MarkdownObjectRenderer<ChatMarkdownRenderer, FencedCodeBlock>
    {
        public ChatCommandHandler CustomFenceHandler { get; set; }

        protected override void Write(ChatMarkdownRenderer renderer, FencedCodeBlock obj)
        {
            renderer.AppendText($"<color={renderer.m_CodeColor}>");

            StringBuilder fullCodeBlock = new StringBuilder();

            for (int i = 0; i < obj.Lines.Count; i++)
            {
                var lineWithoutEscapes = obj.Lines.Lines[i].ToString().Replace(@"\", @"\\");

                fullCodeBlock.Append(lineWithoutEscapes);
                if (i < obj.Lines.Count - 1)
                    fullCodeBlock.Append("\n");
            }

            var codeText = fullCodeBlock.ToString();

            CommandDisplayTemplate displayBlock = null;

            // If the fence handler is not null, use that
            // See if any element is made, if so, use that
            // Otherwise, fallback to the basic code block element
            if (CustomFenceHandler != null)
            {
                displayBlock = CustomFenceHandler.GetDisplayElement(obj.Info);
            }
            if (displayBlock == null)
            {
                displayBlock = new ChatElementCodeBlock();
            }

            // If this is a shared element, we skip doing the additional setup
            if (!renderer.m_OutputTextElements.Contains(displayBlock))
            {
                displayBlock.Fence = obj.Info;
                displayBlock.Initialize();
                displayBlock.visible = false;
                displayBlock.SetContent(codeText);
                renderer.m_OutputTextElements.Add(displayBlock);
                // Create the element if it is opened and complete
                if (!obj.IsOpen)
                {
                    EditorApplication.delayCall += () => SetupElement(displayBlock);
                }
            }
            else
            {
                displayBlock.AddContent(codeText);
            }

            renderer.AppendText("</color>");
        }

        async void SetupElement(CommandDisplayTemplate displayBlock)
        {
            if (!displayBlock.m_MessageReady)
                return;

            // We wait for the last LLM response to finish before doing any validation or repair
            // TODO - may no longer be neccessary as we don't stream validated elements now?
            if (Assistant.instance.MessageUpdatersCount != 0)
            {
                var currentUpdater = Assistant.instance.GetStreamForConversation(displayBlock.m_ParentMessage.Id.ConversationId);

                // We also check the active conversation updater status, as this including writing out messages that occur after code blocks.
                if (currentUpdater != null && currentUpdater.CurrentState != MuseChatStreamHandler.State.Completed)
                {
                    await currentUpdater.TaskCompletionSource.Task;
                }
            }


            // Go through all content in the display block
            displayBlock.Sync();

            var canRepair = Assistant.instance.ValidRepairTarget(displayBlock.m_ParentMessage.Id);
            var isRepaired = Assistant.instance.IsUnderRepair(displayBlock.m_ParentMessage.Id);

            var allContent = displayBlock.ContentGroups;
            var sb = new StringBuilder();
            var contentErrors = new List<int>();

            // Validate content
            for (var i = 0; i < allContent.Count; i++)
            {
                // Validate each piece of content
                if (displayBlock.Validate(i))
                {
                    continue;
                }
                contentErrors.Add(i);
            }


            // Repair if needed and possible
            if (contentErrors.Count > 0 && canRepair && !isRepaired && CustomFenceHandler != null)
            {
                var route = ChatCommands.GetServerRoute(CustomFenceHandler);
                var routeEnum = ProductEnum.AiAssistant;
                var repairType = ScriptType.Monobehaviour;
                var sendMessageID = displayBlock.m_ParentMessage.Id;
                if (string.IsNullOrEmpty(sendMessageID.ConversationId.Value))
                    sendMessageID = new MuseMessageId(Assistant.instance.GetActiveConversation().Id, sendMessageID.FragmentId, sendMessageID.Type);

                switch (route)
                {
                    case ChatCommands.ServerRoute.chat:
                        repairType = ScriptType.Monobehaviour;
                        break;
                    case ChatCommands.ServerRoute.codeGen:
                        repairType = ScriptType.CodeGen;
                        break;
                    case ChatCommands.ServerRoute.action:
                        repairType = ScriptType.AgentAction;
                        break;
                    case ChatCommands.ServerRoute.completion:
                        Enum.TryParse(CustomFenceHandler.Command, true, out routeEnum);
                        break;
                }

                // Set up for single or batch repair
                var contentString = "";
                var errorString = "";

                if (allContent.Count == 1)
                {
                    var currentContent = allContent[0];
                    contentString = currentContent.Content;
                    errorString = currentContent.Logs;
                }
                else
                {
                    // With multi item repair we concatenate all of the content and errors together for better processing
                    foreach (var errorIndex in contentErrors)
                    {
                        var currentContent = allContent[errorIndex];
                        sb.Append(string.Format($"{currentContent.Content}\n\n{currentContent.Logs}\n\n"));
                    }
                    contentString = sb.ToString();
                    errorString = "";
                }

                InternalLog.Log($"Repairing script: {contentString}");
                var repairedMessage = route == ChatCommands.ServerRoute.completion
                    ? await Assistant.instance.RepairCompletion(sendMessageID, displayBlock.m_ParentMessage.MessageIndex, errorString, contentString, routeEnum)
                    : await Assistant.instance.RepairScript(sendMessageID, displayBlock.m_ParentMessage.MessageIndex, errorString, contentString, repairType);

                InternalLog.Log($"Repaired copy: {repairedMessage}");
                repairedMessage = CustomFenceHandler.Preprocess(repairedMessage);
                InternalLog.Log($"Fenced repaired copy: {repairedMessage}");

                // If we're doing multi-item repair, we need to cut the repair message apart carefully to reintegrate it into the reply
                if (allContent.Count == 1)
                {
                    var repairedContent = "";
                    if (!string.IsNullOrEmpty(repairedMessage))
                    {
                        var match = Regex.Match(repairedMessage, $"```{displayBlock.Fence}(.*?)```", RegexOptions.Singleline);
                        if (match.Success)
                            repairedContent = match.Groups[1].Value;
                    }

                    if (!string.IsNullOrWhiteSpace(repairedContent))
                        allContent[0].Content = repairedContent;

                    // If the code is not valid this time, we fail without chance of repair
                    displayBlock.Validate(0);
                }
                else
                {
                    if (!string.IsNullOrEmpty(repairedMessage))
                    {
                        var pprepairedMessage = repairedMessage.Replace("\\n", "\n");
                        pprepairedMessage = pprepairedMessage.Trim();

                        InternalLog.Log($"Repaired message:\n {pprepairedMessage}");
                        int i = 0;
                        var matchCollection = Regex.Matches(repairedMessage, $"```{displayBlock.Fence}(.*?)```");
                        foreach (Match match in matchCollection)
                        {
                            var repairedContent = match.Groups[1].Value;
                            if (string.IsNullOrWhiteSpace(repairedContent))
                            {
                                continue;
                            }

                            repairedContent = repairedContent.Replace("\\n", "\n");
                            repairedContent = repairedContent.Trim();

                            var contentIndex = contentErrors[i++];
                            var currentContent = allContent[contentIndex];

                            InternalLog.Log($"Replacing {contentIndex}: {currentContent.Content.Equals(repairedContent)}");

                            currentContent.Content = repairedContent;
                            // If the code is not valid this time, we fail without chance of repair
                            displayBlock.Validate(contentIndex);
                        }
                    }
                }
            }

            // Everything is done, call display to update the visuals
            displayBlock.Display();
            displayBlock.visible = true;
        }
    }
}
