using System;
using Unity.Muse.Common.Analytics;
#if ENABLE_UNITYENGINE_ANALYTICS
using UnityEngine.Analytics;
#endif

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Analytics data for text to motion generation request.
    /// </summary>
    [Serializable]
    class TextToMotionAnalyticsData : IAnalytic.IData
    {
        public const string eventName = "muse_animate_text_to_motion";
        public const int version = 2;
#if !ENABLE_UNITYENGINE_ANALYTICS
        public string EventName => eventName;
        public int Version => version;
#endif

        public string prompt;
        public int frame_count;
    }

    /// <summary>
    /// Analytics container for text to motion generation request.
    /// </summary>
#if ENABLE_UNITYENGINE_ANALYTICS
    [AnalyticInfo(eventName: TextToMotionAnalyticsData.eventName, vendorKey: AnalyticsManager.vendorKey, TextToMotionAnalyticsData.version, AnalyticsManager.maxEventsPerHour)]
#endif
    class TextToMotionAnalytic : IAnalytic
    {
        string m_Prompt;
        int m_FrameCount;

        internal TextToMotionAnalytic(string prompt, int frameCount)
        {
            m_Prompt = prompt;
            m_FrameCount = frameCount;
        }

        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            var parameters = new TextToMotionAnalyticsData
            {
                prompt = m_Prompt,
                frame_count = m_FrameCount,
            };
            data = parameters;
            return data != null;
        }
    }
}
