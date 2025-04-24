namespace Unity.Muse.StyleTrainer
{
    static class StringConstants
    {
        public const string newVersion = "New Version";

        public const string generateStyle = "Generate Style";
        public const string duplicateStyle = "Duplicate Style";
        public const string stopGenerate = "Stop Generate";

        public const string checkPointDropDownLabel = "Version {0}";
        public const string discardCheckPoint = "Discard Version";

        public const string sampleOutputTab = "Sample Output";
        public const string trainingSetTab = "Training Set";
        public const string styleNotTrained = "(Style is not trained)";
        public const string styleTrainedError = "(Style encountered error while training)";
        public const string styleError = "(Error loading Style)";
        public const string styleTraining = "(Style is training)";

        public const string styleUntrainedText = "Untrained";
        public const string styleTrainedText = "Trained";
        public const string styleErrorText = "Error";
        public const string styleTrainingText = "Training...";
        public const string stylePublishedText = "Published";
        public const string styleLoadingText = "Loading...";
        public const string styleInitialText = "Not Loaded";

        public const string promptsHintText = "These prompts will generate exemplars of each training cycle while your style is being trained.";
        public const string trainingImagesTitleUntrained = "Training Images";
        public const string trainingImagesTitleTrained = "Images used to train style";
        public const string trainingSelectedTrainingRound = "<line-height=120%>Selected Training Round";
        public const string trainingImagesLocked = "Locked";
        public const string trainingRoundDescription = "<line-height=120%>Style training goes through multiple training rounds, but the last round isn’t always best. You can choose which round you prefer and that round will be the quality of the style when the style is used.";
        public const string trainingChooseRoundButton = "Select Different Round";
        public const string trainingSelectRoundText = "Select training round";
        public const string trainingBackButton = "Back";

        public const string dragAndDropDescription = "<line-height=120%>Drag and drop images from project folder<br>Images must be 512x512 or they will be scaled and cropped";
        public const string styleDescriptionPlaceholder = "Set a description here to help you remember what this style is for.";

        public const string createNewStyleText = "Create a new style to begin";
        public const string createOrSelectStyleText = "Create a new style or select an existing style";
    }
}