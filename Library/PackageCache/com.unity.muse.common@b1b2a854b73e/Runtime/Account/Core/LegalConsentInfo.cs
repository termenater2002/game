#if UNITY_EDITOR
using System;
using UnityEditor;

namespace Unity.Muse.Common.Account
{
    [Serializable]
    record LegalConsentInfo
    {
        public bool terms_of_service_legal_info;
        public bool privacy_policy_gen_ai;
        public bool content_usage_data_training;
        public string user_id;

        public bool HasConsented =>
            user_id == CloudProjectSettings.userId &&
            terms_of_service_legal_info &&
            privacy_policy_gen_ai;
    }
}
#endif