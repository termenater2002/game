using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.Muse.Animate.PlatformUtils;

namespace Unity.Muse.Animate
{
    class LibraryItemContextualMenu
    {
        // Note: This end padding is a bit hacky, but it allows enough padding
        // to cleanly display keyboard shortcuts to the right of the menu items label
        const string k_EndPad = ContextMenu.endPad;
        
        const string k_CopyLabel = "Copy"+k_EndPad;
        const string k_CopyIcon = "";
        const string k_CopyShortcut = "C";
        const string k_PasteAndReplaceLabel = "Paste and Replace"+k_EndPad;
        const string k_PasteAndReplaceIcon = "";
        const string k_PasteAndReplaceShortcut = "V";
        const string k_PasteLabel = "Paste"+k_EndPad;
        const string k_PasteIcon = "";
        const string k_PasteShortcut = "";
        const string k_DuplicateLabel = "Duplicate"+k_EndPad;
        const string k_DuplicateIcon = "";
        // Note: Temporarily disabling shortcuts for library items
        const string k_DuplicateShortcut = "";
        //const string k_DuplicateShortcut = "D";
        const string k_DeleteLabel = "Delete"+k_EndPad;
        const string k_DeleteIcon = "";
        const string k_DeleteShortcut = "";

        const string k_ExportLabel = "Export"+k_EndPad;

        const string k_RenameLabel = "Rename";
        
#if UNITY_MUSE_ANIMATE_ENABLE_FBX_EXPORT && UNITY_MUSE_DEV
        const string k_ExportToFbxLabel = "[Dev] Export to FBX"+k_EndPad;
#elif UNITY_MUSE_ANIMATE_ENABLE_FBX_EXPORT
        const string k_ExportToFbxLabel = "Export to FBX"+k_EndPad;
#endif
        const string k_UsePromptLabel = "Use This Prompt"+k_EndPad;

        public Action<ActionType, ClipboardService, SelectionModel<LibraryItemAsset>, LibraryItemUIModel> OnMenuAction;

        public enum ActionType
        {
            Copy,
            PasteAndReplace,
            Paste,
            Duplicate,
            Delete,
            Export,
            ExportToFbx,
            UsePrompt,
            Rename
        }

        SelectionModel<LibraryItemAsset> m_SelectionModel;
        ClipboardService m_ClipboardService;
        LibraryItemUIModel m_Target;
        List<ContextMenu.ActionArgs> m_ActionArgs;
        
        ContextMenu.ActionArgs m_PasteAndReplaceAction;
        ContextMenu.ActionArgs m_PasteAction;
        ContextMenu.ActionArgs m_DeleteAction;
        ContextMenu.ActionArgs m_UsePromptAction;
        readonly ContextMenu.ActionArgs m_CopyAction;
        readonly ContextMenu.ActionArgs m_DuplicateAction;
        readonly ContextMenu.ActionArgs m_ExportAction;
        readonly ContextMenu.ActionArgs m_ExportToFbxAction;
        readonly ContextMenu.ActionArgs m_RenameAction;

