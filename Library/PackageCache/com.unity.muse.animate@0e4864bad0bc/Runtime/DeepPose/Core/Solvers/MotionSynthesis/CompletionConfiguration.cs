using System;
using Unity.DeepPose.ModelBackend;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    /// <summary>
    /// Encapsulates models and their specific parameters.
    /// </summary>
#if UNITY_MUSE_DEV
    [CreateAssetMenu(fileName = "CompletionConfiguration", menuName = "Muse Animate Dev/Motion Synthesis/Completion Configuration")]
#endif
    class CompletionConfiguration : MotionSynthesisConfiguration
    {
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

            }
            else
            {
                Skeleton = null;
            }
        }
    }

    static class CompletionConfigurationUtils
    {
        public static bool HaveSameJointNames(this CompletionConfiguration config1, CompletionConfiguration config2)
        {
            if (config1 == null || config2 == null)
                return false;

            return config1.Skeleton.HaveSameJointNames(config2.Skeleton);
        }
    }
}
