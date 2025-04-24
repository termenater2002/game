using System;
using Unity.Muse.Common.Analytics;
using UnityEditor;
#if ENABLE_UNITYENGINE_ANALYTICS
using UnityEngine.Analytics;
#endif

namespace Unity.Muse.Animate.Editor
{
    /// <summary>
    /// Class responsible to register the events for each type of supported editor analytics and send them using the EditorAnalytics API.
    /// </summary>
    static class AnimateAnalyticsManager
    {
        [InitializeOnLoadMethod]
        static void Init()
        {
#if !ENABLE_UNITYENGINE_ANALYTICS
            EditorAnalytics.RegisterEventWithLimit(TextToMotionAnalyticsData.eventName, AnalyticsManager.maxEventsPerHour, 6, AnalyticsManager.vendorKey, TextToMotionAnalyticsData.version);
            EditorAnalytics.RegisterEventWithLimit(VideoToMotionAnalyticsData.eventName, AnalyticsManager.maxEventsPerHour, 6, AnalyticsManager.vendorKey, VideoToMotionAnalyticsData.version);
            EditorAnalytics.RegisterEventWithLimit(MotionCompletionAnalyticsData.eventName, AnalyticsManager.maxEventsPerHour, 6, AnalyticsManager.vendorKey, MotionCompletionAnalyticsData.version);
            EditorAnalytics.RegisterEventWithLimit(MakeEditableAnalyticsData.eventName, AnalyticsManager.maxEventsPerHour, 6, AnalyticsManager.vendorKey, MakeEditableAnalyticsData.version);
            EditorAnalytics.RegisterEventWithLimit(ExportAnalyticsData.eventName, AnalyticsManager.maxEventsPerHour, 6, AnalyticsManager.vendorKey, ExportAnalyticsData.version);
#endif
            void SendAnalytic(IAnalytic analytic)
            {
#if ENABLE_UNITYENGINE_ANALYTICS
                EditorAnalytics.SendAnalytic(analytic);
#else
                analytic.TryGatherData(out var data, out _);
                var result = EditorAnalytics.SendEventWithLimit(data.EventName, data, data.Version);
#endif
            }

            AnalyticsManager.RegisterEvent<TextToMotionAnalytic>(SendAnalytic);
            AnalyticsManager.RegisterEvent<VideoToMotionAnalytic>(SendAnalytic);
            AnalyticsManager.RegisterEvent<MotionCompletionAnalytic>(SendAnalytic);
            AnalyticsManager.RegisterEvent<MakeEditableAnalytic>(SendAnalytic);
            AnalyticsManager.RegisterEvent<ExportAnalytic>(SendAnalytic);
        }
    }
}
