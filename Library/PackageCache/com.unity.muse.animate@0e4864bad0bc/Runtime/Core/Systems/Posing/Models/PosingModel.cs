using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    [Serializable]
    class PosingModel
    {
        [SerializeField]
        PosingData m_Data;

        public bool IsValid => m_Data.DeepPoseEffectors != null;
        public int EffectorCount => m_Data.DeepPoseEffectors.Length;

        public delegate void Changed(PosingModel model, DeepPoseEffectorModel effectorModel, DeepPoseEffectorModel.Property property);
        public event Changed OnChanged;

        [JsonConstructor]
        public PosingModel(PosingData m_Data)
        {
            this.m_Data = m_Data;
        }

        public PosingModel(ArmatureEffectorIndex[] armatureEffectorIndices)
        {
            var numEffectors = armatureEffectorIndices.Length;
            m_Data.DeepPoseEffectors = new DeepPoseEffectorModel[numEffectors];

            for (var i = 0; i < armatureEffectorIndices.Length; i++)
            {
                var armatureEffectorIndex = armatureEffectorIndices[i];
                var effectorModel = new DeepPoseEffectorModel(armatureEffectorIndex);
                m_Data.DeepPoseEffectors[i] = effectorModel;
                RegisterEffectorModel(effectorModel);
            }
        }

        public PosingModel(PosingModel other)
        {
            m_Data.DeepPoseEffectors = new DeepPoseEffectorModel[other.EffectorCount];
            for (var i = 0; i < other.EffectorCount; i++)
            {
                var sourceEffectorModel = other.GetEffectorModel(i);
                var effectorModel = new DeepPoseEffectorModel(sourceEffectorModel);
                m_Data.DeepPoseEffectors[i] = effectorModel;

                RegisterEffectorModel(effectorModel);
            }
        }

        public void CopyTo(PosingModel other)
        {
            Assert.AreEqual(EffectorCount, other.EffectorCount);

            for (var i = 0; i < m_Data.DeepPoseEffectors.Length; i++)
            {
                m_Data.DeepPoseEffectors[i].CopyTo(other.m_Data.DeepPoseEffectors[i]);
            }
        }

        public DeepPoseEffectorModel GetEffectorModel(int idx)
        {
            if (idx < 0 || idx > EffectorCount)
                AssertUtils.Fail($"Invalid index: {idx.ToString()}");
            var effectorModel = m_Data.DeepPoseEffectors[idx];
            return effectorModel;
        }

        public DeepPoseEffectorModel GetEffectorModel(DeepPoseEffectorIndex effectorIndex)
        {
            if (!TryGetEffectorModel(effectorIndex, out var model))
                AssertUtils.Fail($"Invalid effector index: {effectorIndex.ToString()}");

            return model;
        }

        public bool TryGetEffectorModel(DeepPoseEffectorIndex effectorIndex, out DeepPoseEffectorModel model)
        {
            for (var i = 0; i < m_Data.DeepPoseEffectors.Length; i++)
            {
                var effectorModel = m_Data.DeepPoseEffectors[i];
                if (effectorModel.Index.EffectorIndex == effectorIndex)
                {
                    model = effectorModel;
                    return true;
                }
            }

            model = null;
            return false;
        }

        public void Translate(Vector3 offset)
        {
            for (var i = 0; i < m_Data.DeepPoseEffectors.Length; i++)
            {
                var effectorModel = m_Data.DeepPoseEffectors[i];
                if ((effectorModel.HandlesPosition && effectorModel.PositionEnabled)
                    || (effectorModel.HandlesLookAt && effectorModel.LookAtEnabled))
                    effectorModel.Position += offset;
            }
        }

        public void Rotate(Vector3 pivot, Quaternion offset)
        {
            for (var i = 0; i < m_Data.DeepPoseEffectors.Length; i++)
            {
                var effectorModel = m_Data.DeepPoseEffectors[i];
                if ((effectorModel.HandlesPosition && effectorModel.PositionEnabled)
                    || (effectorModel.HandlesLookAt && effectorModel.LookAtEnabled))
                    effectorModel.Position = pivot + offset * (effectorModel.Position - pivot);
                if (effectorModel.HandlesRotation && effectorModel.RotationEnabled)
                    effectorModel.Rotation = offset * effectorModel.Rotation;
            }
        }

        /// <summary>
        /// Checks if both posing models share compatible data
        /// </summary>
        /// <param name="other">The other posing model to check compatibility with</param>
        /// <returns>true if both models are compatible, false otherwise</returns>
        public bool IsCompatibleWith(PosingModel other)
        {
            if (other.EffectorCount != EffectorCount)
                return false;

            for (var i = 0; i < EffectorCount; i++)
            {
                var myEffector = GetEffectorModel(i);
                var otherEffector = other.GetEffectorModel(i);

                if (!myEffector.IsCompatibleWith(otherEffector))
                    return false;
            }

            return true;
        }

        [OnDeserialized]
        public void OnAfterDeserialize(StreamingContext context)
        {
            for (var i = 0; i < m_Data.DeepPoseEffectors.Length; i++)
            {
                var effectorModel = m_Data.DeepPoseEffectors[i];
                RegisterEffectorModel(effectorModel);
            }
        }

        void RegisterEffectorModel(DeepPoseEffectorModel effectorModel)
        {
            effectorModel.OnPropertyChanged += OnEffectorModelPropertyChanged;
        }

        void UnregisterEffectorModel(DeepPoseEffectorModel effectorModel)
        {
            effectorModel.OnPropertyChanged -= OnEffectorModelPropertyChanged;
        }

        void OnEffectorModelPropertyChanged(DeepPoseEffectorModel model, DeepPoseEffectorModel.Property property)
        {
            OnChanged?.Invoke(this, model, property);
        }
    }
}
