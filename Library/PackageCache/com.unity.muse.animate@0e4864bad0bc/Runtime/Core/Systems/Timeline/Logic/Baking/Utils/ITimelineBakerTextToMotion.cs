namespace Unity.Muse.Animate
{
    interface ITimelineBakerTextToMotion
    {
        public enum Model
        {
            V1,
            V2
        }

        public string Prompt { get; set; }
        
        /// <summary>
        /// The seed to use for this generation. If null, a random seed will be used.
        /// Once baking is complete, this will be updated with the actual seed used for the generation.
        /// </summary>
        public int? Seed { get; set; }
        
        
        /// <summary>
        /// The temperature to use for this generation. If null, the default temperature will be used.
        /// Once baking is complete, this will be updated with the actual temperature used for the generation.
        /// </summary>
        public float? Temperature { get; set; }

        public int Length { get; set; }

        public Model ModelType { get; set; }
    }
}
