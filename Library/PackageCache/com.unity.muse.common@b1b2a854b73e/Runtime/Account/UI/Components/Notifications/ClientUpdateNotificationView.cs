using System;

namespace Unity.Muse.Common.Account
{
    class ClientUpdateNotificationView : NotificationView
    {
        public ClientUpdateNotificationView(bool inlineButton = false, Action action = null) : base(
            new()
            {
                inlineButton = inlineButton,
                buttonText = TextContent.openInPackageManager,
                buttonIcon = "arrow-square-in",
                titleText = TextContent.clientStatusUpdateTitle,
                description = TextContent.clientStatusUpdateDescription,
                action = () =>
                {
                    action?.Invoke();
                    ClientStatus.Instance.OpenInPackageManager();
                },
                style = MuseNotificationStyle.Warning,
                showOrganizations = false
            }
        ) { }
    }
}
