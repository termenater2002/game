using System;
using Unity.AppUI.Core;
using UnityEngine.UIElements;
using Application = UnityEngine.Device.Application;

namespace Unity.Muse.Common.Account
{
    static class AccountUtility
    {
        public static void GoToAccount()
        {
            var organizationId = AccountInfo.Instance.Organization?.Id;
            if (string.IsNullOrEmpty(organizationId))
                Application.OpenURL("https://id.unity.com/account/edit");
            else
                Application.OpenURL($"https://id.unity.com/organizations/{organizationId}");
        }

        public static void GoToMuseAccount()
        {
            Application.OpenURL("https://muse.unity.com/explore");
        }

        public static void DisplayThirdPartyTerms(VisualElement target, ModeStruct modeData, Action<bool> onAccept)
        {
            var dialog = new ThirdPartyDialog(modeData);
            var m = dialog.CreateModal(target);
            dialog.OnAccept = (b) => {
                m.Dismiss(b ? DismissType.Action : DismissType.Cancel);
                onAccept(b);
            };
            m.Show();
        }
    }
}