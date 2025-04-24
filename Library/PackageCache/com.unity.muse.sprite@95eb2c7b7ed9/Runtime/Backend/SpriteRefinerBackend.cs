using System;
using Unity.Muse.Sprite.Common.Backend;
using UnityEngine;

namespace Unity.Muse.Sprite.Backend
{
    internal class SpriteRefineRestCall : SpriteGeneratorRestCall<SpriteRefinerRequest, GenerateResponse, SpriteRefineRestCall>
    {
        public SpriteRefineRestCall(ServerConfig asset, SpriteRefinerRequest request, string generatorProfile)
            : base(asset, request)
        {
            request.access_token = asset.accessToken;
            request.organization_id = asset.organizationId;
            request.asset_id = generatorProfile;
            this.request = request;
        }

        protected override string[] endPoints
        {
            get
            {
                return new[]
                {
                    $"/api/v2/images/sprites/organizations/{request.organization_id}/projects/{request.asset_id}/refine",
                };
            }
        }

        protected override IQuarkEndpoint.EMethod[] methods => new [] {
            IQuarkEndpoint.EMethod.POST,
        };

        protected override string RequestLog()
        {
            var logRequest = request with { };
            logRequest.base64Image = $"Image data removed for logging size:{request.base64Image?.Length}";
            logRequest.mask64Image = $"Image data removed for logging size:{request.mask64Image?.Length}";
            return $"Request:{MakeEndPoint(this)} Payload:{JsonUtility.ToJson(logRequest)}";
        }
    }

    internal class GetSpriteRefinerJobListRestCall : SpriteGeneratorRestCall<ServerRequest<EmptyPayload>, JobListResponse, GetSpriteRefinerJobListRestCall>
    {
        string m_GeneratorProfile;

        public GetSpriteRefinerJobListRestCall(ServerConfig asset, ServerRequest<EmptyPayload> request, string generatorProfile)
            : base(asset, request)
        {
            request.access_token = asset.accessToken;
            request.organization_id = asset.organizationId;
            request.guid = generatorProfile;
            this.request = request;
        }

        protected override string[] endPoints
        {
            get
            {
                return new[]
                {
                    $"/api/v2/images/sprites/organizations/{request.organization_id}/projects/{request.guid}/jobs?gentype=refine",
                };
            }
        }

        protected override IQuarkEndpoint.EMethod[] methods => new [] {
            IQuarkEndpoint.EMethod.GET,
        };
    }

    [Serializable]
    internal struct SpriteRefinerRequestSettings
    {
        public int width;
        public int height;
        public int seed;
        public bool seamless;
        public string negative_prompt;
        public float strength;
    }

    [Serializable]
    internal record SpriteRefinerRequest : BaseRequest
    {
        public string asset_id;
        public string prompt;
        public string base64Image;
        public string mask64Image;
        public int image_count;
        public float maskStrength;
        public bool simulate;
        public SpriteRefinerRequestSettings settings;
        public int scribble;
        public int removeBackground;
        public string inputGuid;
        public string mask0Guid;
        public string checkpoint_id;
    }

}