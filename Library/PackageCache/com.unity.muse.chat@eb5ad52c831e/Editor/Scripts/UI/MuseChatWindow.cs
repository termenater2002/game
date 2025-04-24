using System;
using Unity.Muse.Chat.UI.Components;
using Unity.Muse.Chat.UI.Utils;
using Unity.Muse.Common.Account;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Chat.UI
{
    class MuseChatWindow : EditorWindow, IMuseChatHost
    {
        const string k_WindowName = "Muse Chat";

        static Vector2 k_MinSize = new(400, 400);

        internal MuseChatView m_View;

        public Action FocusLost { get; set; }

        [MenuItem("Muse/Chat")]
        public static void ShowWindow()
        {
            var editor = GetWindow<MuseChatWindow>();
            editor.titleContent = new GUIContent(k_WindowName);
            editor.Show();
            editor.minSize = k_MinSize;
        }

        void CreateGUI()
        {
            m_View = new MuseChatView(this);
            m_View.Initialize();
            m_View.style.flexGrow = 1;
            m_View.style.minWidth = 400;
            rootVisualElement.Add(m_View);

            try
            {
                AccountController.Register(this);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            m_View.InitializeThemeAndStyle();
        }

        void OnDestroy()
        {
            m_View?.Deinit();
        }

        void OnLostFocus()
        {
            FocusLost?.Invoke();
        }
    }
}
