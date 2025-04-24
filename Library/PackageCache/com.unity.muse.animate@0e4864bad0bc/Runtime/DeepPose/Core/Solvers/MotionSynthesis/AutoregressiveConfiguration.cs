using System;
using Unity.DeepPose.ModelBackend;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    /// <summary>
    /// Encapsulates models and their specific parameters.
    /// </summary>
#if UNITY_MUSE_DEV
    [CreateAssetMenu(fileName = "AutoregressiveConfiguration", menuName = "Muse Animate Dev/Motion Synthesis/Autoregressive Configuration")]
#endif
    class AutoregressiveConfiguration : MotionSynthesisConfiguration
    {
        public bool InputAllPositions;
        public bool OutputAllPositions;

        public override void ReadMetaData()
        {
            if (Model == null)
                return;

            var modelDefinition = new ModelDefinition(Model);
            if (modelDefinition.TryReadMetaData(out CompletionModelMetadata metadata))
            {
                Skeleton = metadata.GetSkeleton();

                PredictsFootContacts = metadata.predicts_contacts;
                if (Skeleton == null)
                {
                    ContactJoints = Array.Empty<int>();
                }
                else
                {
                    ContactJoints = new int[metadata.ordered_contact_joints.Length];
                    for (var i = 0; i < metadata.ordered_contact_joints.Length; i++)
                    {
                        var jointName = metadata.ordered_contact_joints[i];
                        var joint = Skeleton.FindJoint(jointName);
                        ContactJoints[i] = joint?.Index ?? -1;
                    }
                }

                TransposeOrtho6D = metadata.model_params.transpose_ortho6d;
                HasTolerance = metadata.model_params.uses_tolerance;
                
                InputAllPositions = metadata.model_params.input_all_positions;
                OutputAllPositions = metadata.model_params.output_all_positions;
                
                // Raycast metadata
                UseRaycasts = metadata.use_raycast_inputs;
                RaycastClampValue = metadata.raycast_clamping_value;
                if (Skeleton == null)
                {
                    RaycastJoints = Array.Empty<int>();
                }
                else
                {
                    RaycastJoints = new int[metadata.raycast_joints.Length];
                    for (var i = 0; i < metadata.raycast_joints.Length; i++)
                    {
                        var jointName = metadata.raycast_joints[i];
                        var joint = Skeleton.FindJoint(jointName);
                        RaycastJoints[i] = joint?.Index ?? -1;
                    }
                }
            }
            else
            {
                Skeleton = null;
            }
        }
    }
    
    static class AutoregressiveConfigurationUtils
    {
        public static bool HaveSameJointNames(this AutoregressiveConfiguration config1, AutoregressiveConfiguration config2)
        {
            if (config1 == null || config2 == null)
                return false;

            return config1.Skeleton.HaveSameJointNames(config2.Skeleton);
        }
    }
}
