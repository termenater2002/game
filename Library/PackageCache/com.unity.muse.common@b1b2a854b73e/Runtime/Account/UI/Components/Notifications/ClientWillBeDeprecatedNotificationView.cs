using System;

namespace Unity.Muse.Common.Account
{
    class ClientWillBeDeprecatedNotificationView : NotificationView
    {
        public ClientWillBeDeprecatedNotificationView(DateTime date, bool inlineButton = false, Action action = null) : base(
            new()
            {
                inlineButton = inlineButton,
                buttonText = TextContent.openInPackageManager,
                buttonIcon = "arrow-square-in",
                titleText = TextContent.clientStatusWillBeDeprecatedTitle,
                description = TextContent.ClientStatusWillBeDeprecatedMessage(date),
                action = () =>
                {
                    action?.Invoke();
                    ClientStatus.Instance.OpenInPackageManager();
                },
                showOrganizations = false
            }
        ) { }
    }
}
