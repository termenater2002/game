using System;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// The UI of the TextToMotion workflow.
    /// </summary>
    class TextToMotionUIModel
    {
        public BakedTimelinePlaybackUIModel PlaybackUI => m_PlaybackUI;
        public event Action<Property> OnChanged;

        public enum Property
        {
            Title,
            Visibility,
            Prompt,
            TakesAmount,
            TakesCounter,
            Seed,
            IsBusy,
            IsBakingTextToMotion,
            CanExport,
            CanMakeEditable
        }

        public string Title
        {
            get => m_Title;
            set
            {
                if (value == m_Title)
                    return;

                m_Title = value;
                OnChanged?.Invoke(Property.Title);
            }
        }

        public string Prompt
        {
            get => m_Prompt;
            set
            {
                if (value == m_Prompt)
                    return;

                m_Prompt = value;
                OnChanged?.Invoke(Property.Prompt);
            }
        }

        public int Seed
        {
            get => m_Seed;
            set
            {
                if (value == m_Seed)
                    return;

                m_Seed = value;
                OnChanged?.Invoke(Property.Seed);
            }
        }
        
        public bool IsVisible
        {
            get => m_IsVisible;
            set
            {
                if (value == m_IsVisible)
                    return;

                m_IsVisible = value;
                m_PlaybackUI.IsVisible = m_IsVisible;
                OnChanged?.Invoke(Property.Visibility);
            }
        }

        public bool IsBakingCurrentTake
        {
            get => m_IsBakingCurrentTake;
            set
            {
                if (value == m_IsBakingCurrentTake)
                    return;

                m_IsBakingCurrentTake = value;
                OnChanged?.Invoke(Property.IsBakingTextToMotion);
            }
        }
        
        public bool CanExport
        {
            get => m_CanExport;
            set
            {
                if (value == m_CanExport)
                    return;

                m_CanExport = value;
                OnChanged?.Invoke(Property.CanExport);
            }
        }
        
        public bool CanMakeEditable
        {
            get => m_CanMakeEditable;
            set
            {
                if (value == m_CanMakeEditable)
                    return;

                m_CanMakeEditable = value;
                OnChanged?.Invoke(Property.CanMakeEditable);
            }
        }
        
        public bool IsBusy
        {
            get => m_IsBusy;
            set
            {
                if (value == m_IsBusy)
                    return;

                m_IsBusy = value;
                OnChanged?.Invoke(Property.IsBusy);
            }
        }

        AuthoringModel m_AuthoringModel;
        BakedTimelinePlaybackUIModel m_PlaybackUI;

        string m_Title;
        string m_Prompt;
        bool m_IsVisible;
        bool m_IsBusy;
        bool m_CanExport;
        bool m_CanMakeEditable;
        TextToMotionAuthoringModel m_Model;
        bool m_IsBakingCurrentTake;
        int m_TakesAmount;
        int m_TakesCounter;
        int m_Seed;

        public TextToMotionUIModel(TextToMotionAuthoringModel model)
        {
            m_Model = model;
            m_PlaybackUI = new BakedTimelinePlaybackUIModel();
        }

        public void RequestShuffle()
        {
            m_Model?.RequestShuffle();
        }

        public void RequestExport()
        {
            m_Model?.RequestExport();
        }

        public void RequestExtractKeys()
        {
            m_Model?.RequestExtractKeys();
        }

        public void RequestDelete()
        {
            m_Model?.RequestDelete();
        }
    }
}
