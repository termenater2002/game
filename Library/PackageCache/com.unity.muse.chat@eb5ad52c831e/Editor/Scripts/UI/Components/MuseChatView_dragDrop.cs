using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components
{
    partial class MuseChatView
    {
        VisualElement m_DropZoneRoot;
        ChatDropZone m_DropZone;

        void OnDropped(IEnumerable<object> obj)
        {
            bool anyAdded = false;

            foreach (object droppedObject in obj)
            {
                if (AddContextFromDraggedObject(droppedObject))
                {
                    anyAdded = true;
                }
            }

            if (anyAdded)
            {
                UpdateContextSelectionElements(true);
            }

            m_DropZone.SetDropZoneActive(false);

            m_DropZone.SetDropZoneActive(false);
        }

        bool IsSupportedAsset(Object unityObject)
        {
            if (unityObject is DefaultAsset)
                return false;

            return true;
        }
    }
}
