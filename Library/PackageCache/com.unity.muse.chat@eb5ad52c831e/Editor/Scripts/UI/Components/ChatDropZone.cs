using System;
using System.Collections.Generic;
using Unity.Muse.Common.Utils;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components
{
    class ChatDropZone : ManagedTemplate
    {
        VisualElement m_DropZoneContent;
        Action<IEnumerable<object>> m_DropCallback;

        public ChatDropZone()
            : base(MuseChatConstants.UIModulePath)
        {
        }

        public void SetupDragDrop(VisualElement rootMain, Action<IEnumerable<object>> onDropped)
        {
            rootMain.RegisterCallback<DragEnterEvent>(OnDragEnter);
            rootMain.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            rootMain.RegisterCallback<DragPerformEvent>(OnDragPerform);
            rootMain.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
            rootMain.RegisterCallback<DragExitedEvent>(OnDragExit);

            m_DropCallback = onDropped;
        }

        protected override void InitializeView(TemplateContainer view)
        {
            m_DropZoneContent = view.Q<VisualElement>("chatDropZoneContent");
        }

        public void SetDropZoneActive(bool active)
        {
            this.SetDisplay(active);
        }

        void OnDragEnter(DragEnterEvent evt)
        {
            StartDraggingEvent();
        }

        void OnDragLeave(DragLeaveEvent evt)
        {
            Reset();
        }

        void OnDragUpdate(DragUpdatedEvent evt)
        {
            StartDraggingEvent();
        }

        void OnDragExit(DragExitedEvent evt)
        {
            Reset();
        }

        void OnDragPerform(DragPerformEvent evt)
        {
            if (!IsDraggingObjects())
            {
                Reset();
                return;
            }

            m_DropCallback?.Invoke(DragAndDrop.objectReferences);
        }

        void Reset()
        {
            SetDropZoneActive(false);
        }

        void StartDraggingEvent()
        {
            if (!IsDraggingObjects())
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            SetDropZoneActive(true);
        }

        bool IsDraggingObjects()
        {
            return DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0;
        }
    }
}
