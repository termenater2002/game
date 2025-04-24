using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class VideoToMotionPlaybackUI : UITemplateContainer, IUITemplate
    {
        const string k_PlaybackName = "v2m-playback";
        const string k_OverlayBoxName = "v2m-overlay-box";
        const string k_ExtractKeysButtonName = "v2m-button-extract-keys";
        const string k_ExportButtonName = "v2m-button-export";
        const string k_DeleteButtonName = "v2m-button-delete";
        const string k_VideoPlaybackImageName = "v2m-overlay-box-video";
        const string k_VideoFileName = "v2m-filename-value";

        VideoToMotionPlaybackUIModel m_Model;
        BakedTimelinePlaybackUI m_PlaybackUI;

        ActionButton m_ExtractKeysButton;
        ActionButton m_ExportButton;
        Button m_DeleteButton;
        VisualElement m_OverlayBox;
        Text m_VideoFileNameText;

        VideoPlayerUI m_VideoPlayer;

        float TimeOffset => m_VideoPlayer.IsLoaded ? m_Model.PreviewStartFrame / m_VideoPlayer.FrameRate : 0;

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<VideoToMotionPlaybackUI, UxmlTraits> { }
#endif

        public VideoToMotionPlaybackUI()
            : base("deeppose-v2m") { }

        public void FindComponents()
        {
            m_PlaybackUI = this.Q<BakedTimelinePlaybackUI>(k_PlaybackName);

            // Overlay text shown when baking / waiting
            m_OverlayBox = this.Q<VisualElement>(k_OverlayBoxName);

            m_ExtractKeysButton = this.Q<ActionButton>(k_ExtractKeysButtonName);
            m_ExportButton = this.Q<ActionButton>(k_ExportButtonName);
            m_DeleteButton = this.Q<Button>(k_DeleteButtonName);

            m_VideoPlayer = this.Q<VideoPlayerUI>(k_VideoPlaybackImageName);
            
            m_VideoFileNameText = this.Q<Text>(k_VideoFileName);
        }

        public void RegisterComponents()
        {
            m_ExtractKeysButton.clicked += ExtractKeys;
            m_ExportButton.clicked += Export;
            m_DeleteButton.clicked += Delete;
        }
        
        public void UnregisterComponents()
        {
            m_ExtractKeysButton.clicked -= ExtractKeys;
            m_ExportButton.clicked -= Export;
            m_DeleteButton.clicked -= Delete;
        }

        public void SetModel(VideoToMotionPlaybackUIModel model)
        {
            UnregisterModel();
            m_Model = model;
            m_PlaybackUI.SetModel(m_Model.PlaybackUIModel);
            RegisterModel();
            UpdateUI();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnPlaybackChanged += UpdateVideoPlayer;
            m_Model.OnChanged += OnModelChanged;
            m_Model.OnSeekedToTime += SeekToTime;

            m_VideoPlayer.readyStateChanged += OnVideoReadyStateChanged;
            m_VideoPlayer.timeChanged += VideoTimeChanged;
            m_VideoPlayer.playStateChanged += OnVideoPlayStateChanged;

            UpdateVideoPlayer();
        }

        void VideoTimeChanged()
        {
            if (m_Model == null)
                return;

            // Make sure we don't run off the end of the clip
            var time = m_VideoPlayer.Time - TimeOffset;
            var duration = m_Model.PreviewFrameCount / m_VideoPlayer.FrameRate;
            if (m_VideoPlayer.Time >= TimeOffset + duration)
            {
                m_VideoPlayer.Pause();
                SeekToTime(0);
                time = 0;
                
                if (m_Model.IsLooping)
                {
                    m_VideoPlayer.Play();
                }
            }

            // Propagate the video playback time to the model
            m_Model.PlaybackTime = time;
        }

        void OnVideoPlayStateChanged()
        {
            if (m_Model == null)
                return;

            // Propagate the video play state to the model
            m_Model.IsPlaying = m_VideoPlayer.IsPlaying;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnPlaybackChanged -= UpdateVideoPlayer;
            m_Model.OnChanged -= OnModelChanged;
            m_Model.OnSeekedToTime -= SeekToTime;

            m_VideoPlayer.readyStateChanged -= OnVideoReadyStateChanged;
            m_VideoPlayer.timeChanged -= VideoTimeChanged;
            m_VideoPlayer.playStateChanged -= VideoTimeChanged;
        }

        void UpdateVideoPlayer()
        {
            if (!m_VideoPlayer.IsReady) return;

            if (m_Model.IsPlaying && !m_VideoPlayer.IsPlaying)
            {
                SeekToTime(m_Model.PlaybackTime);
                m_VideoPlayer.Play();
            }
            else if (!m_Model.IsPlaying && m_VideoPlayer.IsPlaying)
            {
                m_VideoPlayer.Pause();
            }
        }

        void OnVideoReadyStateChanged()
        {
            if (m_VideoPlayer.IsReady)
            {
                SeekToTime(m_Model.PlaybackTime);
            }
        }

        void OnModelChanged(VideoToMotionPlaybackUIModel.Property property)
        {
            if (property is VideoToMotionPlaybackUIModel.Property.VideoPath)
            {
                m_VideoPlayer.Source = m_Model.VideoPath;
            }

            UpdateUI();
        }

        void UpdateUI()
        {
            if (m_Model == null)
                return;

            if (!IsAttachedToPanel)
                return;

            style.display = m_Model.IsVisible ? DisplayStyle.Flex : DisplayStyle.None;

            m_ExportButton.style.display = DisplayStyle.Flex;
            m_ExtractKeysButton.style.display = DisplayStyle.Flex;
            m_OverlayBox.style.display = DisplayStyle.None;

            m_ExportButton.SetEnabled(m_Model.CanExport);
            m_ExtractKeysButton.SetEnabled(m_Model.CanMakeEditable);

            m_DeleteButton.style.display = DisplayStyle.None;
            m_VideoFileNameText.text = m_Model.Title;
        }

        void SeekToTime(float time)
        {
            m_VideoPlayer.SeekToTime(time + TimeOffset);
        }
        
        void ExtractKeys()
        {
            m_Model?.RequestExtractKeys();
        }
        
        void Export()
        {
            m_Model?.RequestExport();
        }
        
        void Delete()
        {
            m_Model?.RequestDelete();
        }
    }
}
