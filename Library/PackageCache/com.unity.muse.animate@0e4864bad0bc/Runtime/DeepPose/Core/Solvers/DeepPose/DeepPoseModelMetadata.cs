using System;
using System.Collections.Generic;

namespace Unity.DeepPose.Core
{
    [Serializable]
    struct DeepPoseParamsMetadata
    {
        public float max_effector_noise_scale;
        public bool generalized_lookat;
        public bool transpose_ortho6d;
        public string[] pos_effectors;
        public string[] rot_effectors;
        public string[] lookat_effectors;
        public bool use_raycast;
        public float max_ray_distance;
    }

    [Serializable]
    struct DeepPoseModelMetadata
    {
        public string model; // Name of the model class
        public DeepPoseParamsMetadata model_params; // Parameters required for inference
        public Dictionary<string, JointMetadata> skeleton; // Skeleton used by the model
    }

    static partial class MetadataUtils
    {
        public static Skeleton GetSkeleton(this DeepPoseModelMetadata metadata)
        {
            var skeleton = new Skeleton();

            foreach (var joint in metadata.skeleton)
            {
                joint.Value.ToJoint(joint.Key, skeleton);
            }

            return skeleton;
        }
    }
}
