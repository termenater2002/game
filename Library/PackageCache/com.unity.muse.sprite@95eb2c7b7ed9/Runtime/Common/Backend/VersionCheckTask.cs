using System;
using UnityEngine;

namespace Unity.Muse.Sprite.Common.Backend
{
    internal class VersionCheckTask
    {
        public int version
        {
            get;
            private set;
        }

        private Action<bool, int> m_OnDone;

        public VersionCheckTask(int version)
        {
            this.version = version;
        }

        public void Execute(Action<bool, int> onDone)
        {
            m_OnDone = onDone;

            var versionCheckRequest = new VersionCheckProjectRequest();
            var versionCheckRestCall = new VersionCheckProjectRestCall(ServerConfig.serverConfig, versionCheckRequest, version);
            versionCheckRestCall.RegisterOnFailure(OnFailure);
            versionCheckRestCall.RegisterOnSuccess(OnSuccess);
            versionCheckRestCall.SendRequest();
        }

        void OnSuccess(VersionCheckProjectRestCall arg1, VersionCheckProjectResponse arg2)
        {
            m_OnDone.Invoke(arg2.success, version);
        }

        void OnFailure(VersionCheckProjectRestCall obj)
        {
            if (obj.retriesFailed)
            {
                m_OnDone.Invoke(false, version);
            }
        }
    }

    abstract class CommonRestCall<T1, T2, T3> : QuarkRestCall<T1, T2, T3>
        where T1 : BaseRequest
        where T3 : QuarkRestCall
    {
        readonly ServerConfig m_ServerConfig;

        protected CommonRestCall(ServerConfig serverConfig, T1 request)
        {
            m_ServerConfig = serverConfig;
            this.request = request;
            maxRetries = serverConfig.maxRetries;
            retryDelay = serverConfig.webRequestPollRate;
        }

        public override string server => m_ServerConfig.serverURL;
    }

    class VersionCheckProjectRestCall : CommonRestCall<VersionCheckProjectRequest, VersionCheckProjectResponse, VersionCheckProjectRestCall>
    {
        private int version;

        public VersionCheckProjectRestCall(ServerConfig asset, VersionCheckProjectRequest request, int version)
            : base(asset, request)
        {
            request.access_token = asset.accessToken;
            request.organization_id = asset.organizationId;
            this.request = request;
            this.version = version;
        }

        protected override string[] endPoints
        {
            get
            {
                return new[] { $"/api/v{version}/images/sprites/organizations/{request.organization_id}/default_project" };
            }
        }

        protected override IQuarkEndpoint.EMethod[] methods
        {
            get
            {
                return new[] { IQuarkEndpoint.EMethod.GET };
            }
        }
    }

    [Serializable]
    record VersionCheckProjectResponse
    {
        public bool success;
        public string guid;
    }

    [Serializable]
    record VersionCheckProjectRequest : BaseRequest;
}