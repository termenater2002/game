using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;
using Unity.Muse.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    static class ContextMenu
    {
        // Note: This end padding is a bit hacky, but it allows enough padding
        // to cleanly display keyboard shortcuts to the right of the menu items label
        public const string endPad = "      ";
        
        static MenuBuilder s_CurrentMenu;
        static VisualElement s_Anchor;
        static event Action<DismissType> OnDismissed;

        public class ActionArgs
        {
            public int KeyIndex;
            public string DisplayText;
            public string ShortcutText;
            public string IconName;
            public Action Action;
            public bool IsClickable = true;
            
            public ActionArgs() { }

            public ActionArgs(int keyIndex, string displayText, string iconName, Action action,
                string shortcutText = "", bool isClickable = true)
            {
                KeyIndex = keyIndex;
                DisplayText = displayText;
                IconName = iconName;
                Action = action;
                ShortcutText = shortcutText;
                IsClickable = isClickable;
            }
        }

        public static void OpenContextMenu(VisualElement parent, Vector2 screenPosition, IEnumerable<ActionArgs> actions, Action<DismissType> onDismissed = null)
        {
            if (s_Anchor == null)
            {
                s_Anchor = new VisualElement();
                s_Anchor.name = "entity-context-menu-anchor";
                s_Anchor.style.position = new StyleEnum<Position>(Position.Absolute);
            }

            var localPosition = screenPosition;

            s_Anchor.style.left = localPosition.x;
            s_Anchor.style.top = parent.layout.height - localPosition.y;

            parent.Add(s_Anchor);

            OpenContextMenu(s_Anchor, actions, onDismissed);
        }

        public static MenuBuilder OpenContextMenu(VisualElement anchor, IEnumerable<ActionArgs> actions, Action<DismissType> onDismissed = null)
        {
            s_CurrentMenu?.Dismiss();
            OnDismissed = onDismissed;

            var menuBuilder = MenuBuilder.Build(anchor);

            foreach (var action in actions)
            {
                var item = new MenuItem
                {
                    label = action.DisplayText,
                    icon = action.IconName,
                    userData = action.KeyIndex,
                    shortcut = action.ShortcutText,
                };

                item.SetEnabled(action.IsClickable);

                item.RegisterCallback<ClickEvent>(_ =>
                {
                    action.Action();

                    // close the menu after action.
                    menuBuilder.Dismiss();
                });

                menuBuilder.currentMenu.Add(item);
            }

            menuBuilder.Show();

            s_CurrentMenu = menuBuilder;
            s_CurrentMenu.dismissed += (builder, type) =>
            {
                s_CurrentMenu = null;
                OnDismissed?.Invoke(type);
            };

            return menuBuilder;
        }
    }
}
