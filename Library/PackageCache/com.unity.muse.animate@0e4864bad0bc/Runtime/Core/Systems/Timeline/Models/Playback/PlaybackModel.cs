using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class PlaybackModel
    {
        const float k_Epsilon = 1e-5f;

        public enum Property
        {
            IsPlaying,
            IsLooping,
            MinTime,
            MaxTime,
            CurrentTime,
            FramesPerSecond,
            PlaybackSpeed
        }

        [SerializeField]
        PlaybackData m_Data;

        public bool IsPlaying
        {
            get => m_Data.IsPlaying;
            set
            {
                if (value == m_Data.IsPlaying)
                    return;

                m_Data.IsPlaying = value;
                if (m_Data.IsPlaying)
                    m_Data.TimeWhenStartedPlaying = CurrentTime;

                OnChanged?.Invoke(this, Property.IsPlaying);
            }
        }

        public bool IsLooping
        {
            get => m_Data.IsLooping;
            set
            {
                if (value == m_Data.IsLooping)
                    return;

                m_Data.IsLooping = value;
                OnChanged?.Invoke(this, Property.IsLooping);
            }
        }

        public float Duration => MaxTime - MinTime;

        public float MinTime
        {
            get => m_Data.MinTime;
            set
            {
                if (value.NearlyEquals(m_Data.MinTime, k_Epsilon))
                    return;

                m_Data.MinTime = value;
                if (CurrentTime < m_Data.MinTime)
                    CurrentTime = m_Data.MinTime;
                OnChanged?.Invoke(this, Property.MinTime);
            }
        }

        public float MaxTime
        {
            get => m_Data.MaxTime;
            set
            {
                if (value.NearlyEquals(m_Data.MaxTime, k_Epsilon))
                    return;

                m_Data.MaxTime = value;
                if (CurrentTime > m_Data.MaxTime)
                    CurrentTime = m_Data.MaxTime;
                OnChanged?.Invoke(this, Property.MaxTime);
            }
        }

        public float CurrentTime
        {
            get => m_Data.CurrentTime;
            set
            {
                var correctedValue = Mathf.Clamp(value, MinTime, MaxTime);
                if (correctedValue.NearlyEquals(m_Data.CurrentTime, k_Epsilon))
                    return;

                m_Data.CurrentTime = correctedValue;
                OnChanged?.Invoke(this, Property.CurrentTime);
            }
        }

        public float PlaybackSpeed
        {
            get => m_Data.PlaybackSpeed;
            set
            {
                if (value.NearlyEquals(m_Data.PlaybackSpeed, k_Epsilon))
                    return;

                m_Data.PlaybackSpeed = value;
                OnChanged?.Invoke(this, Property.PlaybackSpeed);
            }
        }

        public float FramesPerSecond
        {
            get => m_Data.FramesPerSecond;
            set
            {
                if (value.NearlyEquals(m_Data.FramesPerSecond, k_Epsilon))
                    return;

                m_Data.FramesPerSecond = value;
                OnChanged?.Invoke(this, Property.FramesPerSecond);
            }
        }

        public float CurrentFrame
        {
            get => ToFrame(CurrentTime);
            set => CurrentTime = ToTime(value);
        }

        public float MinFrame
        {
            get => ToFrame(MinTime);
            set => MinTime = ToTime(value);
        }

        public float MaxFrame
        {
            get => ToFrame(MaxTime);
            set => MaxTime = ToTime(value);
        }

        public delegate void Changed(PlaybackModel model, Property property);
        public event Changed OnChanged;

        public delegate void Looped(PlaybackModel model);
        public event Looped OnLooped;

        [JsonConstructor]
        public PlaybackModel(PlaybackData m_Data)
        {
            this.m_Data = m_Data;
        }

        public PlaybackModel(float duration, float fps)
        {
            m_Data.IsPlaying = false;
            m_Data.IsLooping = false;
            m_Data.MinTime = 0f;
            m_Data.MaxTime = duration;
            m_Data.CurrentTime = 0f;
            m_Data.PlaybackSpeed = 1f;
            m_Data.FramesPerSecond = fps;
            m_Data.TimeWhenStartedPlaying = 0f;
        }

        public void Update(float deltaTime)
        {
            if (!IsPlaying)
                return;

            var timeOffset = deltaTime * PlaybackSpeed;
            var newTime = CurrentTime + timeOffset;

            if (newTime > MaxTime)
            {
                if (IsLooping && MaxTime > MinTime)
                {
                    while (newTime > MaxTime)
                    {
                        newTime -= Duration;
                        OnLooped?.Invoke(this);
                    }
                }
                else
                {
                    newTime = MaxTime;
                    IsPlaying = false;
                }
            }

            CurrentTime = newTime;
        }

        public void Play(bool restart = false)
        {
            if (IsPlaying)
                return;

            if (restart)
                CurrentTime = MinTime;

            IsPlaying = true;
        }

        public void Stop(bool rewindToInitialTime = true)
        {
            if (!IsPlaying)
                return;

            IsPlaying = false;
            if (rewindToInitialTime)
                CurrentTime = m_Data.TimeWhenStartedPlaying;
        }

        public void Pause()
        {
            Stop(false);
        }

        public void GoToStart()
        {
            CurrentTime = MinTime;
        }

        public void GoToEnd()
        {
            CurrentTime = MaxTime;
        }

        public float ToFrame(float time)
        {
            return time * FramesPerSecond;
        }

        public float ToTime(float frame)
        {
            return frame / FramesPerSecond;
        }
    }
}
