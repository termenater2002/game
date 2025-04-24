using System;
using System.Collections.Generic;
using Unity.Muse.Chat.WebApi;
using Unity.Muse.Chat.BackendApi.Model;
using Unity.Muse.Chat.UI.Utils;
using Unity.Muse.Common.Utils;
using UnityEngine;
using UnityEngine.UIElements;
using TextField = UnityEngine.UIElements.TextField;

namespace Unity.Muse.Chat.UI.Components.ChatElements
{
    class ChatElementResponse : ChatElementBase
    {
        const string k_FeedbackButtonActiveClass = "mui-feedback-button-active";
        private const string k_CategoryResponse = "Response quality";
        private const string k_CategoryCodeGen = "Code generation";
        private const string k_CategorySpeed = "Speed of response";
        private const string k_CategorySources = "Sources";
        private const string k_CategoryResources = "Additional resources";

        readonly IList<VisualElement> m_TextFields = new List<VisualElement>();

        static Texture2D k_MuseAvatarImage;

        Foldout m_SourcesFoldout;
        VisualElement m_SourcesContent;

        VisualElement m_TextFieldRoot;

        VisualElement m_OptionsSection;
        VisualElement m_FeedbackParamSection;
        VisualElement m_ErrorSection;

        Button m_CopyButton;
        Button m_UpVoteButton;
        Button m_DownVoteButton;

        MuseChatImage m_QueryMode;

        Toggle m_FeedbackFlagInappropriateCheckbox;
        DropdownField m_FeedbackTypeDropdown;
        TextField m_FeedbackText;
        Button m_FeedbackSendButton;

        FeedbackEditMode m_FeedbackMode = FeedbackEditMode.None;

        MuseMessageId m_MessageId;
        static readonly int k_TextAnimationDelay = 500; // in ms
        IVisualElementScheduledItem m_ScheduledAnim;

        bool m_FeedbackParametersSetup = false;

        static readonly Dictionary<MuseMessageId, int> k_AnimationIndices = new();

        int AnimationIndex
        {
            get
            {
                if(k_AnimationIndices.TryGetValue(m_MessageId, out var index))
                {
                    return index;
                }

                k_AnimationIndices[m_MessageId] = 0;

                return 0;
            }
            set
            {
                // Don't store non-external message IDs:
                if (m_MessageId.Type == MuseMessageIdType.External)
                {
                    k_AnimationIndices[m_MessageId] = value;
                }
            }
        }

        enum FeedbackEditMode
        {
            None,
            UpVote,
            DownVote
        }

        /// <summary>
        /// Set the data for this response chat element
        /// </summary>
        /// <param name="message">the message to display</param>
        /// <param name="id">the id of the message, used for feedback</param>
        public override void SetData(MuseMessage message)
        {
            if (message.IsError)
            {
                m_ErrorSection.style.display = DisplayStyle.Flex;

                // Override message text for errors to make it more user-friendly:
                message.Content = ErrorTranslator.GetErrorMessage(message.ErrorCode, message.ErrorText, message.Content);
            }

            if (m_MessageId != message.Id)
            {
                if (k_AnimationIndices.ContainsKey(m_MessageId))
                {
                    k_AnimationIndices.Remove(m_MessageId);
                }
            }

            m_MessageId = message.Id;

            MessageCommandHandler = message.GetChatCommandHandler();
            // Determine if querymode should be on
            // Set the tooltip
            // Set the icon name - run, code, or custom
            // Call the pre-processor
            if (MessageCommandHandler != null)
            {
                if (!string.IsNullOrWhiteSpace(MessageCommandHandler.Tooltip) && !string.IsNullOrWhiteSpace(MessageCommandHandler.Icon))
                {
                    m_QueryMode.SetDisplay(true);
                    m_QueryMode.SetTooltip(MessageCommandHandler.Tooltip);
                    m_QueryMode.SetIconClassName(MessageCommandHandler.Icon);
                }
                if (message.IsComplete)
                    message.Content = MessageCommandHandler.Preprocess(message.Content);
            }
            base.SetData(message);

            m_FeedbackMode = FeedbackEditMode.None;

            if (message.Id.Type == MuseMessageIdType.Internal || message.IsError)
            {
                m_OptionsSection.style.display = DisplayStyle.None;
            }

            RefreshText(m_TextFieldRoot, m_TextFields);
            RefreshSourceBlocks();
            RefreshFeedbackParameters();

            // Cancel any active animations:
            if (m_ScheduledAnim != null)
            {
                m_ScheduledAnim.Pause();
                m_ScheduledAnim = null;
            }

            // Schedule update to animate text for incomplete messages:
            if (!message.IsComplete)
            {
                GetAnimationInfo(message.Content, out var remainingSpaces, out _);
                var delay = k_TextAnimationDelay / Math.Max(1, remainingSpaces);
                m_ScheduledAnim = schedule.Execute(() =>
                {
                    SetData(message);
                }).StartingIn(delay);
            }

            RemoveCompleteMessageFromAnimationDictionary();
        }

