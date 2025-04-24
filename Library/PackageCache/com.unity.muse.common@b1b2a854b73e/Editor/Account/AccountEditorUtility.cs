using Unity.Muse.Common.Account;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Common.Editor
{
    static class AccountEditorUtility
    {
        [InitializeOnLoadMethod]
        public static void Init()
        {
            GlobalPreferences.Init(new EditorPreferences());
            EditorApplication.focusChanged += OnFocusChanged;

            // Once per session
            EditorApplication.delayCall += () =>
            {
                if (!AccountInfo.Instance.IsReady) AccountInfo.Instance.UpdateAccountInformation();
                if (!AccountStatus.instance.statusChecked) ClientStatus.Instance.UpdateStatus();

                // Once per user
                UnityConnectUtils.RegisterUserStateChangedEvent(_ =>
                {
                    ForceRefreshAccountInformation();
                });

                UnityConnectUtils.RegisterConnectStateChangedEvent(_ =>
                {
                    // This can happen when getting account information while the user is not connected causes it to
                    // never get called again. So here we make sure to remedy this.
                    if (!AccountInfo.Instance.IsReady)
                        AccountInfo.Instance.UpdateAccountInformation();
                });
            };
        }

        static void ForceRefreshAccountInformation()
        {
            AccountStatus.instance.entitlementsChecked = false;
            AccountStatus.instance.legalConsentChecked = false;

            AccountInfo.Instance.UpdateAccountInformation();
        }

        static void OnFocusChanged(bool focus)
        {
            // Don't constantly check subscription if muse is not even being used
            if (!MuseWindowTracker.IsAnyWindowRegistered())
                return;

            if (focus && !AccountInfo.Instance.IsEntitled)
                AsyncUtils.SafeExecute(AccountInfo.Instance.UpdateEntitlements);
        }
    }
}
