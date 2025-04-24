using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class UniversalEffectorHandleView : HandleView
    {
        UniversalEffectorHandleViewModel m_Model;

        CircleControlView m_CircleControl;
        BallControlView m_BallControl;
        RingControlView m_Front;

        protected override void CreateShapes()
        {
            m_CircleControl = CreateElement<CircleControlView>("Circle", 1);
            m_BallControl = CreateElement<BallControlView>("Ball", 0);
            m_Front = CreateElement<RingControlView>("Front", 0);
        }
        
        public void SetModel(UniversalEffectorHandleViewModel model)
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

            m_CircleControl.SetModel(m_Model.CircleControlViewModel);
            m_BallControl.SetModel(m_Model.BallControlViewModel);
            m_Front.SetModel(m_Model.RingControlViewModelFront);
        }
    }
}
