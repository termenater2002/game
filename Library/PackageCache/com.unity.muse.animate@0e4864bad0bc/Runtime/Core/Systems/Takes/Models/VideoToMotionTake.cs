using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// A take containing dense motion, including a path to a video file. If the file exists,
    /// the video will be displayed along with the motion.
    /// </summary>
    [Serializable]
    sealed class VideoToMotionTake : DenseTake, ICopyable<VideoToMotionTake>
    {
        public string FilePath => m_FilePath;
        public int StartFrame => m_StartFrame;

        public int FrameCount => m_FrameCount;

        [SerializeField]
        string m_FilePath;

        [SerializeField]
        int m_StartFrame;

        [SerializeField]
        int m_FrameCount;

        public VideoToMotionTake(
            string title,
            string filePath,
            int startFrame,
            int frameCount
        )
            : base(
                title,
                System.IO.Path.GetFileNameWithoutExtension(filePath),
                LibraryItemType.VideoToMotionTake,
                true)
        {
            m_FilePath = filePath;
            m_StartFrame = startFrame;
            m_FrameCount = frameCount;
        }

        void ICopyable<VideoToMotionTake>.CopyTo(VideoToMotionTake item)
        {
            base.CopyTo(item);
        }

        public new VideoToMotionTake Clone()
        {
            var clone = new VideoToMotionTake(Title, m_FilePath, m_StartFrame, m_FrameCount);
            CopyTo(clone);
            return clone;
        }
    }
}
