using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate.Editor
{
    [CustomEditor(typeof(ArmatureMappingComponent), false)]
    class ArmatureMappingComponentEditor : UnityEditor.Editor
    {
        static readonly GUIContent k_MappingLabel = new ("Joints");
        static readonly GUIContent k_TransformLabel = new("Transform");
        static readonly GUIContent k_FindTransformsContent = new("Find Transforms");

        const string k_MissingArmatureError = "You must specify an armature definition.";
        const string k_MissingArmatureRootError = "You must specify a root armature.";
        const string k_InvalidMappingError = "Mapping is invalid. Check for missing joints.";

        const int k_NoHeaderHeight = 2;
        const int k_ElementHeightPadding = 2;
        const int k_HorizontalPadding = 6;

        static bool s_MappingFoldout = true;

        SerializedProperty m_ArmatureDefinitionProperty;
        SerializedProperty m_ArmatureRootProperty;
        SerializedProperty m_TransformsProperty;

        ReorderableList m_TransformsList;

        ArmatureMappingComponent Target => (ArmatureMappingComponent)serializedObject.targetObject;
        ArmatureDefinition ArmatureDefinition => (ArmatureDefinition)m_ArmatureDefinitionProperty.objectReferenceValue;

        ArmatureDefinition m_PrevNonNullArmature;

        void OnEnable()
        {
            FindProperties();
            CreateLists();
        }

        void FindProperties()
        {
            m_ArmatureDefinitionProperty = serializedObject.FindProperty("m_ArmatureDefinition");
            m_ArmatureRootProperty = serializedObject.FindProperty("m_ArmatureRoot");

            var mappingProperty = serializedObject.FindProperty("m_ArmatureMappingData");
            m_TransformsProperty = mappingProperty.FindPropertyRelative(nameof(ArmatureMappingData.Transforms));
        }

        void CreateLists()
        {
            m_TransformsList = CreateTransformsList(serializedObject, m_TransformsProperty);
        }

        ReorderableList CreateTransformsList(SerializedObject obj, SerializedProperty property)
        {
            if (ArmatureDefinition == null)
                return null;

            var list = new ReorderableList(obj, property, false, true, false, false);
            list.drawHeaderCallback = DrawTransformsListHeader;
            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                DrawTransformsListItem(list, rect, index, isActive, isFocused);
            };

            list.elementHeight = EditorGUIUtility.singleLineHeight + k_ElementHeightPadding;
            list.headerHeight = k_NoHeaderHeight;

            return list;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (ArmatureDefinition == null)
            {
                EditorGUILayout.HelpBox(k_MissingArmatureError, MessageType.Error);
            }
            else if (m_ArmatureRootProperty.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(k_MissingArmatureRootError, MessageType.Error);
            }
            else if (!ArmatureDefinition.IsValid)
            {
                EditorGUILayout.HelpBox(k_InvalidMappingError, MessageType.Error);
            }

            // Armature Section
            EditorGUI.BeginChangeCheck();
            m_PrevNonNullArmature = ArmatureDefinition != null ? ArmatureDefinition : m_PrevNonNullArmature;
            EditorGUILayout.PropertyField(m_ArmatureDefinitionProperty);
            if (EditorGUI.EndChangeCheck())
            {
                // Note: we try to keep the user configuration if the new config has the same joint names as the previously known config
                var keepOldJoints = ArmatureDefinition == null || ArmatureDefinition.HaveSameJointNames(m_PrevNonNullArmature);
                UpdateTransformsList(!keepOldJoints);
            }

            // Root section
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_ArmatureRootProperty);
            if (EditorGUI.EndChangeCheck())
            {
                if (m_ArmatureDefinitionProperty.objectReferenceValue != null)
                    FindTransforms();
            }

            // Mapping Section
            s_MappingFoldout = EditorGUILayout.Foldout(s_MappingFoldout, k_MappingLabel);
            if (s_MappingFoldout)
            {
                if (m_ArmatureDefinitionProperty.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox(k_MissingArmatureError, MessageType.Info);
                }
                else
                {
                    m_TransformsList.DoLayoutList();
                    if (GUILayout.Button(k_FindTransformsContent))
                        FindTransforms();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        static void DrawTransformsListHeader(Rect rect)
        {
            const int startPadding = 16;

            rect.x += startPadding;
            rect.width -= startPadding;

            var transformRect = new Rect(rect.x, rect.y, rect.width - k_HorizontalPadding, rect.height);

            EditorGUI.indentLevel = 0;
            EditorGUI.LabelField(transformRect, k_TransformLabel);
        }

        void DrawTransformsListItem(ReorderableList list, Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            var offset = k_ElementHeightPadding * 0.5f;

            var transformRect = new Rect(rect.x, rect.y + offset, rect.width, EditorGUIUtility.singleLineHeight);

            var jointName = ArmatureDefinition == null ? "???" : ArmatureDefinition.GetJointName(index);
            EditorGUI.PropertyField(transformRect, element, new GUIContent(index + ": " + jointName));
        }

        void UpdateTransformsList(bool reset)
        {
            if (ArmatureDefinition == null)
            {
                m_TransformsList = null;
                return;
            }

            if (reset || (m_TransformsProperty.arraySize != ArmatureDefinition.NumJoints))
            {
                InitializeTransformsArray();
                FindTransforms();
            }

            m_TransformsList = CreateTransformsList(serializedObject, m_TransformsProperty);
        }

        void InitializeTransformsArray()
        {
            if (ArmatureDefinition == null)
                return;

            m_TransformsProperty.arraySize = ArmatureDefinition.NumJoints;
        }

        void FindTransforms()
        {
            Assert.AreEqual(ArmatureDefinition.NumJoints, m_TransformsProperty.arraySize);

            var rootTransform = Target.transform;
            if (m_ArmatureRootProperty.objectReferenceValue != null)
                rootTransform = (Transform)m_ArmatureRootProperty.objectReferenceValue;

            var transforms = new Transform[ArmatureDefinition.NumJoints];
            ArmatureDefinition.Armature.FindJoints(rootTransform, transforms);

            for (var i = 0; i < transforms.Length; i++)
            {
                var transformProperty = m_TransformsProperty.GetArrayElementAtIndex(i);
                transformProperty.objectReferenceValue = transforms[i];
            }
        }
    }
}
