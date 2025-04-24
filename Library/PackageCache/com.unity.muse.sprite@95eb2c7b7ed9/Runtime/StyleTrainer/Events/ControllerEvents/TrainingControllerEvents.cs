using Unity.Muse.Sprite.Common.Events;

namespace Unity.Muse.StyleTrainer.Events.TrainingControllerEvents
{
    enum ETrainingState
    {
        Validation,
        CreateStyle,
        CreateTrainingSet,
        CreateCheckPoint,
        CheckPointTraining,
        TrainingDone
    }

    class StyleTrainingEvent : BaseEvent<StyleTrainingEvent>
    {
        public EState state;
        public StyleData styleData;
        public ETrainingState trainingState;
    }
}