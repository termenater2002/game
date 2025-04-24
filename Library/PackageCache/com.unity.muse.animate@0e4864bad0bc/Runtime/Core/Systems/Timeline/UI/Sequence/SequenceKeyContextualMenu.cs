using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    class SequenceKeyContextualMenu
    {
        // Note: This end padding is a bit hacky, but it allows enough padding
        // to cleanly display keyboard shortcuts to the right of the menu items label
        const string k_EndPad = ContextMenu.endPad;

        const string k_CopyIcon = "";
        const string k_PasteAndReplaceIcon = "";
        const string k_PasteLeftIcon = "";
        const string k_PasteRightIcon = "";
        const string k_DuplicateRightIcon = "";
        const string k_DuplicateLeftIcon = "";
        const string k_MoveRightIcon = "";
        const string k_MoveLeftIcon = "";
        const string k_DeleteIcon = "";
        
        const string k_CopyLabel = "Copy"+k_EndPad;
        const string k_PasteAndReplaceLabel = "Paste and Replace"+k_EndPad;
        const string k_PasteLeftLabel = "Paste Left"+k_EndPad;
        const string k_PasteRightLabel = "Paste Right"+k_EndPad;
        const string k_DuplicateRightLabel = "Duplicate Right"+k_EndPad;
        const string k_DuplicateLeftLabel = "Duplicate Left"+k_EndPad;
        const string k_MoveRightLabel = "Move Right"+k_EndPad;
        const string k_MoveLeftLabel = "Move Left"+k_EndPad;
        const string k_DeleteLabel = "Delete"+k_EndPad;
        
        // Note: Disabling keyboard shortcuts until handled correctly, since these are shared with other menus and causing conflicts.
        // const string k_CopyShortcut = "C";
        const string k_CopyShortcut = "";
        // const string k_DeleteShortcut = "Delete";
        const string k_DeleteShortcut = "";
        const string k_PasteLeftShortcut = "";
        const string k_PasteRightShortcut = "";
        const string k_MoveRightShortcut = "";
        const string k_MoveLeftShortcut = "";
        // const string k_PasteAndReplaceShortcut = "V";
        const string k_PasteAndReplaceShortcut = "";
        const string k_DuplicateLeftShortcut = "";
        // const string k_DuplicateRightShortcut = "D";
        const string k_DuplicateRightShortcut = "";
        
        public Action<ActionType, ClipboardService, SelectionModel<TimelineModel.SequenceKey>, SequenceItemViewModel<TimelineModel.SequenceKey>> OnMenuAction;

        public enum ActionType
        {
            Copy,
            PasteAndReplace,
            PasteLeft,
            PasteRight,
            DuplicateRight,
            DuplicateLeft,
            MoveRight,
            MoveLeft,
            Delete
        }

        TimelineModel m_Timeline;
        SelectionModel<TimelineModel.SequenceKey> m_SelectionModel;
        ClipboardService m_ClipboardService;
        SequenceItemViewModel<TimelineModel.SequenceKey> m_Target;

        List<ContextMenu.ActionArgs> m_ActionArgs;

        ContextMenu.ActionArgs m_CopyAction;
        ContextMenu.ActionArgs m_PasteAndReplaceAction;
        ContextMenu.ActionArgs m_PasteLeftAction;
        ContextMenu.ActionArgs m_PasteRightAction;
        ContextMenu.ActionArgs m_DuplicateKeyLeftAction;
        ContextMenu.ActionArgs m_DuplicateKeyRightAction;
        ContextMenu.ActionArgs m_MoveKeyLeftAction;
        ContextMenu.ActionArgs m_MoveKeyRightAction;
        ContextMenu.ActionArgs m_DeleteAction;

        /// <summary>
        /// A contextual menu for a given timeline's sequence key.
        /// </summary>
        public SequenceKeyContextualMenu()
        {
            // TODO: Keyboard shortcuts
            // Note: Disabling keyboard shortcuts until handled correctly, since these are shared with other menus and causing conflicts.
            
            m_CopyAction = CreateAction(ActionType.Copy, k_CopyLabel, k_CopyIcon, GetCmd(k_CopyShortcut));
            m_PasteAndReplaceAction = CreateAction(ActionType.PasteAndReplace, k_PasteAndReplaceLabel, k_PasteAndReplaceIcon, GetCmd(k_PasteAndReplaceShortcut));
            m_PasteLeftAction = CreateAction(ActionType.PasteLeft, k_PasteLeftLabel, k_PasteLeftIcon, GetCmd(k_PasteLeftShortcut));
            m_PasteRightAction = CreateAction(ActionType.PasteRight, k_PasteRightLabel, k_PasteRightIcon, GetCmd(k_PasteRightShortcut));
            m_DuplicateKeyLeftAction = CreateAction(ActionType.DuplicateLeft, k_DuplicateLeftLabel, k_DuplicateLeftIcon, GetCmd(k_DuplicateLeftShortcut));
            m_DuplicateKeyRightAction = CreateAction(ActionType.DuplicateRight, k_DuplicateRightLabel, k_DuplicateRightIcon, GetCmd(k_DuplicateRightShortcut));
            m_MoveKeyLeftAction = CreateAction(ActionType.MoveLeft, k_MoveLeftLabel, k_MoveLeftIcon, GetCmd(k_MoveLeftShortcut));
            m_MoveKeyRightAction = CreateAction(ActionType.MoveRight, k_MoveRightLabel, k_MoveRightIcon, GetCmd(k_MoveRightShortcut));
            m_DeleteAction = CreateAction(ActionType.Delete, k_DeleteLabel, k_DeleteIcon, k_DeleteShortcut);

            m_ActionArgs = new List<ContextMenu.ActionArgs>
            {
                m_CopyAction,
                m_PasteAndReplaceAction,
                m_PasteLeftAction,
                m_PasteRightAction,
                m_DuplicateKeyLeftAction,
                m_DuplicateKeyRightAction,
                m_MoveKeyLeftAction,
                m_MoveKeyRightAction,
                m_DeleteAction
            };
        }
        
        /// <summary>
        /// Adds Ctrl+ or ⌘+ before the provided text depending on the platform.
        /// </summary>
        /// <param name="text">The text that follows the added localised command label.</param>
        /// <returns>The provided text with an added Ctrl+ or ⌘+ depending on the platform.</returns>
        static string GetCmd(string text)
        {
            return text == "" ? "" : PlatformUtils.GetCommandLabel(text);
        }
        
        /// <summary>
        /// Opens the contextual menu for a given timeline's sequence key.
        /// </summary>
        /// <param name="timeline">The timeline model of the menu.</param>
        /// <param name="clipboardService">The clipboard service used by the menu.</param>
        /// <param name="keySelection">The key selection model of the menu.</param>
        /// <param name="targetKey">The target key, for which the context menu is opened.</param>
        /// <param name="anchor">An anchor for the contextual menu.</param>
        public void Open(TimelineModel timeline, ClipboardService clipboardService, SelectionModel<TimelineModel.SequenceKey> keySelection, SequenceItemViewModel<TimelineModel.SequenceKey> targetKey, VisualElement anchor)
        {
            m_Target = targetKey;
            m_Timeline = timeline;
            m_ClipboardService = clipboardService;
            m_SelectionModel = keySelection;

            // Enable / Disable actions
            m_PasteAndReplaceAction.IsClickable = m_ClipboardService.CanPaste(m_Target.Target.Key);
            m_PasteLeftAction.IsClickable = m_ClipboardService.CanPaste(m_Target.Target.Key);
            m_PasteRightAction.IsClickable = m_ClipboardService.CanPaste(m_Target.Target.Key);
            m_MoveKeyLeftAction.IsClickable = m_Timeline.IndexOf(m_Target.Target) > 0;
            m_MoveKeyRightAction.IsClickable = m_Timeline.IndexOf(m_Target.Target) < m_Timeline.KeyCount - 1;
            m_DeleteAction.IsClickable = m_Timeline.KeyCount > 1;

            // Open the menu
            ContextMenu.OpenContextMenu(anchor, m_ActionArgs);
        }

        ContextMenu.ActionArgs CreateAction(ActionType actionType, string label, string icon, string shortcut)
        {
            return new ContextMenu.ActionArgs((int)actionType, label, icon, () => { InvokeMenuAction(actionType); }, shortcut);
        }

        void InvokeMenuAction(ActionType type)
        {
            int keyIndex = -1;
            if (m_SelectionModel.HasSelection)
            {
                keyIndex = m_Timeline.IndexOf(m_SelectionModel.GetSelection(0));
            }

            DeepPoseAnalytics.SendKeyAction(Enum.GetName(typeof(ActionType), type), keyIndex);
            
            OnMenuAction?.Invoke(type, m_ClipboardService, m_SelectionModel, m_Target);
        }
    }
}
