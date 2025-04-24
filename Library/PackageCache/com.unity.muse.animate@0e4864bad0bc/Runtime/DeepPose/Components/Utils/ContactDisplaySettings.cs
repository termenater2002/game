using System;
using UnityEngine;

namespace Unity.DeepPose.Components
{
    [Serializable]
    struct ContactDisplaySettings
    {
        public bool Enabled;
        public float Thickness;
        public Color ColorContact;
        public Color ColorNoContact;
        public Transform RightFoot;
        public Transform RightToes;
        public Transform RightToesTip;
        public Transform LeftFoot;
        public Transform LeftToes;
        public Transform LeftToesTip;
    }
}
