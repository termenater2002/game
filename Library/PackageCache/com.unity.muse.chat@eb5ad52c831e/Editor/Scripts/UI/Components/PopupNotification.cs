using System;
using Unity.Muse.Chat.UI.Utils;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components
{
    class PopupNotification : ManagedTemplate
    {
        const int k_AnimationDuration = 100;

        Label m_TextField;

        MuseChatImage m_Icon;

        bool m_Dismissed;

        public Action<PopupNotification> OnDismissed;

        public long TimeOut { get; private set; }

        public class PopupNotificationContext
        {
            public string message;
            public string icon;
            public long timeOut;
        }

        public PopupNotification()
            : base(MuseChatConstants.UIModulePath)
        {
        }

        public void SetData(PopupNotificationContext messageContext)
        {
            m_TextField.text = messageContext.message;
            m_Icon.SetIconClassName(messageContext.icon);
            TimeOut = messageContext.timeOut;
        }

        protected override void InitializeView(TemplateContainer view)
        {
            view.SetupButton("dismissButton", OnDismissClicked);

            m_Icon = view.SetupImage("icon");

            m_TextField = view.Q<Label>("messageField");

            contentContainer.style.opacity = 0;
            contentContainer.experimental.animation.Start(0, 1, k_AnimationDuration, (element, f) =>
            {
                element.style.opacity = f;
            });
        }

        void OnDismissClicked(PointerUpEvent evt)
        {
            Dismiss();
        }

        public void Dismiss(bool animated = true)
        {
            if (m_Dismissed)
            {
                return;
            }

            m_Dismissed = true;

            OnDismissed?.Invoke(this);

            if (animated)
            {
                contentContainer.experimental.animation.Start(1, 0, k_AnimationDuration, (element, f) =>
                {
                    element.style.opacity = f;
                }).OnCompleted(() =>
                {
                    parent.Remove(this);
                });
            }
            else
            {
                parent.Remove(this);
            }
        }
    }
}
