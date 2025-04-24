using System;

namespace Unity.Muse.Common.Api
{
    record TexturesUrl : MuseUrl
    {
        public string textures => $"{images}/textures/{org}";
        public string textureAssetRoot => $"{assets}/images/textures/{org}/assets";

        public string generate => $"{textures}/generate";
        public string upscale => $"{textures}/upscale";
        public string pbr => $"{textures}/pbr";
        public string inpaint => $"{textures}/inpaint";
        public string variate => $"{textures}/variate";
        public string jobs(string jobId) => $"{textures}/jobs/{jobId}";
        public string textureAssets(string assetId) => $"{textureAssetRoot}/{assetId}";
    }
}
