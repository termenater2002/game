using System;

namespace Unity.Muse.Common.Account
{
    [Serializable]
    record OrganizationInfo
    {
        public string org_id;
        public string org_name;
        public string status;

        public string Label => org_name;
        public string Id => org_id;

        public SubscriptionStatus Status => SubscriptionStatusUtils.FromString(status);
        public bool IsExpired => Status is SubscriptionStatus.SubscriptionExpired or SubscriptionStatus.TrialExpired;
        public bool IsEntitled => this is {Status: SubscriptionStatus.FreeTrial} or {Status: SubscriptionStatus.Entitled};
        public bool HasMuseAccount => IsEntitled || IsExpired;     // If the user has a subscription or ever had one


        public void CopyTo(OrganizationInfo target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            target.org_id = org_id;
            target.org_name = org_name;
            target.status = status;
        }
    }
}
