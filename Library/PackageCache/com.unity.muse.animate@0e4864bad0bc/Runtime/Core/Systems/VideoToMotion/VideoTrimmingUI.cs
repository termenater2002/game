using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;
using TextField = Unity.Muse.AppUI.UI.TextField;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class VideoTrimmingUI : UITemplateContainer, IUITemplate
    {
        const string k_UssClassName = "deeppose-video-trimming";
#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<VideoTrimmingUI, UxmlTraits> { }
#endif

        const string k_PlayIcon = "play";
        const string k_PauseIcon = "pause";

        VideoPlayerUI m_VideoPlayer;
        IconButton m_CloseButton;
        VisualElement m_Preloader;
        CircularProgress m_LoaderSpinner;
        VisualElement m_ControlsContainer;
        ActionButton m_PlayPauseButton;
        PlaybackSlider m_PlaybackSlider;
        Text m_FrameNumberText;
        Text m_LengthText;
        TextField m_StartFrameField;
        Button m_UseCurrentFrameButton;
        TouchSliderFloat m_DurationSlider;
        VideoToMotionUIModel m_Model;

        bool m_Seeking;
        bool m_IsVideoLoaded;

        /// <summary>
        /// Raised when the video is loaded and unloaded into the player.
        /// </summary>
        public event Action videoReadyStateChanged;

        /// <summary>
        /// Indicates whether the video is currently loaded in the player.
        /// </summary>
        public bool IsVideoLoaded
        {
            get => m_IsVideoLoaded;
            private set
            {
                if (m_IsVideoLoaded != value)
                {
                    m_IsVideoLoaded = value;
                    videoReadyStateChanged?.Invoke();
                }
            }
        }

        public VideoTrimmingUI() : base(k_UssClassName) { }

        public void FindComponents()
        {
            m_VideoPlayer = this.Q<VideoPlayerUI>("video-preview");
            m_CloseButton = this.Q<IconButton>("close-button");
            m_Preloader = this.Q("video-preloader");
            m_LoaderSpinner = this.Q<CircularProgress>("video-loading-spinner");
            m_ControlsContainer = this.Q("controls-container");
            m_PlayPauseButton = this.Q<ActionButton>("play-pause-button");
            m_PlaybackSlider = this.Q<PlaybackSlider>("playback-slider");
            m_FrameNumberText = this.Q<Text>("frame-number-text");
            m_LengthText = this.Q<Text>("length-text");
            m_StartFrameField = this.Q<TextField>("start-frame-field");
            m_UseCurrentFrameButton = this.Q<Button>("use-current-frame-button");
            m_DurationSlider = this.Q<TouchSliderFloat>("duration-slider");
        }

        public void RegisterComponents()
        {
            m_PlaybackSlider.primary = true;
            m_PlaybackSlider.filled = true;
            m_PlaybackSlider.highValue = 100;
            m_PlaybackSlider.fillStart = 20;
            m_PlaybackSlider.fillEnd = 80;

            m_PlayPauseButton.clicked += TogglePlay;
            m_VideoPlayer.timeChanged += UpdateSliderPosition;
            m_VideoPlayer.playStateChanged += UpdatePlaybackUI;
            m_VideoPlayer.readyStateChanged += OnReadyStateChanged;
            m_VideoPlayer.errorReceived += OnErrorReceived;

            m_CloseButton.clicked += OnCloseButtonClicked;

            m_StartFrameField.RegisterValueChangedCallback(OnStartFrameChanged);
            m_UseCurrentFrameButton.clicked += SetStartToCurrentFrame;
            m_PlaybackSlider.RegisterValueChangingCallback(OnPlaybackSliderValueChanging);

            m_DurationSlider.highValue = ApplicationConstants.VideoToMotionMaxDuration;
            m_DurationSlider.value = ApplicationConstants.VideoToMotionMaxDuration / 2f;
            m_DurationSlider.RegisterValueChangingCallback(OnDurationSliderChanging);
            m_DurationSlider.RegisterValueChangedCallback(OnDurationSliderChanged);
        }

        public void UnregisterComponents()
        {
            m_PlayPauseButton.clicked -= TogglePlay;
            m_VideoPlayer.timeChanged -= UpdateSliderPosition;
            m_VideoPlayer.playStateChanged -= UpdatePlaybackUI;
            m_VideoPlayer.readyStateChanged -= OnReadyStateChanged;
            m_VideoPlayer.errorReceived -= OnErrorReceived;

            m_CloseButton.clicked -= OnCloseButtonClicked;

            m_UseCurrentFrameButton.clicked -= SetStartToCurrentFrame;

            m_StartFrameField.UnregisterValueChangedCallback(OnStartFrameChanged);
            m_PlaybackSlider.UnregisterValueChangingCallback(OnPlaybackSliderValueChanging);
            m_DurationSlider.UnregisterValueChangingCallback(OnDurationSliderChanging);
            m_DurationSlider.UnregisterValueChangedCallback(OnDurationSliderChanged);
        }

        public void SetModel(VideoToMotionUIModel model)
        {
            UnregisterModel();
            m_Model = model;
            RegisterModel();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.changed += OnModelChanged;
            UpdatePlaybackUI();
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.changed -= OnModelChanged;
        }

        void UpdateSliderPosition()
        {
            m_PlaybackSlider.SetValueWithoutNotify(m_VideoPlayer.Frame);
            UpdateFrameNumber(m_VideoPlayer.Frame);
        }

        void UpdateFrameNumber(int frame)
        {
            m_FrameNumberText.text = ToTimecode(frame, m_VideoPlayer.FrameRate);
        }

        void UpdateRangeUI()
        {
            var maxFrame = m_VideoPlayer.NumFrames - 1;
            if (maxFrame <= 0)
                return;

            m_PlaybackSlider.highValue = maxFrame;
            m_PlaybackSlider.fillStart = m_Model.StartFrame;
            m_PlaybackSlider.fillEnd = m_Model.EndFrame;

            m_LengthText.text = ToTimecode(m_VideoPlayer.NumFrames, m_VideoPlayer.FrameRate);
            m_StartFrameField.SetValueWithoutNotify(ToTimecode(m_Model.StartFrame, m_VideoPlayer.FrameRate));

            m_DurationSlider.SetValueWithoutNotify(m_Model.Duration);
        }

        void OnReadyStateChanged()
        {
            IsVideoLoaded = m_VideoPlayer.IsReady;
            UpdatePlaybackUI();
            UpdateRangeUI();
            UpdateVisibility();
        }

        void OnErrorReceived(string errorMsg)
        {
            DevLogger.LogError($"Video player error: {errorMsg}");
            m_Model?.UnloadVideo();
            IsVideoLoaded = false;
        }

        void OnCloseButtonClicked()
        {
            m_Model?.UnloadVideo();
        }

        void UpdatePlaybackUI()
        {
            if (m_VideoPlayer.IsReady)
            {
                m_PlayPauseButton.SetEnabled(true);
                m_PlayPauseButton.icon = m_VideoPlayer.IsPlaying ? k_PauseIcon : k_PlayIcon;
            }
            else
            {
                style.display = DisplayStyle.None;
                m_PlayPauseButton.SetEnabled(false);
                m_PlayPauseButton.icon = k_PlayIcon;
            }
        }

        void UpdateVisibility()
        {
            if (m_Model == null)
                return;

            style.display = string.IsNullOrEmpty(m_Model.VideoPath) ? DisplayStyle.None : DisplayStyle.Flex;
            m_Preloader.style.display = IsVideoLoaded ? DisplayStyle.None : DisplayStyle.Flex;

            // Workaround: The circular progress spinner needs to be itself hidden, not just inside a hidden parent.
            // Otherwise, it will keep forcing the window to repaint.
            m_LoaderSpinner.style.display = IsVideoLoaded ? DisplayStyle.None : DisplayStyle.Flex;

            m_ControlsContainer.style.display = IsVideoLoaded ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void OnModelChanged(VideoToMotionUIModel.Property property)
        {
            switch (property)
            {
                case VideoToMotionUIModel.Property.FrameRange:
                    UpdateRangeUI();
                    break;
                case VideoToMotionUIModel.Property.VideoPath:
                    IsVideoLoaded = false;
                    m_VideoPlayer.Source = m_Model.VideoPath;
                    UpdateVisibility();
                    break;
            }
        }

        void OnPlaybackSliderValueChanging(ChangingEvent<int> evt)
        {
            UpdateFrameNumber(evt.newValue);
            m_VideoPlayer.SeekToFrame(evt.newValue);
        }

        void OnStartFrameChanged(ChangeEvent<string> evt)
        {
            UpdateModel();
        }

        void OnDurationSliderChanging(ChangingEvent<float> evt)
        {
            UpdateModel();
        }

        void OnDurationSliderChanged(ChangeEvent<float> evt)
        {
            UpdateModel();
        }

        void UpdateModel()
        {
            if (!m_VideoPlayer.IsLoaded)
                return;

            m_Model.Duration = m_DurationSlider.value;
            m_Model.StartFrame = ToFrame(m_StartFrameField.value, m_VideoPlayer.FrameRate);
            m_Model.VideoFrameRate = m_VideoPlayer.FrameRate;
        }

        void TogglePlay()
        {
            if (m_VideoPlayer.IsPlaying)
                m_VideoPlayer.Pause();
            else
                m_VideoPlayer.Play();
        }

        void SetStartToCurrentFrame()
        {
            if (m_Model == null)
                return;

            m_Model.StartFrame = m_VideoPlayer.Frame;
        }

        static string ToTimecode(int frame, float frameRate)
        {
            // This a naive implementation that doesn't take into account drop frames.
            // Don't @ me!
            var seconds = Mathf.FloorToInt(frame / frameRate);
            var minutes = seconds / 60;
            var frames = Mathf.FloorToInt(frame - seconds * frameRate);
            seconds %= 60;
            return $"{minutes:D2}:{seconds:D2}.{frames:D2}";
        }

        static int ToFrame(string timecode, float frameRate)
        {
            if (string.IsNullOrEmpty(timecode))
                return 0;

            var parts = timecode.Split(':');

            if (parts.Length < 1)
                return 0;

            var minutes = parts.Length == 2 ? int.Parse(parts[0]) : 0;

            parts = parts[^1].Split('.');

            var seconds = parts.Length == 2 ? int.Parse(parts[0]) : 0;
            var frames = int.Parse(parts[^1]);

            return Mathf.RoundToInt(minutes * 60 * frameRate + seconds * frameRate + frames);
        }
    }
}
