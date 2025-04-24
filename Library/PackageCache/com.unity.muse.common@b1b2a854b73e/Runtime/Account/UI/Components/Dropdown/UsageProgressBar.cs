using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    class UsageProgressBar : LinearProgress
    {
        public UsageProgressBar()
        {
            variant = Variant.Determinate;
            colorOverride = new Color(0.9215f, 0.2549f, 0.47843f);
            this.AddManipulator(new UsageTracker(Refresh));
        }

        void Refresh() => value = AccountInfo.Instance.Usage.Progress;
    }
}
