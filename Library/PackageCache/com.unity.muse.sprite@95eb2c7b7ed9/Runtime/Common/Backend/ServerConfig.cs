using System;
using Unity.Muse.Common.Account;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Sprite.Common.Backend
{
    //[CreateAssetMenu(fileName = "ServerConfig.asset", menuName = "Muse/Sprite/ServerConfig")]
    internal class ServerConfig : ScriptableObject
    {
        internal class Server : IServer
        {
            public void Reset()
            {
                // nothing to do
            }

            public void Init()
            {
                // nothing to do
            }

            public IWebRequest CreateWebRequest(string url, string method)
            {
                return new WebRequest(url, method);
            }

            public string organisationID => AccountInfo.Instance.Organization?.Id;
        }

        [Flags]
        public enum EDebugMode
        {
            ArtifactDebugInfo = 1,
            SessionDebug = 1 << 2,
            OperatorDebug = 1 << 3,
            ForceUseSecretKey = 1 << 4
        }

        public string[] serverList;
        public int serverIndex;
        [SerializeField]
        string secretToken;
        public float webRequestPollRate = 1.0f;
        public int maxRetries = 3;
        public string serverURL => serverList[serverIndex];

        [HideInInspector]
        public int model;
        [HideInInspector]
        public bool simulate;

        [NonSerialized] public bool callApiVersion = false;
        public int apiVersion = -1;
        [NonSerialized] public string lastApiServer = null;

        [SerializeField]
        EDebugMode m_DebugMode;
        public EDebugMode debugMode =>
#if UNITY_EDITOR
            UnityEditor.Unsupported.IsDeveloperMode() ? m_DebugMode : 0;
#else
            0;
#endif


        public string accessToken =>
#if UNITY_EDITOR
            (debugMode & EDebugMode.ForceUseSecretKey) > 0 ? secretToken : CloudProjectSettings.accessToken;
#else
            secretToken;
#endif

        public string organizationId
        {
            get
            {
#if UNITY_EDITOR
                return server.organisationID;
#else
                return secretToken;
#endif
            }
        }
        public static ServerConfig serverConfig =>
#if UNITY_EDITOR
            GetServerConfigEditor();
#else
            ResourceManager.Load<ServerConfig>(PackageResources.spriteGeneratorServerConfig);
#endif

#if UNITY_EDITOR
        static ServerConfig s_ServerConfig = null;
        static ServerConfig GetServerConfigEditor()
        {
            if (s_ServerConfig == null)
            {
                var objs = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget("ProjectSettings/SpriteMuseServerConfig.asset");
                s_ServerConfig = (objs.Length > 0 ? objs[0] : null) as ServerConfig;
                if (s_ServerConfig == null)
                {
                    objs = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(PackageResources.spriteGeneratorServerConfig);
                    s_ServerConfig = (objs.Length > 0 ? objs[0] : null) as ServerConfig;
                }
            }
            return s_ServerConfig;
        }
#endif

#if UNITY_EDITOR

        internal void SaveConfig()
        {
            if(System.IO.File.Exists("ProjectSettings/SpriteMuseServerConfig.asset"))
                UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new []{this}, "ProjectSettings/SpriteMuseServerConfig.asset", true);
            else
                UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new []{this}, PackageResources.spriteGeneratorServerConfig, true);
        }

        internal void ResetConfig()
        {
            s_ServerConfig = null;
        }

        [HideInInspector]
        [SerializeReference]
        IServer m_Server;

        public void SetServer(IServer server)
        {
            m_Server = server;
        }
        public IServer server => m_Server ?? (m_Server = new Server());
#else
        public IServer server => m_Server ?? (m_Server = new Server());
#endif


    }
}