        public override void Reset()
        {
            // If the message is complete, set the animation index to a high value to start the next animation at the last space:
            if (!Message.IsComplete)
            {
                AnimationIndex = int.MaxValue;
            }
        }

        void RemoveCompleteMessageFromAnimationDictionary()
        {
            // No need to keep complete messages in animation data dictionary:
            if (Message.IsComplete && k_AnimationIndices.ContainsKey(m_MessageId))
            {
                k_AnimationIndices.Remove(m_MessageId);
            }
        }

        void GetAnimationInfo(string message, out int remainingSpaces, out int nextSpace)
        {
            if (string.IsNullOrEmpty(message))
            {
                nextSpace = 0;
                remainingSpaces = 0;
                return;
            }

            AnimationIndex = Math.Min(AnimationIndex, message.Length - 1);
            nextSpace = message.IndexOf(' ', AnimationIndex);

            remainingSpaces = 0;
            if (nextSpace > 0)
            {
                remainingSpaces = 0;
                for (var i = nextSpace + 1; i < message.Length; i++)
                {
                    if (message[i] == ' ') remainingSpaces++;
                }

                remainingSpaces = Math.Max(1, remainingSpaces);
            }
        }

        protected override string GetAnimatedMessage(string message)
        {
            if (message.Length > 0)
            {
                GetAnimationInfo(message, out _, out var nextSpace);

                if (nextSpace > 0)
                {
                    AnimationIndex = nextSpace + 1;
                }

                message = message.Substring(0, AnimationIndex);
            }

            return message;
        }

        protected override void InitializeView(TemplateContainer view)
        {
            LoadSharedAsset("icons/muse_small.png", ref k_MuseAvatarImage);
            view.SetupImage("museAvatar").SetTexture(k_MuseAvatarImage);

            m_QueryMode = view.SetupImage("queryMode");
            m_QueryMode.SetPickingMode(PickingMode.Position);
            m_QueryMode.SetDisplay(false);

            m_TextFieldRoot = view.Q<VisualElement>("textFieldRoot");

            m_SourcesFoldout = view.Q<Foldout>("sourcesFoldout");

            m_SourcesContent = view.Q<VisualElement>("sourcesContent");

            m_OptionsSection = view.Q<VisualElement>("optionsSection");
            m_CopyButton = view.SetupButton("copyButton", OnCopyClicked);
            m_UpVoteButton = view.SetupButton("upVoteButton", OnUpvoteClicked);
            m_DownVoteButton = view.SetupButton("downVoteButton", OnDownvoteClicked);

            m_FeedbackParamSection = view.Q<VisualElement>("feedbackParamSection");

            m_ErrorSection = view.Q<VisualElement>("errorFrame");
            m_ErrorSection.SetDisplay(false);
        }

        void SetupFeedbackParameters()
        {
            if (m_FeedbackParametersSetup)
            {
                return;
            }

            m_FeedbackParametersSetup = true;
            m_FeedbackFlagInappropriateCheckbox = m_FeedbackParamSection.Q<Toggle>("feedbackFlagCheckbox");
            m_FeedbackTypeDropdown = m_FeedbackParamSection.SetupEnumDropdown("feedbackType", GetFeedbackTypeDisplayString, Category.ResponseQuality);
            m_FeedbackTypeDropdown.RegisterValueChangedCallback(_ => CheckFeedbackState());

            m_FeedbackText = m_FeedbackParamSection.Q<TextField>("feedbackValueText");
            m_FeedbackText.multiline = true;
            m_FeedbackText.maxLength = MuseChatConstants.MaxFeedbackMessageLength;
            m_FeedbackText.RegisterValueChangedCallback(_ => CheckFeedbackState());
            m_FeedbackSendButton = m_FeedbackParamSection.SetupButton("feedbackSendButton", OnSendFeedback);
        }

