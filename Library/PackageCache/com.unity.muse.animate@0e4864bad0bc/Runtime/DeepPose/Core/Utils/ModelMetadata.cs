using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Unity.DeepPose.Core
{
    [Serializable]
    struct JointMetadata
    {
        public int index;
        public float3 offset;
        public string symmetry;
        public Dictionary<string, JointMetadata> children;
    }

    static partial class MetadataUtils
    {
        static Skeleton.IJoint ToJoint(this JointMetadata jointMetadata, string jointName, Skeleton skeleton, Skeleton.IJoint parent = null)
        {
            var joint = skeleton.AddJoint(jointName, jointMetadata.index, jointMetadata.offset, parent);
            foreach (var child in jointMetadata.children)
            {
                child.Value.ToJoint(child.Key, skeleton, joint);
            }

            return joint;
        }
    }
}
