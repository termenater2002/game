using System;

namespace Unity.Muse.Common.Account
{
    class NetworkNotificationView : NotificationView
    {
        public NetworkNotificationView() : base(
            new()
            {
                titleText = TextContent.clientStatusNoInternetTitle,
                description = TextContent.clientStatusNoInternet,
                showOrganizations = false
            }
        ) { }
    }
}
