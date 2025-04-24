using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace Unity.Muse.Animate.Editor
{
    [CustomEditor(typeof(ArmatureToArmatureMapping))]
    class ArmatureToArmatureMappingEditor : UnityEditor.Editor
    {
        static readonly GUIContent k_ArrowLabel = new ("->");
        static readonly GUIContent k_SourceLabel = new ("Source");
        static readonly GUIContent k_TargetLabel = new ("Target");

        const string k_MissingSourceError = "You must provide a source armature.";
        const string k_MissingTargetError = "You must provide a target armature.";
        const string k_InvalidConfigurationError = "Armature is invalid.";
        const string k_UndoMessage = "Change armature-to-armature mapping";

        const float k_JointHorizontalSpacing = 30f;
        const float k_StartPadding = 16f;

        string[] m_TargetJointLabels = Array.Empty<string>();

        ArmatureToArmatureMapping Target => target as ArmatureToArmatureMapping;

        SerializedProperty m_SourceArmatureProperty;
        SerializedProperty m_TargetArmatureProperty;
        SerializedProperty m_SourceToTargetProperty;
        ReorderableList m_SourceToTargetList;

        void OnEnable()
        {
            FindProperties();
            InitializeList();
            UpdateTargetJointList();
        }

        void FindProperties()
        {
            m_SourceArmatureProperty = serializedObject.FindProperty("m_SourceArmature");
            m_TargetArmatureProperty = serializedObject.FindProperty("m_TargetArmature");

            var mappingProperty = serializedObject.FindProperty("m_MappingData");
            m_SourceToTargetProperty = mappingProperty.FindPropertyRelative("SourceToTarget");
        }

        void InitializeList()
        {
            m_SourceToTargetList = new ReorderableList(serializedObject, m_SourceToTargetProperty, false, true, false, false);

            m_SourceToTargetList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var targetJointIndexProperty = m_SourceToTargetProperty.GetArrayElementAtIndex(index);

                var sourceJointIndex = index;
                var targetJointIndex = targetJointIndexProperty.intValue;

                var indexInLabelsList = targetJointIndex + 1;

                var sourceRect = new Rect(rect.x, rect.y, rect.width / 2 - k_JointHorizontalSpacing / 2, EditorGUIUtility.singleLineHeight);
                var spaceRect = new Rect(rect.x + rect.width / 2 - k_JointHorizontalSpacing / 2, rect.y, k_JointHorizontalSpacing, EditorGUIUtility.singleLineHeight);
                var targetRect = new Rect(rect.x + rect.width / 2 + k_JointHorizontalSpacing / 2, rect.y, rect.width / 2 - k_JointHorizontalSpacing / 2, EditorGUIUtility.singleLineHeight);

                EditorGUI.BeginChangeCheck();
                EditorGUI.LabelField(sourceRect, GetSourceJointName(sourceJointIndex));
                EditorGUI.LabelField(spaceRect, k_ArrowLabel, new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter});

                EditorGUI.BeginProperty(targetRect, GUIContent.none, targetJointIndexProperty);
                indexInLabelsList = EditorGUI.Popup(targetRect, "", indexInLabelsList, m_TargetJointLabels);
                if (indexInLabelsList != -1)
                {
                    targetJointIndexProperty.intValue = indexInLabelsList - 1;
                }
                EditorGUI.EndProperty();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, k_UndoMessage);
                    serializedObject.ApplyModifiedProperties();
                }
            };

            m_SourceToTargetList.drawHeaderCallback += rect =>
            {
                rect.x += k_StartPadding;
                rect.width -= k_StartPadding;
                var pathRect = new Rect(rect.x, rect.y, rect.width / 2 - k_JointHorizontalSpacing / 2, rect.height);
                var nameRect = new Rect(rect.x + rect.width / 2 + k_JointHorizontalSpacing / 2, rect.y, rect.width / 2 - k_JointHorizontalSpacing / 2, rect.height);
                EditorGUI.LabelField(pathRect, k_SourceLabel);
                EditorGUI.LabelField(nameRect, k_TargetLabel);
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_SourceArmatureProperty);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, k_UndoMessage);
                m_SourceToTargetProperty.arraySize = m_SourceArmatureProperty.objectReferenceValue == null
                    ? 0
                    : ((ArmatureDefinition)m_SourceArmatureProperty.objectReferenceValue).NumJoints;
                serializedObject.ApplyModifiedProperties();
                ResetMapping();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_TargetArmatureProperty);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, k_UndoMessage);
                serializedObject.ApplyModifiedProperties();
                UpdateTargetJointList();
                ResetMapping();
            }

            m_SourceToTargetList.DoLayoutList();
        }

        void ResetMapping()
        {
            for (var i = 0; i < m_SourceToTargetProperty.arraySize; i++)
            {
                var targetJointIndexProperty = m_SourceToTargetProperty.GetArrayElementAtIndex(i);
                targetJointIndexProperty.intValue = -1;
            }

            serializedObject.ApplyModifiedProperties();
            Target.FindTargetJoints();
            serializedObject.Update();
        }

        void UpdateTargetJointList()
        {
            var targetArmature = Target.TargetArmature;
            if (targetArmature == null)
            {
                m_TargetJointLabels = new string[1];
            }
            else
            {
                m_TargetJointLabels = new string[targetArmature.NumJoints + 1];
                for (var i = 0; i < targetArmature.NumJoints; i++)
                {
                    m_TargetJointLabels[i + 1] = $"{i}: {targetArmature.GetJointName(i)}";
                }
            }

            m_TargetJointLabels[0] = "None";
        }

        string GetSourceJointName(int sourceJointIndex)
        {
            if (Target.SourceArmature == null)
                return "???";

            return Target.SourceArmature.GetJointName(sourceJointIndex);
        }
    }
}
