using System;

namespace Unity.Muse.Common.Api
{
    record MuseUrl
    {
#if UNITY_MUSE_CLOUD_STAGING
        public const string origin = "https://musetools-stg.unity.com";
#elif UNITY_MUSE_CLOUD_TEST
        public const string origin = "https://musetools-test.unity.com";
#else
        public const string origin = "https://musetools.unity.com";
#endif

        public string version = "v2";
        public string root => $"{origin}/api/{version}";
        public string images => $"{root}/images";
        public string assets => $"{root}/assets";
        public string entitlements => $"{root}/entitlements/list-user-orgs";
        public string legalConsent => $"{root}/entitlements/legal-consent";
        public string startTrial(string id) => $"{root}/entitlements/organizations/{id}/start-free-trial";
        public string usage => $"{root}/usage/{org}";
        public string status => $"{root}/package/status";
        public string orgId = "";
        protected string org => $"organizations/{orgId}";
    }
}
