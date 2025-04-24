using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    class EntityView : MonoBehaviour, IPointerClickHandler
    {
        public event Action<Vector2> OnRightClick;
        public EntityID EntityId { get; set; }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnRightClick?.Invoke(eventData.position);
            }
        }
    }
}
