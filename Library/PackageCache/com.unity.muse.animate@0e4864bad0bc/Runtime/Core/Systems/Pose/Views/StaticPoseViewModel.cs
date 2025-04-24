using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    class StaticPoseViewModel
    {
        ArmatureStaticPoseModel m_StaticPoseModel;

        public delegate void PoseChanged();
        public event PoseChanged OnPoseChanged;

        public StaticPoseViewModel(ArmatureStaticPoseModel staticPoseModel)
        {
            Assert.IsNotNull(staticPoseModel, "You must provide a static pose model");

            m_StaticPoseModel = staticPoseModel;

            m_StaticPoseModel.OnChanged += OnStaticPoseModelChanged;
        }

        void OnStaticPoseModelChanged(ArmatureStaticPoseModel model)
        {
            OnPoseChanged?.Invoke();
        }

        public void ApplyPose(in ArmatureMappingData armatureMappingData)
        {
            m_StaticPoseModel.ApplyTo(in armatureMappingData);
        }
    }
}
