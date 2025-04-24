using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    [Serializable]
    class BakedArmaturePoseModel
    {
        public enum Property
        {
            LocalPose,
            GlobalPose,
            PhysicsState
        }

        [SerializeField]
        BakedArmaturePoseData m_Data;

        public bool IsValid => m_Data.LocalPose != null
            && m_Data.GlobalPose != null
            && m_Data.PhysicsState != null
            && m_Data.LocalPose.IsValid
            && m_Data.GlobalPose.IsValid
            && m_Data.PhysicsState.IsValid
            && m_Data.LocalPose.Type == ArmatureStaticPoseData.PoseType.Local
            && m_Data.GlobalPose.Type == ArmatureStaticPoseData.PoseType.Global
            && m_Data.LocalPose.NumJoints == m_Data.GlobalPose.NumJoints;

        public int NumJoints => m_Data.LocalPose.NumJoints;
        public int NumBodies => m_Data.PhysicsState.NumBodies;

        public ArmaturePhysicsPoseModel PhysicsState => m_Data.PhysicsState;
        public ArmatureStaticPoseModel LocalPose => m_Data.LocalPose;
        public ArmatureStaticPoseModel GlobalPose => m_Data.GlobalPose;

        public delegate void Changed(BakedArmaturePoseModel model, Property property);
        public event Changed OnChanged;

        public BakedArmaturePoseModel(int numJoints, int numBodies)
        {
            m_Data.LocalPose = new ArmatureStaticPoseModel(numJoints, ArmatureStaticPoseData.PoseType.Local);
            m_Data.GlobalPose = new ArmatureStaticPoseModel(numJoints, ArmatureStaticPoseData.PoseType.Global);
            m_Data.PhysicsState = new ArmaturePhysicsPoseModel(numBodies);

            RegisterEvents();
        }

        public BakedArmaturePoseModel(BakedArmaturePoseModel other) :
            this(other.NumJoints, other.NumBodies)
        {
            other.CopyTo(this);
        }

        [JsonConstructor]
        public BakedArmaturePoseModel(BakedArmaturePoseData data)
        {
            m_Data = data;
        }

        public void CopyTo(BakedArmaturePoseModel other)
        {
            m_Data.LocalPose.CopyTo(other.m_Data.LocalPose);
            m_Data.GlobalPose.CopyTo(other.m_Data.GlobalPose);
            m_Data.PhysicsState.CopyTo(other.m_Data.PhysicsState);
        }

        public void Capture(in ArmatureMappingData sourceMapping)
        {
            Assert.IsTrue(IsValid);

            m_Data.LocalPose.Capture(in sourceMapping);
            m_Data.GlobalPose.Capture(in sourceMapping);
        }

        public void Capture(in RagdollData sourceRagdoll)
        {
            Assert.IsTrue(IsValid);

            m_Data.PhysicsState.Capture(in sourceRagdoll);
        }

        public void Capture(in ArmatureMappingData sourceMapping, in RagdollData sourceRagdoll)
        {
            Capture(in sourceMapping);
            Capture(in sourceRagdoll);
        }

        public static void ApplyInterpolated(in ArmatureMappingData targetMapping, BakedArmaturePoseModel from,
            BakedArmaturePoseModel to, float t, Vector3 translation, Quaternion rotation)
        {
            Assert.IsTrue(from.IsValid);
            Assert.IsTrue(to.IsValid);

            ArmatureStaticPoseModel.ApplyToInterpolated(in targetMapping, from.LocalPose, to.LocalPose, t, translation, rotation);
        }

        public void ApplyTo(in ArmatureMappingData targetMapping, Vector3 translation, Quaternion rotation)
        {
            Assert.IsTrue(IsValid);

            m_Data.LocalPose.ApplyTo(in targetMapping, translation, rotation);
            //m_Data.StaticStateGlobal.Apply(in targetMapping, translation, rotation);  // Note: only local is required
        }

        public void ApplyTo(in ArmatureMappingData targetMapping)
        {
            ApplyTo(in targetMapping, Vector3.zero, Quaternion.identity);
        }

        public void ApplyTo(in RagdollData targetRagdoll)
        {
            Assert.IsTrue(IsValid);

            m_Data.PhysicsState.Apply(in targetRagdoll);
        }

        public void ApplyTo(in ArmatureMappingData targetMapping, in RagdollData targetRagdoll, Vector3 translation, Quaternion rotation)
        {
            ApplyTo(in targetMapping, translation, rotation);
            ApplyTo(in targetRagdoll);
        }

        public void ApplyTo(in ArmatureMappingData targetMapping, in RagdollData targetRagdoll)
        {
            ApplyTo(in targetMapping, in targetRagdoll, Vector3.zero, Quaternion.identity);
        }

        [OnDeserialized]
        public void OnAfterDeserialize(StreamingContext context)
        {
            RegisterEvents();
        }

        void RegisterEvents()
        {
            m_Data.LocalPose.OnChanged += OnLocalPoseModelChanged;
            m_Data.GlobalPose.OnChanged += OnGlobalPoseModelChanged;
            m_Data.PhysicsState.OnChanged += OnPhysicsStateModelChanged;
        }

        void OnLocalPoseModelChanged(ArmatureStaticPoseModel model)
        {
            OnChanged?.Invoke(this, Property.LocalPose);
        }

        void OnGlobalPoseModelChanged(ArmatureStaticPoseModel model)
        {
            OnChanged?.Invoke(this, Property.GlobalPose);
        }

        void OnPhysicsStateModelChanged(ArmaturePhysicsPoseModel model)
        {
            OnChanged?.Invoke(this, Property.PhysicsState);
        }

        public Bounds GetWorldBounds()
        {
            var bounds = new Bounds();

            for (int i = 0; i < GlobalPose.NumJoints; i++)
            {
                if (i == 0)
                {
                    bounds.center = GlobalPose.GetPosition(i);
                }
                else
                {
                    bounds.Encapsulate(GlobalPose.GetPosition(i));
                }
            }

            return bounds;
        }

        /// <summary>
        /// Get the root-centered bounds of the pose.
        /// </summary>
        public Bounds GetLocalBounds()
        {
            var rootPosition = GlobalPose.GetPosition(0);
            var bounds = new Bounds();

            for (int i = 1; i < GlobalPose.NumJoints; i++)
            {
                bounds.Encapsulate(GlobalPose.GetPosition(i) - rootPosition);
            }

            return bounds;
        }
    }
}
