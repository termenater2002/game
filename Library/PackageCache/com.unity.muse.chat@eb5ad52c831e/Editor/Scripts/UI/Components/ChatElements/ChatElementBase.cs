using System.Collections.Generic;
using System.Text;
using Unity.Muse.Chat.Commands;
using Unity.Muse.Chat.WebApi;
using Unity.Muse.Editor.Markup;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Unity.Muse.Chat.UI.Components.ChatElements
{
    abstract class ChatElementBase : ManagedTemplate
    {
        // Note: We may have to tweak this dynamically based on what content we intend to add to the text element
        const int k_MessageChunkSize = 5000;

        const string k_ActionCursorClassName = "mui-action-cursor";

        IList<SourceBlock> m_SourceBlocks;
        IList<MuseChatContextEntry> m_ContextEntries;

        readonly IList<VisualElement> m_TextElementCache = new List<VisualElement>();

        protected ChatCommandHandler MessageCommandHandler { get; set;  }
        protected enum LinkType
        {
            Reference,
            GameObject
        }

        protected ChatElementBase()
            : base(MuseChatConstants.UIModulePath)
        {
            MessageChunks = new List<string>();
            HideWhenEmpty = true;
        }

        public virtual void SetData(MuseMessage message)
        {
            Message = message;
            BuildMessageChunks();

            if (HideWhenEmpty && string.IsNullOrEmpty(message.Content))
            {
                style.display = DisplayStyle.None;
            }
            else
            {
                style.display = DisplayStyle.Flex;
            }
        }

        public virtual void Reset()
        {
        }

        public MuseMessageId Id => Message.Id;

        public MuseMessage Message { get; private set; }

        public bool HideWhenEmpty { get; set; }

        protected IList<string> MessageChunks { get; }

        protected IList<SourceBlock> SourceBlocks => m_SourceBlocks;
        protected IList<MuseChatContextEntry> ContextEntries => m_ContextEntries;

        protected void RefreshText(VisualElement root, IList<VisualElement> textFields)
        {
            if (Message.Role == Assistant.k_UserRole)
            {
                // Synchronize each text element for our message chunks
                for (var i = 0; i < MessageChunks.Count; i++)
                {
                    var text = MessageChunks[i];

                    if (Message.Role == Assistant.k_UserRole)
                        text = MarkupUtil.QuoteCarriageReturn(text);

                    if (textFields.Count <= i)
                    {
                        var textElement = new Label { text = text };
                        textElement.RegisterCallback<PointerDownLinkTagEvent>(OnLinkClicked);
                        textElement.RegisterCallback<PointerOverLinkTagEvent>(OnLinkOver);
                        textElement.RegisterCallback<PointerOutLinkTagEvent>(OnLinkOut);
                        textElement.selection.isSelectable = true;
                        textFields.Add(textElement);
                        root.Add(textElement);
                    }
                    else
                    {
                        var textField = textFields[i] as Label;
                        textField.text = text;
                    }
                }

                for (var i = textFields.Count - 1; i >= MessageChunks.Count; i--)
                {
                    var obsoleteField = textFields[i];
                    obsoleteField.RemoveFromHierarchy();
                    obsoleteField.UnregisterCallback<PointerDownLinkTagEvent>(OnLinkClicked);
                    obsoleteField.UnregisterCallback<PointerOverLinkTagEvent>(OnLinkOver);
                    obsoleteField.UnregisterCallback<PointerOutLinkTagEvent>(OnLinkOut);
                    textFields.RemoveAt(i);
                }

                return;
            }

            // For chat responses, parse chunks and add text elements with formatting/rich text tags where applicable

            textFields.Clear();
            root.Clear();

            VisualElement lastElement = null;
            SharedDisplayTemplates.Reset();
            for (var i = 0; i < MessageChunks.Count; i++)
            {
                var text = MessageChunks[i];

                m_TextElementCache.Clear();
                MarkdownAPI.MarkupText(text, m_SourceBlocks, m_TextElementCache, lastElement, MessageCommandHandler);

                for (var id = 0; id < m_TextElementCache.Count; id++)
                {
                    var visualElement = m_TextElementCache[id];

                    if (textFields.Count <= id)
                    {
                        if (visualElement is Label textElement)
                        {
                            textElement.selection.isSelectable = true;

                            visualElement.RegisterCallback<PointerDownLinkTagEvent>(OnLinkClicked);
                            visualElement.RegisterCallback<PointerOverLinkTagEvent>(OnLinkOver);
                            visualElement.RegisterCallback<PointerOutLinkTagEvent>(OnLinkOut);
                        }
                        if (visualElement is CommandDisplayTemplate displayBlock)
                        {
                            displayBlock.SetMessage(Message);
                        }

                        textFields.Add(visualElement);
                        root.Add(visualElement);
                        lastElement = visualElement;
                    }
                }
            }

            // Clear out obsolete fields
            for (var id = textFields.Count - 1; id >= m_TextElementCache.Count; id--)
            {
                var obsoleteField = textFields[id];
                obsoleteField.RemoveFromHierarchy();
                obsoleteField.UnregisterCallback<PointerDownLinkTagEvent>(OnLinkClicked);
                obsoleteField.UnregisterCallback<PointerOverLinkTagEvent>(OnLinkOver);
                obsoleteField.UnregisterCallback<PointerOutLinkTagEvent>(OnLinkOut);
                textFields.RemoveAt(id);
            }
            SharedDisplayTemplates.Reset();
        }

        void OnLinkOut(PointerOutLinkTagEvent evt)
        {
            if (evt.target is Label text)
            {
                text.RemoveFromClassList(k_ActionCursorClassName);
            }
        }

        void OnLinkOver(PointerOverLinkTagEvent evt)
        {
            if (evt.target is Label text)
            {
                text.AddToClassList(k_ActionCursorClassName);
            }
        }

        void OnLinkClicked(PointerDownLinkTagEvent evt)
        {
            if (evt.linkID.IndexOf(MuseChatConstants.SourceReferencePrefix) >= 0)
            {
                HandleLinkClick(LinkType.Reference, evt.linkID.Replace(MuseChatConstants.SourceReferencePrefix, ""));
                return;
            }

            HandleLinkClick(LinkType.GameObject, evt.linkID);

        }

        protected virtual void HandleLinkClick(LinkType type, string id)
        {
            switch (type)
            {
                case LinkType.GameObject:
                {
                    if (!MessageUtils.GetAssetFromLink(id, out var asset))
                    {
                        Debug.LogWarning("Asset not found: " + id);
                        return;
                    }

                    Selection.activeObject = asset;
                    return;
                }

                default:
                {
                    Debug.LogError("Unhandled link type: " + type + " == " + id);
                    return;
                }
            }
        }

        protected virtual string GetAnimatedMessage(string message)
        {
            return message;
        }

        void BuildMessageChunks()
        {
            m_SourceBlocks?.Clear();
            m_ContextEntries?.Clear();
            MessageChunks.Clear();

            if (Message.Context is { Length: > 0 })
            {
                if (m_ContextEntries == null)
                {
                    m_ContextEntries = new List<MuseChatContextEntry>();
                }

                for (var i = 0; i < Message.Context.Length; i++)
                {
                    m_ContextEntries.Add(Message.Context[i]);
                }
            }

            if (string.IsNullOrEmpty(Message.Content))
            {
                return;
            }

            MessageUtils.ProcessText(Message, ref m_SourceBlocks, out var messageContent);

            if (!Message.IsComplete)
            {
                messageContent = GetAnimatedMessage(messageContent);
            }

            string[] lines = messageContent.Split("\n");
            var chunk = new StringBuilder();
            for (var i = 0; i < lines.Length; i++)
            {
                var chunkContent = SplitChunkContent(lines[i]);
                for (var ic = 0; ic < chunkContent.Length; ic++)
                {
                    if (chunk.Length > k_MessageChunkSize)
                    {
                        MessageChunks.Add(chunk.ToString());
                        chunk.Clear();
                    }

                    if (ic < chunkContent.Length - 1)
                    {
                        chunk.Append(chunkContent[ic]);
                    }
                    else
                    {
                        chunk.AppendLine(chunkContent[ic]);
                    }
                }
            }

            if (chunk.Length > 0)
            {
                MessageChunks.Add(chunk.ToString());
            }
        }

        string[] SplitChunkContent(string content)
        {
            int lineChunks = 1 + (int)Mathf.Floor(content.Length / (float)k_MessageChunkSize);
            var result = new string[lineChunks];
            for(var i = 0; i < lineChunks; i++)
            {
                int start = i * k_MessageChunkSize;
                int length = Mathf.Min(content.Length - start, k_MessageChunkSize);
                result[i] = content.Substring(start, length);
            }

            return result;
        }
    }
}
