using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    [RequireComponent(typeof(Outliner))]
    [ExecuteAlways]
    class EntitySelectionView : MonoBehaviour, IPhysicsRaycastHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        Outliner m_Outline;
        EntitySelectionViewModel m_Model;

        public void Awake()
        {
            m_Outline = GetComponent<Outliner>();
            m_Outline.enabled = false;
            
            if (!UnityEngine.Application.isPlaying)
            {
                m_Outline.Awake();
            }
        }

        void OnEnable()
        {
            RegisterModel();
        }

        void OnDisable()
        {
            UnregisterModel();
        }

        public void SetModel(EntitySelectionViewModel model)
        {
            if (enabled)
                UnregisterModel();

            m_Model = model;

            if (enabled)
                RegisterModel();
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Outline.enabled = false;

            m_Model.OnSelectionStateChanged -= OnSelectionStateChanged;
            m_Model.OnHighlightedStateChanged -= OnHighlightedStateChanged;
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            UpdateOutline();

            m_Model.OnSelectionStateChanged += OnSelectionStateChanged;
            m_Model.OnHighlightedStateChanged += OnHighlightedStateChanged;

            // Note: hotfix to make sure the model gets an exit event
            m_Model.OnPointerExit(null);
        }

        void OnHighlightedStateChanged(bool isHighlighted)
        {
            UpdateOutline();
        }

        void OnSelectionStateChanged(bool isSelected)
        {
            UpdateOutline();
        }

        void UpdateOutline()
        {
            if (m_Model == null || (!m_Model.IsSelected && !m_Model.IsHighlighted))
            {
                m_Outline.enabled = false;
                return;
            }

            m_Outline.enabled = true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_Model?.OnPointerEnter(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_Model?.OnPointerExit(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            m_Model?.OnPointerClick(eventData);
        }

        public bool ValidateRaycastHit(Vector2 screenPosition, RaycastHit hit) => true;

        public int GetPhysicsRaycastSortingOrder(Vector2 screenPosition, RaycastHit hit) => ApplicationConstants.EntitiesRaycastOrder;

        public int GetPhysicsRaycastDepth(Vector2 screenPosition, RaycastHit hit) => 0;
    }
}
