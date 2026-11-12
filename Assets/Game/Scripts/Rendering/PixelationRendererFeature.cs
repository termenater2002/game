using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

/// <summary>URP Render Graph fullscreen pixelation, after scene post processing and before overlay UI.</summary>
public sealed class PixelationRendererFeature : ScriptableRendererFeature
{
    public Shader pixelationShader;
    private Material material;
    private PixelationPass pass;

    public override void Create()
    {
        CoreUtils.Destroy(material);
        material = pixelationShader != null ? CoreUtils.CreateEngineMaterial(pixelationShader) : null;
        pass = new PixelationPass(material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var camera = renderingData.cameraData.camera;
        if (material == null || camera.cameraType != CameraType.Game) return;
        if (!camera.TryGetComponent<PixelationCamera>(out var settings) || !settings.isActiveAndEnabled) return;
        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing) { CoreUtils.Destroy(material); }

    private sealed class PixelationPass : ScriptableRenderPass
    {
        private static readonly int PixelParams = Shader.PropertyToID("_PixelationParams");
        private readonly Material material;

        internal PixelationPass(Material material)
        {
            this.material = material;
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(RenderGraph graph, ContextContainer frameData)
        {
            var cameraData = frameData.Get<UniversalCameraData>();
            if (!cameraData.camera.TryGetComponent<PixelationCamera>(out var settings) || !settings.isActiveAndEnabled) return;
            var resources = frameData.Get<UniversalResourceData>();
            if (resources.isActiveTargetBackBuffer || !resources.activeColorTexture.IsValid()) return;

            var source = resources.activeColorTexture;
            var descriptor = source.GetDescriptor(graph);
            int blockSize = Mathf.Max(1, Mathf.RoundToInt(descriptor.height / (float)Mathf.Clamp(settings.verticalPixels, 60, 720)));
            var parameters = settings.Parameters;
            parameters.SetVector(PixelParams, new Vector4(descriptor.width, descriptor.height, blockSize, blockSize));
            descriptor.name = "MAJOR DAMAGE pixelated world";
            descriptor.clearBuffer = false;
            descriptor.depthBufferBits = DepthBits.None;
            var destination = graph.CreateTexture(descriptor);
            // AddBlitPass explicitly tracks and binds source to _BlitTexture. No global textures or read/write aliasing.
            var blit = new RenderGraphUtils.BlitMaterialParameters(source, destination, material, 0) { propertyBlock = parameters };
            graph.AddBlitPass(blit, "MAJOR DAMAGE / Pixelate world");
            resources.cameraColor = destination;
        }
    }
}
