using System;
using UnityEngine;
using UnityEngine.Video;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents the Video to Motion Authoring Model of the Application.
    /// </summary>
    /// <remarks>
    /// Handles requests for actions related to Authoring a <see cref="VideoToMotionTake"/>.
    /// </remarks>
    class VideoToMotionAuthoringModel
    {
        public enum Property
        {
            Target,
            RequestFilePath,
            RequestStartFrame,
            RequestFrameCount,
            PlaybackTime
        }
        
        // Events for App Logic
        public event Action<string, int, int> OnGenerateRequested;
        public event Action<VideoToMotionTake> OnRequestExtractKeys;
        public event Action<VideoToMotionTake> OnRequestExport;
        public event Action<VideoToMotionTake> OnRequestDelete;
        
        // Events for UI updates
        public event Action<Property> OnChanged;
        
        // TODO: Add properties about the video
        
        // The properties for generating a new take
        int m_StartFrame;
        int m_FrameCount;
        string m_VideoPath;
        
        // The generated take being previewed (i.e. a previously-generated take)
        VideoToMotionTake m_PlaybackTarget;

        float m_CurrentDisplayTime;
        
        const float k_Epsilon = 1e-5f;
        
        public VideoToMotionTake PlaybackTarget
        {
            get => m_PlaybackTarget;
            set
            {
                if (m_PlaybackTarget == value)
                    return;

                m_PlaybackTarget = value;
                OnChanged?.Invoke(Property.Target);
            }
        }
        
        public string VideoPath
        {
            get => m_VideoPath;
            set
            {
                if (m_VideoPath == value)
                    return;

                m_VideoPath = value;
                OnChanged?.Invoke(Property.RequestFilePath);
            }
        }
        
        public int StartFrame
        {
            get => m_StartFrame;
            set
            {
                if (m_StartFrame == value)
                    return;

                m_StartFrame = value;
                OnChanged?.Invoke(Property.RequestStartFrame);
            }
        }
        
        public int FrameCount
        {
            get => m_FrameCount;
            set
            {
                if (m_FrameCount == value)
                    return;

                m_FrameCount = value;
                OnChanged?.Invoke(Property.RequestFrameCount);
            }
        }
        
        public float DisplayTime
        {
            get => m_CurrentDisplayTime;
            set
            {
                if (Mathf.Abs(m_CurrentDisplayTime - value) < k_Epsilon)
                    return;
        
                m_CurrentDisplayTime = value;
                OnChanged?.Invoke(Property.PlaybackTime);
            }
        }

        public void RequestGenerate()
        {
            OnGenerateRequested?.Invoke(m_VideoPath, m_StartFrame, m_FrameCount);
        }

        public void RequestExport()
        {
            OnRequestExport?.Invoke(PlaybackTarget);
        }

        public void RequestExtractKeys()
        {
            OnRequestExtractKeys?.Invoke(PlaybackTarget);
        }

        public void RequestDelete()
        {
            OnRequestDelete?.Invoke(PlaybackTarget);
        }
    }
}
