using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    abstract class HandleViewModel
    {
        public event Action OnVisibilityChanged;
        public event Action OnRotationChanged;
        public event Action OnPositionChanged;
        public event Action<float> OnStep;
        
        protected CameraModel CameraModel => m_CameraModel;

        public bool IsVisible
        {
            get => m_IsVisible;
            set
            {
                if (value == m_IsVisible)
                    return;

                m_IsVisible = value;
                OnVisibilityChanged?.Invoke();
            }
        }

        CameraModel m_CameraModel;
        bool m_IsVisible;

        protected HandleViewModel(CameraModel cameraModel)
        {
            m_CameraModel = cameraModel;
            m_IsVisible = true;
        }

        public void Step(float delta)
        {
            OnStep?.Invoke(delta);
        }
        
        protected void InvokeRotationChanged()
        {
            OnRotationChanged?.Invoke();
        }
        
        protected void InvokePositionChanged()
        {
            OnPositionChanged?.Invoke();
        }
    }
}
