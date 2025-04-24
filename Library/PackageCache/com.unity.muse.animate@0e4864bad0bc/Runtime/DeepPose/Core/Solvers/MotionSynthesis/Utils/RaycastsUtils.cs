using UnityEngine;

namespace Unity.DeepPose.Core
{
    static class RaycastsUtils
    {
        public struct DownRay
        {
            public Vector3 Origin;
            public float Distance;
        }

        public struct RayFrame
        {
            public DownRay[] Rays;
            
            public RayFrame(int numRays)
            {
                Rays = new DownRay[numRays];
            }
        }
        
        public struct RaySequence
        {
            public RayFrame[] RayFrames;

            public RaySequence(int numFrames)
            {
                RayFrames = new RayFrame[numFrames];
            }
        }
        
        public static float GetRaycastDistance(Vector3 origin, float maxDistance)
        {
            if (Physics.CheckSphere(origin, 1e-3f))
            {
                return 0f;
            }
            
            if (Physics.Raycast(new Ray(origin, Vector3.down), out var hitInfo, maxDistance))
            {
                return hitInfo.distance;
            }

            return maxDistance;
        }
    }
}
