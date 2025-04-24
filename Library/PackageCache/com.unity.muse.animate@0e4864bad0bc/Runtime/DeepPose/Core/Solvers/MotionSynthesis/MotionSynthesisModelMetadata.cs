using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    [Serializable]
    struct MotionSynthesisModelParamsMetadata
    {
        public bool transpose_ortho6d;
        public int training_min_past_frames;
        public int training_max_past_frames;
        public int training_min_output_frames;
        public int training_max_output_frames;
        public int training_min_future_frames;
        public int training_max_future_frames;
        public int allowed_min_past_frames;
        public int allowed_max_past_frames;
        public int allowed_min_output_frames;
        public int allowed_max_output_frames;
        public int allowed_min_future_frames;
        public int allowed_max_future_frames;
    }

    [Serializable]
    struct MotionSynthesisModelMetadata
    {
        public string model; // Name of the model class
        public MotionSynthesisModelParamsMetadata model_params; // Parameters required for inference
        public Dictionary<string, JointMetadata> skeleton; // Skeleton used by the model
    }

    static partial class MetadataUtils
    {
        public static Skeleton GetSkeleton(this MotionSynthesisModelMetadata metadata)
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
