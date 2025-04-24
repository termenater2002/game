using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    [Serializable]
    class ArmatureStaticPoseModel
    {
        [SerializeField]
        ArmatureStaticPoseData m_Data;

        public bool IsValid => m_Data.TransformModels != null;
        public int NumJoints => m_Data.TransformModels.Length;
        public ArmatureStaticPoseData.PoseType Type => m_Data.Type;

        public delegate void Changed(ArmatureStaticPoseModel model);
        public event Changed OnChanged;

        public ArmatureStaticPoseModel(int numJoints, ArmatureStaticPoseData.PoseType type)
        {
            m_Data.Type = type;
            m_Data.TransformModels = new RigidTransformModel[numJoints];
            for (var i = 0; i < numJoints; i++)
            {
                m_Data.TransformModels[i] = new RigidTransformModel();
            }

            RegisterTransforms();
        }

        public ArmatureStaticPoseModel(ArmatureStaticPoseModel other) :
            this(other.NumJoints, other.Type)
        {
            other.CopyTo(this);
        }

        [JsonConstructor]
        public ArmatureStaticPoseModel(ArmatureStaticPoseData m_Data)
        {
            this.m_Data = m_Data;
        }

        public void CopyTo(ArmatureStaticPoseModel other)
        {
            Assert.AreEqual(NumJoints, other.NumJoints);
            Assert.AreEqual(Type, other.Type);

            for (var i = 0; i < NumJoints; i++)
            {
                m_Data.TransformModels[i].CopyTo(other.m_Data.TransformModels[i]);
            }
        }

        public RigidTransformData GetTransform(int jointIdx)
        {
            var transformModel = GetTransformModel(jointIdx);
            return transformModel.Transform;
        }

        public void SetTransform(int jointIdx, RigidTransformData transform)
        {
            var transformModel = GetTransformModel(jointIdx);
            transformModel.Transform = transform;
        }

        public RigidTransformModel GetTransformModel(int jointIdx)
        {
            if (jointIdx < 0 || jointIdx >= NumJoints)
                AssertUtils.Fail($"Invalid joint index: {jointIdx.ToString()}");
            return m_Data.TransformModels[jointIdx];
        }

        public Vector3 GetPosition(int jointIdx)
        {
            var transformModel = GetTransformModel(jointIdx);
            return transformModel.Position;
        }

        public void SetPosition(int jointIdx, Vector3 position)
        {
            var transformModel = GetTransformModel(jointIdx);
            transformModel.Position = position;
        }

        public Quaternion GetRotation(int jointIdx)
        {
            var transformModel = GetTransformModel(jointIdx);
            return transformModel.Rotation;
        }

        public void SetRotation(int jointIdx, Quaternion rotation)
        {
            var transformModel = GetTransformModel(jointIdx);
            transformModel.Rotation = rotation;
        }

        public void Capture(in ArmatureMappingData source)
        {
            Assert.IsTrue(IsValid);
            Assert.AreEqual(NumJoints, source.NumJoints);

            var globalTransforms = Type == ArmatureStaticPoseData.PoseType.Global;

            for (var i = 0; i < m_Data.TransformModels.Length; i++)
            {
                var sourceTransform = source.Transforms[i];

                var rigidTransform = (i == 0 || globalTransforms) ?
                    new RigidTransformData(sourceTransform.position, sourceTransform.rotation) :
                    new RigidTransformData(sourceTransform.localPosition, sourceTransform.localRotation);
                m_Data.TransformModels[i].Transform = rigidTransform;
            }
        }

        public static void ApplyToInterpolated(in ArmatureMappingData target, ArmatureStaticPoseModel from,
            ArmatureStaticPoseModel to, float t, Vector3 translation, Quaternion rotation)
        {
            Assert.IsTrue(from.IsValid);
            Assert.IsTrue(to.IsValid);
            Assert.AreEqual(from.NumJoints, target.NumJoints);
            Assert.AreEqual(to.NumJoints, target.NumJoints);
            Assert.AreEqual(from.Type, to.Type);

            var globalTransforms = from.Type == ArmatureStaticPoseData.PoseType.Global;

            for (var i = 0; i < from.m_Data.TransformModels.Length; i++)
            {
                var targetTransform = target.Transforms[i];
                var fromRigidTransform = from.m_Data.TransformModels[i].Transform;
                var toRigidTransform = to.m_Data.TransformModels[i].Transform;

                var interpolatedPosition = Vector3.Lerp(fromRigidTransform.Position, toRigidTransform.Position, t);
                var interpolatedRotation = Quaternion.Slerp(fromRigidTransform.Rotation, toRigidTransform.Rotation, t);

                if (i == 0 || globalTransforms)
                {
                    interpolatedPosition = translation + rotation * interpolatedPosition;
                    interpolatedRotation = rotation * interpolatedRotation;
                    targetTransform.SetPositionAndRotation(interpolatedPosition, interpolatedRotation);
                }
                else
                {
                    targetTransform.localPosition = interpolatedPosition;
                    targetTransform.localRotation = interpolatedRotation;
                }
            }
        }

        public static void ApplyToInterpolated(in ArmatureMappingData target, ArmatureStaticPoseModel from,
            ArmatureStaticPoseModel to, float t)
        {
            ApplyToInterpolated(in target, from, to, t, Vector3.zero, Quaternion.identity);
        }

        public void ApplyTo(in ArmatureMappingData target, Vector3 translation, Quaternion rotation)
        {
            Assert.IsTrue(IsValid);
            Assert.AreEqual(NumJoints, target.NumJoints);

            var globalTransforms = Type == ArmatureStaticPoseData.PoseType.Global;

            for (var i = 0; i < m_Data.TransformModels.Length; i++)
            {
                var targetTransform = target.Transforms[i];
                var rigidTransform = m_Data.TransformModels[i].Transform;
                
                if (i == 0 || globalTransforms)
                {
                    var targetPosition = translation + rotation * rigidTransform.Position;
                    var targetRotation = rotation * rigidTransform.Rotation;
                    targetTransform.SetPositionAndRotation(targetPosition, targetRotation);
                }
                else
                {
                    targetTransform.localPosition = rigidTransform.Position;
                    targetTransform.localRotation = rigidTransform.Rotation;
                }
            }
        }

        public void ApplyTo(in ArmatureMappingData target)
        {
            ApplyTo(target, Vector3.zero, Quaternion.identity);
        }

        public void Interpolate(ArmatureStaticPoseModel from, ArmatureStaticPoseModel to, float t)
        {
            Assert.IsTrue(IsValid);
            Assert.IsTrue(from.IsValid);
            Assert.IsTrue(to.IsValid);
            Assert.AreEqual(NumJoints, from.NumJoints);
            Assert.AreEqual(NumJoints, to.NumJoints);
            Assert.AreEqual(Type, from.Type);
            Assert.AreEqual(Type, to.Type);

            for (var i = 0; i < m_Data.TransformModels.Length; i++)
            {
                var fromTransform = from.m_Data.TransformModels[i].Transform;
                var toTransform = to.m_Data.TransformModels[i].Transform;
                var position = math.lerp(fromTransform.Position, toTransform.Position, t);
                var rotation = math.slerp(fromTransform.Rotation, toTransform.Rotation, t);

                m_Data.TransformModels[i].Transform = new RigidTransformData(position, rotation);
            }
        }

        public void Translate(Vector3 offset)
        {
            if (NumJoints == 0)
                return;

            for (var i = 0; i < m_Data.TransformModels.Length; i++)
            {
                m_Data.TransformModels[i].Translate(offset);

                if (Type == ArmatureStaticPoseData.PoseType.Local)
                    break;
            }
        }

        public void Rotate(Vector3 pivot, Quaternion offset)
        {
            if (NumJoints == 0)
                return;

            for (var i = 0; i < m_Data.TransformModels.Length; i++)
            {
                m_Data.TransformModels[i].Rotate(pivot, offset);

                if (Type == ArmatureStaticPoseData.PoseType.Local)
                    break;
            }
        }

        /// <summary>
        /// Checks if both poses share compatible data
        /// </summary>
        /// <param name="other">The other pose to check compatibility with</param>
        /// <returns>true if both poses are compatible, false otherwise</returns>
        public bool IsCompatibleWith(ArmatureStaticPoseModel other)
        {
            return other.Type == Type && other.NumJoints == NumJoints;
        }

        [OnDeserialized]
        public void OnAfterDeserialize(StreamingContext context)
        {
            RegisterTransforms();
        }

        void RegisterTransforms()
        {
            for (var i = 0; i < NumJoints; i++)
            {
                var transformModel = m_Data.TransformModels[i];
                transformModel.OnChanged += (model) => OnChanged?.Invoke(this);
            }
        }
    }
}
