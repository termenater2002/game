using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct RigidTransformData
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public RigidTransformData(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}
