using System;
using Unity.Muse.Common.Analytics;
#if ENABLE_UNITYENGINE_ANALITICS
using UnityEngine.Analytics;
#endif

namespace Unity.Muse.Sprite.Analytics
{
    [Serializable]
    class GenerateAnalyticsData : IAnalytic.IData
    {
        public const string eventName = "muse_spriteTool_generate";
        public const int version = 1;
#if !ENABLE_UNITYENGINE_ANALITICS
        public string EventName => eventName;
        public int Version => version;
#endif

        public string prompt;
        public string prompt_negative;
        public bool inpainting_used;
        public int images_generated_nr;
        public bool reference_image_used;
        public bool is_doodle;
    }

#if ENABLE_UNITYENGINE_ANALITICS
    [AnalyticInfo(eventName: GenerateAnalyticsData.eventName, vendorKey: AnalyticsManager.vendorKey, GenerateAnalyticsData.version, AnalyticsManager.maxEventsPerHour)]
#endif
    class GenerateAnalytic : IAnalytic
    {
        string m_Prompt;
        string m_PromptNegative;
        bool m_InpaintingUsed;
        int m_ImagesGeneratedNr;
        bool m_ReferenceImageUsed;
        bool m_IsDoodle;

        internal GenerateAnalytic(string prompt, string promptNegative, bool inpaintingUsed, int imagesGeneratedNr, bool referenceImageUsed, bool isDoodle)
        {
            m_Prompt = prompt;
            m_PromptNegative = promptNegative;
            m_InpaintingUsed = inpaintingUsed;
            m_ImagesGeneratedNr = imagesGeneratedNr;
            m_ReferenceImageUsed = referenceImageUsed;
            m_IsDoodle = isDoodle;
        }

        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            var parameters = new GenerateAnalyticsData
            {
                prompt = m_Prompt,
                prompt_negative = m_PromptNegative,
                inpainting_used = m_InpaintingUsed,
                images_generated_nr = m_ImagesGeneratedNr,
                reference_image_used = m_ReferenceImageUsed,
                is_doodle = m_IsDoodle
            };
            data = parameters;
            return data != null;
        }
    }
}
