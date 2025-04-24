#if UNITY_EDITOR
using System;
using Unity.Muse.AppUI.UI;
using UnityEditor;

namespace Unity.Muse.Common.Account
{
    class SignInDialog : AccountDialog
    {
        public SignInDialog()
        {
            AddToClassList("muse-subscription-dialog-signin");
            AddHeaderImage();
            dialogDescription.Add(new Text {text = TextContent.signinTitle, name="muse-description-title"});
            dialogDescription.Add(new Text {text = TextContent.subDescription1, name="muse-description-secondary", enableRichText = true});
            dialogDescription.AddToClassList("muse-description-section");

            AddCancelButton(TextContent.subViewPlan, AccountLinks.ViewPricing);
            var signIn = AddPrimaryButton(TextContent.signinAccept, () => { });
#if UNITY_EDITOR
            signIn.clicked += () => CloudProjectSettings.ShowLogin();
#endif
        }
    }
}
#endif