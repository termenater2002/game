using System;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents the TextToMotionTake Authoring Model of the Application.
    /// </summary>
    /// <remarks>
    /// Handles requests for actions related to Authoring a <see cref="TextToMotionTake"/>.
    /// </remarks>
    class TextToMotionAuthoringModel
    {
        public enum Property
        {
            Target,
            RequestPrompt,
            RequestSeed,
            RequestTakesAmount,
            RequestTakesCounter,
            RequestDuration,
            RequestModel,
        }

        int? m_RequestSeed;
        string m_RequestPrompt;
        int m_RequestTakesAmount = 1;
        int m_RequestTakesCounter;
        float m_RequestDuration = 3f;
        ITimelineBakerTextToMotion.Model m_RequestModel = ITimelineBakerTextToMotion.Model.V2;
        TextToMotionTake m_Target;

        public event Action<Property> OnChanged;
        public event Action<TextToMotionTake> OnRequestShuffle;
        public event Action<TextToMotionTake> OnRequestExtractKeys;
        public event Action<TextToMotionTake> OnRequestExport;
        public event Action<TextToMotionTake> OnRequestDelete;

        public TextToMotionTake Target
        {
            get => m_Target;
            set
            {
                if (m_Target == value)
                    return;

                m_Target = value;
                OnChanged?.Invoke(Property.Target);
            }
        }

        public int RequestTakesAmount
        {
            get => m_RequestTakesAmount;
            set
            {
                if (m_RequestTakesAmount == value)
                    return;

                m_RequestTakesAmount = value;
                OnChanged?.Invoke(Property.RequestTakesAmount);
            }
        }

        public int RequestTakesCounter
        {
            get => m_RequestTakesCounter;
            set
            {
                if (m_RequestTakesCounter == value)
                    return;

                m_RequestTakesCounter = value;
                OnChanged?.Invoke(Property.RequestTakesCounter);
            }
        }

        public string RequestPrompt
        {
            get => m_RequestPrompt;
            set
            {
                if (m_RequestPrompt == value)
                    return;

                m_RequestPrompt = value;
                OnChanged?.Invoke(Property.RequestPrompt);
            }
        }

        public float RequestDuration
        {
            get => m_RequestDuration;
            set
            {
                if (m_RequestDuration.NearlyEquals(value, 1e-4f))
                    return;

                m_RequestDuration = value;
                OnChanged?.Invoke(Property.RequestDuration);
            }
        }

        public ITimelineBakerTextToMotion.Model RequestModel
        {
            get => m_RequestModel;
            set
            {
                if (m_RequestModel == value)
                    return;

                m_RequestModel = value;
                OnChanged?.Invoke(Property.RequestModel);
            }
        }

        public int? RequestSeed
        {
            get => m_RequestSeed;
            set
            {
                if (m_RequestSeed == value)
                    return;

                m_RequestSeed = value;
                OnChanged?.Invoke(Property.RequestSeed);
            }
        }

        public void RequestShuffle()
        {
            OnRequestShuffle?.Invoke(Target);
        }

        public void RequestExport()
        {
            OnRequestExport?.Invoke(Target);
        }

        public void RequestExtractKeys()
        {
            OnRequestExtractKeys?.Invoke(Target);
        }

        public void RequestDelete()
        {
            OnRequestDelete?.Invoke(Target);
        }
    }
}
