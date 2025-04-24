using System;
using Unity.Muse.Sprite.Common.Backend;

namespace Unity.Muse.Sprite.Backend
{
    [Serializable]
    record FeedbackRequest : BaseRequest
    {
        public string guid;
        public int feedback_flags;
        public string feedback_comment;
    }

    [Serializable]
    struct FeedbackResponse
    {
        public bool success;
        public string guid;
    }

    internal class SubmitFeedbackRestCall : SpriteGeneratorRestCall<FeedbackRequest, FeedbackResponse, SubmitFeedbackRestCall>
    {
        public SubmitFeedbackRestCall(ServerConfig asset, FeedbackRequest request)
            : base(asset, request)
        {
            request.access_token = asset.accessToken;
            request.organization_id = asset.organizationId;
            this.request = request;
        }

        protected override string[] endPoints
        {
            get
            {
                return new[]
                {
                    $"/api/v2/images/sprites/organizations/{request.organization_id}/assets/{request.guid}/feedback",
                };
            }
        }

        protected override IQuarkEndpoint.EMethod[] methods => new [] {
            IQuarkEndpoint.EMethod.PUT,
        };
    }
}