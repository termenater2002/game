using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Muse.Common.Account
{
    class TrialForm
    {
        public bool startTrial;
        public OrganizationInfo organization;
        public LegalConsentRequest legalConsent = new();
        internal bool processing;
        public AccountState state = AccountState.Trial;

        public async Task Apply()
        {
            try
            {
                // Avoids changing the state to another dialog while processing the trial start
                // because of various organization and legal change events
                processing = true;

                if (startTrial)
                    await GenerativeAIBackend.StartTrial(organization.Id);

                // No need to send it again if the user has already consented
                if (!AccountInfo.Instance.LegalConsent.HasConsented)
                    await GenerativeAIBackend.SetLegalConsent(legalConsent);

                // Fetch server information to update the account status
                await AccountInfo.Instance.UpdateAccountInformation();

                // Organization can be null if the user is not starting a trial and only being asked for legal consent.
                if (organization is not null)
                    AccountInfo.Instance.Organization = organization; // Switch to trial form's organization
            }
            finally
            {
                processing = false;
            }
        }
    }
}
