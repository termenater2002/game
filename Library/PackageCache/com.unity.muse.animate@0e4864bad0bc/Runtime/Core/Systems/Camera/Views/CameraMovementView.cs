using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    [ExecuteAlways]
    [RequireComponent(typeof(CustomPhysicsRaycaster))]
    class CameraMovementView : MonoBehaviour, IPhysicsRaycastHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        CameraMovementViewModel m_Model;
        CustomPhysicsRaycaster m_CustomPhysicsRaycaster;
        
        void OnEnable()
        {
            RegisterModel();
        }

        void OnDisable()
        {
            UnregisterModel();
        }

        void Update()
        {
            if (m_Model == null)
                return;

            m_Model.OnUpdate(Time.deltaTime);
        }

        public void SetModel(CameraMovementViewModel model)
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

            if (m_CustomPhysicsRaycaster != null)
                m_CustomPhysicsRaycaster.UnregisterInfiniteObject(gameObject);

            m_CustomPhysicsRaycaster = null;
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            m_CustomPhysicsRaycaster = GetComponent<CustomPhysicsRaycaster>();
            Assert.IsNotNull(m_CustomPhysicsRaycaster, "Could not find CustomPhysicsRaycaster in scene");
            m_CustomPhysicsRaycaster.RegisterInfiniteObject(gameObject);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Log("OnBeginDrag()");
            m_Model?.OnBeginDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Log("OnEndDrag()");
            m_Model?.OnEndDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Log($"OnDrag({eventData.delta})");
            m_Model?.OnDrag(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Log("OnPointerClick()");
            m_Model?.OnPointerClick(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Log("OnPointerDown()");
            m_Model?.OnPointerDown(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Log("OnPointerEnter()");
            m_Model?.OnPointerEnter(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Log("OnPointerExit()");
            m_Model?.OnPointerExit(eventData);
        }

        void Log(string msg)
        {
            if (!ApplicationConstants.DebugCameraMovement)
                return;
            
            Debug.Log(GetType().Name+" -> "+msg);
        }
        
        public bool ValidateRaycastHit(Vector2 screenPosition, RaycastHit hit) => true;

        public int GetPhysicsRaycastSortingOrder(Vector2 screenPosition, RaycastHit hit)
            => m_Model.ShouldCameraCaptureInput ? ApplicationConstants.ForegroundCameraOrder : ApplicationConstants.BackgroundCameraOrder;

        public int GetPhysicsRaycastDepth(Vector2 screenPosition, RaycastHit hit) => 0;

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (m_Model == null)
                return;

            UnityEditor.Handles.color = Color.cyan;
            UnityEditor.Handles.DrawWireCube(m_Model.Pivot, new Vector3(0.1f, 0.1f, 0.1f));
        }
#endif
    }
}
