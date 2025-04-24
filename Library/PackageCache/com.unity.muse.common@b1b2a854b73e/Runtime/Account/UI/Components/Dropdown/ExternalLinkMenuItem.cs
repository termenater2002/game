using System;
using Unity.Muse.AppUI.UI;

namespace Unity.Muse.Common.Account
{
    class ExternalLinkMenuItem : MenuItem
    {
        public ExternalLinkMenuItem()
        {
            hierarchy.Add(new Icon
            {
                iconName = "arrow-square-out",
                size = IconSize.S
            });
        }
    }
}
