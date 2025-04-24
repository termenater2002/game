using System;
using Unity.DeepPose.Components;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    static class PosingModelUtils
    {
        public static void SnapToPose(this PosingModel model, ArmatureStaticPoseModel staticPoseModel, bool onlyIfInactive)
        {
            Assert.AreEqual(ArmatureStaticPoseData.PoseType.Global, staticPoseModel.Type);

            var effectorsCount = model.EffectorCount;
            for (var i = 0; i < effectorsCount; i++)
            {
                var effectorModel = model.GetEffectorModel(i);

                effectorModel.SnapToPose(staticPoseModel, onlyIfInactive);
            }
        }

        public static void SyncFromComponent(this PosingModel model, DeepPoseComponent component)
        {
            Assert.IsNotNull(component, "You must provide a DeepPoseComponent");

            for (var i = 0; i < model.EffectorCount; i++)
            {
                var effectorModel = model.GetEffectorModel(i);
                effectorModel.SyncFromComponent(component);
            }
        }

        public static void ApplyToComponent(this PosingModel model, DeepPoseComponent component)
        {
            Assert.IsNotNull(component, "You must provide a DeepPoseComponent");

            for (var i = 0; i < model.EffectorCount; i++)
            {
                var effectorModel = model.GetEffectorModel(i);
                effectorModel.ApplyToComponent(component);
            }
        }

        public static ArmatureEffectorIndex[] GetEffectorIndices(this DeepPoseComponent component, ArmatureToArmatureMapping componentToTargetMapping)
        {
            component.GetEffectorIndices(out var effectorIndices, out var jointIndices);

            var indices = new ArmatureEffectorIndex[effectorIndices.Length];
            for (var i = 0; i < indices.Length; i++)
            {
                var effectorIndex = effectorIndices[i];
                var sourceJointIndex = jointIndices[i];
                var targetJointIndex = componentToTargetMapping.GetTargetJointIndex(sourceJointIndex);
                indices[i] = new ArmatureEffectorIndex(effectorIndex, targetJointIndex);
            }

            return indices;
        }

        static void GetEffectorIndices(this DeepPoseComponent component, out DeepPoseEffectorIndex[] outEffectorIndices, out int[] outJointIndices)
        {
            using var tmpEffectorIndexList = TempList<DeepPoseEffectorIndex>.Allocate();
            using var tmpJointIndexList = TempList<int>.Allocate();

            var data = component.DeepPoseSolverData;

            var numJoints = data.Config.Skeleton.Count;
            for (var jointId = 0; jointId < numJoints; jointId++)
            {
                var positionIdx = component.GetPositionEffectorIndex(jointId);
                var rotationIdx = component.GetRotationEffectorIndex(jointId);
                var lookAtIdx = component.GetLookAtEffectorIndex(jointId);

                if (positionIdx >= 0 || rotationIdx >= 0)
                {
                    tmpEffectorIndexList.Add(new DeepPoseEffectorIndex(positionIdx, rotationIdx, -1));
                    tmpJointIndexList.Add(jointId);
                }

                if (lookAtIdx >= 0)
                {
                    tmpEffectorIndexList.Add(DeepPoseEffectorIndex.LookAtEffector(lookAtIdx));
                    tmpJointIndexList.Add(jointId);
                }
            }

            outEffectorIndices = new DeepPoseEffectorIndex[tmpEffectorIndexList.Count];
            tmpEffectorIndexList.List.CopyTo(outEffectorIndices, 0);

            outJointIndices = new int[tmpJointIndexList.Count];
            tmpJointIndexList.List.CopyTo(outJointIndices, 0);
        }

        static int GetPositionEffectorIndex(this DeepPoseComponent component, int jointId)
        {
            var data = component.DeepPoseSolverData;
            for (var i = 0; i < data.Positions.Count; i++)
            {
                var effector = data.Positions[i];
                if (effector.id == jointId)
                    return i;
            }

            return -1;
        }

        static int GetRotationEffectorIndex(this DeepPoseComponent component, int jointId)
        {
            var data = component.DeepPoseSolverData;
            for (var i = 0; i < data.Rotations.Count; i++)
            {
                var effector = data.Rotations[i];
                if (effector.id == jointId)
                    return i;
            }

            return -1;
        }

        static int GetLookAtEffectorIndex(this DeepPoseComponent component, int jointId)
        {
            var data = component.DeepPoseSolverData;
            for (var i = 0; i < data.LookAts.Count; i++)
            {
                var effector = data.LookAts[i];
                if (effector.id == jointId)
                    return i;
            }

            return -1;
        }

        public static void CopyTo(this ArmatureStaticPoseModel source,
            Span<Quaternion> target,
            ArmatureToArmatureMapping sourceToTargetMapping,
            bool inverseMapping = false)
        {
            if (inverseMapping)
            {
                Assert.AreEqual(source.NumJoints, sourceToTargetMapping.NumTargetJoints);
                Assert.AreEqual(target.Length, sourceToTargetMapping.NumSourceJoints);

                for (var i = 0; i < target.Length; i++)
                {
                    if (sourceToTargetMapping.TryGetTargetJointIndex(i, out var targetJointIndex))
                    {
                        target[i] = source.GetRotation(targetJointIndex);
                    }
                }

                return;
            }

            Assert.AreEqual(source.NumJoints, sourceToTargetMapping.NumSourceJoints);
            Assert.AreEqual(target.Length, sourceToTargetMapping.NumTargetJoints);

            for (var j = 0; j < source.NumJoints; j++)
            {
                if (sourceToTargetMapping.TryGetTargetJointIndex(j, out var targetJointIndex))
                {
                    target[targetJointIndex] = source.GetRotation(j);
                }
            }
        }

        public static void CopyFrom(this ArmatureStaticPoseModel target, ReadOnlySpan<Quaternion> source, ArmatureToArmatureMapping sourceToTargetMapping, bool inverseMapping)
        {
            if (inverseMapping)
            {
                Assert.AreEqual(source.Length, sourceToTargetMapping.NumTargetJoints);
                Assert.AreEqual(target.NumJoints, sourceToTargetMapping.NumSourceJoints);

                for (var i = 0; i < target.NumJoints; i++)
                {
                    if (sourceToTargetMapping.TryGetTargetJointIndex(i, out var targetJointIndex))
                    {
                        target.SetRotation(i, source[targetJointIndex]);
                    }
                }

                return;
            }

            Assert.AreEqual(source.Length, sourceToTargetMapping.NumSourceJoints);
            Assert.AreEqual(target.NumJoints, sourceToTargetMapping.NumTargetJoints);

            for (var i = 0; i < source.Length; i++)
            {
                if (sourceToTargetMapping.TryGetTargetJointIndex(i, out var targetJointIndex))
                {
                    target.SetRotation(targetJointIndex, source[i]);
                }
            }
        }

        public static void CopyTo(this ArmatureStaticPoseModel source,
            ArmatureStaticPoseModel target,
            ArmatureToArmatureMapping sourceToTargetMapping,
            bool inverseMapping = false)
        {
            if (inverseMapping)
            {
                Assert.AreEqual(source.NumJoints, sourceToTargetMapping.NumTargetJoints);
                Assert.AreEqual(target.NumJoints, sourceToTargetMapping.NumSourceJoints);

                for (var i = 0; i < target.NumJoints; i++)
                {
                    if (sourceToTargetMapping.TryGetTargetJointIndex(i, out var targetJointIndex))
                    {
                        target.SetRotation(i, source.GetRotation(targetJointIndex));
                        target.SetPosition(i, source.GetPosition(targetJointIndex));
                    }
                }

                return;
            }

            Assert.AreEqual(source.NumJoints, sourceToTargetMapping.NumSourceJoints);
            Assert.AreEqual(target.NumJoints, sourceToTargetMapping.NumTargetJoints);

            for (var i = 0; i < source.NumJoints; i++)
            {
                if (sourceToTargetMapping.TryGetTargetJointIndex(i, out var targetJointIndex))
                {
                    target.SetRotation(targetJointIndex, source.GetRotation(i));
                    target.SetPosition(targetJointIndex, source.GetPosition(i));
                }
            }
        }
    }
}
