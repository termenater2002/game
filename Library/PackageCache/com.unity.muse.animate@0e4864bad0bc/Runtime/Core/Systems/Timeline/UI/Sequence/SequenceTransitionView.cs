using UnityEngine.UIElements;
using Unity.Muse.AppUI.UI;

namespace Unity.Muse.Animate
{
    class SequenceTransitionView : SequenceItemView<TimelineModel.SequenceTransition>
    {
        const string k_UssClassName = "deeppose-sequence-transition";
        const string k_UssClassNameSelected = "deeppose-sequence-item-selected";
        const string k_UssClassNameHighlighted = "deeppose-sequence-item-highlighted";
        const string k_UssClassNameEditing = "deeppose-sequence-item-editing";
        const string k_IconName = "caret-right";
        const string k_IconUssStyleName = "deeppose-sequence-transition-icon";

        public SequenceTransitionView() :
            base(k_UssClassName, k_UssClassNameSelected, k_UssClassNameHighlighted, k_UssClassNameEditing)
        {
            name = "sequence-transition";
            var iconElement = new Icon() {iconName = k_IconName, pickingMode = PickingMode.Ignore};
            iconElement.AddToClassList(k_IconUssStyleName);
            Add(iconElement);
        }
    }
}
