using System;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Common.Account
{
    class SizeToContentButton : VisualElement
    {
        public Button button;

        public SizeToContentButton(Action action = null, bool inline = false)
        {
            AddToClassList("button-row-container");
            button = new(action);
            if (inline)
            {
                button.AddToClassList("inline-style");
                AccountDialog.SetQuiet(button);
            }
            Add(button);
        }
    }
}
