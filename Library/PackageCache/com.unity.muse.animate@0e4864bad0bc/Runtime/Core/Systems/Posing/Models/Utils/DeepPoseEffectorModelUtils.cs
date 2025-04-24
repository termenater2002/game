using Unity.DeepPose.Components;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    static class DeepPoseEffectorModelUtils
    {
        const float k_LookAtSnapDistance = 1f;

        public static void SnapToPose(this DeepPoseEffectorModel model, ArmatureStaticPoseModel globalStaticPoseModel, bool onlyIfInactive)
        {
            Assert.AreEqual(ArmatureStaticPoseData.PoseType.Global, globalStaticPoseModel.Type);

            var effectorIndex = model.Index;
            if (!effectorIndex.IsValid)
                AssertUtils.Fail($"Invalid effector index: {effectorIndex.ToString()}");

            var jointIdx = effectorIndex.JointIndex;
            var rigidTransform = globalStaticPoseModel.GetTransform(jointIdx);

            if (model.HandlesLookAt)
            {
                if (!onlyIfInactive || !model.LookAtEnabled)
                {
                    // TODO: custom look-at direction per-effector for custom characters
                    var lookAtDirection = Vector3.forward;
                    model.Position = rigidTransform.Position + rigidTransform.Rotation * (k_LookAtSnapDistance * lookAtDirection);
                    model.Rotation = Quaternion.identity;
                }
            }
            else
            {
                // Snap position if effector does not handle position or position is not enabled, or snapping is forced
                if (!model.HandlesPosition || !onlyIfInactive || !model.PositionEnabled)
                    model.Position = rigidTransform.Position;

                // Snap rotation if effector does not handle position or position is not enabled, or snapping is forced
                if (!model.HandlesRotation || !onlyIfInactive || !model.RotationEnabled)
                    model.Rotation = rigidTransform.Rotation;
            }
        }

        public static void SyncFromComponent(this DeepPoseEffectorModel model, DeepPoseComponent component)
        {
            Assert.IsNotNull(component, "You must provide a DeepPoseComponent");

            var effectorIndex = model.Index.EffectorIndex;

            if (effectorIndex.HasPosition)
            {
                var positionEffectors = component.DeepPoseSolverData.Positions;
                Assert.IsTrue(effectorIndex.PositionIndex < positionEffectors.Count);

                var effector = positionEffectors[effectorIndex.PositionIndex];
                model.Position = effector.transform.position;
                model.PositionWeight = effector.weight;
                model.PositionTolerance = effector.tolerance;
            }

            if (effectorIndex.HasRotation)
            {
                var rotationEffectors = component.DeepPoseSolverData.Rotations;
                Assert.IsTrue(effectorIndex.RotationIndex < rotationEffectors.Count);

                var effector = rotationEffectors[effectorIndex.RotationIndex];
                model.Rotation = effector.transform.rotation;
                model.RotationWeight = effector.weight;
            }

            if (effectorIndex.HasLookAt)
            {
                var lookAtEffectors = component.DeepPoseSolverData.LookAts;
                Assert.IsTrue(effectorIndex.LookAtIndex < lookAtEffectors.Count);

                var effector = lookAtEffectors[effectorIndex.LookAtIndex];
                model.Position = effector.transform.position;
                model.Rotation = effector.transform.rotation;
                model.LookAtWeight = effector.weight;
            }
        }

        public static void ApplyToComponent(this DeepPoseEffectorModel model, DeepPoseComponent component)
        {
            Assert.IsNotNull(component, "You must provide a DeepPoseComponent");

            var effectorIndex = model.Index.EffectorIndex;

            if (effectorIndex.HasPosition)
            {
                var positionEffectors = component.DeepPoseSolverData.Positions;
                Assert.IsTrue(effectorIndex.PositionIndex < positionEffectors.Count);

                var effector = positionEffectors[effectorIndex.PositionIndex];

                effector.transform.position = model.Position;
                effector.weight = model.PositionWeight;
                effector.tolerance = model.PositionTolerance;

                positionEffectors[effectorIndex.PositionIndex] = effector;
                component.DeepPoseSolverData.Positions = positionEffectors;
            }

            if (effectorIndex.HasRotation)
            {
                var rotationEffectors = component.DeepPoseSolverData.Rotations;
                Assert.IsTrue(effectorIndex.RotationIndex < rotationEffectors.Count);

                var effector = rotationEffectors[effectorIndex.RotationIndex];

                effector.transform.rotation = model.Rotation;
                effector.weight = model.RotationWeight;

                rotationEffectors[effectorIndex.RotationIndex] = effector;
                component.DeepPoseSolverData.Rotations = rotationEffectors;
            }

            if (effectorIndex.HasLookAt)
            {
                var lookAtEffectors = component.DeepPoseSolverData.LookAts;
                Assert.IsTrue(effectorIndex.LookAtIndex < lookAtEffectors.Count);

                var effector = lookAtEffectors[effectorIndex.LookAtIndex];

                effector.transform.position = model.Position;
                effector.transform.rotation = model.Rotation;
                effector.weight = model.LookAtWeight;

                lookAtEffectors[effectorIndex.LookAtIndex] = effector;
                component.DeepPoseSolverData.LookAts = lookAtEffectors;
            }
        }
    }
}
