using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    class ActorContextualMenu
    {
        // Note: This end padding is a bit hacky, but it allows enough padding
        // to cleanly display keyboard shortcuts to the right of the menu items label
        const string k_EndPad = ContextMenu.endPad;

        const string k_CopyIcon = "";
        const string k_PasteIcon = "";
        const string k_ResetIcon = "";
        
        const string k_CopyLabel = "Copy Pose"+k_EndPad;
        const string k_PasteLabel = "Paste Pose"+k_EndPad;
        const string k_ResetLabel = "Reset Pose"+k_EndPad;
        
        // Note: Disabling keyboard shortcuts until handled correctly, since these are shared with other menus and causing conflicts.
        // const string k_CopyShortcut = "C";
        const string k_CopyShortcut = "";
        // const string k_PasteShortcut = "V";
        const string k_PasteShortcut = "";
        // const string k_ResetShortcut = "R";
        const string k_ResetShortcut = "";

        public Action<ActionType, ClipboardService, AuthoringModel, EntityKeyModel> OnMenuAction;

        public enum ActionType
        {
            CopyPose,
            PastePose,
            ResetPose
        }

        AuthoringModel m_AuthoringModel;
        ClipboardService m_ClipboardService;
        EntityView m_Target;

        List<ContextMenu.ActionArgs> m_ActionArgs;

        ContextMenu.ActionArgs m_CopyAction;
        ContextMenu.ActionArgs m_PasteAction;
        ContextMenu.ActionArgs m_ResetAction;
        CameraModel m_CameraModel;
        VisualElement m_UIRoot;
        Vector2 m_Position;
        EntityKeyModel m_TargetKey;

        /// <summary>
        /// A contextual menu for a given timeline's sequence key.
        /// </summary>
        public ActorContextualMenu()
        {
            // TODO: Keyboard shortcuts
            // Note: Disabling keyboard shortcuts until handled correctly, since these are shared with other menus and causing conflicts.
            
            m_CopyAction = CreateAction(ActionType.CopyPose, k_CopyLabel, k_CopyIcon, GetCmd(k_CopyShortcut));
            m_PasteAction = CreateAction(ActionType.PastePose, k_PasteLabel, k_PasteIcon, GetCmd(k_PasteShortcut));
            m_ResetAction = CreateAction(ActionType.ResetPose, k_ResetLabel, k_ResetIcon, GetCmd(k_ResetShortcut));
            
            m_ActionArgs = new List<ContextMenu.ActionArgs>
            {
                m_CopyAction,
                m_PasteAction,
                m_ResetAction
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
        /// Opens the contextual menu for a given actor.
        /// </summary>
        public void Open(AuthoringModel authoring, CameraModel cameraModel, VisualElement uiRoot, Vector2 position, ClipboardService clipboardService, EntityView target)
        {
            if (target == null)
                return;
            
            if (authoring.Timeline.ActiveKey == null)
                return;
            
            if (!authoring.Timeline.ActiveKey.Key.TryGetKey(target.EntityId, out var keyModel))
                return;
            
            
            m_Target = target;
            m_TargetKey = keyModel;
            m_AuthoringModel = authoring;
            m_ClipboardService = clipboardService;
            m_CameraModel = cameraModel;
            m_UIRoot = uiRoot;
            m_Position = position;

            // Enable / Disable actions
            m_PasteAction.IsClickable = m_ClipboardService.CanPaste(keyModel);
            m_CopyAction.IsClickable = true;
            m_ResetAction.IsClickable = true;
            
            // Open the menu from the camera
            m_CameraModel.OpenContextMenu(m_UIRoot, m_Position, m_ActionArgs);
            
        }

        ContextMenu.ActionArgs CreateAction(ActionType actionType, string label, string icon, string shortcut)
        {
            return new ContextMenu.ActionArgs((int)actionType, label, icon, () => { InvokeMenuAction(actionType); }, shortcut);
        }

        void InvokeMenuAction(ActionType type)
        {
            int keyIndex = -1;
            
            if (m_AuthoringModel.Timeline.ActiveKey != null)
            {
                keyIndex = m_AuthoringModel.Timeline.ActiveKey.Key.ListIndex;
            }

            DeepPoseAnalytics.SendKeyAction(Enum.GetName(typeof(ActionType), type), keyIndex);
            OnMenuAction?.Invoke(type, m_ClipboardService, m_AuthoringModel, m_TargetKey);
        }
    }
}
