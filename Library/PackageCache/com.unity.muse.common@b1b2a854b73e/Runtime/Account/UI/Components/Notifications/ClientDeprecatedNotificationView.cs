using System;

namespace Unity.Muse.Common.Account
{
    class ClientDeprecatedNotificationView : NotificationView
    {
        public ClientDeprecatedNotificationView(bool inlineButton = false, Action action = null) : base(
            new()
            {
                inlineButton = inlineButton,
                buttonText = TextContent.openInPackageManager,
                buttonIcon = "arrow-square-in",
                titleText = TextContent.clientStatusDeprecatedTitle,
                description = TextContent.clientStatusDeprecatedMessage,
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
