using Unity.Muse.Common.Analytics;
using Unity.Muse.Sprite.Analytics;
using UnityEditor;
#if ENABLE_UNITYENGINE_ANALITICS
using UnityEngine.Analytics;
#endif

namespace Unity.Muse.Sprite.Editor.Analytics
{
    static class SpriteAnalyticsManager
    {
        [InitializeOnLoadMethod]
        static void Init()
        {
#if !ENABLE_UNITYENGINE_ANALITICS
            // so the correct package is recorded as having sent the event in Unity <6
            EditorAnalytics.RegisterEventWithLimit(SaveSpriteData.eventName, AnalyticsManager.maxEventsPerHour, 6, AnalyticsManager.vendorKey);
            EditorAnalytics.RegisterEventWithLimit(GenerateAnalyticsData.eventName, AnalyticsManager.maxEventsPerHour, 6, AnalyticsManager.vendorKey);
#endif
            void SendAnalytic(IAnalytic analytic)
            {
#if ENABLE_UNITYENGINE_ANALITICS
                EditorAnalytics.SendAnalytic(analytic);
#else
                analytic.TryGatherData(out var data, out _);
                var result = EditorAnalytics.SendEventWithLimit(data.EventName, data, data.Version);
#endif
            }

            // so the correct package is recorded as having sent the event in Unity 6+
            AnalyticsManager.RegisterEvent<SaveSpriteAnalytic>(SendAnalytic);
            AnalyticsManager.RegisterEvent<GenerateAnalytic>(SendAnalytic);
        }
    }
}
