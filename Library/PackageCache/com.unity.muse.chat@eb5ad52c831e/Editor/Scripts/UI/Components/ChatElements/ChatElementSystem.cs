using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components.ChatElements
{
    class ChatElementSystem : ChatElementBase
    {
        readonly IList<VisualElement> m_TextFields = new List<VisualElement>();

        VisualElement m_TextFieldRoot;
        public override void SetData(MuseMessage message)
        {
            MessageCommandHandler = message.GetChatCommandHandler();
            if (MessageCommandHandler != null)
            {
                message.Content = MessageCommandHandler.Preprocess(message.Content);
            }
            base.SetData(message);

            RefreshText(m_TextFieldRoot, m_TextFields);
        }

        protected override void InitializeView(TemplateContainer view)
        {
            m_TextFieldRoot = view.Q<VisualElement>("textFieldRoot");
        }
    }
}
