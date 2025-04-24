#if UNITY_EDITOR
using System;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Unity.Muse.Common.Account
{
    /// <summary>
    /// The client status provides the status of a specific client, for instance if it is scheduled to be deprecated
    /// or if updates are available.
    /// </summary>
    class ClientStatus
    {
        static ClientStatus s_Instance;
        public static ClientStatus Instance => s_Instance ??= new(Preferences.packageName);

        public event Action<ClientStatusResponse> OnClientStatusChanged;

        string m_PackageName;
        public PackageInfo packageInfo => PackageInfo.FindForAssetPath($"Packages/{m_PackageName}/package.json");

        public string apiVersion => GenerativeAIBackend.TexturesUrl.version;

        public ClientStatus(string packageName)
        {
            m_PackageName = packageName;
        }

        public void OpenInPackageManager() =>
            UnityEditor.PackageManager.UI.Window.Open(packageInfo.name);

        public ClientStatusResponse Status
        {
            get => AccountStatus.instance.status;
            internal set
            {
                AccountStatus.instance.status = value;
                OnClientStatusChanged?.Invoke(value);
            }
        }

        public void UpdateStatus()
        {
            if (!UnityConnectUtils.GetIsLoggedIn())
                return;

            var requestData = new ClientStatusRequest(packageInfo, apiVersion);
            GenerativeAIBackend.GetStatus(requestData, (result, error) =>
            {
                AccountStatus.instance.statusChecked = true;

                if (!string.IsNullOrEmpty(error))
                    return;

                Status = result;
            });
        }
    }
}
#endif