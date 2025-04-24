using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    class SequenceDraggedItemView : VisualElement
    {
        SequenceDraggedItemViewModel m_Model;

        const string k_UssClassName = "deeppose-key";
        const string k_Name = "dragged-key";
        const string k_ThumbnailUssClassName = "deeppose-sequence-item-thumbnail";

        Image m_ThumbnailElement;

        public SequenceDraggedItemView()
        {
            name = k_Name;
            AddToClassList(k_UssClassName);
            m_ThumbnailElement = new Image() { name="key-thumbnail"};
            m_ThumbnailElement.pickingMode = PickingMode.Ignore;
            m_ThumbnailElement.AddToClassList(k_ThumbnailUssClassName);
            Add(m_ThumbnailElement);
        }

        public void SetModel(SequenceDraggedItemViewModel model)
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

        void Update()
        {
            style.display = m_Model.IsVisible ? DisplayStyle.Flex : DisplayStyle.None;
            m_ThumbnailElement.image = m_Model.Thumbnail;
        }

    }
}
