using System;

namespace Unity.Muse.Common
{
    [Serializable]
    struct ImageVariationSettingsRequest
    {
        public string negative_prompt;
        public bool seamless;
        public uint seed;
        public int model;
        public uint width;
        public uint height;
        public float strength;
        public bool strength_normalized_on_ui;

        /// <summary>
        ///
        /// </summary>
        /// <param name="negativePrompt"></param>
        /// <param name="seamless"></param>
        /// <param name="seed"></param>
        /// <param name="model"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="strength"></param>
        public ImageVariationSettingsRequest(string negativePrompt, bool seamless, uint seed, int model, uint width, uint height, float strength)
        {
            negative_prompt = negativePrompt;
            this.seamless = seamless;
            this.seed = seed;
            this.model = model;
            this.width = width;
            this.height = height;
            this.strength = strength;
            strength_normalized_on_ui = false; //Should be set to false indicating that the transformations are occuring srv side
        }
    }
}
