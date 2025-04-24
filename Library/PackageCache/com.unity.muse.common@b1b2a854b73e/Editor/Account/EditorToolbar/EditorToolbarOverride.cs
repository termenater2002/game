#if MUSE_TOOLBAR_BUTTON_ENABLED
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Common.Account.UI.Components.Dropdown.EditorToolbar
{
    [InitializeOnLoad]
    public class EditorToolbarOverride
    {
        static EditorToolbarOverride()
        {
            UnityEditor.Toolbars.MuseDropdown.OnDropdownOpened = (rect, visualElement) =>
                AccountDropdownWindow.ShowMuseAccountSettingsAsPopup(rect);
        }
    }
}
#endif
