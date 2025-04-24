using System;
using Unity.Muse.Chat.UI.Utils;
using Unity.Muse.Common.Utils;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components
{
    class MuseChatNotificationBanner : ManagedTemplate
    {
        VisualElement m_NotificationBanner;
        Label m_NotificationBannerTitle;
        Label m_NotificationBannerMessage;
        Button m_DismissButton;
        Button m_ActionButton;
        Label m_ActionButtonLabel;

        Action m_ActionCallback;
        Action m_DismissCallback;

        public MuseChatNotificationBanner()
            : base(MuseChatConstants.UIModulePath)
        {
        }

        public void Show(string title, string message, Action actionCallback = null, string actionCallbackText = "", bool canDismiss = true, Action dismissCallback = null)
        {
            Show();
            m_NotificationBannerTitle.text = title;
            m_NotificationBannerMessage.text = message;

            m_ActionCallback = actionCallback;
            m_DismissCallback = dismissCallback;

            m_DismissButton.SetDisplay(canDismiss);
            m_ActionButton.SetDisplay(actionCallback != null);
            m_ActionButtonLabel.text = actionCallbackText;
        }

        void DismissClicked(PointerUpEvent evt)
        {
            m_DismissCallback?.Invoke();
            m_DismissCallback = null;

            Hide();
        }

        void ActionClicked(PointerUpEvent evt)
        {
            m_ActionCallback?.Invoke();
        }

        protected override void InitializeView(TemplateContainer view)
        {
            m_NotificationBanner = view.Q<VisualElement>("notificationBanner");
            m_NotificationBannerTitle = view.Q<Label>("notificationBannerTitle");
            m_NotificationBannerMessage = view.Q<Label>("notificationBannerMessage");
            m_ActionButton = view.SetupButton("notificationBannerActionButton", ActionClicked);
            m_ActionButtonLabel = view.Q<Label>("notificationBannerActionButtonLabel");
            m_DismissButton = view.SetupButton("notificationBannerDismissButton", DismissClicked);

            Show(false);
            Hide(false);
        }
    }
}
