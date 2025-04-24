using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Core
{
    static class JointUtils
    {
        public static void SetupAsCharacterJoint(this ConfigurableJoint joint)
        {
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = Mathf.Infinity;

            joint.rotationDriveMode = RotationDriveMode.Slerp;
            var slerpDrive = joint.slerpDrive;
            slerpDrive.maximumForce = Mathf.Infinity;
            joint.slerpDrive = slerpDrive;
        }

        public static void ConvertJoint(this CharacterJoint source, ConfigurableJoint target)
        {
            target.SetupAsCharacterJoint();

            target.autoConfigureConnectedAnchor = source.autoConfigureConnectedAnchor;
            target.anchor = source.anchor;
            target.connectedBody = source.connectedBody;
            target.connectedAnchor = source.connectedAnchor;
            target.connectedArticulationBody = source.connectedArticulationBody;

            target.axis = source.axis;
            target.secondaryAxis = source.swingAxis;

            target.angularXLimitSpring = source.twistLimitSpring;
            target.angularYZLimitSpring = source.swingLimitSpring;

            target.lowAngularXLimit = source.lowTwistLimit;
            target.highAngularXLimit = source.highTwistLimit;
            target.angularYLimit = source.swing1Limit;
            target.angularZLimit = source.swing2Limit;

            target.projectionMode = source.enableProjection ? JointProjectionMode.PositionAndRotation : JointProjectionMode.None;
            target.breakForce = source.breakForce;
            target.breakTorque = source.breakTorque;
            target.enableCollision = source.enableCollision;
            target.enablePreprocessing = source.enablePreprocessing;
            target.massScale = source.massScale;
            target.connectedMassScale = source.connectedMassScale;
        }

		public static void SetTargetRotationLocal(this ConfigurableJoint joint, Quaternion targetLocalRotation, Quaternion startLocalRotation)
		{
			Assert.IsFalse(joint.configuredInWorldSpace, "SetTargetRotationLocal should not be used with joints that are configured in world space.");
			SetTargetRotationInternal(joint, targetLocalRotation, startLocalRotation, Space.Self);
		}

		public static void SetTargetRotation(this ConfigurableJoint joint, Quaternion targetWorldRotation, Quaternion startWorldRotation)
		{
			Assert.IsTrue(joint.configuredInWorldSpace, "SetTargetRotation must be used with joints that are configured in world space.");
			SetTargetRotationInternal(joint, targetWorldRotation, startWorldRotation, Space.World);
		}

		static void SetTargetRotationInternal(ConfigurableJoint joint, Quaternion targetRotation, Quaternion startRotation, Space space)
		{
			var right = joint.axis;
			var forward = Vector3.Cross (right, joint.secondaryAxis).normalized;
			var up = Vector3.Cross(forward, right).normalized;
			var worldToJointSpace = Quaternion.LookRotation(forward, up);

			// Transform into world space
			var resultRotation = Quaternion.Inverse(worldToJointSpace);

			if (space == Space.World)
			{
				resultRotation *= startRotation * Quaternion.Inverse(targetRotation);
			}
			else
			{
				resultRotation *= Quaternion.Inverse(targetRotation) * startRotation;
			}

			resultRotation *= worldToJointSpace;
			joint.targetRotation = resultRotation;
		}
    }
}
