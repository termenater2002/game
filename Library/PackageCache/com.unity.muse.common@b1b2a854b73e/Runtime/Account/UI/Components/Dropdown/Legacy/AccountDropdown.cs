using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Common
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class AccountDropdown : VisualElement
    {
#if ENABLE_UXML_TRAITS
        internal new class UxmlFactory : UxmlFactory<AccountDropdown, UxmlTraits> { }
#endif

        public AccountDropdown()
        {
           Add(new Button(() => ShowMuseAccountSettings(this))
            {
                name = "muse-account-dropdown",
                title = TextContent.museTitle,
                leadingIcon = "muse-logo",
                trailingIcon = "caret-down--fill"
            });
        }

        public static void ShowMuseAccountSettings(VisualElement parent)
        {
            Popover modal = null;

            var content = new AccountDropdownContent {OnAction = () =>
            {
                modal?.Dismiss();
                modal = null;
            }};
            content.AddToClassList("muse-account-settings-dropdown");

            modal = Popover.Build(parent, content);
            modal.SetAnchor(parent);
            modal.SetPlacement(PopoverPlacement.Bottom);
            modal.SetArrowVisible(false);

            modal.Show();
        }
    }
}
