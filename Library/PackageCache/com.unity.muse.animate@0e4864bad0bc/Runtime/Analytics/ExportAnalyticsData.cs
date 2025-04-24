using System;
using Unity.Muse.Common.Analytics;
#if ENABLE_UNITYENGINE_ANALYTICS
using UnityEngine.Analytics;
#endif

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Analytics data for the event of exporting an animation.
    /// </summary>
    [Serializable]
    class ExportAnalyticsData : IAnalytic.IData
    {
        public const string eventName = "muse_animate_export";
        public const int version = 1;
#if !ENABLE_UNITYENGINE_ANALYTICS
        public string EventName => eventName;
        public int Version => version;
#endif

        public string prompt;
        public string export_type;
        public string export_flow;
    }

    /// <summary>
    /// Analytics container for the event of exporting an animation.
    /// </summary>
#if ENABLE_UNITYENGINE_ANALYTICS
    [AnalyticInfo(eventName: ExportAnalyticsData.eventName, vendorKey: AnalyticsManager.vendorKey, ExportAnalyticsData.version, AnalyticsManager.maxEventsPerHour)]
#endif
    class ExportAnalytic : IAnalytic
    {
        string m_Prompt;
        string m_ExportType;
        string m_ExportFlow;

        internal ExportAnalytic(string prompt, string exportType, string exportFlow)
        {
            m_Prompt = prompt;
            m_ExportType = exportType;
            m_ExportFlow = exportFlow;
        }

        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            var parameters = new ExportAnalyticsData
            {
                prompt = m_Prompt,
                export_type = m_ExportType,
                export_flow = m_ExportFlow,
            };
            data = parameters;
            return data != null;
        }
    }
}
