using System;
using System.Collections.Generic;
using System.Linq;
using StyleTrainer.Backend;
using Unity.Muse.Sprite.Common.Backend;
using Unity.Muse.StyleTrainer.Debug;
using UnityEngine;

namespace Unity.Muse.StyleTrainer
{
    [Serializable]
    class StyleTrainerData : Artifact<StyleTrainerData, StyleTrainerData>
    {
        // Unity version + XXXXX
        public const string k_Version = "202231001";

        [SerializeReference]
        List<StyleData> m_Styles = new();

        [SerializeField]
        string m_Version = k_Version;

        public string version => m_Version;

        public StyleTrainerData(EState state)
            : base(state)
        {
            if (!Utilities.ValidStringGUID(guid))
                guid = Guid.NewGuid().ToString();
        }

        public override void OnDispose()
        {
            for (var i = 0; i < m_Styles?.Count; ++i)
                m_Styles[i]?.OnDispose();
            base.OnDispose();
        }

        public override void GetArtifact(Action<StyleTrainerData> onDoneCallback, bool useCache)
        {
            OnArtifactLoaded += onDoneCallback;
            if (state == EState.Loaded && useCache)
            {
                ArtifactLoaded(this);
            }
            else
            {
                if (state != EState.Loading)
                {
                    state = EState.Loading;
                    var getStylesRequest = new GetStylesRequest
                    {
                        guid = guid
                    };
                    var getStylesRestCall = new GetStylesRestCall(ServerConfig.serverConfig, getStylesRequest);
                    getStylesRestCall.RegisterOnSuccess(OnGetStylesSuccess);
                    getStylesRestCall.RegisterOnFailure(OnGetStylesFailure);
                    getStylesRestCall.SendRequest();
                }
            }
        }

        void OnGetStylesFailure(GetStylesRestCall obj)
        {
            if (obj.retriesFailed)
            {
                state = EState.Error;
                StyleTrainerDebug.LogError($"Unable to load style trainer project {guid} {obj.requestError} {obj.errorMessage}");
                ArtifactLoaded(this);
            }
        }

        void OnGetStylesSuccess(GetStylesRestCall arg1, GetStylesResponse arg2)
        {
            if (arg2.success && !disposing)
            {
                ClearStyles();
                state = EState.Loaded;
                UpdateVersion();
                if (arg2.styleIDs.Length == 0)
                {
                    OnGetStyleDone(null);
                }
                else
                {
                    foreach (var id in arg2.styleIDs)
                    {
                        var styleData = new StyleData(EState.Initial, id, guid);
                        AddStyle(styleData);
                        styleData.GetArtifact(OnGetStyleDone, false);
                    }
                }
            }
            else
            {
                state = EState.Error;
                StyleTrainerDebug.Log($"OnGetStylesSuccess but call failed. {guid} {arg1.errorMessage}");
                ArtifactLoaded(this);
            }
        }

        void OnGetStyleDone(StyleData obj)
        {
            int i = 0;
            for(; i < m_Styles?.Count; ++i)
            {
                if (m_Styles[i].state != EState.Error && m_Styles[i].state != EState.Loaded)
                    break;
            }

            if (i >= m_Styles?.Count())
            {
                state = EState.Loaded;
                ArtifactLoaded(this);
            }
        }

        public IReadOnlyList<StyleData> styles => m_Styles;

        public void AddStyle(StyleData style)
        {
            m_Styles.Add(style);
            DataChanged(this);
        }

        public void RemoveStyle(StyleData style)
        {
            if (m_Styles.Remove(style))
            {
                style.Delete();
                style.OnDispose();
                DataChanged(this);
            }
        }

        public void ClearStyles()
        {
            for (int i = 0; i < m_Styles.Count; ++i)
            {
                m_Styles[i].OnDispose();
                m_Styles[i].Delete();
            }
            m_Styles.Clear();
            DataChanged(this);
        }

        public override void Init()
        {
            base.Init();
            for (var i = 0; i < m_Styles?.Count; ++i)
                m_Styles[i]?.Init();
            if (!Utilities.ValidStringGUID(guid)) guid = Guid.NewGuid().ToString();
        }

        public void Delete()
        {
            foreach (var style in m_Styles) style?.Delete();

            m_Styles = null;
        }

        public void UpdateVersion()
        {
            m_Version = k_Version;
        }

        public bool HasTraining()
        {
            for (int i = 0; i < m_Styles?.Count; ++i)
            {
                if (m_Styles[i].HasTraining())
                    return true;
            }

            return false;
        }
    }
}