using System;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents the MotionToTimeline Authoring Model of the Application.
    /// </summary>
    /// <remarks>
    /// Handles requests for actions related to creating a new <see cref="KeySequenceTake"/> with keys sampled from a <see cref="DenseTake"/>.
    /// </remarks>
    class MotionToTimelineAuthoringModel
    {
        public event Action<Property> OnChanged;
        public event Action OnStepChanged;
        public event Action OnRequestConfirm;
        public event Action OnRequestCancel;
        public event Action OnRequestPreview;

        public event Action<TimelineModel.SequenceKey> OnRequestedSeekToKey;
        public event Action<int> OnRequestedSeekToFrame;

        /// <summary>
        /// Motion to timeline authoring steps.
        /// </summary>
        public enum AuthoringStep
        {
            /// <summary>No mode set</summary>
            None,
            /// <summary>Input hasn't been decimated yet</summary>
            NoPreview,
            /// <summary>Input is being decimated to timeline keys</summary>
            PreviewIsSamplingMotionToKeys,
            /// <summary>Input has been decimated to timeline keys and is now baking the preview timeline keys result</summary>
            PreviewIsBakingTimelineOutput,
            /// <summary>The timeline output is baked and available</summary>
            PreviewIsAvailable,
            /// <summary>The timeline output is baked and available but it is obsolete compared to the parsing parameters selected</summary>
            PreviewIsObsolete
        }

        public enum Property
        {
            Target,
            Step,
            FrameDensity,
            UseMotionCompletion,
            IsPreviewObsolete
        }

        AuthoringStep m_Step;
        bool m_UseMotionCompletion;
        bool m_IsPreviewObsolete;
        float m_KeyFrameSamplingSensitivity = 0.5f;

        public AuthoringStep Step
        {
            get => m_Step;
            set
            {
                if (m_Step == value)
                    return;

                m_Step = value;
                OnChanged?.Invoke(Property.Step);
                OnStepChanged?.Invoke();
            }
        }

        public float KeyFrameSamplingSensitivity
        {
            get => m_KeyFrameSamplingSensitivity;
            set
            {
                if (m_KeyFrameSamplingSensitivity == value)
                    return;

                m_KeyFrameSamplingSensitivity = value;
                OnChanged?.Invoke(Property.FrameDensity);
            }
        }
        
        public bool UseMotionCompletion
        {
            get => m_UseMotionCompletion;
            set
            {
                if ( m_UseMotionCompletion == value)
                    return;

                m_UseMotionCompletion = value;
                OnChanged?.Invoke(Property.UseMotionCompletion);
            }
        }

        public bool IsPreviewObsolete
        {
            get => m_IsPreviewObsolete;
            set
            {
                if (m_IsPreviewObsolete == value)
                    return;

                m_IsPreviewObsolete = value;
                OnChanged?.Invoke(Property.IsPreviewObsolete);
            }
        }

        internal void RegisterToUI(TimelineViewModel timelineViewModel)
        {
            timelineViewModel.OnRequestedEditKey += OnTimelineUIRequestedEditKey;
            timelineViewModel.OnRequestedKeyToggle += OnTimelineUIRequestedKeyToggle;
            timelineViewModel.OnRequestedSeekToKey += OnTimelineUIRequestedSeekToKey;
            timelineViewModel.OnRequestedSeekToFrame += OnTimelineUIRequestedSeekToFrame;
            timelineViewModel.OnRequestedMoveKey += OnTimelineUIRequestedMoveKey;
            timelineViewModel.OnRequestedDuplicateKey += OnTimelineUIRequestedDuplicateKey;
            timelineViewModel.OnRequestedDeleteKey += OnTimelineUIRequestedDeleteKey;
        }

        internal void UnregisterFromUI(TimelineViewModel timelineViewModel)
        {
            timelineViewModel.OnRequestedEditKey -= OnTimelineUIRequestedEditKey;
            timelineViewModel.OnRequestedKeyToggle -= OnTimelineUIRequestedKeyToggle;
            timelineViewModel.OnRequestedSeekToKey -= OnTimelineUIRequestedSeekToKey;
            timelineViewModel.OnRequestedSeekToFrame -= OnTimelineUIRequestedSeekToFrame;
            timelineViewModel.OnRequestedMoveKey -= OnTimelineUIRequestedMoveKey;
            timelineViewModel.OnRequestedDuplicateKey -= OnTimelineUIRequestedDuplicateKey;
            timelineViewModel.OnRequestedDeleteKey -= OnTimelineUIRequestedDeleteKey;
        }

        // [Section] Timeline UI Event Handlers

        void OnTimelineUIRequestedKeyToggle(TimelineModel.SequenceKey key)
        {
            AskSeekToKey(key);
        }

        void OnTimelineUIRequestedDeleteKey(TimelineModel.SequenceKey key)
        {
            //AskDeleteKey(key);
        }

        void OnTimelineUIRequestedDuplicateKey(int fromIndex, int toIndex)
        {
            //AskDuplicateKey(fromIndex, toIndex);
        }

        void OnTimelineUIRequestedMoveKey(int fromIndex, int toIndex)
        {
            //AskMoveKey(fromIndex, toIndex);
        }

        void OnTimelineUIRequestedEditKey(TimelineModel.SequenceKey key)
        {
            AskSeekToKey(key);
        }

        void OnTimelineUIRequestedSeekToFrame(float frame)
        {
            AskSeekToFrame((int)frame);
        }

        void OnTimelineUIRequestedSeekToKey(TimelineModel.SequenceKey key)
        {
            AskSeekToKey(key);
        }

        void OnPlaybackUIRequestedSeekToFrame(float frame)
        {
            AskSeekToFrame((int)frame);
        }

        public void RequestPreview()
        {
            OnRequestPreview?.Invoke();
        }
        
        public void RequestConfirm()
        {
            OnRequestConfirm?.Invoke();
        }

        public void RequestCancel()
        {
            OnRequestCancel?.Invoke();
        }

        void AskSeekToFrame(int frame)
        {
            RequestSeekToFrame(frame);
        }

        void AskSeekToKey(TimelineModel.SequenceKey key)
        {
            RequestSeekToKey(key);
        }

        void RequestSeekToFrame(int frame)
        {
            OnRequestedSeekToFrame?.Invoke(frame);
        }

        void RequestSeekToKey(TimelineModel.SequenceKey key)
        {
            OnRequestedSeekToKey?.Invoke(key);
        }
    }
}
