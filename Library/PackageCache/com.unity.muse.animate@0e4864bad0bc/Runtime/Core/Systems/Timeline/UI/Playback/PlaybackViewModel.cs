using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class PlaybackViewModel
    {
        public delegate void Changed();
        public event Changed OnChanged;
        public delegate void RequestedInsertKey(int keyIndex, float transitionProgress);
        public event RequestedInsertKey OnRequestedInsertKey;
        public event Action<float> OnRequestedSeekToFrame;

        public int CurrentFrame => Mathf.RoundToInt(m_PlaybackModel?.CurrentFrame ?? 0);
        public int MinFrame => Mathf.FloorToInt(m_PlaybackModel?.MinFrame ?? 0);
        public int MaxFrame => Mathf.CeilToInt(m_PlaybackModel?.MaxFrame ?? 0);

        public bool EmphasizeTransition
        {
            get => m_EmphasizeTransition;
            set
            {
                if (m_EmphasizeTransition == value)
                    return;

                m_EmphasizeTransition = value;
                OnChanged?.Invoke();
            }
        }

        public bool ShowPlusButton
        {
            get => m_ShowPlusButton && !IsPlaying && !IsCurrentFrameKey;
            set
            {
                if (m_ShowPlusButton == value)
                    return;

                m_ShowPlusButton = value;
                OnChanged?.Invoke();
            }
        }

        public bool IsPlaying => m_PlaybackModel?.IsPlaying ?? false;
        public bool IsLooping => m_PlaybackModel?.IsLooping ?? false;
        public int FramesPerSecond => Mathf.RoundToInt(m_PlaybackModel?.FramesPerSecond ?? 30);
        public float PlaybackSpeed => m_PlaybackModel?.PlaybackSpeed ?? 1f;
        public int KeyCount => m_TimelineModel?.KeyCount ?? 0;
        public bool ShowTransition => m_ShowTransition;
        public int TransitionStart => m_TransitionStart;
        public int TransitionEnd => m_TransitionEnd;
        public List<int> KeyFrames => m_KeyFrames;

        PlaybackModel m_PlaybackModel;
        TimelineModel m_TimelineModel;
        BakedTimelineMappingModel m_BakedTimelineMappingModel;
        List<int> m_KeyFrames = new();

        bool m_ShowTransition;
        int m_TransitionStart;
        int m_TransitionEnd;
        bool m_EmphasizeTransition;
        bool m_ShowPlusButton;
        
        bool IsCurrentFrameKey => m_BakedTimelineMappingModel.TryGetKeyIndex(CurrentFrame, out _);
        
        public TimelineModel TimelineModel
        {
            get => m_TimelineModel;
            set => SetTimelineModel(value);
        }
        
        public PlaybackModel PlaybackModel
        {
            get => m_PlaybackModel;
            set => SetPlaybackModel(value);
        }
        public BakedTimelineMappingModel BakedTimelineMappingModel
        {
            get => m_BakedTimelineMappingModel;
            set => SetBakedTimelineMappingModel(value);
        }

        public PlaybackViewModel(PlaybackModel playbackModel,
            TimelineModel timelineModel,
            BakedTimelineMappingModel mappingModel)
        {
            SetPlaybackModel(playbackModel, true, true);
            SetTimelineModel(timelineModel, true, true);
            SetBakedTimelineMappingModel(mappingModel, true, true);
            UpdateKeyFrames();
            UpdateTransition();
            OnChanged?.Invoke();
        }

        internal void SetTimelineModel(TimelineModel model, bool silent = false, bool skipUpdate = false)
        {
            if (m_TimelineModel == model)
                return;
            
            m_TimelineModel = model;
            
            if (!skipUpdate)
            {
                UpdateKeyFrames();
                UpdateTransition();
            }

            if (!silent)
                OnChanged?.Invoke();
        }

        internal void SetPlaybackModel(PlaybackModel model, bool silent = false, bool skipUpdate = false)
        {
            if (m_PlaybackModel == model)
                return;
            
            if(m_PlaybackModel != null)
                m_PlaybackModel.OnChanged -= OnPlaybackChanged;
        
            m_PlaybackModel = model;
            
            if(m_PlaybackModel != null)
                m_PlaybackModel.OnChanged += OnPlaybackChanged;
            
            if (!skipUpdate)
            {
                UpdateKeyFrames();
                UpdateTransition();
            }

            if (silent)
                return;
            
            OnChanged?.Invoke();
        }

        internal void SetBakedTimelineMappingModel(BakedTimelineMappingModel model, bool silent = false, bool skipUpdate = false)
        {
            if (m_BakedTimelineMappingModel == model)
                return;
            
            if(m_BakedTimelineMappingModel != null)
                m_BakedTimelineMappingModel.OnChanged -= OnBakedTimelineMappingChanged;
        
            m_BakedTimelineMappingModel = model;
            
            if(m_BakedTimelineMappingModel != null)
                m_BakedTimelineMappingModel.OnChanged += OnBakedTimelineMappingChanged;

            if (!skipUpdate)
            {
                UpdateKeyFrames();
                UpdateTransition();
            }
            
            if (silent)
                return;

            OnChanged?.Invoke();
        }
        
        public void Play()
        {
            DeepPoseAnalytics.SendTimelineAction(DeepPoseAnalytics.TimelineAction.Play);

            if (CurrentFrame == MaxFrame)
                m_PlaybackModel.GoToStart();

            m_PlaybackModel.Play();
        }

        public void Pause()
        {
            DeepPoseAnalytics.SendTimelineAction(DeepPoseAnalytics.TimelineAction.Pause);
            
            m_PlaybackModel.Pause();
        }

        public void ToggleLooping()
        {
            m_PlaybackModel.IsLooping = !m_PlaybackModel.IsLooping;

            DeepPoseAnalytics.SendTimelineAction((m_PlaybackModel.IsLooping) ? 
                DeepPoseAnalytics.TimelineAction.LoopEnable :
                DeepPoseAnalytics.TimelineAction.LoopDisable);
        }

        public void RequestSeekToFrame(float frame)
        {
            if (Math.Abs(m_PlaybackModel.CurrentFrame - frame) < Mathf.Epsilon)
                return;

            OnRequestedSeekToFrame?.Invoke(frame);
        }

        public void GoToPrevKey()
        {
            if (KeyCount == 0)
                return;

            var currentFrameIdx = Mathf.FloorToInt(m_PlaybackModel.CurrentFrame);
            if (!m_BakedTimelineMappingModel.TryGetFirstKeyBefore(currentFrameIdx, out var keyBakedFrameIndex, out var keyTimelineIndex, true))
                return;

            DeepPoseAnalytics.SendTimelineKeyAction(DeepPoseAnalytics.TimelineKeyAction.GoToPreviousKey, keyTimelineIndex);
       
            RequestSeekToFrame(keyBakedFrameIndex);
        }

        public void GoToNextKey()
        {
            if (KeyCount == 0)
                return;

            var currentFrameIdx = Mathf.FloorToInt(m_PlaybackModel.CurrentFrame);
            if (!m_BakedTimelineMappingModel.TryGetFirstKeyAfter(currentFrameIdx, out var keyBakedFrameIndex, out var keyTimelineIndex, true))
                return;

            DeepPoseAnalytics.SendTimelineKeyAction(DeepPoseAnalytics.TimelineKeyAction.GoToNextKey, keyTimelineIndex);
            
            RequestSeekToFrame(keyBakedFrameIndex);
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

            DeepPoseAnalytics.SendTimelineSetPlaybackSpeed(nextPlaybackSpeed);
        }

        public void SetFramesPerSecond(int framesPerSecond)
        {
            m_PlaybackModel.FramesPerSecond = framesPerSecond;
        }

        void OnPlaybackChanged(PlaybackModel model, PlaybackModel.Property property)
        {
            switch (property)
            {
                case PlaybackModel.Property.IsPlaying:
                    UpdateTransition();
                    break;
                
                case PlaybackModel.Property.MinTime:
                case PlaybackModel.Property.MaxTime:
                    UpdateKeyFrames();
                    UpdateTransition();
                    break;
                
                case PlaybackModel.Property.CurrentTime:
                    UpdateTransition();
                    break;
                
                case PlaybackModel.Property.IsLooping:
                case PlaybackModel.Property.FramesPerSecond:
                case PlaybackModel.Property.PlaybackSpeed:
                    // Nothing to update in this case
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(property), property, null);
            }
            
            OnChanged?.Invoke();
        }

        void OnBakedTimelineMappingChanged(BakedTimelineMappingModel model)
        {
            UpdateKeyFrames();
            UpdateTransition();
            OnChanged?.Invoke();
        }

        void UpdateKeyFrames()
        {
            if (m_TimelineModel == null || m_BakedTimelineMappingModel == null)
                return;

            m_KeyFrames.Clear();

            // Note: we skip first and last key for display
            for (var i = 0; i < m_TimelineModel.KeyCount; i++)
            {
                if (!m_BakedTimelineMappingModel.TryGetBakedKeyIndex(i, out var bakedFrameIndex))
                    continue;

                m_KeyFrames.Add(bakedFrameIndex);
            }
        }

        void UpdateTransition()
        {
            m_ShowTransition = false;
            
            if (m_BakedTimelineMappingModel == null || m_PlaybackModel == null)
                return;
            
            if (KeyCount == 0)
                return;
            
            // Get the closest key in the past
            if (!m_BakedTimelineMappingModel.TryGetFirstKeyBefore(CurrentFrame, out var startBakedFrameIndex, out var keyTimelineIndex, false))
                return;
            
            // Get the out transition of that key
            var key = m_TimelineModel.GetKey(keyTimelineIndex);
            var transition = key.OutTransition;

            // Special case for last key
            if (transition == null)
            {
                m_ShowTransition = true;
                m_TransitionStart = startBakedFrameIndex;
                m_TransitionEnd = startBakedFrameIndex + 1;
                return;
            }

            // Get the index of the out transition
            var transitionIdx = m_TimelineModel.IndexOf(transition);
            if (transitionIdx < 0)
                return;

            // Get the baked range for the transition
            if (!m_BakedTimelineMappingModel.TryGetBakedTransitionSegment(transitionIdx, out startBakedFrameIndex, out var endBakedFrameIndex))
                return;

            m_ShowTransition = true;
            m_TransitionStart = startBakedFrameIndex;
            m_TransitionEnd = endBakedFrameIndex;
        }

        public void InsertKeyAtCurrentFrame()
        {
            m_BakedTimelineMappingModel.GetBakedKeyProgressAt(m_PlaybackModel.CurrentFrame, out var keyIndex, out var transitionProgress);
            DevLogger.LogSeverity(TraceLevel.Info, $"GetBakedKeyProgressAt({m_PlaybackModel.CurrentFrame} -> {keyIndex}, {transitionProgress})");

            DeepPoseAnalytics.SendTimelineKeyAction(DeepPoseAnalytics.TimelineKeyAction.InsertKey, keyIndex + 1);

            UnityEngine.Debug.Assert(transitionProgress > 0, "Attempting to insert on top of another keyframe. This will result in unexpected behaviour.");
            OnRequestedInsertKey?.Invoke(keyIndex+1, transitionProgress);
        }
    }
}
