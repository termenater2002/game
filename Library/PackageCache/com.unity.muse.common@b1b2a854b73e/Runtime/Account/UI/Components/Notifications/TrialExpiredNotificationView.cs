using System;

namespace Unity.Muse.Common.Account
{
    class TrialExpiredNotificationView : NotificationView
    {
        public TrialExpiredNotificationView(bool inlineButton = false, Action action = null)
            : base(
                new()
                {
                    inlineButton = inlineButton,
                    buttonText = TextContent.subNotificationSubscribeAction,
                    titleText = TextContent.subNotificationTrialExpiredTitle,
                    description = TextContent.subNotificationTrialExpiredDescription,
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
