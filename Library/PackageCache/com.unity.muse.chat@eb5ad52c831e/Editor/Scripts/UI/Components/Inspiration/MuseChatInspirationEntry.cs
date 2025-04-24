using System;
using Unity.Muse.Chat.UI.Utils;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components.Inspiration
{
    class MuseChatInspirationEntry : ManagedTemplate
    {
        MuseChatInspiration m_Value;
        Button m_Button;

        public MuseChatInspirationEntry()
            : base(MuseChatConstants.UIModulePath)
        {
        }

        public event Action<MuseChatInspiration> Clicked;

        public MuseChatInspiration Value
        {
            get => m_Value;

            set
            {
                m_Value = value;
                m_Button.text = value.Value;
            }
        }

        protected override void InitializeView(TemplateContainer view)
        {
            m_Button = view.SetupButton("inspirationEntryButton", OnEntryClicked);
        }

        void OnEntryClicked(PointerUpEvent evt)
        {
            Clicked?.Invoke(m_Value);
        }
    }
}
