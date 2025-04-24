using System;
using Unity.Muse.AppUI.UI;

namespace Unity.Muse.Common.Account
{
    class SubscriptionStartedDialog : AccountDialog
    {
        public Action OnAccept;

        public SubscriptionStartedDialog()
        {
            AddToClassList("muse-subscription-dialog-started");

            dialogDescription.Add(new Text {text = TextContent.subStartedTitle, name = "muse-description-title"});
            var descriptionStart = new Text {text = TextContent.subStartedDesc1, name = "muse-description-primary"};
            descriptionStart.AddToClassList("muse-description-content-start");
            dialogDescription.Add(descriptionStart);
            var descriptionEnd = new Text {text = TextContent.subStartedDesc2, name = "muse-description-secondary"};
            descriptionEnd.AddToClassList("muse-description-content-end");
            dialogDescription.Add(descriptionEnd);
            dialogDescription.Add(new Text {text = TextContent.subStartedDesc3, name = "muse-description-primary"});

            AddCancelButton(TextContent.subViewPlan, AccountLinks.ViewPricing);
            AddPrimaryButton(TextContent.subStartTrial, () => OnAccept?.Invoke());
        }
    }
}
