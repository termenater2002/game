using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// The ViewModel that provides the UI for playing back a video-to-motion take.
    /// </summary>
    class VideoToMotionPlaybackUIModel
    {
        public BakedTimelinePlaybackUIModel PlaybackUIModel { get; }

        public enum Property
        {
            Visibility,
            IsBusy,
            IsBaking,
            CanExport,
            CanMakeEditable,
            VideoPath,
        }
        
        public event Action<Property> OnChanged;
        public event Action OnPlaybackChanged;
        public event Action<float> OnSeekedToTime;
        
        public float PlaybackTime
        {
            get => m_VideoToMotionAuthoringModel.DisplayTime;
            set => m_VideoToMotionAuthoringModel.DisplayTime = value;
        }

        public string VideoPath => m_VideoToMotionAuthoringModel.PlaybackTarget?.FilePath;
        
        public string Title => m_VideoToMotionAuthoringModel.PlaybackTarget?.Title;

        /// <summary>
        /// The frame of the video that we should start playing on when previewing the take.
        /// </summary>
        public int PreviewStartFrame => m_VideoToMotionAuthoringModel.PlaybackTarget?.StartFrame ?? 0;

        /// <summary>
        /// The number of frames in the video that we should play when previewing the take.
        /// </summary>
        public int PreviewFrameCount => m_VideoToMotionAuthoringModel.PlaybackTarget?.FrameCount ?? 0;

        VideoToMotionAuthoringModel m_VideoToMotionAuthoringModel;
        
        bool m_IsVisible;
        bool m_IsBusy;
        bool m_CanExport;
        bool m_CanMakeEditable;
        bool m_IsBakingCurrentTake;
        
        public VideoToMotionPlaybackUIModel(VideoToMotionAuthoringModel authoringModel, BakedTimelinePlaybackUIModel playbackUI)
        {
            m_VideoToMotionAuthoringModel = authoringModel;
            m_VideoToMotionAuthoringModel.OnChanged += OnModelChanged;
            
            PlaybackUIModel = playbackUI;
            
            // Pass through the playback model events
            PlaybackUIModel.OnChanged += TriggerPlaybackChanged;
            PlaybackUIModel.OnSeekedToFrame += TriggerSeekToFrame;
        }

        void TriggerSeekToFrame()
        {
            OnSeekedToTime?.Invoke((float)PlaybackUIModel.CurrentFrame / PlaybackUIModel.FramesPerSecond);
        }

        public bool IsVisible
        {
            get => m_IsVisible;
            set
            {
                if (value == m_IsVisible)
                    return;

                m_IsVisible = value;
                PlaybackUIModel.IsVisible = m_IsVisible;
                OnChanged?.Invoke(Property.Visibility);
            }
        }

        public bool IsBakingCurrentTake
        {
            get => m_IsBakingCurrentTake;
            set
            {
                if (value == m_IsBakingCurrentTake)
                    return;

                m_IsBakingCurrentTake = value;
                OnChanged?.Invoke(Property.IsBaking);
            }
        }
        
        public bool CanExport
        {
            get => m_CanExport;
            set
            {
                if (value == m_CanExport)
                    return;

                m_CanExport = value;
                OnChanged?.Invoke(Property.CanExport);
            }
        }
        
        public bool CanMakeEditable
        {
            get => m_CanMakeEditable;
            set
            {
                if (value == m_CanMakeEditable)
                    return;

                m_CanMakeEditable = value;
                OnChanged?.Invoke(Property.CanMakeEditable);
            }
        }
        
        public bool IsBusy
        {
            get => m_IsBusy;
            set
            {
                if (value == m_IsBusy)
                    return;

                m_IsBusy = value;
                OnChanged?.Invoke(Property.IsBusy);
            }
        }
        
        public bool IsPlaying
        {
            get => PlaybackUIModel.IsPlaying;
            set
            {
                if (value == PlaybackUIModel.IsPlaying)
                    return;
                
                if (value)
                {
                    PlaybackUIModel.Play();
                }
                else
                {
                    PlaybackUIModel.Pause();
                }
            }
        }

        public bool IsLooping => PlaybackUIModel.IsLooping;
        
        public float PlaybackSpeed => PlaybackUIModel.PlaybackSpeed;

        void OnModelChanged(VideoToMotionAuthoringModel.Property property)
        {
            if (property == VideoToMotionAuthoringModel.Property.Target)
            {
                OnChanged?.Invoke(Property.VideoPath);
            }
        }
        
        void TriggerPlaybackChanged()
        {
            OnPlaybackChanged?.Invoke();
        }

        public void RequestExport()
        {
            m_VideoToMotionAuthoringModel?.RequestExport();
        }

        public void RequestExtractKeys()
        {
            m_VideoToMotionAuthoringModel.RequestExtractKeys();
        }

        public void RequestDelete()
        {
            m_VideoToMotionAuthoringModel.RequestDelete();
        }
    }
}
