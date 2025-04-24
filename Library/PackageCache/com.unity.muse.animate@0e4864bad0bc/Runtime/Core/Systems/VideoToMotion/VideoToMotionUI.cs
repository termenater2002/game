using System;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class VideoToMotionUI : UITemplateContainer, IUITemplate
    {
#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<VideoToMotionUI, UxmlTraits> { }
#endif

        VideoUploadUI m_UploadUI;
        VideoTrimmingUI m_TrimmingUI;
        Button m_GenerateButton;

        VideoToMotionUIModel m_Model;

        public VideoToMotionUI()
            : base("deeppose-takes-v2m")
        {
        }

        public void FindComponents()
        {
            m_UploadUI = this.Q<VideoUploadUI>("v2m-video-upload");
            m_TrimmingUI = this.Q<VideoTrimmingUI>("v2m-trimming");
            m_GenerateButton = this.Q<Button>("v2m-generate");
        }

        public void SetModel(VideoToMotionUIModel model)
        {
            UnregisterModel();
            m_Model = model;
            m_UploadUI.SetModel(model);
            m_TrimmingUI.SetModel(model);
            RegisterModel();
        }

        public void RegisterComponents()
        {
            m_GenerateButton.clicked += Generate;
            m_TrimmingUI.videoReadyStateChanged += OnVideoReadyStateChanged;
            UpdateUI();
        }

        void OnVideoReadyStateChanged()
        {
            if (m_TrimmingUI.IsVideoLoaded)
            {
                // We should reset the start frame and duration when the video is loaded.
                m_Model.StartFrame = 0;
                m_Model.Duration = ApplicationConstants.VideoToMotionDefaultDuration;
            }
            UpdateUI();
        }

        public void UnregisterComponents()
        {
            m_GenerateButton.clicked -= Generate;
            m_TrimmingUI.videoReadyStateChanged -= OnVideoReadyStateChanged;
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.changed += OnModelChanged;
            UpdateUI();
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.changed -= OnModelChanged;
        }

        void OnModelChanged(VideoToMotionUIModel.Property property)
        {
            UpdateUI();
        }

        void UpdateUI()
        {
            m_GenerateButton.style.display = m_TrimmingUI.IsVideoLoaded
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            m_GenerateButton.SetEnabled(m_Model.CanGenerate);
        }

        void Generate()
        {
            m_Model?.Generate();
        }
    }
}
