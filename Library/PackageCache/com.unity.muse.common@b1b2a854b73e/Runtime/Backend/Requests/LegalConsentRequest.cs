using System;

namespace Unity.Muse.Common
{
    [Serializable]
    class LegalConsentRequest : ItemRequest
    {
        public bool terms_of_service_legal_info;
        public bool privacy_policy_gen_ai;
        public bool content_usage_data_training;
    }
}
