using System;

namespace Unity.Muse.Common.Account
{
    enum SubscriptionStatus
    {
        Entitled,
        FreeTrial,
        TrialExpired,
        SubscriptionExpired,
        NotEntitled
    }

    static class SubscriptionStatusUtils
    {
        public static SubscriptionStatus FromString(string status) =>
            status switch
            {
                "not-entitled" => SubscriptionStatus.NotEntitled,
                "free-trial" => SubscriptionStatus.FreeTrial,
                "entitled" => SubscriptionStatus.Entitled,
                "trial-expired" => SubscriptionStatus.TrialExpired,
                "subscription-expired" => SubscriptionStatus.SubscriptionExpired,
                _ => SubscriptionStatus.NotEntitled
            };

        public static string ToString(SubscriptionStatus status) =>
            status switch
            {
                SubscriptionStatus.NotEntitled => "No subscription",
                SubscriptionStatus.FreeTrial => "Free trial",
                SubscriptionStatus.Entitled => "Subscribed",
                SubscriptionStatus.TrialExpired => "Trial expired",
                SubscriptionStatus.SubscriptionExpired => "Subscription expired",
                _ => "No subscription"
            };
    }
}
