using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    class EffectorsContextualMenu
    {
        // Note: This end padding is a bit hacky, but it allows enough padding
        // to cleanly display keyboard shortcuts to the right of the menu items label
        const string k_EndPad = ContextMenu.endPad;

        const string k_DisableAllIcon = "";
        const string k_DisablePositionIcon = "";
        const string k_DisableRotationIcon = "";
        const string k_DisableLookAtIcon = "";
        
        const string k_DisableAllLabel = "Disable Effector"+k_EndPad;
        const string k_DisablePositionLabel = "Disable Effector Position"+k_EndPad;
        const string k_DisableRotationLabel = "Disable Effector Rotation"+k_EndPad;
        const string k_DisableLookAtLabel = "Disable Effector Look At"+k_EndPad;
        
        // Note: Disabling keyboard shortcuts until handled correctly, since these are shared with other menus and causing conflicts.
        // const string k_DisableAllShortcut = "Delete";
        const string k_DisableAllShortcut = "";
        const string k_DisablePositionShortcut = "";
        const string k_DisableRotationShortcut = "";
        const string k_DisableLookAtShortcut = "";

        public Action<ActionType, AuthoringModel> OnMenuAction;

        public enum ActionType
        {
            DisableAll,
            DisablePosition,
            DisableRotation,
            DisableLookAt
        }

        AuthoringModel m_AuthoringModel;

        List<ContextMenu.ActionArgs> m_ActionArgs;

        ContextMenu.ActionArgs m_DisableAllAction;
        ContextMenu.ActionArgs m_DisablePositionAction;
        ContextMenu.ActionArgs m_DisableRotationAction;
        ContextMenu.ActionArgs m_DisableLookAtAction;
        VisualElement m_UIRoot;
        CameraModel m_Camera;
        Vector2 m_Position;

        /// <summary>
        /// A contextual menu for a given timeline's sequence key.
        /// </summary>
        public EffectorsContextualMenu()
        {
            // TODO: Keyboard shortcuts
            // Note: Disabling keyboard shortcuts until handled correctly, since these are shared with other menus and causing conflicts.
            
            m_DisableAllAction = CreateAction(ActionType.DisableAll, k_DisableAllLabel, k_DisableAllIcon, GetCmd(k_DisableAllShortcut));
            m_DisablePositionAction = CreateAction(ActionType.DisablePosition, k_DisablePositionLabel, k_DisablePositionIcon, GetCmd(k_DisablePositionShortcut));
            m_DisableRotationAction = CreateAction(ActionType.DisableRotation, k_DisableRotationLabel, k_DisableRotationIcon, GetCmd(k_DisableRotationShortcut));
            m_DisableLookAtAction = CreateAction(ActionType.DisableLookAt, k_DisableLookAtLabel, k_DisableLookAtIcon, GetCmd(k_DisableLookAtShortcut));
            
            m_ActionArgs = new List<ContextMenu.ActionArgs>
            {
                m_DisableAllAction,
                m_DisablePositionAction,
                m_DisableRotationAction,
                m_DisableLookAtAction
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
        /// Opens the contextual menu for a given effector.
        /// </summary>
        public void Open(AuthoringModel authoring, VisualElement uiRoot, CameraModel camera, Vector2 position)
        {
            m_AuthoringModel = authoring;
            m_UIRoot = uiRoot;
            m_Camera = camera;
            m_Position = position;
            var selectedEffectors = new List<DeepPoseEffectorModel>();
            authoring.PoseAuthoringLogic.GetSelectedEffectorModels(selectedEffectors);
            
            var usingPosition = false;
            var usingRotation = false;
            var usingLookAt = false;
            
            for (var i = 0; i < selectedEffectors.Count; ++i)
            {
                var effector = selectedEffectors[i];
                usingPosition = usingPosition || effector.PositionEnabled;
                usingRotation = usingRotation || effector.RotationEnabled;
                usingLookAt = usingLookAt || effector.LookAtEnabled;
            }

            // Enable / Disable actions
            m_DisableAllAction.IsClickable = usingPosition || usingRotation || usingLookAt;
            m_DisablePositionAction.IsClickable = usingPosition;
            m_DisableRotationAction.IsClickable = usingRotation;
            m_DisableLookAtAction.IsClickable = usingLookAt;
            
            // Open the menu
            camera.OpenContextMenu(uiRoot, position, m_ActionArgs);
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
            OnMenuAction?.Invoke(type, m_AuthoringModel);
        }
    }
}
