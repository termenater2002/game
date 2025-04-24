using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class TranslationHandleView : HandleView
    {
        TranslationHandleViewModel m_Model;
        CircleControlView m_Circle;
        AxisControlView m_AxisX;
        AxisControlView m_AxisY;
        AxisControlView m_AxisZ;
        PlaneControlView m_PlaneXY;
        PlaneControlView m_PlaneXZ;
        PlaneControlView m_PlaneYZ;

        protected override void CreateShapes()
        {
            m_Circle = CreateElement<CircleControlView>("Circle", 1);
            m_AxisX = CreateElement<AxisControlView>("AxisX", 0);
            m_AxisY = CreateElement<AxisControlView>("AxisY", 0);
            m_AxisZ = CreateElement<AxisControlView>("AxisZ", 0);
            m_PlaneXY = CreateElement<PlaneControlView>("PlaneXY", 0);
            m_PlaneXZ = CreateElement<PlaneControlView>("PlaneXZ", 0);
            m_PlaneYZ = CreateElement<PlaneControlView>("PlaneYZ", 0);
        }
        
        public void SetModel(TranslationHandleViewModel model)
        {
            base.SetModel(model);
            UnregisterModel();
            m_Model = model;
            RegisterModel();
            ForceUpdate();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            m_Circle.SetModel(m_Model.Circle);

            m_AxisX.SetModel(m_Model.AxisX);
            m_AxisY.SetModel(m_Model.AxisY);
            m_AxisZ.SetModel(m_Model.AxisZ);

            m_PlaneXY.SetModel(m_Model.PlaneXY);
            m_PlaneXZ.SetModel(m_Model.PlaneXZ);
            m_PlaneYZ.SetModel(m_Model.PlaneYZ);
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;
            
            UnregisterElements();
        }
    }
}
