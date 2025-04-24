using System;

namespace Unity.Muse.Animate
{
    class VideoToMotionRequest : BakingRequest<VideoToMotionTake>
    {
        public string FilePath => m_FilePath;
        public int StartFrame => m_StartFrame;
        public int FrameCount => m_FrameCount;
        public TimelineBakerVideoToMotion.Model Model => m_Model;

        string m_FilePath;
        int m_StartFrame;
        int m_FrameCount;
        TimelineBakerVideoToMotion.Model m_Model;

        TimelineBakerVideoToMotion m_Baker;

        public VideoToMotionRequest(VideoToMotionTake target,
            BakingLogic baking,
            TimelineBakerVideoToMotion baker,
            string filePath,
            int startFrame = -1,
            int frameCount = -1,
            TimelineBakerVideoToMotion.Model model = TimelineBakerVideoToMotion.Model.V1)
            : base(baking)
        {
            Target = target;
            m_Baker = baker;
            m_FilePath = filePath;
            m_StartFrame = startFrame;
            m_FrameCount = frameCount;
            m_Model = model;
        }

        protected override void InitializeParameters()
        {
            m_Baker.FilePath = m_FilePath;
            m_Baker.StartFrame = m_StartFrame >= 0 ? m_StartFrame : null;
            m_Baker.FrameCount = m_FrameCount >= 0 ? m_FrameCount : null;
        }

        protected override void FinalizeBaking(BakedTimelineModel output) { }
    }
}
