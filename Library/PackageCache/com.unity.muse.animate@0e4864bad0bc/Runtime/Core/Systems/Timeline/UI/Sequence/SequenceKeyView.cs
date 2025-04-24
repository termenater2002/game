using System;
using Unity.Muse.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    class SequenceKeyView : SequenceItemView<TimelineModel.SequenceKey>
    {
        public TimelineModel.SequenceKey Key => m_Model?.Target;
        public TimelineModel.SequenceKey Model => Key;

        const string k_UssClassName = "deeppose-sequence-key";
        const string k_UssClassNameSelected = "deeppose-sequence-item-selected";
        const string k_UssClassNameHighlighted = "deeppose-sequence-item-highlighted";
        const string k_UssClassNameEditing = "deeppose-sequence-item-editing";
        const string k_ThumbnailUssClassName = "deeppose-sequence-item-thumbnail";
        const string k_ItemOverlayUssClassName = "deeppose-sequence-item-overlay";
        const string k_TypeIconUssClassName = "deeppose-sequence-item-type-icon";
        
        static string[] s_KeyTypeIcons = { "ellipsis", "", "arrows-u-counter-clockwise" };
        
        Image m_ThumbnailElement;
        Icon m_TypeIcon;

        public SequenceKeyView()
            :
            base(k_UssClassName, k_UssClassNameSelected, k_UssClassNameHighlighted, k_UssClassNameEditing)
        {
            name = "sequence-key";
        }

        public void SetModel(SequenceKeyViewModel viewModel)
        {
            base.SetModel(viewModel);
        }

        protected override void Update()
        {
            base.Update();
            
            if (m_Model?.Target == null)
                return;
            
            UpdateThumbnail();
            UpdateTypeIcon();
        }

        void UpdateThumbnail()
        {
            if (m_Model?.Target?.Thumbnail == null)
                return;
            
            if (m_ThumbnailElement == null)
            {
                m_ThumbnailElement = new Image() { name = "key-thumbnail" };
                m_ThumbnailElement.pickingMode = PickingMode.Ignore;
                m_ThumbnailElement.AddToClassList(k_ThumbnailUssClassName);
                Add(m_ThumbnailElement);
                
                var overlay = new VisualElement()
                {
                    name = "key-thumbnail-overlay",
                    pickingMode = PickingMode.Ignore,
                };
                overlay.AddToClassList(k_ItemOverlayUssClassName);
                Add(overlay);
            }

            if (m_Model.Target.Key.Type == KeyData.KeyType.FullPose)
            {
                m_ThumbnailElement.style.display = DisplayStyle.Flex;
                m_ThumbnailElement.image = m_Model.Target.Thumbnail.Texture;
            }
            else
            {
                m_ThumbnailElement.style.display = DisplayStyle.None;
                m_ThumbnailElement.image = null;
            }
        }

        void UpdateTypeIcon()
        {
            if (m_TypeIcon == null)
            {
                m_TypeIcon = new Icon() { name = "key-type-icon" };
                m_TypeIcon.pickingMode = PickingMode.Ignore;
                m_TypeIcon.AddToClassList(k_TypeIconUssClassName);
                Add(m_TypeIcon);
            }

            m_TypeIcon.iconName = s_KeyTypeIcons[(int)m_Model.Target.Key.Type];
        }
    }
}
