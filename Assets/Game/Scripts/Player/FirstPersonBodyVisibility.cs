using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Keep the animated body in the FPS shadow pass. Other cameras draw the complete rig.</summary>
public class FirstPersonBodyVisibility : MonoBehaviour
{
    public Camera firstPersonCamera;
    public Renderer[] bodyRenderers;
    private ShadowCastingMode[] previous;
    private bool hidden;
    private void OnEnable()
    {
        if (bodyRenderers == null || bodyRenderers.Length == 0) bodyRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        previous = new ShadowCastingMode[bodyRenderers.Length];
        RenderPipelineManager.beginCameraRendering += Begin;
        RenderPipelineManager.endCameraRendering += End;
        Camera.onPreCull += BeforeCamera; Camera.onPostRender += AfterCamera;
    }
    private void BeforeCamera(Camera camera)
    {
        if (!Application.isPlaying || camera != firstPersonCamera || hidden) return;
        hidden = true;
        for (int i = 0; i < bodyRenderers.Length; i++) if (bodyRenderers[i] != null) { previous[i] = bodyRenderers[i].shadowCastingMode; bodyRenderers[i].shadowCastingMode = ShadowCastingMode.ShadowsOnly; }
    }
    private void AfterCamera(Camera camera) { if (camera == firstPersonCamera) Restore(); }
    private void Begin(ScriptableRenderContext context, Camera camera) => BeforeCamera(camera);
    private void End(ScriptableRenderContext context, Camera camera) => AfterCamera(camera);
    private void Restore()
    {
        if (!hidden) return;
        for (int i = 0; i < bodyRenderers.Length; i++) if (bodyRenderers[i] != null) bodyRenderers[i].shadowCastingMode = previous[i];
        hidden = false;
    }
    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= Begin; RenderPipelineManager.endCameraRendering -= End;
        Camera.onPreCull -= BeforeCamera; Camera.onPostRender -= AfterCamera; Restore();
    }
}
