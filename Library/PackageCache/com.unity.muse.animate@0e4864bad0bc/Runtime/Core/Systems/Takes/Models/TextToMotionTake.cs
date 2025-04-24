using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// A take containing dense motion, including information that may be used to regenerate the motion.
    /// </summary>
    [Serializable]
    class TextToMotionTake : DenseTake, ICopyable<TextToMotionTake>
    {
        [SerializeField]
        int m_Seed;

        [SerializeField]
        float m_Temperature;

        [SerializeField]
        int m_Length;

        [SerializeField]
        string m_Prompt;

        [SerializeField]
        ITimelineBakerTextToMotion.Model m_Model;

        public int Seed
        {
            get => m_Seed;
            internal set => m_Seed = value;
        }

        public float Temperature
        {
            get => m_Temperature;
            internal set => m_Temperature = value;
        }

        public int Length
        {
            get => m_Length;
            set => m_Length = value;
        }

        public ITimelineBakerTextToMotion.Model Model
        {
            get => m_Model;
            internal set => m_Model = value;
        }

        public string Prompt => m_Prompt;
        public int? RequestedSeed { get; }

        public float? RequestTemperature { get; }

        public TextToMotionTake(
            string prompt,
            int? seed,
            string title,
            string description,
            float? temperature,
            int length,
            ITimelineBakerTextToMotion.Model model)
            : base(
                title,
                description,
                LibraryItemType.TextToMotionTake,
                true)
        {
            m_Prompt = prompt;
            m_Length = length;
            m_Model = model;

            if (temperature != null)
                m_Temperature = (float)temperature;

            if (seed != null)
                m_Seed = (int)seed;

            RequestedSeed = seed;
            RequestTemperature = temperature;
        }

        public new TextToMotionTake Clone()
        {
            var clone = new TextToMotionTake(m_Prompt, m_Seed, Title, Description, m_Temperature, m_Length, m_Model);
            CopyTo(clone);
            return clone;
        }

        void ICopyable<TextToMotionTake>.CopyTo(TextToMotionTake item)
        {
            base.CopyTo(item);
            item.Length = m_Length;
            item.Model = m_Model;
            item.Temperature = m_Temperature;
            item.Seed = m_Seed;
        }

        public override string ToString() => m_Prompt;
    }
}
