#if UNITY_EDITOR
using System;
using System.Threading.Tasks;
using Unity.Muse.AppUI.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account.UI
{
    class AccountDropdownWindow : EditorWindow
    {
        internal static Func<Rect> toolbarPosition;
        internal static VisualElement toolbarButton;
        const int k_MinPopupWidth = 300;

        /// <summary>
        /// Show the account settings without it being clicked by the user.
        /// </summary>
        internal static void ShowMuseAccountSettingsAsPopup()
        {
            var rect = toolbarButton.worldBound;
            rect.position = VisualElementUtility.GetScreenPosition(toolbarPosition(), toolbarButton);
            ShowMuseAccountSettingsAsPopupInternal(rect);
        }

        /// <summary>
        /// Show muse account settings
        /// </summary>
        /// <param name="rect">Element bounds/Rect relative to its EditorWindow</param>
        internal static void ShowMuseAccountSettingsAsPopup(Rect rect) =>
            ShowMuseAccountSettingsAsPopupInternal(GUIUtility.GUIToScreenRect(rect));

        static void ShowMuseAccountSettingsAsPopupInternal(Rect buttonRect)
        {
            ClearPreviousWindows();
            var popup = CreateInstance<AccountDropdownWindow>();
            popup.hideFlags = HideFlags.DontSave;
            const int minPopupHeight = 1;
            popup.ShowAsDropDown(buttonRect, new Vector2(k_MinPopupWidth, minPopupHeight));
        }

        static void ClearPreviousWindows()
        {
            var windows = Resources.FindObjectsOfTypeAll<AccountDropdownWindow>();
            foreach (var window in windows)
            {
                try
                {
                    window.Close();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        void CreateGUI()
        {
            var panel = new Panel();
            panel.AddToClassList("dropdown-editor-window");
            rootVisualElement.Add(panel);

            AccountController.Register(this);

            var scrollView = new ScrollView(); // Wrap in a scrollview to be certain all content will always be shown.
            var content = new AccountDropdownContent { OnAction = Close };

            content.RegisterCallback<GeometryChangedEvent>(async evt =>
            {
                const int popupHeightOffset = 4;
                var height = evt.newRect.height + popupHeightOffset;

                // on Mac OS this is necessary
                await Task.Yield();

                minSize = new Vector2(k_MinPopupWidth, height);
            });

            scrollView.Add(content);
            panel.Add(scrollView);
        }
    }
}

#endif
