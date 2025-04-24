using System;
using UnityEngine;

namespace Unity.Muse.Common.Account
{
    class SubscriptionExpiredNotificationView : NotificationView
    {
        public SubscriptionExpiredNotificationView(bool inlineButton = false, Action action = null)
            : base(
                new()
                {
                    inlineButton = inlineButton,
                    buttonText = TextContent.subNotificationSubscribeAction,
                    titleText = TextContent.subNotificationSubscribeTitle,
                    description = TextContent.subNotificationSubscribeDescription,
                    action = () =>
                    {
                        action?.Invoke();
                        AccountLinks.StartSubscription();
                    }
                }
            )
        { }
    }
}
