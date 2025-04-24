using Unity.AppUI.Core;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    static class ToastExtensions
    {
        const int k_DismissAction = 0;
        public static void OpenToast(this VisualElement ve, string message, NotificationStyle style, NotificationDuration duration, AnimationMode animationMode)
        {
            var toast = Toast.Build(ve, message, duration)
                .SetStyle(style)
                .SetAnimationMode(animationMode);

            if (style == NotificationStyle.Informative)
                toast.SetIcon("info");

            if (duration == NotificationDuration.Indefinite)
                toast.AddAction(k_DismissAction, "Dismiss", _ => Debug.Log("Dismissed"));

            toast.Show();
        }
    }
}
