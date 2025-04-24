using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Unity.Muse.Sprite.UIComponents
{
#if UNITY_EDITOR
    internal class SpritePicker : Manipulator
    {
        public event Action onPickStart;
        public event Action onPickEnd;

        public event Action<UnityEngine.Sprite> onSelectedObject;

        public bool isPicking => m_StartedPicking;

        UnityEngine.Sprite m_SelectedSprite;

        bool m_StartedPicking;

        Object m_FocusedWindow;

        protected override void RegisterCallbacksOnTarget() { }

        protected override void UnregisterCallbacksFromTarget() { }

        void AddListeners()
        {
            target.RegisterCallback<FocusOutEvent>(OnFocusOut);
            SceneView.duringSceneGui += OnSceneGui;
            Selection.selectionChanged += OnSelectionChanged;
        }

        void RemoveListeners()
        {
            target.UnregisterCallback<FocusOutEvent>(OnFocusOut);
            SceneView.duringSceneGui -= OnSceneGui;
            Selection.selectionChanged -= OnSelectionChanged;
        }

        public void StartPicking()
        {
            if (isPicking)
                return;

            m_SelectedSprite = null;
            m_StartedPicking = true;

            onPickStart?.Invoke();

            m_FocusedWindow = EditorWindow.focusedWindow;

            AddListeners();

            target.schedule.Execute(() => { target.CaptureMouse(); });
        }

        public void EndPicking()
        {
            if (!isPicking)
                return;

            m_StartedPicking = false;

            RemoveListeners();

            if (target.HasMouseCapture())
                target.ReleaseMouse();

            if (m_SelectedSprite != null)
                onSelectedObject?.Invoke(m_SelectedSprite);

            onPickEnd?.Invoke();
        }

        void CheckWindowFocus()
        {
            if (!isPicking)
                return;

            if (EditorWindow.focusedWindow != m_FocusedWindow)
            {
                switch (EditorWindow.focusedWindow.titleContent.text)
                {
                    case "Hierarchy":
                    case "Project":
                        EditorApplication.delayCall += OnSelectionChanged;
                        break;
                    default:
                        EndPicking();
                        break;
                }
            }
        }

        void OnSelectionChanged()
        {
            if (!isPicking)
                return;

            switch (Selection.activeObject)
            {
                case GameObject go:
                {
                    var spriteRenderer = go.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                        m_SelectedSprite = spriteRenderer.sprite;
                    break;
                }
                case UnityEngine.Sprite sprite:
                    m_SelectedSprite = sprite;
                    break;
                default:
                {
                    var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                    var sprite = AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(assetPath);
                    if (sprite != null)
                        m_SelectedSprite = sprite;
                    break;
                }
            }

            EndPicking();
        }
        
        void OnFocusOut(FocusOutEvent evt)
        {
            EditorApplication.delayCall += CheckWindowFocus;
        }

        void OnSceneGui(SceneView sceneView)
        {
            if (!isPicking)
                return;

            var e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                var sprite = FindObject();
                if (sprite != null)
                    m_SelectedSprite = sprite;

                EndPicking();
            }
        }

        static UnityEngine.Sprite FindObject()
        {
            var go = HandleUtility.PickGameObject(Event.current.mousePosition, true);
            if (go != null)
            {
                var selection = go.GetComponentsInChildren<SpriteRenderer>();
                if (selection.Length > 0)
                    return selection[0].sprite;
            }

            return null;
        }
    }
#endif
}
