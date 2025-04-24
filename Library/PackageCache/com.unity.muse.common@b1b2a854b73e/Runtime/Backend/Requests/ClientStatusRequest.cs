using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
#endif

namespace Unity.Muse.Common
{
    [Serializable]
    class ClientStatusRequest : ItemRequest
    {
        public override string parameters => $"package_name={package_name}&package_version={package_version}&api_version={api_version}";

        public string package_version;
        public string package_name;
        public string api_version;

#if UNITY_EDITOR
        public ClientStatusRequest(PackageInfo info, string apiVersion = null)
        {
            package_name = info.name;
            package_version = info.version;
            api_version = apiVersion ?? GenerativeAIBackend.TexturesUrl.version;
        }
#endif
    }
}
