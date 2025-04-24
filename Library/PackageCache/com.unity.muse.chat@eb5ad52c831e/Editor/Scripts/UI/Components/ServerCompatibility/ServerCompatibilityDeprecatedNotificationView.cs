using System;
using Unity.Muse.Common.Account;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components.ServerCompatibility
{
    class ServerCompatibilityDeprecatedNotificationView : NotificationView
    {
        public ServerCompatibilityDeprecatedNotificationView(Action dismiss, bool inlineButton = false) : base(
            new()
            {
                titleText = ServerCompatibilityText.DeprecatedTitle,
                description = ServerCompatibilityText.DeprecatedMessage,
                action = dismiss,
                buttonText = "Dismiss"
            }
        )
        {
            var subscriptionCallout = this.Q<SubscriptionCallout>();
            var sizeToContextButton = this.Q<SizeToContentButton>();
            subscriptionCallout.Add(sizeToContextButton);
        }
    }
}