        /// <summary>
        /// A contextual menu for a given library item.
        /// </summary>
        public LibraryItemContextualMenu()
        {
            // TODO: Keyboard shortcuts
            // Note: Disabling keyboard shortcuts until handled correctly, since these are shared with other menus and causing conflicts.
            
            // Actions that can be used in the Library Items Contextual menus
            m_DuplicateAction = CreateAction(ActionType.Duplicate, k_DuplicateLabel, k_DuplicateIcon, GetCommandLabel(k_DuplicateShortcut));
            m_DeleteAction = CreateAction(ActionType.Delete, k_DeleteLabel, k_DeleteIcon, k_DeleteShortcut);
            m_UsePromptAction = CreateAction(ActionType.UsePrompt, k_UsePromptLabel, "", "");
            m_ExportAction = CreateAction(ActionType.Export, k_ExportLabel, "", "");
            m_RenameAction = CreateAction(ActionType.Rename, k_RenameLabel, "", "");
            m_RenameAction.IsClickable = true;
#if UNITY_MUSE_ANIMATE_ENABLE_FBX_EXPORT
            m_ExportToFbxAction = CreateAction(ActionType.ExportToFbx, k_ExportToFbxLabel, "", "");            
#endif
            
            // TODO: Investigate if copy/paste is necessary if we have duplicate already
            // Note: For now, copy and paste actions are not used, but will be implemented later.
            m_CopyAction = CreateAction(ActionType.Copy, k_CopyLabel, k_CopyIcon, GetCommandLabel(k_CopyShortcut));
            m_PasteAndReplaceAction = CreateAction(ActionType.PasteAndReplace, k_PasteAndReplaceLabel, k_PasteAndReplaceIcon, GetCommandLabel(k_PasteAndReplaceShortcut));
            m_PasteAction = CreateAction(ActionType.Paste, k_PasteLabel, k_PasteIcon, GetCommandLabel(k_PasteShortcut));

            // Note: The list is populated in Open() bellow
            m_ActionArgs = new List<ContextMenu.ActionArgs>();
        }

        /// <summary>
        /// Opens the contextual menu for a given Library item.
        /// </summary>
        /// <param name="library">The library targeted by the menu.</param>
        /// <param name="clipboardService">The clipboard service used by the menu.</param>
        /// <param name="selectionModel">The selection model of the menu.</param>
        /// <param name="targetItem">The library item for which the context menu is opened.</param>
        /// <param name="anchor">An anchor for the contextual menu.</param>
        public void Open(
            ClipboardService clipboardService,
            SelectionModel<LibraryItemAsset> selectionModel,
            LibraryItemUIModel targetItem,
            VisualElement anchor)
        {
            m_Target = targetItem;
            m_ClipboardService = clipboardService;
            m_SelectionModel = selectionModel;

            // Enable / Disable actions
            var canPaste = m_ClipboardService.CanPaste(m_Target.Model);
            m_PasteAndReplaceAction.IsClickable = canPaste;
            m_PasteAction.IsClickable = canPaste;
            m_UsePromptAction.IsClickable = m_Target.Model is TextToMotionTake;
            
            // Populate the menu
            m_ActionArgs.Clear();

            if (m_Target.Model is TextToMotionTake)
                m_ActionArgs.Add(m_UsePromptAction);

            m_ActionArgs.Add(m_ExportAction);
#if UNITY_MUSE_ANIMATE_ENABLE_FBX_EXPORT
            m_ActionArgs.Add(m_ExportToFbxAction);
#endif
            m_ActionArgs.Add(m_DuplicateAction);
            m_ActionArgs.Add(m_RenameAction);
            m_ActionArgs.Add(m_DeleteAction);

            // TODO: Investigate if copy/paste is necessary if we have duplicate already
            /*
            m_ActionArgs.Add(m_CopyAction);
            m_ActionArgs.Add(m_PasteAction);
            m_ActionArgs.Add(m_PasteAndReplaceAction);
            */
            
            // Update the item
            m_Target.IsInContextualMenu = true;
            
            // Open the menu
            ContextMenu.OpenContextMenu(anchor, m_ActionArgs, OnMenuDismissed);
        }

        void OnMenuDismissed(DismissType dismissType)
        {
            m_Target.IsInContextualMenu = false;
        }

        ContextMenu.ActionArgs CreateAction(ActionType actionType, string label, string icon, string shortcut)
        {
            return new ContextMenu.ActionArgs((int)actionType, label, icon, () => { InvokeMenuAction(actionType); }, shortcut);
        }

        void InvokeMenuAction(ActionType type)
        {
            int itemIndex = -1;

            if (m_SelectionModel.HasSelection)
            {
                itemIndex = LibraryRegistry.IndexOf(m_SelectionModel.GetSelection(0));
            }

            DeepPoseAnalytics.SendKeyAction(Enum.GetName(typeof(ActionType), type), itemIndex);
            OnMenuAction?.Invoke(type, m_ClipboardService, m_SelectionModel, m_Target);
        }
    }
}
