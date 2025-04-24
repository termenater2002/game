using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Muse.Common.Account
{
    class AccountInfo
    {
        static AccountInfo s_Instance;
        public static AccountInfo Instance => s_Instance ??= new();

        public event Action OnOrganizationChanged;
        public event Action OnUsageChanged;
        public event Action OnOrganizationListChanged;
        public event Action OnLegalConsentChanged;

        public bool IsEntitled => Organization is {IsEntitled: true};
        public bool IsExpired => Organization is {IsExpired: true};
        public bool HasMuseAccount => Organization is {HasMuseAccount: true};
        public bool RequestSeat;

        // Stuck state possible: No valid token, so can't do the ready calls.
        // Since they never get retried, the state never changes back to a correct one.
        public bool IsReady =>
            AccountStatus.instance.entitlementsChecked &&
            AccountStatus.instance.legalConsentChecked;

        public LegalConsentInfo LegalConsent
        {
            get => GlobalPreferences.legalConsent ??= new();
            set
            {
                var changed = GlobalPreferences.legalConsent != value;
                GlobalPreferences.legalConsent = value;
                if (changed)
                    OnLegalConsentChanged?.Invoke();
            }
        }

        /// <summary>
        /// List of entitled organizations
        /// </summary>
        public List<OrganizationInfo> Organizations
        {
            get => GlobalPreferences.organizations;
            internal set
            {
                GlobalPreferences.organizations = value
                    .OrderBy(org => org.Status)
                    .ThenBy(org => org.Label)
                    .ToList();
                OnOrganizationListChanged?.Invoke();
                RefreshOrganization();
            }
        }

        void RefreshOrganization()
        {
#if UNITY_EDITOR
            var projectOrgId = UnityEditor.CloudProjectSettings.organizationId;
#endif
            // Try to select the most appropriate organization
            // In order of preference:
            //      Keep current organization
            //      Use entitled project organization
            //      Use any entitled organization
            //      Use not entitled project organization
            //      Use the first organization
            Organization = Organizations
                .OrderByDescending(org => org.Id == Organization?.Id)
                .ThenByDescending(org => org.IsEntitled && org.Id == projectOrgId)
                .ThenByDescending(org => org.IsEntitled)
                .ThenByDescending(org => !org.IsEntitled && org == Organization)
                .ThenByDescending(org => !org.IsEntitled && org.Id == projectOrgId)
                .FirstOrDefault();
        }

        /// <summary>
        /// Currently selected organization
        ///
        /// May not be entitled.
        /// </summary>
        public OrganizationInfo Organization
        {
            get => GlobalPreferences.organization;
            internal set
            {
                // Always privilege the organization from the current list if possible (should be all cases except for tests/debug cases)
                var organization = Organizations.Find(org => org.Id == value.Id) ?? value;

                var changed = Organization != organization;  // Record comparison will check for different fields by default
                GlobalPreferences.organization = organization;
                if (changed)
                {
                    OnOrganizationChanged?.Invoke();
                    UpdateUsage();
                }
            }
        }

        public bool ShouldCheckEntitlementsOnFocus { get; set; }

        public UsageInfo Usage
        {
            get => GlobalPreferences.usage;
            set
            {
                var changed = Usage != value;
                GlobalPreferences.usage = value;
                if (changed)
                    OnUsageChanged?.Invoke();
            }
        }

        Task<(SubscriptionResponse response, string error)> m_UpdatingEntitlements;
        Task<(LegalConsentResponse response, string error)> m_UpdatingLegalConsent;

        public async Task UpdateEntitlements()
        {
            if (!UnityConnectUtils.GetIsLoggedIn())
                return;
            if (m_UpdatingEntitlements is not null)
            {
                await m_UpdatingEntitlements;
                return;
            }

            try
            {
                m_UpdatingEntitlements = GenerativeAIBackend.GetEntitlements();
                var result = await m_UpdatingEntitlements;

                if (!string.IsNullOrEmpty(result.error))
                    return; // This can happen if the token or request failed.

                AccountStatus.instance.entitlementsChecked = true;
                Organizations = result.response.orgs.ToList();
            }
            finally
            {
                m_UpdatingEntitlements = null;
            }
        }

        public async Task UpdateLegalConsent()
        {
            if (!UnityConnectUtils.GetIsLoggedIn())
                return;
            if (m_UpdatingLegalConsent is not null)
            {
                await m_UpdatingLegalConsent;
                return;
            }

            try
            {
                m_UpdatingLegalConsent = GenerativeAIBackend.GetLegalConsent();
                var result = await m_UpdatingLegalConsent;

                if (!string.IsNullOrEmpty(result.error))
                    return; // This can happen if the token or request failed.

                AccountStatus.instance.legalConsentChecked = true;
                LegalConsent = new()
                {
                    content_usage_data_training = result.response.content_usage_data_training,
                    privacy_policy_gen_ai = result.response.privacy_policy_gen_ai,
                    terms_of_service_legal_info = result.response.terms_of_service_legal_info,
                    user_id = result.response.user_id
                };
            }
            finally
            {
                m_UpdatingLegalConsent = null;
            }
        }

        public Task UpdateAccountInformation() => Task.WhenAll(UpdateEntitlements(), UpdateLegalConsent());

        public void UpdateUsage()
        {
            if (!UnityConnectUtils.GetIsLoggedIn())
                return;
            if (Organization is null or {IsEntitled: false} or {IsExpired: true})
                return;

            GenerativeAIBackend.GetUsage((result, error) =>
            {
                if (!string.IsNullOrEmpty(error))
                    return;

                Usage = new UsageInfo {used = result.points_used, total = result.points_balance + result.points_used};
            });
        }
    }
}