        string GetFeedbackTypeDisplayString(Category type)
        {
            return type switch
            {
                Category.ResponseQuality => k_CategoryResponse,
                Category.CodeGeneration => k_CategoryCodeGen,
                Category.SpeedToResponse => k_CategorySpeed,
                Category.Sources => k_CategorySources,
                Category.AdditionalResources => k_CategoryResources,
                _ => type.ToString(),
            };
        }

        bool TryGetCategoryFromDisplayString(string displayString, out Category category)
        {
            switch (displayString)
            {
                case k_CategoryResponse:
                    category = Category.ResponseQuality;
                    return true;
                case k_CategoryCodeGen:
                    category = Category.CodeGeneration;
                    return true;
                case k_CategorySpeed:
                    category = Category.SpeedToResponse;
                    return true;
                case k_CategorySources:
                    category = Category.Sources;
                    return true;
                case k_CategoryResources:
                    category = Category.AdditionalResources;
                    return true;
            };
            category = default;
            return false;
        }
        void CheckFeedbackState()
        {
            if (string.IsNullOrEmpty(m_FeedbackText.value))
            {
                m_FeedbackSendButton.SetEnabled(false);
            }
            else
            {
                m_FeedbackSendButton.SetEnabled(true);
            }
        }

        bool GetSelectedFeedbackType(out Category type)
        {
            return TryGetCategoryFromDisplayString(m_FeedbackTypeDropdown.value, out type);
        }

        void OnSendFeedback(PointerUpEvent evt)
        {
            if (string.IsNullOrEmpty(m_FeedbackText.value))
            {
                MuseChatView.ShowNotification($"Failed to send Feedback: 'your feedback' section is empty", PopNotificationIconType.Error);
                return;
            }
            string message = m_FeedbackText.value.Trim();

            if(!GetSelectedFeedbackType(out var type))
            {
                MuseChatView.ShowNotification($"Failed to send Feedback: 'Add a feedback category' requires a category to be selected", PopNotificationIconType.Error);
                return;
            }

            if (m_FeedbackMode != FeedbackEditMode.DownVote && m_FeedbackMode != FeedbackEditMode.UpVote)
            {
                MuseChatView.ShowNotification($"Failed to send Feedback: Sentiment must be set", PopNotificationIconType.Error);
                return;
            }

            if (m_FeedbackFlagInappropriateCheckbox.value)
            {
                message += " (Message was flagged as inappropriate.)";
            }

            var feedback = new MessageFeedback
            {
                MessageId = Id,
                FlagInappropriate = m_FeedbackFlagInappropriateCheckbox.value,
                Type = type,
                Message = message,
                Sentiment = m_FeedbackMode == FeedbackEditMode.UpVote
                    ? Sentiment.Positive
                    : Sentiment.Negative
            };

            Assistant.instance.SendFeedback(feedback);
            m_FeedbackMode = FeedbackEditMode.None;
            ClearFeedbackParameters();

            MuseChatView.ShowNotification("Feedback sent", PopNotificationIconType.Info);
        }

        void ClearFeedbackParameters()
        {
            m_FeedbackTypeDropdown.value = default;

            m_FeedbackFlagInappropriateCheckbox.value = false;
            m_FeedbackText.value = string.Empty;
            RefreshFeedbackParameters();
        }

        void OnDownvoteClicked(PointerUpEvent evt)
        {
            if (m_FeedbackMode == FeedbackEditMode.DownVote)
            {
                m_FeedbackMode = FeedbackEditMode.None;
                RefreshFeedbackParameters();
                return;
            }

            m_FeedbackMode = FeedbackEditMode.DownVote;
            RefreshFeedbackParameters();
        }

