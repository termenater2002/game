using System;
using Unity.Muse.AppUI.UI;

namespace Unity.Muse.Common.Account
{
    class RequestSeatDialog : AccountDialog
    {
        public RequestSeatDialog()
        {
            AddToClassList("muse-subscription-dialog-request-seat");

            dialogDescription.Add(new Text {text = TextContent.requestSeatTitle, name="muse-description-title"});
            dialogDescription.Add(new Text {text = TextContent.requestSeatDescription, name="muse-description-secondary", enableRichText = true});

            AddPrimaryButton(TextContent.requestSeatAccept, AccountLinks.RequestSeat);
        }
    }
}
