using UnityEngine;
using Unity.Muse.AppUI.UI;
using UnityEngine.UIElements;
using AppUI = Unity.Muse.AppUI.UI;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class BakedTimelinePlaybackUI : UITemplateContainer, IUITemplate
    {
        const string k_UssClassName = "deeppose-baked-timeline-playback";

        const string k_TimeSliderRootName = "baked-timeline-playback-slider";
        const string k_ControlsGroupName = "baked-timeline-playback-controls";
        const string k_PlayButtonName = "play";
        const string k_PauseButtonName = "pause";
        const string k_ToggleLoopButtonName = "toggle-loop";
        const string k_PlaybackSpeedButtonName = "playback-speed";
        const string k_MoreButtonName = "appui-actiongroup__more-button";
        
        BakedTimelinePlaybackUIModel m_Model;
        BakedTimelinePlaybackSlider m_PlaybackSlider;
        
        ActionButton m_PlayButton;
        ActionButton m_PauseButton;
        ActionButton m_ToggleLoopButton;
        ActionButton m_PlaybackSpeedButton;
        ActionGroup m_ControlsGroup;
        ActionButton m_MoreButton;

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<BakedTimelinePlaybackUI, UxmlTraits> { }
#endif

        public BakedTimelinePlaybackUI(): base(k_UssClassName) { }

        public void FindComponents()
        {
            m_ControlsGroup = this.Q<ActionGroup>(k_ControlsGroupName);
            m_MoreButton = m_ControlsGroup.Q<ActionButton>(k_MoreButtonName);
            m_PlayButton = this.Q<ActionButton>(k_PlayButtonName);
            m_PauseButton = this.Q<ActionButton>(k_PauseButtonName);
            m_ToggleLoopButton = this.Q<ActionButton>(k_ToggleLoopButtonName);
            m_PlaybackSpeedButton = this.Q<ActionButton>(k_PlaybackSpeedButtonName);
            m_PlaybackSlider = this.Q<BakedTimelinePlaybackSlider>(k_TimeSliderRootName);
        }

        public void RegisterComponents()
        {
            m_PlayButton.RegisterCallback<ClickEvent>(OnPlayButtonClicked);
            m_PauseButton.RegisterCallback<ClickEvent>(OnPauseButtonClicked);
            m_ToggleLoopButton.RegisterCallback<ClickEvent>(OnToggleLoopButtonClicked);
            m_PlaybackSpeedButton.RegisterCallback<ClickEvent>(OnPlaybackSpeedButtonClicked);
            m_PlaybackSlider.RegisterValueChangingCallback(OnPlaybackSliderValueChanged);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
        
        public void UnregisterComponents()
        {
            m_PlayButton.UnregisterCallback<ClickEvent>(OnPlayButtonClicked);
            m_PauseButton.UnregisterCallback<ClickEvent>(OnPauseButtonClicked);
            m_ToggleLoopButton.UnregisterCallback<ClickEvent>(OnToggleLoopButtonClicked);
            m_PlaybackSpeedButton.UnregisterCallback<ClickEvent>(OnPlaybackSpeedButtonClicked);
            m_PlaybackSlider.UnregisterValueChangingCallback(OnPlaybackSliderValueChanged);

            UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
        
        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (m_Model == null)
                return;

            Update();
        }

        public void SetModel(BakedTimelinePlaybackUIModel model)
        {
            UnregisterModel();
            m_Model = model;
            RegisterModel();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged += OnChanged;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged -= OnChanged;
            m_Model = null;
        }

        void OnChanged()
        {
            Update();
        }

        public void Update()
        {
            // Always hide the "More" button
            // Note: Was causing the action-group to be wider than it should be
            m_MoreButton.style.display = DisplayStyle.None;
            
            if (m_Model == null)
                return;

            if (!IsAttachedToPanel)
                return;

            if (!m_Model.IsVisible)
            {
                style.display = DisplayStyle.None;
                return;
            }

            style.display = DisplayStyle.Flex;
            
            // TODO: Update icon to indicate enabled/disabled state.
            var hasFrames = m_Model.MaxFrame > 0;
            
            // Play button
            m_PlayButton.style.display = m_Model.IsPlaying ? DisplayStyle.None : DisplayStyle.Flex;
            m_PlayButton.SetEnabled(hasFrames);
            
            // Pause button
            m_PauseButton.style.display = m_Model.IsPlaying ? DisplayStyle.Flex : DisplayStyle.None;
            m_PauseButton.SetEnabled(hasFrames);

            // Toggle loop button
            m_ToggleLoopButton.SetSelectedWithoutNotify(hasFrames && m_Model.IsLooping);

            // Playback speed button
            // TODO: Update icon to indicate playback speed.
            m_PlaybackSpeedButton.label = $"x{m_Model.PlaybackSpeed}";

            // Playback slider
            m_PlaybackSlider.SetEnabled(hasFrames);
            m_PlaybackSlider.lowValue = m_Model.MinFrame;
            m_PlaybackSlider.highValue = m_Model.MaxFrame + 1;
            m_PlaybackSlider.SetValueWithoutNotify(m_Model.CurrentFrame);
            m_PlaybackSlider.filled = true;
            m_PlaybackSlider.primary = true;
        }

        void OnPlayButtonClicked(ClickEvent evt)
        {
            m_Model?.Play();
        }

        void OnPauseButtonClicked(ClickEvent evt)
        {
            m_Model?.Pause();
        }

        void OnToggleLoopButtonClicked(ClickEvent evt)
        {
            m_Model?.ToggleLooping();
        }
        
        void OnPlaybackSliderValueChanged(ChangingEvent<int> evt)
        {
            m_PlaybackSlider.Blur();
            m_Model?.SetCurrentFrame(evt.newValue);
        }

        void OnPlaybackSpeedButtonClicked(ClickEvent evt)
        {
            m_Model?.SetNextPlaybackSpeed();
        }

        void OnFramesPerSecondValueChanged(ChangeEvent<int> evt)
        {
            m_Model?.SetFramesPerSecond(evt.newValue);
        }

        void OnCurrentFrameValueChanged(ChangeEvent<int> evt)
        {
            m_Model?.SetCurrentFrame(evt.newValue);
        }
    }
}
