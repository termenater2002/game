using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class RotationHandleView : HandleView
    {
        RotationHandleViewModel m_Model;
        BallControlView m_BallControl;
        RingControlView m_Front;
        RingControlView m_AxisX;
        RingControlView m_AxisY;
        RingControlView m_AxisZ;

        protected override void CreateShapes()
        {
            m_BallControl = CreateElement<BallControlView>("Ball", 0);
            m_Front = CreateElement<RingControlView>("Front", 0);
            m_AxisX = CreateElement<RingControlView>("AxisX", 0);
            m_AxisY = CreateElement<RingControlView>("AxisY", 0);
            m_AxisZ = CreateElement<RingControlView>("AxisZ", 0);
        }
        
        public void SetModel(RotationHandleViewModel model)
        {
            base.SetModel(model);
            UnregisterModel();
            m_Model = model;
            RegisterModel();
            ForceUpdate();
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            UnregisterElements();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;
            
            m_BallControl.SetModel(m_Model.Ball);
            m_Front.SetModel(m_Model.RingFront);
            m_AxisX.SetModel(m_Model.RingX);
            m_AxisY.SetModel(m_Model.RingY);
            m_AxisZ.SetModel(m_Model.RingZ);
        }
    }
}
