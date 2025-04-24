namespace Unity.Muse.Sprite.UIComponents
{
    static class TextContent
    {
        public static readonly string doodleStartTooltip = "Draw a shape or drag & drop reference image here.";
        public static readonly string doodleBrushTooltip = "Scribble Tool. (B)\n'[' or ']' decreases/increases brush size.";
        public static readonly string doodleEraserTooltip = "Erases the scribbles. (E)\n'[' or ']' decreases/increases brush size.";
        public static readonly string doodleBrushSizeTooltip = "Sets the brush size.";
        public static readonly string doodleTooltipDisabled = "Clear reference image to use Scribble/Eraser Tools";
        public static readonly string doodleClearTooltip = "Clear Tool clears all the scribbles and/or image reference.";
        public static readonly string doodleSelectorTooltip = "Sprite Picker Tool picks sprites from the Scene view or the Project window.";
        public static readonly string styleSelectionTooltip = "Style used for generation. Train new styles in Menu > Muse > Style Trainer.";

        public static readonly string operatorStyleTooltip = "Influences the style of the generation.";
        public static readonly string operatorStrengthTooltip = "Determines how much influence the selected style and prompts have on the results. The higher the value, the closer the results will look like the selected style and prompts.";
        public static readonly string operatorTightnessTooltip = "Determines how closely the generation follows the outline of the scribble or image reference. The higher the value, the closer the generations take the shape of the reference outlines.";
        public static readonly string operatorRemoveBackgroundTooltip = "Removes the background from the selected image.";
        public static readonly string operatorSeedTooltip = "Sets a seed number used to generate the sprites.";

        public static readonly string controlMaskEraserToolTooltip = "Erases parts of the masks.";
        public static readonly string controlMaskClearToolTooltip = "Clears all the masks.";

        public static readonly string backButtonTooltip = "Snaps back the Generations panel, hides the canvas, and exits the Refine mode.";
        public static readonly string customSeed = "Custom Seed";
        public static readonly string randomSeed = "Random Seed";
        public static readonly string styleUndefined = "Style Not Defined";
        public static readonly string style = "Style";
        public static readonly string styleStrength = "Style Strength";
        public static readonly string removeBackground = "Remove Background";
    }
}