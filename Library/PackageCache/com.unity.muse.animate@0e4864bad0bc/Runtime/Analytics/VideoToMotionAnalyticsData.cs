using System;
using Unity.Muse.Common.Analytics;
#if ENABLE_UNITYENGINE_ANALYTICS
using UnityEngine.Analytics;
#endif

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Analytics data for video to motion generation request.
    /// </summary>
    [Serializable]
    class VideoToMotionAnalyticsData : IAnalytic.IData
    {
        public const string eventName = "muse_animate_video_to_motion";
        public const int version = 1;
#if !ENABLE_UNITYENGINE_ANALYTICS
        public string EventName => eventName;
        public int Version => version;
#endif

        public string filename;
        public int start_frame;
        public int frame_count;
        public string model;
    }

    /// <summary>
    /// Analytics container for video to motion generation request.
    /// </summary>
#if ENABLE_UNITYENGINE_ANALYTICS
    [AnalyticInfo(eventName: VideoToMotionAnalyticsData.eventName, vendorKey: AnalyticsManager.vendorKey, VideoToMotionAnalyticsData.version, AnalyticsManager.maxEventsPerHour)]
#endif
    class VideoToMotionAnalytic : IAnalytic
    {
        string m_Filename;
        int m_StartFrame;
        int m_FrameCount;
        string m_Model;

        internal VideoToMotionAnalytic(string filename, int startFrame, int frameCount, TimelineBakerVideoToMotion.Model model)
        {
            m_Filename = filename;
            m_StartFrame = startFrame;
            m_FrameCount = frameCount;
            m_Model = model.ToString();
        }

        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            var parameters = new VideoToMotionAnalyticsData
            {
                filename = m_Filename,
                start_frame = m_StartFrame,
                frame_count = m_FrameCount,
                model = m_Model,
            };
            data = parameters;
            return data != null;
        }
    }
}
