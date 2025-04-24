using JetBrains.Annotations;
using Unity.Muse.Common.Utils;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components.ChatElements
{
    [UsedImplicitly]
    class ChatElementWrapper : AdaptiveListViewEntry
    {
        VisualElement m_Root;
        Label m_IndexDebugElement;

        ChatElementBase m_ChatElement;

        protected override void InitializeView(TemplateContainer view)
        {
            m_Root = view.Q<VisualElement>("wrapperRoot");
            m_IndexDebugElement = view.Q<Label>("indexDebugText");
        }

        public override void SetData(int index, object data, bool isSelected = false)
        {
            base.SetData(index, data);

            var message = (MuseMessage)data;
            SetupChatElement(ref m_ChatElement, message);
        }

        void SetupChatElement(ref ChatElementBase element, MuseMessage message)
        {
            bool hideIfEmpty = false;

            if (element == null)
            {
                switch (message.Role)
                {
                    case Assistant.k_UserRole:
                        element = new ChatElementUser { EditEnabled = true };
                        break;
                    case Assistant.k_SystemRole:
                        element = new ChatElementSystem();
                        hideIfEmpty = true;
                        break;
                    case Assistant.k_AssistantRole:
                        element = new ChatElementResponse();
                        hideIfEmpty = true;
                        break;
                }

                element.Initialize();
                m_Root.Add(element);
            }
            else
            {
                element.SetDisplay(true);
            }

            if (hideIfEmpty && string.IsNullOrEmpty(message.Content))
            {
                element.SetDisplay(false);
            }
            else
            {
                element.SetDisplay(true);
            }

            if (element.Message.Content == message.Content &&
                ArrayUtils.ArrayEquals(element.Message.Context, message.Context) &&
                element.Message.IsComplete == message.IsComplete)   // complete flag removes last word when false.
            {
                // No change to content, no need to update
                return;
            }

            element.SetData(message);
        }
    }
}
