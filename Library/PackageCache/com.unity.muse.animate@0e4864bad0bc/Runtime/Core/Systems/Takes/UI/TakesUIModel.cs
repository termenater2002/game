using System;

namespace Unity.Muse.Animate
{
    class TakesUIModel
    {
        public enum Property
        {
            Visibility,
            IsWriting,
            Prompt,
            Seed,
            TakesAmount,
            TakesCounter,
            IsBakingTextToMotion,
            Duration
        }
        
        public event Action<Property> OnChanged;
        public event Action OnRequestedGenerate;

        AuthoringModel AuthoringModel => m_ApplicationContext.AuthorContext.Authoring;
        
        ApplicationContext m_ApplicationContext;
        
        bool m_IsVisible;
        bool m_IsWriting;
        bool m_IsBakingTextToMotion;

        internal ApplicationContext ApplicationContext => m_ApplicationContext;
        
        public string Prompt
        {
            get => AuthoringModel.TextToMotion.RequestPrompt;
            set => AuthoringModel.TextToMotion.RequestPrompt = value;
        }
        
        public int? Seed
        {
            get => AuthoringModel.TextToMotion.RequestSeed;
            set => AuthoringModel.TextToMotion.RequestSeed = value;
        }
        
        public int TakesAmount
        {
            get =>  AuthoringModel.TextToMotion.RequestTakesAmount;
            set => AuthoringModel.TextToMotion.RequestTakesAmount = value;
        }
        
        public int TakesCounter
        {
            get => AuthoringModel.TextToMotion.RequestTakesCounter;
            set => AuthoringModel.TextToMotion.RequestTakesCounter = value;
        }

        public float Duration
        {
            get => AuthoringModel.TextToMotion.RequestDuration;
            set => AuthoringModel.TextToMotion.RequestDuration = value;
        }

        public ITimelineBakerTextToMotion.Model InferenceModel
        {
            get => AuthoringModel.TextToMotion.RequestModel;
            set => AuthoringModel.TextToMotion.RequestModel = value;
        }
        
        public bool IsVisible
        {
            get => m_IsVisible;
            set
            {
                if (value == m_IsVisible)
                    return;

                m_IsVisible = value;
                OnChanged?.Invoke(Property.Visibility);
            }
        }
        
        public bool IsWriting 
        { 
            get => m_IsWriting;
            set
            {
                if (value == m_IsWriting)
                    return;

                m_IsWriting = value;
                OnChanged?.Invoke(Property.IsWriting);
            }
        }
        
        public bool IsBakingTextToMotion { get => m_IsBakingTextToMotion;
            set
            {
                if (value == m_IsBakingTextToMotion)
                    return;

                m_IsBakingTextToMotion = value;
                OnChanged?.Invoke(Property.IsBakingTextToMotion);
            }
        }
        
        public bool IsBusy => IsBakingTextToMotion;
        
        public TakesUIModel()
        {
            
        }
        
        internal void SetContext(ApplicationContext context)
        {
            if (m_ApplicationContext != null)
            {
                Unsubscribe();
            }
                
            m_ApplicationContext = context;
                
            if (m_ApplicationContext != null)
            {
                Subscribe();
            }
        }

        void Subscribe()
        {
            if (m_ApplicationContext == null)
                return;

            AuthoringModel.TextToMotion.OnChanged += OnModelChanged;
        }
        
        void Unsubscribe()
        {
            if (m_ApplicationContext == null)
                return;

            AuthoringModel.TextToMotion.OnChanged -= OnModelChanged;
        }
        
        void OnModelChanged(TextToMotionAuthoringModel.Property property)
        {
            switch (property)
            {
                case TextToMotionAuthoringModel.Property.RequestPrompt:
                    OnChanged?.Invoke(Property.Prompt);
                    break;
                case TextToMotionAuthoringModel.Property.RequestSeed:
                    OnChanged?.Invoke(Property.Seed);
                    break;
                case TextToMotionAuthoringModel.Property.RequestTakesAmount:
                    OnChanged?.Invoke(Property.TakesAmount);
                    break;
                case TextToMotionAuthoringModel.Property.RequestTakesCounter:
                    OnChanged?.Invoke(Property.TakesCounter);
                    break;
                case TextToMotionAuthoringModel.Property.RequestDuration:
                    OnChanged?.Invoke(Property.Duration);
                    break;
            }
        }
        
        public void RequestGenerate()
        {
            OnRequestedGenerate?.Invoke();
        }
    }
}
