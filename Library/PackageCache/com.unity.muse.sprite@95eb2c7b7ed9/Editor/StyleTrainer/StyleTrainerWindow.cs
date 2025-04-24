using System;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using Unity.Muse.Common.Account;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Muse.StyleTrainer.Events.TrainingControllerEvents;

namespace Unity.Muse.StyleTrainer.Editor
{
    class StyleTrainerWindow : EditorWindow
    {
        [SerializeField]
        StyleTrainerProjectData m_Asset;
        [SerializeField]
        [HideInInspector]
        string m_AssetPath;

        StyleModelController m_Controller;
        StyleTrainerMainUI m_MainUI;

        [UnityEditor.MenuItem("Muse/Style Trainer", false, 100)]
        static void Launch()
        {
            OpenAsset(Preference.project);
        }

        void CreateGUI()
        {
            m_Controller = new StyleModelController();
            var currentEditorTheme = EditorGUIUtility.isProSkin ? "editor-dark" : "editor-light";
            m_MainUI = StyleTrainerMainUI.CreateFromUxml(m_Controller, rootVisualElement);
            var museRoot = rootVisualElement.Q<Panel>("muse-root");
            museRoot.theme = currentEditorTheme;
            m_MainUI.AddToClassList("unity-editor");
            m_Controller.SetModel(m_Asset.data);
            m_Controller.eventBus.RegisterEvent<SystemEvents>(OnSystemEvent);
            AccountController.Register(this);
        }

        void OnSystemEvent(SystemEvents arg0)
        {
            if(arg0.state == SystemEvents.ESystemState.CloseWindow)
                this.Close();
        }

        static StyleTrainerWindow FindWindow(StyleTrainerProjectData asset)
        {
            var allWindows = Resources.FindObjectsOfTypeAll<StyleTrainerWindow>();
            foreach (var m in allWindows)
                if (m.m_Asset == asset)
                    return m;

            return null;
        }

        [OnOpenAsset(OnOpenAssetAttributeMode.Execute)]
        public static bool OpenAsset(int instanceID)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as StyleTrainerProjectData;
            if (asset != null)
            {
                OpenAsset(asset);

                return true;
            }

            return false;
        }

        public static StyleTrainerWindow OpenAsset(StyleTrainerProjectData asset)
        {
            var window = FindWindow(asset);
            if (window != null)
            {
                window.Show();
                window.Focus();
                return window;
            }

            var newWindow = CreateInstance<StyleTrainerWindow>();
            newWindow.SetAsset(asset);
            newWindow.titleContent = new GUIContent("Style Trainer");
            newWindow.minSize = new Vector2(800, 600);
            newWindow.Show();

            newWindow.position = new Rect(newWindow.position.position, newWindow.position.size);

            return newWindow;
        }

        public static void CloseWindow(StyleTrainerProjectData asset)
        {
            var window = FindWindow(asset);
            if (window != null) window.Close();
        }

        void OnEnable()
        {
            SetAsset(Preference.project);
        }

        void OnDisable()
        {
            if (m_Asset != null)
            {
                m_Asset.Save();
                m_Asset.data?.OnDispose();
            }

            m_MainUI?.Dispose();
            m_Controller?.Dispose();
        }

        void OnDestroy()
        {
            ArtifactCache.Dispose();
        }

        void SetAsset(StyleTrainerProjectData asset)
        {
            if (m_Asset != asset && m_Asset != null)
                m_Asset.onDataChanged -= OnProjectDataChanged;
            m_Asset = asset;
            m_Asset.onDataChanged += OnProjectDataChanged;
            m_AssetPath = AssetDatabase.GetAssetPath(asset);
            if (m_Controller != null)
                m_Controller.SetModel(m_Asset.data);
        }

        void OnProjectDataChanged(StyleTrainerProjectData obj)
        {
            SetAsset(obj);
        }
    }
}