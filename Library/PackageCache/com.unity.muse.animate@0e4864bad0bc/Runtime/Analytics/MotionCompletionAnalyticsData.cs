using System;
using Unity.Muse.Common.Analytics;
#if ENABLE_UNITYENGINE_ANALYTICS
using UnityEngine.Analytics;
#endif

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Analytics data for the event of requesting a motion completion.
    /// </summary>
    [Serializable]
    class MotionCompletionAnalyticsData : IAnalytic.IData
    {
        public const string eventName = "muse_animate_motion_completion";
        public const int version = 1;
#if !ENABLE_UNITYENGINE_ANALYTICS
        public string EventName => eventName;
        public int Version => version;
#endif
    }

    /// <summary>
    /// Analytics container for the event of requesting a motion completion.
    /// </summary>
#if ENABLE_UNITYENGINE_ANALYTICS
    [AnalyticInfo(eventName: MotionCompletionAnalyticsData.eventName, vendorKey: AnalyticsManager.vendorKey, MotionCompletionAnalyticsData.version, AnalyticsManager.maxEventsPerHour)]
#endif
    class MotionCompletionAnalytic : IAnalytic
    {
        internal MotionCompletionAnalytic()
        {
        }

        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            var parameters = new MotionCompletionAnalyticsData();
            data = parameters;
            return data != null;
        }
    }
}
