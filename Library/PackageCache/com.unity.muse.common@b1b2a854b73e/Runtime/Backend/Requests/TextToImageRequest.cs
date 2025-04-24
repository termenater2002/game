using System;
using System.Collections.Generic;

namespace Unity.Muse.Common
{
    [Serializable]
    struct TextToImageRequest
    {
        public string negative_prompt;
        public bool seamless;
        public uint seed;
        public int model;
        public uint width;
        public uint height;
        public float strength;
        public bool strength_normalized_on_ui;

        public TextToImageRequest(string negative_prompt, bool seamless, uint seed, int model, uint width, uint height, float strength)
        {
            this.negative_prompt = negative_prompt;
            this.seamless = seamless;
            this.seed = seed;
            this.model = model;
            this.width = width;
            this.height = height;
            this.strength = strength;
            strength_normalized_on_ui = false; //Should be set to false indicating that the transformations are occuring srv side
        }

        public static explicit operator TextToImageRequest(ImageVariationSettingsRequest settings)
        {
            return new TextToImageRequest(settings.negative_prompt, settings.seamless, settings.seed, settings.model, settings.width, settings.height, settings.strength);
        }
    }
}
