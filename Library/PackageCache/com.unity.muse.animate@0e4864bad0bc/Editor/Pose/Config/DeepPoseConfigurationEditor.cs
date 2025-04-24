using System.Globalization;
using Unity.Sentis;
using Unity.DeepPose.ModelBackend;
using UnityEditor;
using UnityEngine;

namespace Unity.DeepPose.Core.Editor
{
#if UNITY_MUSE_DEV
    [CustomEditor(typeof(DeepPoseConfiguration), true)]
#endif
    class DeepPoseConfigurationEditor : UnityEditor.Editor
    {
        SerializedProperty m_ModelProperty;
        SerializedProperty m_BarracudaBackendProperty;
        SerializedProperty m_AvatarProperty;
        SerializedProperty m_AdditiveProperty;
        SerializedProperty m_TransposeOrtho6DProperty;
        SerializedProperty m_GeneralizedLookAtProperty;
        SerializedProperty m_UseRaycastProperty;
        SerializedProperty m_MaxRayDistanceProperty;

        static bool s_SkeletonFoldout;
        static Vector2 s_SkeletonListScrollPosition;

        const float k_Space = 5f;
        const float k_SkeletonListMaxHeight = 200f;

        void OnEnable()
        {
            m_ModelProperty = serializedObject.FindProperty(nameof(DeepPoseConfiguration.Model));
            m_BarracudaBackendProperty = serializedObject.FindProperty(nameof(DeepPoseConfiguration.BarracudaBackend));
            m_AvatarProperty = serializedObject.FindProperty(nameof(DeepPoseConfiguration.Avatar));
            m_AdditiveProperty = serializedObject.FindProperty(nameof(DeepPoseConfiguration.Additive));
            m_TransposeOrtho6DProperty = serializedObject.FindProperty(nameof(DeepPoseConfiguration.TransposeOrtho6D));
            m_GeneralizedLookAtProperty = serializedObject.FindProperty(nameof(DeepPoseConfiguration.GeneralizedLookAt));
            m_UseRaycastProperty = serializedObject.FindProperty(nameof(DeepPoseConfiguration.UseRaycast));
            m_MaxRayDistanceProperty = serializedObject.FindProperty(nameof(DeepPoseConfiguration.MaxRayDistance));
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_ModelProperty);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                SetupFromMetadata();
            }

            if (m_ModelProperty.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("You must provide a valid model.", MessageType.Warning);
            }
            else
            {
                var modelDefinition = new ModelDefinition(m_ModelProperty.objectReferenceValue as ModelAsset);
                if (!modelDefinition.IsOnnxRuntime)
                    EditorGUILayout.PropertyField(m_BarracudaBackendProperty);

                EditorGUILayout.PropertyField(m_AvatarProperty);
                var avatar = m_AvatarProperty.objectReferenceValue as Avatar;
                if (avatar != null && (!avatar.isHuman || !avatar.isValid))
                    EditorGUILayout.HelpBox("Avatar must be a valid humanoid avatar.", MessageType.Warning);

                if (HasValidMetadata())
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(m_AdditiveProperty);
                    EditorGUILayout.PropertyField(m_GeneralizedLookAtProperty);
                    EditorGUILayout.PropertyField(m_TransposeOrtho6DProperty);
                    EditorGUILayout.PropertyField(m_UseRaycastProperty);
                    if (m_UseRaycastProperty.boolValue)
                        EditorGUILayout.PropertyField(m_MaxRayDistanceProperty);
                    EditorGUI.EndDisabledGroup();

                    ShowSkeletonList();
                }
                else
                {
                    EditorGUILayout.HelpBox("Invalid metadata. The specified model seems to not have valid metadata", MessageType.Warning);
                }

                if (GUILayout.Button("Reload Metadata"))
                    SetupFromMetadata();
            }

            serializedObject.ApplyModifiedProperties();
        }

        bool HasValidMetadata()
        {
            var config = target as DeepPoseConfiguration;
            return config != null && config.IsValid();
        }

        void SetupFromMetadata()
        {
            serializedObject.ApplyModifiedProperties();

            var config = target as DeepPoseConfiguration;
            if (config == null)
                return;

            config.ReadMetaData();

            serializedObject.Update();
            EditorUtility.SetDirty(config);
        }

        void ShowSkeletonList()
        {
            var config = target as DeepPoseConfiguration;
            if (config == null)
                return;

            var skeleton = config.Skeleton;
            if (skeleton == null)
                return;

            GUILayout.Space(k_Space);

            s_SkeletonFoldout = EditorGUILayout.Foldout(s_SkeletonFoldout, "Skeleton", true, EditorStyles.foldoutHeader);
            if (s_SkeletonFoldout)
            {
                var jointsCount = skeleton.Count;
                if (jointsCount == 0)
                    return;

                var height = Mathf.Min(jointsCount * 20f + 2f, k_SkeletonListMaxHeight);

                s_SkeletonListScrollPosition = GUILayout.BeginScrollView(s_SkeletonListScrollPosition, GUI.skin.box, GUILayout.MinHeight(height));

                foreach (var rootJoint in skeleton.RootJoints)
                {
                    ShowJoint(config, rootJoint);
                }

                GUILayout.EndScrollView();
            }
        }

        static void ShowJoint(DeepPoseConfiguration config, Skeleton.IJoint joint, float indent = 0f)
        {
            var rect = EditorGUILayout.GetControlRect(false, 16f);
            rect.xMin += indent;

            var jointName = $"{joint.Index}: {joint.Name}";
            var jointDescription = "";

            if (config.JointsWithPosition.Contains(joint.Index))
                jointDescription += "pos";

            if (config.JointsWithRotation.Contains(joint.Index))
                jointDescription += jointDescription.Length > 0 ? " + rot" : "rot";

            if (config.JointsWithLookAt.Contains(joint.Index))
                jointDescription += jointDescription.Length > 0 ? " + lookat" : "lookat";

            if (jointDescription.Length == 0)
                jointDescription = "none";

            // layer name on the right side
            var locRect = rect;
            locRect.xMax = locRect.xMin;
            var gc = new GUIContent(jointName.ToString(CultureInfo.InvariantCulture));

            // calculate size so we can left-align it
            var size = EditorStyles.miniBoldLabel.CalcSize(gc);
            locRect.xMax += size.x;
            GUI.Label(locRect, gc, EditorStyles.miniBoldLabel);
            locRect.xMax += 2;

            // message
            var msgRect = rect;
            msgRect.xMin = locRect.xMax;
            GUI.Label(msgRect, new GUIContent(jointDescription.ToString(CultureInfo.InvariantCulture)), EditorStyles.miniLabel);

            foreach (var child in joint.Children)
            {
                ShowJoint(config, child, indent + 10f);
            }
        }
    }
}
