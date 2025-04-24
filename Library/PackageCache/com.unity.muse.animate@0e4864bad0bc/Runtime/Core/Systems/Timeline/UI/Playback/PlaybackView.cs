using UnityEngine;
using Unity.Muse.AppUI.UI;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class PlaybackView : UITemplateContainer, IUITemplate
    {
        const string k_UssClassName = "deeppose-playback";

        const string k_TimeSliderRootName = "playback-slider";
        const string k_PlayButtonName = "play";
        const string k_PauseButtonName = "pause";
        const string k_GoToPrevKeyButtonName = "go-to-prev-key";
        const string k_GoToNextKeyButtonName = "go-to-next-key";
        const string k_ToggleLoopButtonName = "toggle-loop";
        const string k_PlaybackSpeedButtonName = "playback-speed";
        const string k_PlusButtonName = "playback-add-key";

        PlaybackViewModel m_Model;
        VisualElement m_PlaybackSliderHandle;
        PlaybackSlider m_PlaybackSlider;
        Button m_PlusButton;
        ActionButton m_PlayButton;
        ActionButton m_PauseButton;
        ActionButton m_GoToPrevKeyButton;
        ActionButton m_GoToNextKeyButton;
        ActionButton m_ToggleLoopButton;
        ActionButton m_PlaybackSpeedButton;

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<PlaybackView, UxmlTraits> { }
#endif

        public PlaybackView(): base(k_UssClassName) { }

        public void FindComponents()
        {
            m_PlaybackSlider = this.Q<PlaybackSlider>(k_TimeSliderRootName);
            m_PlayButton = this.Q<ActionButton>(k_PlayButtonName);
            m_PauseButton = this.Q<ActionButton>(k_PauseButtonName);
            m_GoToPrevKeyButton = this.Q<ActionButton>(k_GoToPrevKeyButtonName);
            m_GoToNextKeyButton = this.Q<ActionButton>(k_GoToNextKeyButtonName);
            m_ToggleLoopButton = this.Q<ActionButton>(k_ToggleLoopButtonName);
            m_PlaybackSpeedButton = this.Q<ActionButton>(k_PlaybackSpeedButtonName);
            
            m_PlusButton = this.Q<Button>(k_PlusButtonName);
            m_PlaybackSliderHandle = m_PlaybackSlider.Q<VisualElement>("playback-slider__handle-container");
        }

        public void RegisterComponents()
        {
            m_PlayButton.RegisterCallback<ClickEvent>(OnPlayButtonClicked);
            m_PauseButton.RegisterCallback<ClickEvent>(OnPauseButtonClicked);
            m_ToggleLoopButton.RegisterCallback<ClickEvent>(OnToggleLoopButtonClicked);
            m_GoToPrevKeyButton.RegisterCallback<ClickEvent>(OnGoToPrevKeyButtonClicked);
            m_GoToNextKeyButton.RegisterCallback<ClickEvent>(OnGoToNextKeyButtonClicked);
            m_PlaybackSlider.RegisterValueChangingCallback(OnPlaybackSliderValueChanging);
            m_PlaybackSlider.RegisterValueChangedCallback(OnPlaybackSliderValueChanged);
            m_PlaybackSliderHandle.RegisterCallback<GeometryChangedEvent>(OnSliderHandleGeometryChanged);
            m_PlaybackSpeedButton.RegisterCallback<ClickEvent>(OnPlaybackSpeedButtonClicked);
            m_PlusButton.RegisterCallback<ClickEvent>(OnPlusButtonClicked);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        public void UnregisterComponents()
        {
            m_PlayButton.UnregisterCallback<ClickEvent>(OnPlayButtonClicked);
            m_PauseButton.UnregisterCallback<ClickEvent>(OnPauseButtonClicked);
            m_ToggleLoopButton.UnregisterCallback<ClickEvent>(OnToggleLoopButtonClicked);
            m_GoToPrevKeyButton.UnregisterCallback<ClickEvent>(OnGoToPrevKeyButtonClicked);
            m_GoToNextKeyButton.UnregisterCallback<ClickEvent>(OnGoToNextKeyButtonClicked);
            m_PlaybackSlider.UnregisterValueChangingCallback(OnPlaybackSliderValueChanging);
            m_PlaybackSlider.UnregisterValueChangedCallback(OnPlaybackSliderValueChanged);
            m_PlaybackSliderHandle.UnregisterCallback<GeometryChangedEvent>(OnSliderHandleGeometryChanged);
            m_PlaybackSpeedButton.UnregisterCallback<ClickEvent>(OnPlaybackSpeedButtonClicked);
            m_PlusButton.UnregisterCallback<ClickEvent>(OnPlusButtonClicked);

            UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
        
        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (m_Model == null)
                return;

            Update();
        }

        void OnSliderHandleGeometryChanged(GeometryChangedEvent evt)
        {
            const int xOffset = -3;

            // Playback slider plus button
            m_PlusButton.style.left = m_PlaybackSliderHandle.localBound.xMin - xOffset;
        }

        public void SetModel(PlaybackViewModel model)
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

            Update();
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
            if (m_Model == null)
                return;

            if (!IsAttachedToPanel)
                return;

            // Play button
            m_PlayButton.style.display = m_Model.IsPlaying ? DisplayStyle.None : DisplayStyle.Flex;
            m_PlayButton.SetEnabled(m_Model.KeyCount > 0);

            // Stop button
            m_PauseButton.style.display = m_Model.IsPlaying ? DisplayStyle.Flex : DisplayStyle.None;
            m_PauseButton.SetEnabled(m_Model.KeyCount > 0);

            // Go to first key button
            m_GoToPrevKeyButton.SetEnabled(m_Model.KeyCount > 0 && !m_Model.IsPlaying);

            // Go to last key button
            m_GoToNextKeyButton.SetEnabled(m_Model.KeyCount > 0 && !m_Model.IsPlaying);

            // Toggle loop button
            m_ToggleLoopButton.SetSelectedWithoutNotify(m_Model.KeyCount > 0 && m_Model.IsLooping);

            // Playback speed button
            // TODO: Update icon to indicate playback speed.
            m_PlaybackSpeedButton.label = $"x{m_Model.PlaybackSpeed}";

            // Playback slider
            m_PlaybackSlider.SetEnabled(m_Model.KeyCount > 0);
            m_PlaybackSlider.lowValue = m_Model.MinFrame;
            m_PlaybackSlider.highValue = m_Model.MaxFrame + 1;
            m_PlaybackSlider.SetValueWithoutNotify(m_Model.CurrentFrame);
            m_PlaybackSlider.tickValues = m_Model.KeyFrames;
            m_PlaybackSlider.filled = m_Model.ShowTransition;
            m_PlaybackSlider.fillStart = m_Model.TransitionStart;
            m_PlaybackSlider.fillEnd = m_Model.TransitionEnd;
            m_PlaybackSlider.primary = m_Model.EmphasizeTransition;
            
            m_PlusButton.style.display = m_Model.ShowPlusButton ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void OnPlusButtonClicked(ClickEvent evt)
        {
            m_Model?.InsertKeyAtCurrentFrame();
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

        void OnGoToPrevKeyButtonClicked(ClickEvent evt)
        {
            m_Model?.GoToPrevKey();
        }

        void OnGoToNextKeyButtonClicked(ClickEvent evt)
        {
            m_Model?.GoToNextKey();
        }

        void OnPlaybackSliderValueChanging(ChangingEvent<int> evt)
        {
            m_PlaybackSlider.Blur();
            m_Model?.RequestSeekToFrame(evt.newValue);
        }
        
        void OnPlaybackSliderValueChanged(ChangeEvent<int> evt)
        {
            DeepPoseAnalytics.SendSetCurrentFrameAction(evt.newValue);
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
            m_Model?.RequestSeekToFrame(evt.newValue);
        }
    }
}
