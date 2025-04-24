using System;

namespace Unity.Muse.Common
{
    [Serializable]
    class ImageVariationBase64Request : TextToImageItemRequest
    {
        public string image_base64;

        public ImageVariationBase64Request(string imageB64, string prompt, ImageVariationSettingsRequest settings)
            : base(prompt, (TextToImageRequest)settings)
        {
            image_base64 = imageB64;
        }
    }
}
