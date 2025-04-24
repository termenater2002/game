using System;
using Unity.Muse.Common.Analytics;
#if ENABLE_UNITYENGINE_ANALYTICS
using UnityEngine.Analytics;
#endif

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Analytics data for the event of making a take editable.
    /// </summary>
    [Serializable]
    class MakeEditableAnalyticsData : IAnalytic.IData
    {
        public const string eventName = "muse_animate_make_editable";
        public const int version = 1;
#if !ENABLE_UNITYENGINE_ANALYTICS
        public string EventName => eventName;
        public int Version => version;
#endif

        public string prompt;
    }

    /// <summary>
    /// Analytics container for the event of making a take editable.
    /// </summary>
#if ENABLE_UNITYENGINE_ANALYTICS
    [AnalyticInfo(eventName: MakeEditableAnalyticsData.eventName, vendorKey: AnalyticsManager.vendorKey, MakeEditableAnalyticsData.version, AnalyticsManager.maxEventsPerHour)]
#endif
    class MakeEditableAnalytic : IAnalytic
    {
        string m_Prompt;

        internal MakeEditableAnalytic(string prompt)
        {
            m_Prompt = prompt;
        }

        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            var parameters = new MakeEditableAnalyticsData
            {
                prompt = m_Prompt,
            };
            data = parameters;
            return data != null;
        }
    }
}
