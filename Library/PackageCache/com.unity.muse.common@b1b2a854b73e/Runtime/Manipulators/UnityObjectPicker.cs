using System;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.Muse.Common.Manipulators
{
    class UnityObjectPicker : Manipulator
    {
        public event Action pickStarted;
        
        public event Action pickEnded;

        public event Action<Object> objectSelected;

        public bool isPicking => m_StartedPicking;

        Object m_SelectedObj;

        bool m_StartedPicking;

        Object m_FocusedWindow;
        
        public void StartPicking()
        {
#if UNITY_EDITOR
            StartPickingInEditor();
#endif
        }
        
        public void EndPicking()
        {
#if UNITY_EDITOR
            EndPickingInEditor();
#endif
        }

        protected override void RegisterCallbacksOnTarget() { }

        protected override void UnregisterCallbacksFromTarget() { }

#if UNITY_EDITOR
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

        void StartPickingInEditor()
        {
            if (isPicking)
                return;

            m_SelectedObj = null;
            m_StartedPicking = true;

            pickStarted?.Invoke();

            m_FocusedWindow = EditorWindow.focusedWindow;

            AddListeners();

            target.schedule.Execute(() =>
            {
                target.CaptureMouse();
            });
        }

        void EndPickingInEditor()
        {
            if (!isPicking)
                return;

            m_StartedPicking = false;

            RemoveListeners();

            if (target.HasMouseCapture())
                target.ReleaseMouse();

            if (m_SelectedObj)
                objectSelected?.Invoke(m_SelectedObj);

            pickEnded?.Invoke();
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

            m_SelectedObj = Selection.activeObject;

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
                var go = FindObject();
                if (go)
                    m_SelectedObj = go;

                EndPicking();
            }
        }

        static GameObject FindObject()
        {
            var go = HandleUtility.PickGameObject(Event.current.mousePosition, true);
            return go ? go : null;
        }
#endif
    }
}