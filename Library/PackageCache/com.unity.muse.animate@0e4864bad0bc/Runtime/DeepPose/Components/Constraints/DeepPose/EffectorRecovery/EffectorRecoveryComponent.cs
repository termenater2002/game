using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.DeepPose.Components
{
    class EffectorRecoveryComponent : EffectorRecoveryBase
    {
        public DeepPoseComponent Target;

        protected override int JointsCount => Target != null ? Target.Joints.Count : 0;
        protected override int PositionEffectorsCount => Target != null ? Target.DeepPoseSolverData.Positions.Count : 0;
        protected override int GetPositionEffectorJoint(int effectorIdx) => Target.DeepPoseSolverData.Positions[effectorIdx].id;
        protected override bool HasEffector(int jointId)
        {
            return FindEffectorIndex(PosingEffector.EffectorType.Position, jointId) > 0
                || FindEffectorIndex(PosingEffector.EffectorType.Rotation, jointId) > 0
                || FindEffectorIndex(PosingEffector.EffectorType.LookAt, jointId) > 0;
        }

        protected override void EnableEffector(PosingEffector.EffectorType type, int jointId, bool enable = true)
        {
            var index = FindEffectorIndex(type, jointId);
            if (index < 0)
                return;

            var weight = enable ? 1f : 0f;

            switch (type)
            {
                case PosingEffector.EffectorType.Position:
                    Target.DeepPoseSolverData.SetPositionalEffectorWeight(index, weight);
                    break;

                case PosingEffector.EffectorType.Rotation:
                    Target.DeepPoseSolverData.SetRotationalEffectorWeight(index, weight);
                    break;

                case PosingEffector.EffectorType.LookAt:
                    Target.DeepPoseSolverData.SetLookAtEffectorWeight(index, weight);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        protected override Transform GetJointTransform(int jointIdx) => Target != null ? Target.Joints[jointIdx].Transform : null;

        protected override void SolvePose() => Target.Solve();

        protected override void SnapAllEffectors() => Target.SnapAllEffectors();

        protected override void DisableAllEffectors()
        {
            for (var i = 0; i < Target.DeepPoseSolverData.Positions.Count; i++)
            {
                Target.DeepPoseSolverData.SetPositionalEffectorWeight(i, 0f);
            }

            for (var i = 0; i < Target.DeepPoseSolverData.Rotations.Count; i++)
            {
                Target.DeepPoseSolverData.SetRotationalEffectorWeight(i, 0f);
            }

            for (var i = 0; i < Target.DeepPoseSolverData.LookAts.Count; i++)
            {
                Target.DeepPoseSolverData.SetLookAtEffectorWeight(i, 0f);
            }
        }

        void OnEnable()
        {
            CapturePose();
            Target.IsSolverAlwaysActive = false;
        }

        int FindEffectorIndex(PosingEffector.EffectorType type, int jointId)
        {
            switch (type)
            {
                case PosingEffector.EffectorType.Position:
                    for (var i = 0; i < Target.DeepPoseSolverData.Positions.Count; i++)
                    {
                        if (Target.DeepPoseSolverData.Positions[i].id == jointId)
                            return i;
                    }
                    break;

                case PosingEffector.EffectorType.Rotation:
                    for (var i = 0; i < Target.DeepPoseSolverData.Rotations.Count; i++)
                    {
                        if (Target.DeepPoseSolverData.Rotations[i].id == jointId)
                            return i;
                    }
                    break;

                case PosingEffector.EffectorType.LookAt:
                    for (var i = 0; i < Target.DeepPoseSolverData.LookAts.Count; i++)
                    {
                        if (Target.DeepPoseSolverData.LookAts[i].id == jointId)
                            return i;
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return -1;
        }
    }
}
