using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.Muse.Animate.Editor
{
    [CustomEditor(typeof(JointMask))]
    class JointMaskEditor : UnityEditor.Editor
    {
        const float k_FieldWidth = 50f;
        const float k_FieldPadding = 4f;
        SerializedProperty m_ArmatureProperty;
        SerializedProperty m_JointsProperty;
        SerializedProperty m_PositionWeightsProperty;
        SerializedProperty m_RotationWeightsProperty;
        ReorderableList m_JointsList;

        JointMask Target => (JointMask)target;
        
        static class Content
        {
            public static readonly GUIContent PositionWeightLabel = new GUIContent("Pos", "Position weight");
            public static readonly GUIContent RotationWeightLabel = new GUIContent("Rot", "Rotation weight");
            public const string ListHeader = "Joints";
        }

        void OnEnable()
        {
            m_ArmatureProperty = serializedObject.FindProperty("m_Armature");
            m_PositionWeightsProperty = serializedObject.FindProperty("m_PositionWeights");
            m_RotationWeightsProperty = serializedObject.FindProperty("m_RotationWeights");
            UpdateProperties();
            InitializeList();
        }

        void InitializeList()
        {
            if (m_JointsProperty == null)
            {
                m_JointsList = null;
                return;
            }
            
            m_JointsList = new ReorderableList(serializedObject, m_JointsProperty, false, true, false, false)
            {
                drawElementCallback = (rect, index, active, focused) =>
                {
                    var element = m_JointsProperty.GetArrayElementAtIndex(index);
                    var nameProperty = element.FindPropertyRelative("Name");

                    rect.height -= 2f;
                    rect.y += 1f;
                    
                    var fieldRect = rect;
                    
                    using (var changeCheck = new EditorGUI.ChangeCheckScope())
                    {
                        fieldRect.width = k_FieldWidth;
                        fieldRect.x = rect.xMax - (2 * fieldRect.width + k_FieldPadding);
                        
                        var positionWeightProperty = m_PositionWeightsProperty.GetArrayElementAtIndex(index);
                        EditorGUI.PropertyField(fieldRect, positionWeightProperty, GUIContent.none);
                        
                        fieldRect.x += fieldRect.width + k_FieldPadding;
                        
                        var rotationWeightProperty = m_RotationWeightsProperty.GetArrayElementAtIndex(index);
                        EditorGUI.PropertyField(fieldRect, rotationWeightProperty, GUIContent.none);
                        
                        if (changeCheck.changed)
                            serializedObject.ApplyModifiedProperties();
                    }

                    var pathRect = rect;
                    pathRect.x += k_FieldPadding;
                    pathRect.width = rect.width - k_FieldPadding - fieldRect.width;
                    EditorGUI.LabelField(pathRect, $"{index}: {nameProperty.stringValue}");
                },
                drawHeaderCallback = rect =>
                {
                    var fieldRect = rect;
                    fieldRect.x += k_FieldPadding;
                    EditorGUI.LabelField(fieldRect, Content.ListHeader);

                    fieldRect = rect;
                    fieldRect.width = k_FieldWidth;
                    fieldRect.x = rect.xMax - fieldRect.width;
                    EditorGUI.LabelField(fieldRect, Content.RotationWeightLabel);
                    
                    fieldRect.x -= fieldRect.width + k_FieldPadding;
                    EditorGUI.LabelField(fieldRect, Content.PositionWeightLabel);
                },
                elementHeightCallback = _ => EditorGUIUtility.singleLineHeight + 2f
            };
        }

        void UpdateProperties()
        {
            var armatureDefinition = m_ArmatureProperty.objectReferenceValue;
            if (armatureDefinition == null)
            {
                m_JointsProperty = null;
                return;
            }

            m_JointsProperty = new SerializedObject(armatureDefinition).FindProperty("m_Armature").FindPropertyRelative("Joints");
            
            Target.Initialize();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(m_ArmatureProperty);

                if (changeCheck.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                    UpdateProperties();
                    InitializeList();
                }
            }

            m_JointsList?.DoLayoutList();
        }
    }
}
