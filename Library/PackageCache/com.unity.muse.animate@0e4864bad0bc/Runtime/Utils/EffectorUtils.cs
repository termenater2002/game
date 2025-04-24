using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    static class EffectorUtils
    {
        public enum RotationMode
        {
            Orientation,
            PositionAndLookAtTargetAndEnabledRotations
        }

        public static void RotateEffectors(List<DeepPoseEffectorModel> effectors, Vector3 pivot, Quaternion rotationOffset, RotationMode mode)
        {
            switch (mode)
            {
                case RotationMode.Orientation:
                    foreach (var effectorModel in effectors)
                    {
                        effectorModel.Rotation = rotationOffset * effectorModel.Rotation;
                    }
                    break;

                case RotationMode.PositionAndLookAtTargetAndEnabledRotations:
                    foreach (var effectorModel in effectors)
                    {
                        var positionOffset = effectorModel.Position - pivot;
                        effectorModel.Position = pivot + rotationOffset * positionOffset;

                        if (effectorModel.RotationEnabled)
                            effectorModel.Rotation = rotationOffset * effectorModel.Rotation;
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}
