using System;
using Unity.Muse.AppUI.UI;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class VideoUploadUI : UITemplateContainer, IUITemplate
    {
        const string k_UssClassName = "deeppose-video-upload";
        Text m_FileTypesText;
        Text m_FileSizeText;
        Button m_BrowseButton;

        public VideoUploadUI()
            : base(k_UssClassName)
        {
#if ENABLE_UXML_SERIALIZED_DATA
            AcceptedFileTypes = ApplicationConstants.VideoExtensions;
#endif
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public float MaxFileSize
        {
            get => m_MaxFileSize;
            set
            {
                m_MaxFileSize = value;
                m_FileSizeText.text = $"Max file size: <b>{m_MaxFileSize} MB</b>";
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string[] AcceptedFileTypes
        {
            get => m_AcceptedFileTypes;
            set
            {
                m_AcceptedFileTypes = value ?? Array.Empty<string>();
                m_FileTypesText.text = $"Video files: {string.Join(", ", m_AcceptedFileTypes)}";
            }
        }

        string[] m_AcceptedFileTypes;
        float m_MaxFileSize = 120f;
        VideoToMotionUIModel m_Model;

#if ENABLE_UXML_TRAITS
        public new class UxmlTraits : UITemplateContainer.UxmlTraits
        {
            readonly UxmlStringAttributeDescription m_AcceptedFileTypes = new() { name = "accepted-file-types", defaultValue = string.Join(",", ApplicationConstants.VideoExtensions)};
            readonly UxmlFloatAttributeDescription m_MaxFileSize = new() { name = "max-file-size", defaultValue = 120 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var element = (VideoUploadUI)ve;
                element.MaxFileSize = m_MaxFileSize.GetValueFromBag(bag, cc);
                element.AcceptedFileTypes = m_AcceptedFileTypes.GetValueFromBag(bag, cc).Split(",");
            }
        }

        public new class UxmlFactory : UxmlFactory<VideoUploadUI, UxmlTraits> { }
#endif

        public void FindComponents()
        {
            m_FileTypesText = this.Q<Text>("file-types");
            m_FileSizeText = this.Q<Text>("max-size");
            m_BrowseButton = this.Q<Button>("browse-button");
        }

        public void RegisterComponents()
        {
            m_BrowseButton.clicked += BrowseFile;
        }

        public void UnregisterComponents()
        {
            m_BrowseButton.clicked -= BrowseFile;
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
        }

        void OnModelChanged(VideoToMotionUIModel.Property property)
        {
            if (property is VideoToMotionUIModel.Property.VideoPath)
            {
                UpdateVisibility();
            }
        }

        public void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.changed -= OnModelChanged;
        }

        void BrowseFile()
        {
            m_Model?.BrowseFile(AcceptedFileTypes, m_MaxFileSize);
        }

        void UpdateVisibility()
        {
            if (m_Model == null)
                return;

            var hasVideoPath = !string.IsNullOrEmpty(m_Model.VideoPath);
            style.display = hasVideoPath ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}
