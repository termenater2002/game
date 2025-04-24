using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    [Serializable]
    struct SkeletonAnimationFrame
    {
        public int Frame;
        public float Time;
        public Vector3 RootPosition;
        public List<Quaternion> JointRotations;
        public List<Quaternion> JointRotationsGlobal;
        public List<Vector3> JointPositionsGlobal;
        public float4 FootContacts;

        public SkeletonAnimationFrame(int frame, float time, Vector3 rootPosition, List<Quaternion> jointRotations, List<Quaternion> jointRotationsGlobal, List<Vector3> jointPositionsGlobal, float4 footContacts)
        {
            Frame = frame;
            Time = time;
            RootPosition = rootPosition;
            JointRotations = jointRotations;
            JointRotationsGlobal = jointRotationsGlobal;
            JointPositionsGlobal = jointPositionsGlobal;
            FootContacts = footContacts;
        }

        public SkeletonAnimationFrame(SkeletonAnimationFrame other)
        {
            Frame = other.Frame;
            Time = other.Time;
            RootPosition = other.RootPosition;
            JointRotations = other.JointRotations.ToList();
            JointRotationsGlobal = other.JointRotationsGlobal.ToList();
            JointPositionsGlobal = other.JointPositionsGlobal.ToList();
            FootContacts = other.FootContacts;
        }
    }

    [Serializable]
    struct SkeletonAnimationFrameDistances
    {
        public float[] LocalRotations;
        public float[] GlobalRotations;
        public float[] Positions;

        public SkeletonAnimationFrameDistances(List<Vector3> sourcePositions, List<Quaternion> sourceLocalRotations, List<Quaternion> sourceGlobalRotations, List<Vector3> targetPositions, List<Quaternion> targetLocalRotations, List<Quaternion> targetGlobalRotations)
        {
            Positions = new float[sourcePositions.Count];
            LocalRotations = new float[sourceLocalRotations.Count];
            GlobalRotations = new float[sourceGlobalRotations.Count];

            for (var i = 0; i < sourcePositions.Count; i++)
            {
                LocalRotations[i] = Quaternion.Angle(sourceLocalRotations[i], targetLocalRotations[i]);
                GlobalRotations[i] = Quaternion.Angle(sourceGlobalRotations[i], targetGlobalRotations[i]);
                Positions[i] = (sourcePositions[i] - targetPositions[i]).magnitude;
            }
        }
    }

    [Serializable]
    struct SkeletonAnimationFrameFootContacts
    {
        public List<Vector4> Contacts;

        public SkeletonAnimationFrameFootContacts(List<Vector4> contacts)
        {
            Contacts = contacts;
        }
    }
}
