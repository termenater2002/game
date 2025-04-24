using System;
using System.ComponentModel;
using UnityEngine;

namespace Unity.Muse.Common
{
    [Serializable]
    class InpaintingItemRequest: TextToImageItemRequest
    {
        public string mask_image_b64;
        public string mask_type;
        public string guid;

        public InpaintingItemRequest(string prompt, string sourceGuid, Texture2D mask, MaskType masktype, TextToImageRequest settings)
            : base(prompt, settings)
        {
            mask_type = masktype.GetDescription();
            mask_image_b64 = Convert.ToBase64String(mask.EncodeToPNG());
            guid = sourceGuid;
        }
    }
    enum MaskType
    {
        [Description("user_defined")]
        UserDefined,
        [Description("inpaint_variation")]
        InpaintVariation
    }

    static class EnumHelper
     {
        public static string GetDescription(this Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
    }
}