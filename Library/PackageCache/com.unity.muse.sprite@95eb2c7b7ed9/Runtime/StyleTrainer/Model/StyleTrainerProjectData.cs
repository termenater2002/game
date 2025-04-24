#if UNITY_EDITOR
using Unity.Muse.Sprite.Common.Backend;
using UnityEditor;
#else
using Unity.Muse.StyleTrainer.EditorMockClass;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using StyleTrainer.Backend;
using Unity.Muse.StyleTrainer.Debug;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

namespace Unity.Muse.StyleTrainer
{
    [Serializable]
    [Preserve]
    [FilePath("ProjectSettings/StyleTrainerProjectData.asset", FilePathAttribute.Location.ProjectFolder)]
    class StyleTrainerProjectData : ScriptableSingleton<StyleTrainerProjectData>
    {
        [SerializeReference]
        StyleTrainerData m_StyleTrainerData;
        [FormerlySerializedAs("m_DefaultProjectData")]
        [SerializeReference]
        StyleTrainerData m_DefaultStyleData = new StyleTrainerData(EState.New);

        [SerializeField]
        ulong m_DefaultStyleVersion;
        [SerializeReference]
        List<StyleData> m_DefaultStyles;

        [SerializeField]
        [HideInInspector]
        string m_AssetPath;

        [SerializeField]
        List<string> m_PreviousProjectIDs = new();
        public string guid => m_StyleTrainerData?.guid;
        public event Action<StyleTrainerProjectData> onDataChanged = _ => { };

        RetrieveDefaultStyleTask m_RetrieveDefaultStyleTask;
        public string assetPath
        {
            set => m_AssetPath = value;
        }

        public void Save()
        {
#if UNITY_EDITOR
            StyleTrainerDebug.Log("Saving Asset");
            Save(true);
#else
            StyleTrainerDebug.LogWarning("No save implemetation for non-editor builds");
#endif
        }

        public StyleTrainerData data => m_StyleTrainerData;

        public IReadOnlyList<StyleData> GetDefaultStyles(Action<IReadOnlyList<StyleData>> onDone, Action onFailed, bool cache)
        {
            if (m_DefaultStyleData.state == EState.Loaded && cache)
            {
                var buildStyleList = GetDefaultStyles();
                onDone(buildStyleList);
            }
            else if (m_DefaultStyleData.state != EState.Loading || m_RetrieveDefaultStyleTask == null)
            {
                m_RetrieveDefaultStyleTask = new RetrieveDefaultStyleTask();
                m_RetrieveDefaultStyleTask.Execute(m_DefaultStyleData, () =>
                {
                    m_RetrieveDefaultStyleTask = null;

                    if (m_DefaultStyleData.state == EState.Error)
                        onFailed?.Invoke();
                    else
                        onDone(GetDefaultStyles());
                });
            }

            return builtInStyles;
        }

        IReadOnlyList<StyleData> GetDefaultStyles()
        {
            IReadOnlyList<StyleData> serverDefaultStyles = m_DefaultStyleData.styles.Where(s => s.state == EState.Loaded && s.visible && s.checkPoints != null && s.checkPoints.Any()).ToList();
            if (serverDefaultStyles.Count == 0)
                return builtInStyles;

            return serverDefaultStyles;
        }

        IReadOnlyList<StyleData> builtInStyles
        {
            get
            {
                if (m_DefaultStyleVersion != StyleTrainerConfig.config.defaultStyleVersion)
                {
                    m_DefaultStyleVersion = StyleTrainerConfig.config.defaultStyleVersion;
                    m_DefaultStyles = StyleTrainerConfig.config.defaultStyles.ToList();
                }
                return m_DefaultStyles;
            }
        }
        
        void OnStyleTrainerDataChanged(StyleTrainerData _)
        {
            Save();
        }

        internal void Init()
        {
            if (m_StyleTrainerData != null)
            {
                m_StyleTrainerData.Init();
                m_StyleTrainerData.OnDataChanged += OnStyleTrainerDataChanged;
            }
            m_DefaultStyles = StyleTrainerConfig.config.defaultStyles.ToList();
            Save();
        }

        void OnEnable()
        {
            m_StyleTrainerData?.Init();
            StyleTrainerDebug.Log("Asset enabled");
        }

        void OnDisable()
        {
            if (StyleTrainerConfig.config)
            {
                StyleTrainerConfig.config.artifactCache?.Prune();
                StyleTrainerConfig.config.artifactCache?.Dispose();
            }
            StyleTrainerDebug.Log("Asset disabled");
            if (m_StyleTrainerData != null)
            {
                m_StyleTrainerData.OnDataChanged -= OnStyleTrainerDataChanged;
                m_StyleTrainerData.OnDispose();
            }
        }

        void OnDestroy()
        {
            StyleTrainerDebug.Log("Asset destroy");
        }

        public void Reset()
        {
            if (Utilities.ValidStringGUID(guid))
            {
                if (m_PreviousProjectIDs.FindIndex(x => x == guid) < 0)
                    m_PreviousProjectIDs.Add(guid);
            }
            StyleTrainerConfig.config.artifactCache.Clear();
            if (m_StyleTrainerData != null)
            {
                m_StyleTrainerData.OnDataChanged -= OnStyleTrainerDataChanged;
                m_StyleTrainerData.OnDispose();
                m_StyleTrainerData.Delete();
            }
            m_StyleTrainerData = new StyleTrainerData(EState.New);
            m_StyleTrainerData.OnDataChanged += OnStyleTrainerDataChanged;
#if UNITY_EDITOR
            ServerConfig.serverConfig.server.Reset();
#endif
            Save();
            onDataChanged.Invoke(this);
        }

        public void ClearProjectData()
        {
            var oldGuid = m_StyleTrainerData?.guid;
            m_StyleTrainerData?.OnDispose();
            m_StyleTrainerData?.Delete();
            m_StyleTrainerData = new StyleTrainerData(EState.New)
            {
                guid = oldGuid,
                state = EState.Initial
            };
            Save();
            onDataChanged.Invoke(this);
        }
    }
}