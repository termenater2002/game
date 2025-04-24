using System;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// The UI of the TextToMotion workflow.
    /// </summary>
    class MotionToTimelineUIModel
    {
        public event Action<Property> OnChanged;

        public enum Property
        {
            Visibility,
            Step,
            IsSamplingMotionToKeys,
            IsBakingOutputTimeline,
            UseMotionCompletion,
            KeyframeSamplingSensitivity,
            CanConfirm,
            CanConvert
        }

        public bool IsVisible
        {
            get => m_IsVisible;
            set
            {
                if (value == m_IsVisible)
                    return;

                m_IsVisible = value;
                OnChanged?.Invoke(Property.Visibility);
            }
        }
        
        public MotionToTimelineAuthoringModel.AuthoringStep Step
        {
            get => m_Step;
            set
            {
                if (value == m_Step)
                    return;

                m_Step = value;
                OnChanged?.Invoke(Property.Step);
            }
        }

        public bool UseMotionCompletionSampling
        {
            get => m_Model.UseMotionCompletion;
            set
            {
                if (value == m_Model.UseMotionCompletion)
                    return;

                m_Model.UseMotionCompletion = value;
            }
        }

        public float KeyFrameSamplingSensitivity
        {
            get => m_Model.KeyFrameSamplingSensitivity * 100f;
            set
            {
                if (Math.Abs(m_Model.KeyFrameSamplingSensitivity - value) < 0.001f)
                    return;

                m_Model.KeyFrameSamplingSensitivity = value / 100f;
            }
        }

        public bool IsSamplingMotionToKeys
        {
            get => m_IsSamplingMotionToKeys;
            set
            {
                if (value == m_IsSamplingMotionToKeys)
                    return;

                m_IsSamplingMotionToKeys = value;
                OnChanged?.Invoke(Property.IsSamplingMotionToKeys);
            }
        }

        public bool IsBakingOutputTimeline
        {
            get => m_IsBakingOutputTimeline;
            set
            {
                if (value == m_IsBakingOutputTimeline)
                    return;

                m_IsBakingOutputTimeline = value;
                OnChanged?.Invoke(Property.IsBakingOutputTimeline);
            }
        }
        public bool CanConfirm
        {
            get => m_CanConfirm;
            set
            {
                if (value == m_CanConfirm)
                    return;

                m_CanConfirm = value;
                OnChanged?.Invoke(Property.CanConfirm);
            }
        }
        
        public bool CanConvert
        {
            get => m_CanConvert;
            set
            {
                if (value == m_CanConvert)
                    return;

                m_CanConvert = value;
                OnChanged?.Invoke(Property.CanConvert);
            }
        }
        
        public bool IsBusy => IsBakingOutputTimeline || IsSamplingMotionToKeys;
        public TimelineViewModel TimelineUIModel => m_TimelineUIModel;
        public BakedTimelinePlaybackUIModel PlaybackUIModel => m_PlaybackUIModel;

        AuthoringModel m_AuthoringModel;
        BakedTimelinePlaybackUIModel m_PlaybackUIModel;
        MotionToTimelineAuthoringModel.AuthoringStep m_Step;

        MotionToTimelineAuthoringModel m_Model;

        bool m_IsVisible;
        bool m_CanConvert;
        bool m_CanConfirm;
        bool m_IsSamplingMotionToKeys;
        bool m_IsBakingOutputTimeline;
        TimelineViewModel m_TimelineUIModel;
        BakingNoticeViewModel m_Notice;
        
        readonly ITimelineBakerTextToMotion m_TimelineBaker;

        public MotionToTimelineUIModel(MotionToTimelineAuthoringModel motionToTimelineAuthoringModel, TimelineViewModel timelineUIModel, BakingNoticeViewModel noticeView)
        {
            m_PlaybackUIModel = new BakedTimelinePlaybackUIModel();
            m_TimelineUIModel = timelineUIModel;
            m_Notice = noticeView;
            RegisterModel(motionToTimelineAuthoringModel);
        }

        void RegisterModel(MotionToTimelineAuthoringModel model)
        {
            UnregisterModel();

            if (model == null)
                return;

            m_Model = model;
            m_Model.OnStepChanged += OnMotionToTimelineAuthoringStepChanged;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnStepChanged -= OnMotionToTimelineAuthoringStepChanged;
            m_Model = null;
        }

        void OnMotionToTimelineAuthoringStepChanged()
        {
            Step = m_Model.Step;
            Refresh();
            OnChanged?.Invoke(Property.Visibility);
        }

        public void Refresh()
        {
            if (IsVisible)
            {
                switch (Step)
                {
                    case MotionToTimelineAuthoringModel.AuthoringStep.PreviewIsAvailable:
                        m_Notice.Show("Press <b><color=#48aeff>Convert to Frames</color></b> to keep the result", "check");
                        TimelineUIModel.IsVisible = true;

                        PlaybackUIModel.IsVisible = false;
                        break;

                    case MotionToTimelineAuthoringModel.AuthoringStep.None:
                        m_Notice.Hide();
                        TimelineUIModel.IsVisible = false;
                        PlaybackUIModel.IsVisible = false;
                        break;
                    
                    case MotionToTimelineAuthoringModel.AuthoringStep.NoPreview:
                        m_Notice.Show("Press <b><color=#48aeff>Sample</color></b> to preview the result", "help");
                        TimelineUIModel.IsVisible = false;
                        PlaybackUIModel.IsVisible = true;
                        break;
                    
                    case MotionToTimelineAuthoringModel.AuthoringStep.PreviewIsSamplingMotionToKeys:
                        m_Notice.Show("Sampling motion to keys...", "film-roll");
                        TimelineUIModel.IsVisible = false;
                        PlaybackUIModel.IsVisible = false;
                        break;
                    case MotionToTimelineAuthoringModel.AuthoringStep.PreviewIsBakingTimelineOutput:
                        m_Notice.Show("Baking Timeline Output...", "clap-board");
                        TimelineUIModel.IsVisible = false;
                        PlaybackUIModel.IsVisible = false;
                        break;
                    case MotionToTimelineAuthoringModel.AuthoringStep.PreviewIsObsolete:
                        default:
                        m_Notice.Show("Preview is obsolete");
                        TimelineUIModel.IsVisible = false;
                        PlaybackUIModel.IsVisible = false;
                        break;
                }
            }
            else
            {
                TimelineUIModel.IsVisible = false;
                PlaybackUIModel.IsVisible = false;
            }

            CanConfirm = m_Model.Step == MotionToTimelineAuthoringModel.AuthoringStep.PreviewIsAvailable;
            CanConvert = m_Model.IsPreviewObsolete && m_Model.Step != MotionToTimelineAuthoringModel.AuthoringStep.PreviewIsAvailable && m_Model.Step != MotionToTimelineAuthoringModel.AuthoringStep.PreviewIsBakingTimelineOutput && m_Model.Step != MotionToTimelineAuthoringModel.AuthoringStep.PreviewIsSamplingMotionToKeys;
        }
        
        public void RequestCancel()
        {
            m_Model.RequestCancel();
        }

        public void RequestConfirm()
        {
            m_Model.RequestConfirm();
        }

        public void RequestPreview()
        {
            m_Model.RequestPreview();
        }
    }
}
