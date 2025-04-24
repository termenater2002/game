using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    class EntitySelectionViewModel
    {
        public bool IsSelected => m_SelectionModel.IsSelected(m_EntityID);

        public bool IsHighlighted
        {
            get => m_IsHighlighted;
            private set
            {
                if (m_IsHighlighted == value)
                    return;

                m_IsHighlighted = value;
                OnHighlightedStateChanged?.Invoke(m_IsHighlighted);
            }
        }

        public delegate void HighlightedStateChanged(bool isHighlighted);
        public event HighlightedStateChanged OnHighlightedStateChanged;

        public delegate void SelectionStateChanged(bool isSelected);
        public event SelectionStateChanged OnSelectionStateChanged;

        bool IsExclusiveSelection => InputUtils.IsExclusiveSelection;

        SelectionModel<EntityID> m_SelectionModel;
        EntityID m_EntityID;
        bool m_IsHighlighted;

        public EntitySelectionViewModel(EntityID entityID, SelectionModel<EntityID> selectionModel)
        {
            m_EntityID = entityID;

            m_SelectionModel = selectionModel;
            m_SelectionModel.OnSelectionStateChanged += OnSelectionModelStateChanged;
        }

        void OnSelectionModelStateChanged(SelectionModel<EntityID> model, EntityID index, bool isSelected)
        {
            if (index != m_EntityID)
                return;

            OnSelectionStateChanged?.Invoke(isSelected);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsHighlighted = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsHighlighted = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsExclusiveSelection)
            {
                m_SelectionModel.Clear();
                m_SelectionModel.Select(m_EntityID);
            }
            else
            {
                var isSelected = m_SelectionModel.IsSelected(m_EntityID);
                m_SelectionModel.SetSelected(m_EntityID, !isSelected);
            }
        }
    }
}
