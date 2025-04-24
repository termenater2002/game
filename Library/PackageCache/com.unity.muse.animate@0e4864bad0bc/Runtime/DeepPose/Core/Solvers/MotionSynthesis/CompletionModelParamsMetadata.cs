using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    [Serializable]
    struct CompletionModelParamsMetadata
    {
        public bool transpose_ortho6d;
        public int max_sequence_length;
        public bool uses_tolerance;
        public bool input_all_positions;
        public bool output_all_positions;
    }
    
    [Serializable]
    struct CompletionModelMetadata
    {
        public string model; // Name of the model class
        public CompletionModelParamsMetadata model_params; // Parameters required for inference
        public Dictionary<string, JointMetadata> skeleton; // Skeleton used by the model
        public bool predicts_contacts;
        public string[] ordered_contact_joints;
        public bool use_raycast_inputs;
        public string[] raycast_joints;
        public float raycast_clamping_value;
    }
    
    static partial class MetadataUtils
    {
        public static Skeleton GetSkeleton(this CompletionModelMetadata metadata)
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
