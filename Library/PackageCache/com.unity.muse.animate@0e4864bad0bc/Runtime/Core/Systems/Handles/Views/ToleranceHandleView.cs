using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class ToleranceHandleView : HandleView
    {
        ToleranceHandleViewModel m_Model;
        RadiusControlView m_RadiusControl;
        
        public void SetModel(ToleranceHandleViewModel model)
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

            m_RadiusControl.SetModel(m_Model.RadiusControlViewModel);
        }

        protected override void CreateShapes()
        {
            m_RadiusControl = CreateElement<RadiusControlView>("Radius", 0);
        }
    }
}
