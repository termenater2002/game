using System;
using Unity.Muse.Common;
using UnityEngine;
using MuseArtifact = Unity.Muse.Common.Artifact;

namespace Unity.Muse.StyleTrainer
{
    //[CreateAssetMenu(fileName = "StyleTrainerConfig", menuName = "Muse/StyleTrainerConfig")]
    class StyleTrainerConfig : ScriptableObject
    {
        public int trainingSteps = 2000;
        public int minTrainingSetSize = 3;
        public int maxTrainingSetSize = 10;
        public int minSampleSetSize = 3;
        public int maxSampleSetSize = 5;
        public Vector2Int minTrainingImageSize = new(128, 128);
        public Vector2Int maxTrainingImageSize = new(512, 512);
        public bool debugLog;
        public bool logToFile;
        public ulong defaultStyleVersion = 1;
        public StyleData[] defaultStyles;
        public static StyleTrainerConfig config => ResourceManager.Load<StyleTrainerConfig>(PackageResources.styleTrainerConfig);

        StyleTrainerArtifactCache m_ArtifactCache;
        public string artifactCachePath =>
#if UNITY_EDITOR
            $"{ApplicationExtensions.museDbPath}/StyleTrainerCache.db";
#else
            $"{Application.persistentDataPath}/StyleTrainerCache.db";
#endif
        public StyleTrainerArtifactCache artifactCache
        {
            get
            {
                if (m_ArtifactCache == null)
                {
                    m_ArtifactCache = new StyleTrainerArtifactCache(artifactCachePath);
                }
                return m_ArtifactCache;
            }
        }

        void OnDisable()
        {
            if (m_ArtifactCache != null)
            {
                m_ArtifactCache.Dispose();
                m_ArtifactCache = null;
            }
        }
    }
}
