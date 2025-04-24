using System;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    [Serializable]
    struct DynamicCcdJoint
    {
        public Transform Transform;
        public bool Stiff;

        public DynamicCcdJoint(Transform transform, bool stiff = false)
        {
            Transform = transform;
            Stiff = stiff;
        }
    }
}