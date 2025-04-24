using System;
using Unity.Muse.Animate;
using UnityEditor;
using UnityEngine;

namespace Muse.Animate.Editor
{
    [CustomEditor(typeof(ConeMesh), false)]
    class HandleConeMeshEditor : UnityEditor.Editor
    {
        ConeMesh Component => target as ConeMesh;

        public override void OnInspectorGUI()
        {
            Component.RotationOffset = EditorGUILayout.Vector3Field("Rotation Offset", Component.RotationOffset);
            Component.Resolution = EditorGUILayout.IntSlider("Resolution", Component.Resolution, ConeMesh.k_MinResolution,ConeMesh.k_MaxResolution);
            Component.Radius = EditorGUILayout.FloatField("Radius", Component.Radius);
            Component.Height = EditorGUILayout.FloatField("Height", Component.Height);
            Component.Color = EditorGUILayout.ColorField("Color", Component.Color);
        }
    }
}
