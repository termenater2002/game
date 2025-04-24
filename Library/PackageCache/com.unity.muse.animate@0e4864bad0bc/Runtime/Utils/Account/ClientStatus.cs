using System;
using Unity.Muse.Common;
using Unity.Muse.Common.Account;
using UnityEditor.PackageManager;

namespace Unity.Muse.Animate.Editor
{
    class ClientStatus
    {
        static ClientStatus s_Instance;

        public static ClientStatus Instance
        {
            get => s_Instance ??= new ClientStatus();
            internal set => s_Instance = value;
        }
        
        // TODO: Add account checks here
        public bool IsClientUsable => !m_Status.IsDeprecated;
        
        public event Action<ClientStatusResponse> OnClientStatusChanged;
        
        ClientStatusResponse m_Status = new();
        
        public ClientStatusResponse Status
        {
            get => m_Status;
            internal set
            {
                m_Status = value;
                OnClientStatusChanged?.Invoke(value);
            }
        }
        
        static readonly PackageInfo k_PackageInfo = PackageInfo.FindForAssetPath("Packages/com.unity.muse.animate/package.json");
        
        // TODO: Change this when we upgrade the API
        const string k_ApiVersion = "v1";

        public void UpdateStatus()
        {
            if (!UnityConnectUtils.GetIsLoggedIn())
                return;
            
            var requestData = new ClientStatusRequest(k_PackageInfo, k_ApiVersion);
            
            GenerativeAIBackend.GetStatus(requestData, (result, error) =>
            {
                if (!string.IsNullOrEmpty(error))
                    return;
                
                Status = result;
            });
        }
    }
}
