using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    class MuseShortcutHandler : Manipulator
    {
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            ExecuteShortcuts(ref evt);
        }

        static void ExecuteShortcuts<T>(ref T evt) where T : KeyboardEventBase<T>, new()
        {
            var keyCode = evt.keyCode;
            var keyModifier = GetModifier(evt);

            var shortcuts = MuseShortcuts.GetShortcuts(keyCode, keyModifier);
            if (shortcuts != null)
            {
                foreach (var shortcut in shortcuts)
                {
                    if (evt.target is VisualElement element)
                    {
                        if (element.panel != shortcut.source?.panel)
                            continue;

                        if (shortcut.requireFocus &&
                            element.panel?.focusController.focusedElement is VisualElement focusedElement &&
                            shortcut.source != null && focusedElement != shortcut.source &&
                            !shortcut.source.Contains(focusedElement))
                            continue;
                    }

                    shortcut.action?.Invoke();
                }

                evt.StopPropagation();
                evt.imguiEvent?.Use();
            }
        }

        static KeyModifier GetModifier(IKeyboardEvent evt)
        {
            if (evt.altKey)
                return KeyModifier.Alt;
            if (evt.actionKey)
                return KeyModifier.Action;
            if (evt.ctrlKey)
                return KeyModifier.Control;
            if (evt.shiftKey)
                return KeyModifier.Shift;

            return KeyModifier.None;
        }
    }
}
