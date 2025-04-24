using System.Collections.Generic;
using Unity.DeepPose.Core;
using UnityEditor;
using UnityEngine;

namespace Unity.DeepPose.Components
{
    class RaycastDrawer
    {
        public Color color = new Color(1f,1f,1f);
        public float thickness = 1f;
        
        List<RaycastsUtils.RaySequence> m_Rays = new List<RaycastsUtils.RaySequence>();

        public void AddRaySequence(RaycastsUtils.RaySequence raySequence)
        {
            m_Rays.Add(raySequence);
        }

        public void Clear()
        {
            m_Rays.Clear();
        }
        

        public void DrawDebugRaycasts(int frameIdx, Color color, float thickness)
        {
            if (m_Rays.Count > 0)
            {
                foreach (var raySequence in m_Rays)
                {
                    var rayFrame = raySequence.RayFrames[frameIdx];
                    for (var rayIdx = 0; rayIdx < rayFrame.Rays.Length; rayIdx++)
                    {
                        var rayStart = rayFrame.Rays[rayIdx].Origin;
                        var rayDistance = rayFrame.Rays[rayIdx].Distance;
                        var rayStop = new Vector3(rayStart.x, rayStart.y - rayDistance, rayStart.z);
                        GizmoUtils.DrawLine(rayStart, rayStop, color, thickness);
                    }
                }
                
            }   
        }
    }
}
