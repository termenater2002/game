using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents the key of a single entity
    /// </summary>
    [Serializable]
    class EntityKeyModel : ICopyable<EntityKeyModel>
    {
        public enum Property
        {
            Posing = 0,
            LocalPose = 1,
            GlobalPose = 2
        }

        [SerializeField]
        EntityKeyData m_Data;

        /// <summary>
        /// Checks if the key is in a valid state
        /// </summary>
        public bool IsValid => m_Data.LocalPose != null
            && m_Data.GlobalPose != null
            && m_Data.Posing != null
            && m_Data.LocalPose.IsValid
            && m_Data.GlobalPose.IsValid
            && m_Data.Posing.IsValid
            && m_Data.LocalPose.Type == ArmatureStaticPoseData.PoseType.Local
            && m_Data.GlobalPose.Type == ArmatureStaticPoseData.PoseType.Global
            && m_Data.LocalPose.NumJoints == m_Data.GlobalPose.NumJoints;

        public int NumJoints => m_Data.LocalPose.NumJoints;
        public int NumEffectors => m_Data.Posing.EffectorCount;

        public PosingModel Posing => m_Data.Posing;
        public ArmatureStaticPoseModel LocalPose => m_Data.LocalPose;
        public ArmatureStaticPoseModel GlobalPose => m_Data.GlobalPose;

        public delegate void Changed(EntityKeyModel model, Property property);
        public event Changed OnChanged;

        /// <summary>
        /// Creates a new key
        /// </summary>
        /// <param name="posing">The initial posing state, which will be copied to the key.</param>
        /// <param name="numJoints">The number of joints of the entity.</param>
        public EntityKeyModel(PosingModel posing, int numJoints)
        {
            m_Data.Posing = new PosingModel(posing);
            m_Data.LocalPose = new ArmatureStaticPoseModel(numJoints, ArmatureStaticPoseData.PoseType.Local);
            m_Data.GlobalPose = new ArmatureStaticPoseModel(numJoints, ArmatureStaticPoseData.PoseType.Global);

            RegisterEvents();
        }

        /// <summary>
        /// Creates a new key
        /// </summary>
        /// <param name="posing">The initial posing state, which will be copied to the key.</param>
        /// <param name="localPose">The initial local pose state, which will be copied to the key.</param>
        /// <param name="globalPose">The initial global pose state, which will be copied to the key.</param>
        public EntityKeyModel(PosingModel posing, ArmatureStaticPoseModel localPose, ArmatureStaticPoseModel globalPose)
        {
            m_Data.Posing = new PosingModel(posing);
            m_Data.LocalPose = new ArmatureStaticPoseModel(localPose);
            m_Data.GlobalPose = new ArmatureStaticPoseModel(globalPose);

            RegisterEvents();
        }

        [JsonConstructor]
        public EntityKeyModel(EntityKeyData m_Data)
        {
            this.m_Data = m_Data;
        }

        /// <summary>
        /// Applies the posing state of the key to another posing state
        /// </summary>
        /// <param name="target">The target posing state to which the key will be applied</param>
        public void Apply(PosingModel target)
        {
            m_Data.Posing.CopyTo(target);
        }

        /// <summary>
        /// Applies the pose state of the key to another pose state.
        /// Depending on the target pose type, this will transfer the local or the global pose of the key.
        /// </summary>
        /// <param name="target">The target pose state to which the key will be applied</param>
        public void Apply(ArmatureStaticPoseModel target)
        {
            switch (target.Type)
            {
                case ArmatureStaticPoseData.PoseType.Local:
                    m_Data.LocalPose.CopyTo(target);
                    break;

                case ArmatureStaticPoseData.PoseType.Global:
                    m_Data.GlobalPose.CopyTo(target);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Applies the pose state of the key to an armature.
        /// </summary>
        /// <param name="target">The target armature to which the key will be applied</param>
        public void Apply(in ArmatureMappingData target)
        {
            m_Data.LocalPose.ApplyTo(target);
        }

        /// <summary>
        /// Captures the key state from an external posing and pose states
        /// </summary>
        /// <param name="sourcePosing">The source posing state, which will be copied to the key.</param>
        /// <param name="sourceLocalPose">The source local pose state, which will be copied to the key.</param>
        /// <param name="sourceGlobalPose">The source global pose state, which will be copied to the key.</param>
        public void Capture(PosingModel sourcePosing, ArmatureStaticPoseModel sourceLocalPose, ArmatureStaticPoseModel sourceGlobalPose)
        {
            sourcePosing.CopyTo(m_Data.Posing);
            sourceLocalPose.CopyTo(m_Data.LocalPose);
            sourceGlobalPose.CopyTo(m_Data.GlobalPose);
        }

        /// <summary>
        /// Copies the key to another key
        /// </summary>
        /// <param name="target">The target key to which this key will be copied</param>
        public void CopyTo(EntityKeyModel target)
        {
            m_Data.Posing.CopyTo(target.m_Data.Posing);
            m_Data.LocalPose.CopyTo(target.m_Data.LocalPose);
            m_Data.GlobalPose.CopyTo(target.m_Data.GlobalPose);
        }

        /// <summary>
        /// Creates a new instance that is a copy of that key
        /// </summary>
        /// <returns>A new key instance with the same data</returns>
        public EntityKeyModel Clone()
        {
            var newKey = new EntityKeyModel(m_Data.Posing, m_Data.LocalPose, m_Data.GlobalPose);
            CopyTo(newKey);

            return newKey;
        }

        /// <summary>
        /// Translate the key by a given offset
        /// </summary>
        /// <param name="horizontalOffset">The offset to be applied to the key</param>
        public void Translate(Vector3 horizontalOffset)
        {
            m_Data.GlobalPose.Translate(horizontalOffset);
            m_Data.LocalPose.Translate(horizontalOffset);
            m_Data.Posing.Translate(horizontalOffset);
        }

        /// <summary>
        /// Checks if both key share compatible data, that is to say data for the same character configuration
        /// </summary>
        /// <param name="other">The other key to check compatibility with</param>
        /// <returns>true if both keys are compatible, false otherwise</returns>
        public bool IsCompatibleWith(EntityKeyModel other)
        {
            return Posing.IsCompatibleWith(other.Posing)
                && LocalPose.IsCompatibleWith(other.LocalPose)
                && GlobalPose.IsCompatibleWith(other.GlobalPose);
        }

        [OnDeserialized]
        public void OnAfterDeserialize(StreamingContext context)
        {
            RegisterEvents();
        }

        void RegisterEvents()
        {
            m_Data.Posing.OnChanged += OnPosingModelChanged;
            m_Data.LocalPose.OnChanged += OnLocalPoseModelChanged;
            m_Data.GlobalPose.OnChanged += OnGlobalPoseModelChanged;
        }

        void OnPosingModelChanged(PosingModel model, DeepPoseEffectorModel effectorModel, DeepPoseEffectorModel.Property property)
        {
            OnChanged?.Invoke(this, Property.Posing);
        }

        void OnLocalPoseModelChanged(ArmatureStaticPoseModel model)
        {
            OnChanged?.Invoke(this, Property.LocalPose);
        }

        void OnGlobalPoseModelChanged(ArmatureStaticPoseModel model)
        {
            OnChanged?.Invoke(this, Property.GlobalPose);
        }

        public Bounds GetWorldBounds()
        {
            var bounds = new Bounds();
            for (int i = 0; i < m_Data.GlobalPose.NumJoints; i++)
            {
                if (i == 0)
                {
                    bounds.center = m_Data.GlobalPose.GetPosition(i);
                }
                else
                {
                    var jointPosition = m_Data.GlobalPose.GetPosition(i);
                    bounds.Encapsulate(jointPosition);
                }
            }

            return bounds;
        }
    }
}
