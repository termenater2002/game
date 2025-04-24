using System;
using Unity.Muse.Animate;
using UnityEditor;
using UnityEngine;

namespace Muse.Animate.Editor
{
    [CustomEditor(typeof(DiscMesh), false)]
    class HandleDiscMeshEditor : UnityEditor.Editor
    {
        DiscMesh Component => target as DiscMesh;

        public override void OnInspectorGUI()
        {
            Component.RotationOffset = EditorGUILayout.Vector3Field("Rotation Offset", Component.RotationOffset);
            Component.Resolution = EditorGUILayout.IntSlider("Resolution", Component.Resolution, DiscMesh.k_MinResolution,DiscMesh.k_MaxResolution);
            Component.Radius = EditorGUILayout.FloatField("Radius", Component.Radius);
            EditorGUILayout.Separator();
            Component.Fill = EditorGUILayout.Toggle("Fill", Component.Fill);
            Component.ColorFill = EditorGUILayout.ColorField("Fill Color", Component.ColorFill);
            EditorGUILayout.Separator();
            Component.BorderSize = EditorGUILayout.FloatField("Border Size", Component.BorderSize);
            Component.BorderSpacing = EditorGUILayout.FloatField("Border Spacing", Component.BorderSpacing);
            Component.BorderSideRatio = EditorGUILayout.Slider("Border Side Ratio", Component.BorderSideRatio, 0f, 1f);
            Component.ColorBorder = EditorGUILayout.ColorField("Border Color", Component.ColorBorder);
        }
    }
}
