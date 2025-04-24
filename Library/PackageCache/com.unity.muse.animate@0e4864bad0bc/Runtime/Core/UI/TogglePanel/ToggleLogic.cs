using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Unity.Muse.Animate
{
    class ToggleLogic
    {
        public delegate void Changed(ToggleLogic logic);
        public event Changed OnChanged;
        
        public delegate void ShowCompleted(ToggleLogic logic);
        public event ShowCompleted OnShowCompleted;
        
        public delegate void HideCompleted(ToggleLogic logic);
        public event HideCompleted OnHideCompleted;
        
        public delegate void ShowBegun(ToggleLogic logic);
        public event ShowBegun OnShowBegun;
        
        public delegate void HideBegun(ToggleLogic logic);
        public event HideBegun OnHideBegun;
        
        public delegate void ShowInterrupted(ToggleLogic logic);
        public event ShowInterrupted OnShowInterrupted;
        
        public delegate void HideInterrupted(ToggleLogic logic);
        public event HideInterrupted OnHideInterrupted;

        public VisualElement Element => m_Element;
        
        public float ShownRatio => m_ShownRatio;
        
        public bool IsShowing => m_IsShowing;
        public bool IsHiding => m_IsHiding;
        public bool IsTransitioning => m_IsTransitioning;

        bool m_IsShowing;
        bool m_IsHiding;
        bool m_IsTransitioning;
        float m_TimeToShow;
        float m_TimeToHide;
        float m_ShownRatio;
        
        ValueAnimation<float> m_Animation;
        VisualElement m_Element;

        public ToggleLogic(VisualElement element, float timeToShow, float timeToHide)
        {
            m_TimeToShow = timeToShow;
            m_TimeToHide = timeToHide;
            m_Element = element;
        }
        
        public void Show()
        {
            var dist = 1f - m_ShownRatio;
            var easedDist = Easing.InOutCubic(dist);
            var time = (int)(easedDist * m_TimeToShow*1000f);
            
            StopAnimation();
            m_Animation = m_Element.experimental.animation.Start(m_ShownRatio, 1f, time, (element, val) => SetShownRatio(val)).Ease(Easing.InOutCubic).KeepAlive();
            
            m_IsTransitioning = true;

            if (m_IsHiding)
            {
                m_IsHiding = false;
                OnHideInterrupted?.Invoke(this);
            }

            if (!m_IsShowing)
            {
                m_IsShowing = true;
                OnShowBegun?.Invoke(this);
            }
        }

        public void Hide()
        {
            var dist = m_ShownRatio;
            var easedDist = Easing.InOutCubic(dist);
            var time = (int)(easedDist * m_TimeToHide*1000f);
            
            StopAnimation();
            m_Animation = m_Element.experimental.animation.Start(m_ShownRatio, 0f, time, (element, val) => SetShownRatio(val)).Ease(Easing.InOutCubic).KeepAlive();
            
            m_IsTransitioning = true;

            if (m_IsShowing)
            {
                m_IsShowing = false;
                OnShowInterrupted?.Invoke(this);
            }

            if (!m_IsHiding)
            {
                m_IsHiding = true;
                OnHideBegun?.Invoke(this);
            }
        }

        void StopAnimation()
        {
            if (m_Animation != null)
            {
                if (m_Animation.isRunning)
                {
                    m_Animation.Stop();
                }

                m_Animation.Recycle();
                m_Animation = null;
            }
        }

        protected virtual void SetShownRatio(float val)
        {
            m_ShownRatio = val;
            OnChanged?.Invoke(this);
            
            if (m_IsShowing && m_ShownRatio >= 1f)
            {
                m_IsShowing = false;
                m_IsTransitioning = false;
                OnShowCompleted?.Invoke(this);
            }
            else if (m_IsHiding && m_ShownRatio <= 0f)
            {
                m_IsShowing = false;
                m_IsTransitioning = false;
                OnHideCompleted?.Invoke(this);
            }
        }
    }
}
