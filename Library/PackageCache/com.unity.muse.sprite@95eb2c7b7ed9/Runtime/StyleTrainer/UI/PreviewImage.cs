using System;
using Unity.Muse.Common;
using Unity.Muse.Common.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.StyleTrainer
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class PreviewImage : LoadableImage
    {
        ImageArtifact m_ImageArtifact;
        static int s_ImageCount = 0;
        const int k_MaxRequest = 3;
        int m_Tries = 0;
        const int k_MaxTries = 5;
        readonly Vector2Int k_DelayLoadMS = new(20,200) ;
        public PreviewImage()
            : base(false) { }

        public void SetArtifact(ImageArtifact artifact)
        {
            m_ImageArtifact = artifact;
            if (m_ImageArtifact == null)
            {
                OnLoaded(null, ImageDisplay.BackgroundImage);
                return;
            }

            OnImageArtifactDataChanged(artifact);
            if (m_ImageArtifact != null && !Utilities.ValidStringGUID(m_ImageArtifact.guid))
                m_ImageArtifact.OnGUIDChanged += OnImageArtifactDataChanged;
        }

        public void ShowLoading()
        {
            if(m_ImageArtifact == null)
                return;

            OnLoading();
        }

        public void ShowImage()
        {
            OnImageArtifactDataChanged(m_ImageArtifact);
        }

        void OnImageArtifactDataChanged(ImageArtifact imageArtifact)
        {
            if (imageArtifact == null)
            {
                OnLoaded(null, ImageDisplay.BackgroundImage);
                return;
            }

            OnLoading();
            var result = imageArtifact.GetLoaded();
            if (result.cached)
            {
                OnDoneCallback(result.texture);
            }
            else
            {
                if(s_ImageCount < k_MaxRequest)
                {
                    ++s_ImageCount;
                    imageArtifact.GetArtifact(OnDoneCallback, true);
                }
                else
                {
                    schedule.Execute(DelayLoad).StartingIn(UnityEngine.Random.Range(k_DelayLoadMS.x, k_DelayLoadMS.y));
                }
            }
        }

        void DelayLoad()
        {
            if (m_ImageArtifact == null)
                return;

            if (m_Tries > k_MaxTries && s_ImageCount >= k_MaxRequest)
            {
                s_ImageCount = 0;
            }

            if(s_ImageCount < k_MaxRequest || m_Tries > k_MaxTries)
            {
                ++s_ImageCount;
                m_ImageArtifact.GetArtifact(OnDoneCallback, true);
                m_Tries = 0;
            }
            else
            {
                ++m_Tries;
                schedule.Execute(DelayLoad).StartingIn(UnityEngine.Random.Range(k_DelayLoadMS.x, k_DelayLoadMS.y));
            }
        }

        void OnDoneCallback(Texture2D obj)
        {
            OnLoaded(obj, ImageDisplay.BackgroundImage);
        }

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<PreviewImage, UxmlTraits> { }
#endif
    }
}