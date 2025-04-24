using System.Collections.Generic;

namespace Unity.Muse.Animate
{
    class VideoToMotionService : MotionGenerationService<TimelineBakerVideoToMotion, VideoToMotionTake, VideoToMotionRequest>
    {
        public VideoToMotionService()
            : base(new TimelineBakerVideoToMotion())
        {
        }

        public VideoToMotionRequest CreateRequest(VideoToMotionTake target, string filePath, int startFrame = -1, int frameCount = -1)
        {
            return new VideoToMotionRequest(target, Baking, Baker, filePath, startFrame, frameCount);
        }
    }
}
