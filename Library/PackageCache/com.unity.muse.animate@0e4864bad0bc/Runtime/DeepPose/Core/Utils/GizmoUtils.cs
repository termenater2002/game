using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    static class GizmoUtils
    {
        public static void DrawLine(Vector3 from, Vector3 to, Color color, float thickness)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.DrawLine(from, to, thickness);
#endif
        }

        public static void DrawSphere(Vector3 position, Color color, float size)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.SphereHandleCap(0, position, Quaternion.identity, size, EventType.Repaint);
#endif
        }

        public static void DrawLocalAxes(this Transform transform, float thickness, float length)
        {
            DrawLocalAxes(transform.position, transform.rotation, thickness, length);
        }

        public static void DrawLocalAxes(Vector3 position, Quaternion rotation, float thickness, float length)
        {
            var toX = position + length * (rotation * Vector3.right);
            var toY = position + length * (rotation * Vector3.up);
            var toZ = position + length * (rotation * Vector3.forward);

            DrawLine(position, toX, Color.red, thickness);
            DrawLine(position, toY, Color.green, thickness);
            DrawLine(position, toZ, Color.blue, thickness);
        }

        public static void DrawTransformAxes(IList<Transform> transforms, float thickness, float length)
        {
            for (var i = 0; i < transforms.Count; i++)
            {
                transforms[i].transform.DrawLocalAxes(thickness, length);
            }
        }
    }
}
