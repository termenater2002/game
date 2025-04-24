#if UNITY_EDITOR
using System;
using UnityEditor;

namespace Unity.Muse.Common.Account
{
    class SignInNotificationView : NotificationView
    {
        public SignInNotificationView(bool inlineButton = false) : base(
            new()
            {
                buttonText = TextContent.signinAccept,
                titleText = TextContent.signinNotificationTitle,
                description = TextContent.signinNotificationDescription,
                action = CloudProjectSettings.ShowLogin,
                inlineButton = inlineButton,
                showOrganizations = false
            }
        ) { }
    }
}
#endif