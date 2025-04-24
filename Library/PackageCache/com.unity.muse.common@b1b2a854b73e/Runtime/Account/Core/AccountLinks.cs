using System;
using UnityEngine.Device;

namespace Unity.Muse.Common.Account
{
    static class AccountLinks
    {
        public static void StartSubscription() => Application.OpenURL("https://store.unity.com/configure-plan/unity-muse");
        public static void ViewPricing() => Application.OpenURL("https://unity.com/products/muse");
        public static void TrialLearnMore() => Application.OpenURL("https://unity.com/products/muse");
        public static void PrivacyNotice() => Application.OpenURL("https://unity.com/legal/supplemental-privacy-statement-unity-muse");
        public static void PrivacyPolicy() => Application.OpenURL("https://unity.com/legal/developer-privacy-policy");
        public static void PrivacyStatement() => Application.OpenURL("https://unity.com/legal/supplemental-privacy-statement-unity-muse");
        public static void TermsOfService() => Application.OpenURL("https://unity.com/legal/terms-of-service");
        public static void LegalInfo() => Application.OpenURL("https://unity.com/legal");
        public static void RequestSeat() => Application.OpenURL("https://id.unity.com/en/organizations");
        public static void LearnMuse() => Application.OpenURL("https://learn.unity.com/project/prototype-a-game-with-unity-muse-AI");
        public static void ExperimentalProgram() => Application.OpenURL("https://discussions.unity.com/t/about-muse-ai-2024-experimental/361144");
        public static void SignUpExperimentalProgram() => Application.OpenURL("https://create.unity.com/ai-sign-up");
    }
}
