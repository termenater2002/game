using System;
using System.IO;
using StyleTrainer.Backend;
using Unity.Muse.Sprite.Common.Backend;
using Unity.Muse.Sprite.Common.DebugConfig;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEngine.UIElements;
using UnityDebug = UnityEngine.Debug;

namespace Unity.Muse.StyleTrainer.Editor
{
    [CustomEditor(typeof(StyleTrainerProjectData))]
    class StyleTrainerProjectDataEditor : UnityEditor.Editor, IDisposable
    {
        SerializedProperty m_StyleTrainerData;
        SerializedProperty m_StyleTrainerDataGUID;
        bool m_Foldout;

        void OnEnable()
        {
            m_StyleTrainerData = serializedObject.FindProperty("m_StyleTrainerData");
            m_StyleTrainerDataGUID = m_StyleTrainerData.FindPropertyRelative("m_Guid");
        }

        void ReleaseUI()
        {
            serializedObject.Update();
            m_Foldout = EditorGUILayout.Foldout(m_Foldout, "Style Trainer Project Data");
            if (m_Foldout)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(m_StyleTrainerDataGUID);
                EditorGUI.EndDisabledGroup();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("New Project") &&
                    EditorUtility.DisplayDialog("Style Trainer",
                        "This will create a new Style Trainer Project and the previous project will be lost and unrecoverable.\nAre you sure you want to create a project?", "Yes", "No"))
                {
                    (target as StyleTrainerProjectData)?.Reset();
                    serializedObject.Update();
                }

                if (GUILayout.Button("Reload Project") &&
                    EditorUtility.DisplayDialog("Style Trainer",
                            "This will reload the Style Trainer Project. Any untrained modification will be discard.", "Yes", "No"))
                {
                    (target as StyleTrainerProjectData)?.ClearProjectData();
                    serializedObject.Update();
                }

                GUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DeveloperUI()
        {
            serializedObject.Update();
            m_Foldout = EditorGUILayout.Foldout(m_Foldout, "Style Trainer Project Data");
            if (m_Foldout)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Open Editor")) StyleTrainerWindow.OpenAsset(target as StyleTrainerProjectData);

                if (GUILayout.Button("Save"))
                {
                    (target as StyleTrainerProjectData)?.Save();
                    serializedObject.Update();
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("New Project"))
                {
                    (target as StyleTrainerProjectData)?.Reset();
                    serializedObject.Update();
                }

                if (GUILayout.Button("Reload Project"))
                {
                    (target as StyleTrainerProjectData)?.ClearProjectData();
                    serializedObject.Update();
                }

                GUILayout.EndHorizontal();

                if (GUILayout.Button("Test Get Styles")) GetStyles();

                base.OnInspectorGUI();
            }

            serializedObject.ApplyModifiedProperties();
        }

        void GetStyles()
        {
            var getStylesRequest = new GetStylesRequest
            {
                guid = ((StyleTrainerProjectData)target).guid
            };
            var getStylesRestCall = new GetStylesRestCall(ServerConfig.serverConfig, getStylesRequest);
            getStylesRestCall.RegisterOnSuccess(OnGetStylesSuccess);
            getStylesRestCall.RegisterOnFailure(OnGetStylesFailure);
            getStylesRestCall.SendRequest();
        }

        static void OnGetStylesFailure(GetStylesRestCall obj)
        {
            UnityDebug.Log($"GetStylesRestCall failed {obj.requestError}");
        }

        void OnGetStylesSuccess(GetStylesRestCall arg1, GetStylesResponse arg2)
        {
            foreach (var a in arg2.styleIDs)
            {
                UnityDebug.Log(a);
                var getStyleRequest = new GetStyleRequest
                {
                    guid = ((StyleTrainerProjectData)target).guid,
                    style_guid = a
                };
                var getStyleRestCall = new GetStyleRestCall(ServerConfig.serverConfig, getStyleRequest);
                getStyleRestCall.RegisterOnSuccess(OnGetStyleSuccess);
                getStyleRestCall.RegisterOnFailure(OnGetStyleFailure);
                getStyleRestCall.SendRequest();
            }
        }

        static void OnGetStyleFailure(GetStyleRestCall obj)
        {
            UnityDebug.Log($"OnGetStyleFailure failed {obj.requestError}");
        }

