using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class BakedTimelinePlaybackUIModel
    {
        public delegate void Changed();

        public event Changed OnChanged;

        public delegate void SeekedToFrame();

        public event SeekedToFrame OnSeekedToFrame;

        public int CurrentFrame => m_PlaybackModel!=null?Mathf.RoundToInt(m_PlaybackModel.CurrentFrame):0;
        public int MinFrame => m_PlaybackModel!=null?Mathf.FloorToInt(m_PlaybackModel.MinFrame):0;
        public int MaxFrame => m_PlaybackModel!=null?Mathf.CeilToInt(m_PlaybackModel.MaxFrame):0;
        
        public bool IsVisible
        {
            get => m_IsVisible;
            set
            {
                if (value == m_IsVisible)
                    return;

                m_IsVisible = value;
                OnChanged?.Invoke();
            }
        }

        public bool IsPlaying => m_PlaybackModel.IsPlaying;
        public bool IsLooping => m_PlaybackModel.IsLooping;
        public int FramesPerSecond => Mathf.RoundToInt(m_PlaybackModel.FramesPerSecond);
        public float PlaybackSpeed => m_PlaybackModel.PlaybackSpeed;

        PlaybackModel m_PlaybackModel;
        bool m_SeekedToFrame;
        bool m_IsVisible;

        public void SetPlaybackModel(PlaybackModel playback, bool silent = false)
        {
            if (playback == m_PlaybackModel)
                return;

            UnregisterModel();
            RegisterModel(playback);
            
            if (silent)
                return;
            
            OnChanged?.Invoke();
        }

        public void Play()
        {
            if (CurrentFrame == MaxFrame)
                m_PlaybackModel.GoToStart();

            m_PlaybackModel.Play();
        }

        public void Pause()
        {
            m_PlaybackModel.Pause();
        }

        public void ToggleLooping()
        {
            m_PlaybackModel.IsLooping = !m_PlaybackModel.IsLooping;
        }

        public void SetCurrentFrame(float frame)
        {
            if (Math.Abs(m_PlaybackModel.CurrentFrame - frame) < Mathf.Epsilon)
                return;

            m_SeekedToFrame = true;
            m_PlaybackModel.CurrentFrame = frame;
        }

        public void SetNextPlaybackSpeed()
        {
            var currentPlaybackSpeed = m_PlaybackModel.PlaybackSpeed;
            var nextPlaybackSpeed = currentPlaybackSpeed.Equals(0.25f) ? 0.5f
                : currentPlaybackSpeed.Equals(0.5f) ? 1f
                : currentPlaybackSpeed + 1f;

            if (currentPlaybackSpeed.Equals(3))
            {
                nextPlaybackSpeed = 0.25f;
            }

            m_PlaybackModel.PlaybackSpeed = nextPlaybackSpeed;
        }

        public void SetFramesPerSecond(int framesPerSecond)
        {
            m_PlaybackModel.FramesPerSecond = framesPerSecond;
        }

        void RegisterModel(PlaybackModel model)
        {
            m_PlaybackModel = model;

            if (m_PlaybackModel == null)
                return;

            m_PlaybackModel.OnChanged += OnPlaybackChanged;
        }

        void UnregisterModel()
        {
            if (m_PlaybackModel == null)
                return;

            m_PlaybackModel.OnChanged -= OnPlaybackChanged;
            m_PlaybackModel = null;
        }

        void OnPlaybackChanged(PlaybackModel model, PlaybackModel.Property property)
        {
            if (m_SeekedToFrame)
            {
                OnSeekedToFrame?.Invoke();
                m_SeekedToFrame = false;
            }

            OnChanged?.Invoke();
        }
    }
}
