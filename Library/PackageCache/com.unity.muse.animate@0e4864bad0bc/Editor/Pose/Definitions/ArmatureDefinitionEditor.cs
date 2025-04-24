using System;
using System.Linq;
using Unity.DeepPose.Core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace Unity.Muse.Animate.Editor
{
    [CustomEditor(typeof(ArmatureDefinition))]
    class ArmatureDefinitionEditor : UnityEditor.Editor
    {
        static readonly GUIContent k_ArrowLabel = new ("->");
        static readonly GUIContent k_PathLabel = new ("Path");
        static readonly GUIContent k_NameLabel = new ("Name");

        const string k_InvalidConfigurationError = "Armature is invalid. Check for duplicated names.";
        const string k_DropHelpMessage = "Drag & Drop an object here to automatically build the Armature";
        const string k_UndoMessage = "Change armature";

        const float k_JointHorizontalSpacing = 30f;
        const float k_StartPadding = 16f;

        ArmatureDefinition Target => target as ArmatureDefinition;

        SerializedProperty m_JointsProperty;
        ReorderableList m_JointsList;

        void OnEnable()
        {
            FindProperties();
            InitializeList();
        }

        void FindProperties()
        {
            var armatureProperty = serializedObject.FindProperty("m_Armature");
            m_JointsProperty = armatureProperty.FindPropertyRelative(nameof(ArmatureData.Joints));
        }

        void InitializeList()
        {
            m_JointsList = new ReorderableList(serializedObject, m_JointsProperty, true, true, false, false);

            m_JointsList.onAddCallback = list =>
            {
                m_JointsProperty.InsertArrayElementAtIndex(m_JointsProperty.arraySize);
                serializedObject.ApplyModifiedProperties();
            };

            m_JointsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = m_JointsProperty.GetArrayElementAtIndex(index);
                var pathProperty = element.FindPropertyRelative(nameof(ArmatureData.Joint.Path));
                var nameProperty = element.FindPropertyRelative(nameof(ArmatureData.Joint.Name));

                var pathRect = new Rect(rect.x, rect.y, rect.width / 2 - k_JointHorizontalSpacing / 2, EditorGUIUtility.singleLineHeight);
                var spaceRect = new Rect(rect.x + rect.width / 2 - k_JointHorizontalSpacing / 2, rect.y, k_JointHorizontalSpacing, EditorGUIUtility.singleLineHeight);
                var nameRect = new Rect(rect.x + rect.width / 2 + k_JointHorizontalSpacing / 2, rect.y, rect.width / 2 - k_JointHorizontalSpacing / 2, EditorGUIUtility.singleLineHeight);

                EditorGUI.BeginChangeCheck();
                EditorGUI.LabelField(pathRect, new GUIContent(pathProperty.stringValue));
                EditorGUI.LabelField(spaceRect, k_ArrowLabel, new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter});
                EditorGUI.PropertyField(nameRect, nameProperty, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, k_UndoMessage);
                    serializedObject.ApplyModifiedProperties();
                }
            };

            m_JointsList.drawHeaderCallback += rect =>
            {
                rect.x += k_StartPadding;
                rect.width -= k_StartPadding;
                var pathRect = new Rect(rect.x, rect.y, rect.width / 2 - k_JointHorizontalSpacing / 2, rect.height);
                var nameRect = new Rect(rect.x + rect.width / 2 + k_JointHorizontalSpacing / 2, rect.y, rect.width / 2 - k_JointHorizontalSpacing / 2, rect.height);
                EditorGUI.LabelField(pathRect, k_PathLabel);
                EditorGUI.LabelField(nameRect, k_NameLabel);
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DoDragAndDrop();
            m_JointsList.DoLayoutList();
        }

        void DoDragAndDrop()
        {
            if (!Target.IsValid)
                EditorGUILayout.HelpBox(k_InvalidConfigurationError, MessageType.Error);
            EditorGUILayout.HelpBox(k_DropHelpMessage, MessageType.Info);

            var rect = GUILayoutUtility.GetLastRect();
            if (rect.Contains(Event.current.mousePosition))
            {
                var deepPoseConfig = DragAndDrop.objectReferences.Where(x => x is DeepPoseConfiguration).Cast<DeepPoseConfiguration>().FirstOrDefault();
                var motionConfig = DragAndDrop.objectReferences.Where(x => x is MotionSynthesisConfiguration).Cast<MotionSynthesisConfiguration>().FirstOrDefault();

                var go = DragAndDrop.objectReferences.Where(x => x is GameObject).Cast<GameObject>().FirstOrDefault();

                switch (Event.current.type)
                {
                    case EventType.DragUpdated:
                    {
                        DragAndDrop.visualMode = go != null || deepPoseConfig != null || motionConfig != null ? DragAndDropVisualMode.Generic : DragAndDropVisualMode.Rejected;
                        Event.current.Use();
                        break;
                    }

                    case EventType.DragPerform:
                    {
                        if (go != null)
                        {
                            Undo.RecordObject(target, k_UndoMessage);
                            Target.FromHierarchy(go.transform);
                            serializedObject.Update();
                            EditorUtility.SetDirty(Target);
                        }

                        if (deepPoseConfig != null)
                        {
                            Undo.RecordObject(target, k_UndoMessage);
                            Target.FromSkeleton(deepPoseConfig.Skeleton);
                            serializedObject.Update();
                            EditorUtility.SetDirty(Target);
                        }

                        if (motionConfig != null)
                        {
                            Undo.RecordObject(target, k_UndoMessage);
                            Target.FromSkeleton(motionConfig.Skeleton);
                            serializedObject.Update();
                            EditorUtility.SetDirty(Target);
                        }

                        Event.current.Use();
                        break;
                    }
                }
            }
        }
    }
}
