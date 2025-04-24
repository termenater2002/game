using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    [ExecuteAlways]
    abstract class HandleView : MonoBehaviour, IPointerClickHandler
    {
        bool m_IsInitialized;
        List<ControlView> m_Elements = new();

        public void Initialize()
        {
            if (m_IsInitialized)
                return;
            
            gameObject.SetActive(true);
            
            m_IsInitialized = true;
            
            CreateShapes();
            ForceUpdate();
        }
        
        protected void SetModel(HandleViewModel model)
        {
            Assert.IsTrue(m_IsInitialized, "HandleView must be Initialized first");
            
            model.OnPositionChanged += ForceUpdate;
            model.OnRotationChanged += ForceUpdate;
            model.OnVisibilityChanged += ForceUpdate;
            model.OnStep += OnStep;
            ForceUpdate();
        }

        void OnStep(float delta)
        {
            Step(delta);
        }

        protected T CreateElement<T>(string createdObjectName, int priority = 0) where T : ControlView
        {
            T element = HandlesUtils.CreateElement<T>(createdObjectName, transform, priority);
            m_Elements.Add(element);
            return element;
        }

        public event Action<Vector2> OnHandleRightClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right || OnHandleRightClick == null)
            {
                return;
            }

            OnHandleRightClick(eventData.position);
            eventData.Use();
        }
        
        protected virtual void CreateShapes()
        {
            
        }
        
        protected virtual void Step(float delta)
        {
            for (int i = 0; i < m_Elements.Count; i++)
            {
                m_Elements[i].Step(delta);
            }
        }
        
        protected virtual void ForceUpdate()
        {
            for (int i = 0; i < m_Elements.Count; i++)
            {
                m_Elements[i].ForceUpdate();
            }
        }

        protected void UnregisterElements()
        {
            for (int i = 0; i < m_Elements.Count; i++)
            {
                m_Elements[i].SetModel(null);
            }
        }
    }
}
