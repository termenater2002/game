using System;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    static class CcdUtils
    {
        public static bool CheckIfReachedDestination(Vector3 currentPosition, Vector3 targetPosition, float minDistance)
        {
            return (currentPosition - targetPosition).magnitude <= minDistance;
        }
    }
}