        void OnUpvoteClicked(PointerUpEvent evt)
        {
            if (m_FeedbackMode == FeedbackEditMode.UpVote)
            {
                m_FeedbackMode = FeedbackEditMode.None;
                RefreshFeedbackParameters();
                return;
            }

            m_FeedbackMode = FeedbackEditMode.UpVote;
            RefreshFeedbackParameters();
            m_FeedbackFlagInappropriateCheckbox.value = false;
        }

        void OnCopyClicked(PointerUpEvent evt)
        {
            string disclaimerHeader = string.Format(MuseChatConstants.DisclaimerText, DateTime.Now.ToShortDateString());

            // Format message with footnotes (indices to sources)
            IList<SourceBlock> sourceBlocks = new List<SourceBlock>();

            MessageUtils.ProcessText(Message, ref sourceBlocks, out var outMessage,
                MessageUtils.FootnoteFormat.SimpleIndexForClipboard);

            // Add sources in same order of footnote indices
            MessageUtils.AppendSourceBlocks(sourceBlocks, ref outMessage);

            GUIUtility.systemCopyBuffer = string.Concat(disclaimerHeader, outMessage);

            MuseChatView.ShowNotification("Copied to clipboard", PopNotificationIconType.Info);
        }

        protected override void HandleLinkClick(LinkType type, string id)
        {
            switch (type)
            {
                case LinkType.Reference:
                {
                    if (!int.TryParse(id, out var sourceId) || SourceBlocks.Count <= sourceId || sourceId < 0)
                    {
                        Debug.LogError("Invalid Source ID: " + sourceId);
                        return;
                    }

                    var sourceBlock = SourceBlocks[sourceId];
                    Application.OpenURL(sourceBlock.source);

                    return;
                }
            }

            base.HandleLinkClick(type, id);
        }

        void RefreshSourceBlocks()
        {
            if (Message.IsError || !Message.IsComplete || SourceBlocks == null || SourceBlocks.Count == 0)
            {
                m_SourcesFoldout.style.display = DisplayStyle.None;
                return;
            }

            m_SourcesFoldout.style.display = DisplayStyle.Flex;
            m_SourcesContent.Clear();

            for (var index = 0; index < SourceBlocks.Count; index++)
            {
                var sourceBlock = SourceBlocks[index];
                var entry = new ChatElementSourceEntry();
                entry.Initialize();
                entry.SetData(index, sourceBlock);
                m_SourcesContent.Add(entry);
            }
        }

        void RefreshFeedbackParameters()
        {
            if (Message.IsError || !Message.IsComplete)
            {
                m_UpVoteButton.SetEnabled(false);
                m_DownVoteButton.SetEnabled(false);
                m_FeedbackParamSection.style.display = DisplayStyle.None;
                return;
            }

            m_UpVoteButton.SetEnabled(true);
            m_DownVoteButton.SetEnabled(true);

            switch (m_FeedbackMode)
            {
                case FeedbackEditMode.None:
                {
                    m_FeedbackParamSection.style.display = DisplayStyle.None;
                    m_UpVoteButton.RemoveFromClassList(k_FeedbackButtonActiveClass);
                    m_DownVoteButton.RemoveFromClassList(k_FeedbackButtonActiveClass);
                    return;
                }

                case FeedbackEditMode.DownVote:
                {
                    SetupFeedbackParameters();
                    m_FeedbackParamSection.style.display = DisplayStyle.Flex;
                    m_FeedbackFlagInappropriateCheckbox.style.display = DisplayStyle.Flex;
                    m_UpVoteButton.RemoveFromClassList(k_FeedbackButtonActiveClass);
                    m_DownVoteButton.AddToClassList(k_FeedbackButtonActiveClass);
                    break;
                }

                case FeedbackEditMode.UpVote:
                {
                    SetupFeedbackParameters();
                    m_FeedbackParamSection.style.display = DisplayStyle.Flex;
                    m_FeedbackFlagInappropriateCheckbox.style.display = DisplayStyle.None;
                    m_UpVoteButton.AddToClassList(k_FeedbackButtonActiveClass);
                    m_DownVoteButton.RemoveFromClassList(k_FeedbackButtonActiveClass);
                    break;
                }
            }
        }
    }
}
