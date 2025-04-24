using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    [RequireComponent(typeof(ArmatureMappingComponent))]
    [ExecuteAlways]
    class StaticPoseView : MonoBehaviour
    {
        StaticPoseViewModel m_Model;
        ArmatureMappingComponent m_ArmatureMappingComponent;
        bool m_NeedToUpdatePose = false;

        public void Awake()
        {
            m_ArmatureMappingComponent = GetComponent<ArmatureMappingComponent>();
            Assert.IsTrue(m_ArmatureMappingComponent.IsValid, "Invalid ArmatureMappingComponent");
        }
        
        public void Setup(StaticPoseViewModel model, string objectName, bool activeByDefault, bool updateWhenOffscreen, int layer)
        {
            gameObject.name = objectName;
            gameObject.SetActive(activeByDefault);
            gameObject.SetLayer(layer, true);
            
            SetModel(model);
            SetUpdateWhenOffscreen(updateWhenOffscreen);
            
            // Must call Awake here if not in play mode
            if (!UnityEngine.Application.isPlaying)
            {
                Awake();
            }
        }
        
        public void Step(float delta)
        {
            if (m_Model == null)
                return;

            if (m_NeedToUpdatePose)
                ApplyPose();
        }

        public void SetModel(StaticPoseViewModel model)
        {
            ForceMeshUpdate();
            UnregisterModel();
            m_Model = model;
            RegisterModel();
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnPoseChanged -= OnPoseChanged;
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            ApplyPose();
            m_Model.OnPoseChanged += OnPoseChanged;
        }

        void OnPoseChanged()
        {
            m_NeedToUpdatePose = true;
        }

        public void ApplyPose()
        {
            Assert.IsNotNull(m_ArmatureMappingComponent, "Cannot Update Pose: Missing ArmatureMappingComponent");
            Assert.IsTrue(m_ArmatureMappingComponent.IsValid, "Cannot Update Pose: Invalid ArmatureMappingComponent");
            
            m_Model.ApplyPose(m_ArmatureMappingComponent.ArmatureMappingData);
            
            m_NeedToUpdatePose = false;
        }
        
        public void ForceMeshUpdate()
        {
            foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                skinnedMeshRenderer.forceMatrixRecalculationPerRender = true;
            }
        }
        
        public void SetUpdateWhenOffscreen(bool value)
        {
            foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                skinnedMeshRenderer.updateWhenOffscreen = value;
            }
        }
    }
}