        void OnGetStyleSuccess(GetStyleRestCall arg1, GetStyleResponse arg2)
        {
            foreach (var checkpoint in arg2.checkpointIDs)
            {
                var getCheckPointRequest = new GetCheckPointRequest
                {
                    guid = ((StyleTrainerProjectData)target).guid,
                    checkpoint_guid = checkpoint
                };
                var getCheckPointRestCall = new GetCheckPointRestCall(ServerConfig.serverConfig, getCheckPointRequest);
                getCheckPointRestCall.RegisterOnSuccess(OnGetCheckPointSuccess);
                getCheckPointRestCall.RegisterOnFailure(OnGetCheckPointFailure);
                getCheckPointRestCall.SendRequest();
            }

            foreach (var trainingset in arg2.trainingsetIDs)
            {
                var getTrainingSetRequest = new GetTrainingSetRequest
                {
                    guid = ((StyleTrainerProjectData)target).guid,
                    training_set_guid = trainingset
                };

                var getTrainingSetRestCall = new GetTrainingSetRestCall(ServerConfig.serverConfig, getTrainingSetRequest);
                getTrainingSetRestCall.RegisterOnSuccess(OnGetTrainingSetSuccess);
                getTrainingSetRestCall.RegisterOnFailure(OnGetTrainingSetFailure);
                getTrainingSetRestCall.SendRequest();
            }
        }

        static void OnGetTrainingSetFailure(GetTrainingSetRestCall obj)
        {
            UnityDebug.Log($"OnGetTrainingSetFailure failed {obj.requestError}");
        }

        static void OnGetTrainingSetSuccess(GetTrainingSetRestCall arg1, GetTrainingSetResponse arg2)
        {
            UnityDebug.Log($"OnGetTrainingSetSuccess  {arg2.training_image_guids}");
        }

        void OnGetCheckPointSuccess(GetCheckPointRestCall arg1, GetCheckPointResponse arg2)
        {
            UnityDebug.Log($"OnGetCheckPointSuccess {arg2.checkpointID} {arg2.status}");
            var setCheckPointRequest = new SetCheckPointFavouriteRequest
            {
                checkpoint_guid = arg2.checkpointID,
                guid = arg2.asset_id,
                style_guid = arg2.styleID
            };
            var setCheckPointRestCall = new SetCheckPointFavouriteRestCall(ServerConfig.serverConfig, setCheckPointRequest);
            setCheckPointRestCall.RegisterOnSuccess(OnSetCheckPointSuccess);
            setCheckPointRestCall.RegisterOnFailure(OnSetCheckPointFailure);
        }

        static void OnSetCheckPointFailure(SetCheckPointFavouriteRestCall obj)
        {
            UnityDebug.Log($"OnSetCheckPointFailure failed {obj.requestError}");
        }

        static void OnSetCheckPointSuccess(SetCheckPointFavouriteRestCall arg1, SetCheckPointFavouriteResponse arg2)
        {
            UnityDebug.Log($"OnSetCheckPointSuccess");
        }

        static void OnGetCheckPointFailure(GetCheckPointRestCall obj)
        {
            UnityDebug.Log($"OnGetCheckPointFailure failed {obj.requestError}");
        }

        internal class CreateAssetEndNameEditAction : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var uniqueName = AssetDatabase.GenerateUniqueAssetPath(pathName);
                var asset = CreateInstance<StyleTrainerProjectData>();
                asset.name = Path.GetFileName(uniqueName);
                asset.Init();
                AssetDatabase.CreateAsset(asset, uniqueName);
            }
        }

        public static void CreateFile(string name, EndNameEditAction action)
        {
            var assetSelectionPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            var isFolder = false;
            if (!string.IsNullOrEmpty(assetSelectionPath))
                isFolder = File.GetAttributes(assetSelectionPath).HasFlag(FileAttributes.Directory);
            var path = "Assets/";
            if (isFolder) path = assetSelectionPath;
            var destName = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, name));
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, action, destName, null, null);
        }

        public void CreateInspectorUI(VisualElement element)
        {
            VisualElement imguiContainer;
            if (DebugConfig.developerMode)
                imguiContainer = new IMGUIContainer(DeveloperUI);
            else
                imguiContainer = new IMGUIContainer(ReleaseUI);
            element.Add(imguiContainer);
        }

        public void Dispose()
        {
            // not used for now
        }

        static StyleTrainerProjectData LoadProject(string path)
        {
            StyleTrainerProjectData project;
            var instance = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(path);
            if (instance == null || instance.Length <= 0 || instance[0] == null || instance[0].GetType() != typeof(StyleTrainerProjectData))
            {
                project = CreateInstance<StyleTrainerProjectData>();
                UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new[] { project }, path, true);
            }
            else
            {
                project = (StyleTrainerProjectData)instance[0];
            }

            project.assetPath = path;
            return project;
        }
    }
}