using System;
using System.Linq;
using Unity.Muse.Common.Account;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Common.Editor
{
    static class AccountEditorInternals
    {
        const string s_SubscribeMenuItem = "Muse/Internals/Subscribe";
        const string s_StatusMenuItem = "Muse/Internals/Client Status";
        const string s_ResetSettingsMenuItem = "Muse/Internals/Reset Settings";
        const string s_ShowDialogsMenuItem = "Muse/Internals/Show Dialog";
        const string s_ShowDialogExploreMenuItem = "Muse/Internals/Show Explore Dialog";
        const string s_ShowCloudInfoMenuItem = "Muse/Internals/Cloud Info";

        [MenuItem("internal:" + s_ShowCloudInfoMenuItem)]
        static void CloudInfo()
        {
            Debug.Log($"Organization Id: {CloudProjectSettings.organizationId} -- User Id: {CloudProjectSettings.userId} -- Access Token: {CloudProjectSettings.accessToken} -- Access token copied to clipboard! -- Organization Id: {AccountInfo.Instance.Organization.Id}");
            EditorGUIUtility.systemCopyBuffer = CloudProjectSettings.accessToken;
        }

        [MenuItem("internal:" + s_SubscribeMenuItem)]
        static void Subscribe()
        {
            if (AccountInfo.Instance.IsEntitled)
                AccountInfo.Instance.Organization = null;
            else
            {
                var organization = AccountInfo.Instance.Organizations?.FirstOrDefault() ?? new OrganizationInfo {org_name = "Test Organization"};
                AccountInfo.Instance.Organization = organization;
            }
        }

        [MenuItem("internal:" + s_SubscribeMenuItem, true)]
        static bool SubscribeValidate()
        {
            Menu.SetChecked(s_SubscribeMenuItem, AccountInfo.Instance.IsEntitled);
            return true;
        }


        [MenuItem("internal:" + s_StatusMenuItem)]
        static void SetUsable()
        {
            var options = EditorUtility.DisplayDialogComplex("Set client status", "Set the status you want.", "Set to deprecated", "Set to upgrade", "Set to latest");

            if (options == 0)
            {
                var deprecatedOptions = EditorUtility.DisplayDialog("Set client status", "Set the deprecation status you want.", "Set to deprecated", "Set to will be deprecated");
                if (deprecatedOptions)
                    ClientStatus.Instance.Status = new ClientStatusResponse {status = "Deprecated"};
                else
                    ClientStatus.Instance.Status = new ClientStatusResponse {status = "Deprecated", obsolete_date = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd")};
            }
            else if (options == 1)
                ClientStatus.Instance.Status = new ClientStatusResponse {status = "Update"};
            else if (options == 2)
                ClientStatus.Instance.Status = new ClientStatusResponse {status = "Latest"};
        }

        [MenuItem("internal:" + s_ResetSettingsMenuItem)]
        static void ResetSettings()
        {
            GlobalPreferences.Delete<UsageInfo>(nameof(GlobalPreferences.usage));
            GlobalPreferences.Delete<UsageInfo>(nameof(GlobalPreferences.exploreShown));
            GlobalPreferences.Delete<UsageInfo>(nameof(GlobalPreferences.onboardingShown));
        }

        [MenuItem("internal:Muse/Internals/Update entitlements")]
        static void UpdateEntitlements()
        {
            _ = AccountInfo.Instance.UpdateEntitlements();
        }

        [MenuItem("internal:Muse/Internals/Set usage")]
        static void SetUsage()
        {
            var options = EditorUtility.DisplayDialogComplex("Set Muse Points", "Set the muse points you want.", "Set to 0", "Set to a lot", "Add 5000");

            var usage = AccountInfo.Instance.Usage;

            if (options == 0)
                usage.used = 0;
            else if (options == 1)
                usage.used = 4000000;
            else if (options == 2)
                usage.used += 5000;

            AccountInfo.Instance.Usage = usage;
        }

        [MenuItem("internal:Muse/Internals/Set Organizations")]
        static void SetOrganizations()
        {
            var options = EditorUtility.DisplayDialogComplex("Set Organizations", "Set the muse entitled organizations you want.", "Set to none", "Set to one", "Set to many");

            if (options == 0)
                AccountInfo.Instance.Organizations = new();
            else if (options == 1)
                AccountInfo.Instance.Organizations = new() {new OrganizationInfo {org_name = "Test Organization", org_id = "123"}};
            else if (options == 2)
            {
                AccountInfo.Instance.Organizations = new()
                {
                    new OrganizationInfo {org_name = "Unity Technologies", org_id = "456"},
                    new OrganizationInfo {org_name = "Mat's Cool org", org_id = "789"},
                };

                AccountInfo.Instance.Organization = AccountInfo.Instance.Organizations[0];
            }
        }

        [MenuItem("internal:" + s_ShowDialogsMenuItem)]
        static void ShowDialogs()
        {
            var options2 = 0;
            var options = EditorUtility.DisplayDialogComplex("Show dialogs", "Choose the dialog you want to display.", "Sign in", "Trial Confirm", "More");
            if (options == 2)
                options2 = EditorUtility.DisplayDialogComplex("Show dialogs", "Choose the dialog you want to display.", "Start trial", "Data opt-in", "Explore");

            if (options == 0)
                IntroductionManipulator.current.DisplaySignIn();
            if (options == 1)
                IntroductionManipulator.current.DisplayStartTrialConfirm();
            if (options == 2)
            {
                if (options2 == 0)
                    IntroductionManipulator.current.DisplayStartTrial();
                if (options2 == 1)
                    IntroductionManipulator.current.DisplayDataOptIn();
                if (options2 == 2)
                    IntroductionManipulator.current.DisplayExplore();
            }
        }

        [MenuItem("internal:" + s_ShowDialogExploreMenuItem)]
        static void ShowExploreDialog()
        {
            ExploreWindow.ShowExplore();
        }
    }
}
