using UnityEngine;

namespace Unity.Muse.Chat.UI.Utils
{
    interface IContextReferenceVisualElement
    {
        void RefreshVisualElement(Object activeTargetObject, Component activeTargetComponent);
    }
}
