using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class PosingView : MonoBehaviour
    {
        PosingViewModel m_Model;
        List<PosingEffectorControlView> m_Effectors;

        public void Initialize()
        {
            m_Effectors = new();
        }
        
        public void SetModel(PosingViewModel model)
        {
            UnregisterModel();
            m_Model = model;
            RegisterModel();
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            foreach (var effector in m_Effectors)
            {
                effector.SetModel(null);
            }
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;
            
            for (var i = 0; i < m_Model.EffectorCount; i++)
            {
                var posingEffector = CreatePosingEffector(m_Model.GetEffector(i));
                m_Effectors.Add(posingEffector);
            }
        }

        public void Step(float delta)
        {
            for (var i = 0; i < m_Effectors.Count; i++)
            {
                m_Effectors[i].Step(delta);
            }
        }
        
        public void ForceUpdate()
        {
            for (var i = 0; i < m_Effectors.Count; i++)
            {
                m_Effectors[i].ForceUpdate();
            }
        }
        
        PosingEffectorControlView CreatePosingEffector(PosingEffectorControlViewModel model)
        {
            var view = HandlesUtils.CreateElement<PosingEffectorControlView>("PosingEffector", transform, ApplicationConstants.PosingEffectorRaycastOrder);
            view.SetModel(model);
            return view;
        }
    }
}
