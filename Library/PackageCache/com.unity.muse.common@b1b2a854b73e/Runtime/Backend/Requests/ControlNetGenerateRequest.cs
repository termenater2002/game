using System;
using UnityEngine;

namespace Unity.Muse.Common
{
    [Serializable]
    class ControlNetGenerateRequest: TextToImageItemRequest
    {
        public string guid;

        public string canny_base64;

        public float controlnet_conditioning_scale;
        public string control_color;

        public ControlNetGenerateRequest(string sourceGuid, string sourceBase64, string prompt, string controlColor, ImageVariationSettingsRequest settings)
            : base(prompt, (TextToImageRequest)settings)
        {
            guid = sourceGuid;
            canny_base64 = sourceBase64;
            controlnet_conditioning_scale = settings.strength;
            control_color = controlColor;
        }
    }
}