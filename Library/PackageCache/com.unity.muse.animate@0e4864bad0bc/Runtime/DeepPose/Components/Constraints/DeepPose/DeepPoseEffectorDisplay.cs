using System;
using System.Collections.Generic;
using System.Linq;
using Unity.DeepPose.Core;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.DeepPose.Components
{
    [ExecuteAlways]
    class DeepPoseEffectorDisplay : EffectorDisplayBase
    {
        public DeepPoseComponent Component;

        protected override bool IsValid => Component != null;
        protected override IEnumerable<Effector> Positions => Component.DeepPoseSolverData.Positions;
        protected override IEnumerable<Effector> Rotations => Component.DeepPoseSolverData.Rotations;
        protected override IEnumerable<Effector> LookAts => Component.DeepPoseSolverData.LookAts;
        protected override IList<Transform> JointTransforms => Component.Joints.Select(x => x.Transform).ToList();

        protected override Vector3 GetLookAtDirection(int jointId)
        {
            if (Component == null)
                return DeepPoseSolver.DefaultLookAtDirection;

            return Component.GetLookAtDirection(jointId);
        }

        protected override bool IsVisible()
        {
            if (Component != null && !Component.IsSolverAlwaysActive)
                return false;

            return base.IsVisible();
        }
    }
}
