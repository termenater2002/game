using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Hide the full body for the FPS camera only. Scene/preview cameras retain the complete rig.</summary>
public class FirstPersonBodyVisibility : MonoBehaviour
{
    public Camera firstPersonCamera;
    public Renderer[] bodyRenderers;
    private bool[] previous;
    private bool hidden;
    private void OnEnable()
    {
        if (bodyRenderers == null || bodyRenderers.Length == 0) bodyRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        previous = new bool[bodyRenderers.Length];
        RenderPipelineManager.beginCameraRendering += Begin;
        RenderPipelineManager.endCameraRendering += End;
        Camera.onPreCull += BeforeCamera; Camera.onPostRender += AfterCamera;
    }
    private void BeforeCamera(Camera camera)
    {
        if (!Application.isPlaying || camera != firstPersonCamera || hidden) return;
        hidden = true;
        for (int i = 0; i < bodyRenderers.Length; i++) if (bodyRenderers[i] != null) { previous[i] = bodyRenderers[i].forceRenderingOff; bodyRenderers[i].forceRenderingOff = true; }
    }
    private void AfterCamera(Camera camera) { if (camera == firstPersonCamera) Restore(); }
    private void Begin(ScriptableRenderContext context, Camera camera) => BeforeCamera(camera);
    private void End(ScriptableRenderContext context, Camera camera) => AfterCamera(camera);
    private void Restore()
    {
        if (!hidden) return;
        for (int i = 0; i < bodyRenderers.Length; i++) if (bodyRenderers[i] != null) bodyRenderers[i].forceRenderingOff = previous[i];
        hidden = false;
    }
    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= Begin; RenderPipelineManager.endCameraRendering -= End;
        Camera.onPreCull -= BeforeCamera; Camera.onPostRender -= AfterCamera; Restore();
    }
}
