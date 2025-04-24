using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using Button = Unity.Muse.AppUI.UI.Button;
using Toggle = Unity.Muse.AppUI.UI.Toggle;

namespace Unity.Muse.Common.Account
{
    class ThirdPartyDialog : AccountDialog
    {
        public Action<bool> OnAccept;
        readonly Button m_PrimaryAction;
        public ThirdPartyDialog(ModeStruct modeData)
        {
            AddToClassList("muse-thirdparty-dialog");

            var icon = this.Q<Image>(className:"muse-dialog-logo");
            var parent = icon.parent;
            icon.RemoveFromHierarchy();

            icon = new WebImage(modeData.icon_path){name = "muse-partner-logo"};
            parent.Add(icon);

            var header = new Text {text = $"Using " + modeData.icon_label, name = "muse-description-title"};

            var description2 = new Text {text = TextContent.thirdPartyLegalDisclaimer, name = "muse-description-secondary", enableRichText = true};
            description2.AddToClassList("muse-description-section");

            var optInGroup = new VisualElement{name="muse-dialog-description-group"};
            optInGroup.AddToClassList("muse-dialog-description-group");
            optInGroup.AddToClassList("muse-opt-in-group");
            optInGroup.AddToClassList("muse-description-section");
            var notProtectedInCourtOptin = new Toggle {value = true};
            optInGroup.Add(notProtectedInCourtOptin);
            optInGroup.Add(new Text {text = TextContent.thirdPartyNotProtectedInCourt, enableRichText = true});

            var optInGroup2 = new VisualElement{name="muse-dialog-description-group"};
            optInGroup2.AddToClassList("muse-dialog-description-group");
            optInGroup2.AddToClassList("muse-opt-in-group");
            optInGroup2.AddToClassList("muse-description-section");
            optInGroup2.style.justifyContent = Justify.FlexStart;
            var haveReadThirdPartyEulaOptin = new Toggle {value = true};
            optInGroup2.Add(haveReadThirdPartyEulaOptin);
            optInGroup2.Add(new Text {text = string.Format(TextContent.thirdPartyEulaRead, modeData.icon_label), enableRichText = true});
            optInGroup2.RegisterCallback<PointerDownLinkTagEvent>(e => Application.OpenURL(modeData.eula_url));

            dialogDescription.Add(icon);
            dialogDescription.Add(header);
            dialogDescription.Add(description2);
            dialogDescription.Add(optInGroup);
            dialogDescription.Add(optInGroup2);

            AddCancelButton(TextContent.thirdPartyLearnMore, () => Application.OpenURL(modeData.eula_url));
            AddPrimaryButton(TextContent.thirdPartyClose, () => OnAccept?.Invoke(false), false);
            m_PrimaryAction = AddPrimaryButton(TextContent.thirdPartyAccept, () => OnAccept?.Invoke(true), true);
            void CheckboxCallback(ChangeEvent<bool> _) =>
                m_PrimaryAction.SetEnabled(
                    notProtectedInCourtOptin.value &&
                    haveReadThirdPartyEulaOptin.value);

            CheckboxCallback(default);
            notProtectedInCourtOptin.RegisterValueChangedCallback(CheckboxCallback);
            haveReadThirdPartyEulaOptin.RegisterValueChangedCallback(CheckboxCallback);
        }
    }
}