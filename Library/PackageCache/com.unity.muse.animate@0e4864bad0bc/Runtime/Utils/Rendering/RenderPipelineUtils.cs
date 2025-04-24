using UnityEngine;
using UnityEngine.Rendering;
#if DEEPPOSE_URP
using UnityEngine.Rendering.Universal;
#endif
#if DEEPPOSE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Unity.Muse.Animate
{
    static class RenderPipelineUtils
    {
#if DEEPPOSE_HDRP
        public static bool IsUsingHdrp() => GraphicsSettings.currentRenderPipeline is HDRenderPipelineAsset;
#else
        public static bool IsUsingHdrp() => false;
#endif
#if DEEPPOSE_URP
        public static bool IsUsingUrp() => GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset;
#else
        public static bool IsUsingUrp() => false;
#endif
    }
}
