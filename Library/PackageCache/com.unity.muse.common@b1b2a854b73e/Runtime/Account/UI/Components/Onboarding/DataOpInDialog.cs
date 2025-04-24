using System;
using Unity.Muse.AppUI.UI;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;
using Toggle = Unity.Muse.AppUI.UI.Toggle;

namespace Unity.Muse.Common.Account
{
    class DataOpInDialog : AccountDialog
    {
        public Action<bool> OnAccept;
        readonly Button m_PrimaryAction;
        Checkbox m_UsageOptIn;

        public DataOpInDialog()
        {
            AddToClassList("muse-subscription-dialog-data-opt-in");

            var header = new Text {text = TextContent.subDataTitle, name = "muse-description-title"};
            var description2 = new Text {text = TextContent.subDataDescription2, name = "muse-description-secondary", enableRichText = true};
            description2.AddToClassList("muse-description-section");
            dialogDescription.Add(header);

            var optInGroup = new VisualElement();
            optInGroup.AddToClassList("muse-opt-in-group");
            optInGroup.AddToClassList("muse-description-section");
            m_UsageOptIn = new Checkbox {value = CheckboxState.Checked};
            optInGroup.Add(m_UsageOptIn);
            var optInTextGroup = new VisualElement();
            var sectionTitle = new Text {text = TextContent.subDataLegalOptInTitle};
            sectionTitle.AddToClassList("muse-dialog-section-title");
            optInTextGroup.Add(sectionTitle);
            optInTextGroup.Add(new Text {text = TextContent.subDataLegalOptInMessage});
            optInGroup.Add(optInTextGroup);

            dialogDescription.Add(description2);
            dialogDescription.Add(optInGroup);

            m_PrimaryAction = AddPrimaryButton(TextContent.subDataClose, () => OnAccept?.Invoke(m_UsageOptIn.value == CheckboxState.Checked));
            AddCancelButton(TextContent.subDataReadPolicy, AccountLinks.PrivacyNotice);
        }

        public override void SetProcessing()
        {
            m_PrimaryAction.SetEnabled(false);
            m_UsageOptIn.SetEnabled(false);
        }
    }
}
