using System;
using UnityEngine;
using Unity.Muse.AppUI.UI;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct TutorialTrackStepData
    {
        public int Index;
        public string StepId;
        public string Body;
        public string Title;
        public string TargetUI;
        public bool ShowAcceptButton;
        public bool ShowDismissButton;
        public bool ShowSkipButton;
        public PopoverPlacement Placement;
        public bool UseMask;
        public Rect MaskRect;
        public float MaskRadius;
        public bool UseNormalizedMaskRect;
        public bool MaskIsTargetingUI;
        public bool HidePopoverArrow;
        public string AcceptButtonLabel;
        public string DismissButtonLabel;
        public string SkipButtonLabel;
        public string NextButtonLabel;
        public string PreviousButtonLabel;
        public Texture2D BackgroundImage;
    }
}
