using System;

namespace Unity.Muse.Common.Account
{
    record NotificationOptions
    {
        public string titleText;
        public string description;
        public string buttonText;
        public Action action;

        public string buttonIcon = "arrow-square-out";
        public bool inlineButton = false;
        public MuseNotificationStyle style = MuseNotificationStyle.Alert;

        public bool showOrganizations = true;
    }
}
