using System;

namespace Unity.Muse.Common.Account
{
    class TrialNotificationView : NotificationView
    {
        public TrialNotificationView(bool inlineButton = false, Action action = null) : base(
            new()
            {
                buttonText = TextContent.subStartTrial,
                buttonIcon = "arrow-square-in",
                titleText = TextContent.subNotificationTrialTitle,
                description = TextContent.subNotificationTrialDescription,
                style = MuseNotificationStyle.Warning,
                inlineButton = inlineButton,
                action = () =>
                {
                    action?.Invoke();
                    OnboardWindow.ShowOnboarding();
                }
            }
        ) { }
    }
}
