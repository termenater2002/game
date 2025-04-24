using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Defines the mapping between an armature and actual Transforms
    /// </summary>
    [Serializable]
    struct ArmatureMappingData
    {
        public Transform[] Transforms;
        public int NumJoints => Transforms.Length;

        public bool IsValid
        {
            get
            {
                if (Transforms == null)
                    return false;

                for (var i = 0; i < Transforms.Length; i++)
                {
                    if (Transforms[i] == null)
                        return false;
                }

                return true;
            }
        }

        public ArmatureMappingData(int numJoints)
        {
            Transforms = new Transform[numJoints];
        }
    }
}
