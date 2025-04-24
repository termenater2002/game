using Unity.Muse.Sprite.Common.Events;

namespace Unity.Muse.StyleTrainer.Events.StyleModelEditorUIEvents
{
    class ThumbnailSizeChangedEvent : BaseEvent<ThumbnailSizeChangedEvent>
    {
        public float thumbnailSize;
    }

    class CheckPointSelectionChangeEvent : BaseEvent<CheckPointSelectionChangeEvent>
    {
        public StyleData styleData;
        public int index;
    }

    class GenerateButtonClickEvent : BaseEvent<GenerateButtonClickEvent> { }

    class DuplicateButtonClickEvent : BaseEvent<DuplicateButtonClickEvent> { }

    class ChangeStyleNameEvent : BaseEvent<ChangeStyleNameEvent>
    {
        public string newStyleName;
    }

    class SetFavouriteCheckPointEvent : BaseEvent<SetFavouriteCheckPointEvent>
    {
        public StyleData styleData;
        public string checkPointGUID;
    }

    class FavoritePreviewSampleOutputEvent : BaseEvent<FavoritePreviewSampleOutputEvent>
    {
        public CheckPointData checkPointData;
        public SampleOutputData favoriteSampleOutputData;
    }

    class ChooseRoundsButtonClickEvent : BaseEvent<ChooseRoundsButtonClickEvent> { }

    class SeeTrainedStyleEvent : BaseEvent<SeeTrainedStyleEvent> { }

    class AddImagesToTrainingSetEvent : BaseEvent<AddImagesToTrainingSetEvent>{}
}
