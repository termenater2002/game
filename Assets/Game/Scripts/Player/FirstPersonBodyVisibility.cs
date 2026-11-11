using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

/// <summary>FPS draws the same animated body without head triangles; the complete rig still casts shadows.</summary>
public class FirstPersonBodyVisibility : MonoBehaviour
{
    public Camera firstPersonCamera;
    public Renderer[] bodyRenderers;
    public bool showFirstPersonBody = true;
    private ShadowCastingMode[] previous;
    private bool hidden;
    private readonly List<SkinnedMeshRenderer> firstPersonRenderers = new List<SkinnedMeshRenderer>();
    private readonly List<SkinnedMeshRenderer> sources = new List<SkinnedMeshRenderer>();
    private readonly List<Mesh> meshes = new List<Mesh>();
    public IReadOnlyList<SkinnedMeshRenderer> FirstPersonRenderers => firstPersonRenderers;
    private void OnEnable()
    {
        if (bodyRenderers == null || bodyRenderers.Length == 0) bodyRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        previous = new ShadowCastingMode[bodyRenderers.Length];
        if (Application.isPlaying) BuildFirstPersonBody();
        RenderPipelineManager.beginCameraRendering += Begin;
        RenderPipelineManager.endCameraRendering += End;
        Camera.onPreCull += BeforeCamera; Camera.onPostRender += AfterCamera;
    }
    private void BeforeCamera(Camera camera)
    {
        if (!Application.isPlaying || camera != firstPersonCamera || hidden) return;
        hidden = true;
        for (int i = 0; i < bodyRenderers.Length; i++) if (bodyRenderers[i] != null) { previous[i] = bodyRenderers[i].shadowCastingMode; bodyRenderers[i].shadowCastingMode = ShadowCastingMode.ShadowsOnly; }
        for (int i = 0; i < firstPersonRenderers.Count; i++)
            firstPersonRenderers[i].forceRenderingOff = !showFirstPersonBody || sources[i] == null || !sources[i].enabled || sources[i].forceRenderingOff;
    }
    private void AfterCamera(Camera camera) { if (camera == firstPersonCamera) Restore(); }
    private void Begin(ScriptableRenderContext context, Camera camera) => BeforeCamera(camera);
    private void End(ScriptableRenderContext context, Camera camera) => AfterCamera(camera);
    private void Restore()
    {
        if (!hidden) return;
        for (int i = 0; i < bodyRenderers.Length; i++) if (bodyRenderers[i] != null) bodyRenderers[i].shadowCastingMode = previous[i];
        foreach (var renderer in firstPersonRenderers) if (renderer != null) renderer.forceRenderingOff = true;
        hidden = false;
    }
    private void BuildFirstPersonBody()
    {
        foreach (var renderer in bodyRenderers)
        {
            var source = renderer as SkinnedMeshRenderer;
            if (source == null || source.sharedMesh == null) continue;
            if (!source.sharedMesh.isReadable)
            {
                Debug.LogWarning("Enable Read/Write on the player mesh for the first-person body: " + source.name, source);
                continue;
            }
            var animator = source.GetComponentInParent<Animator>();
            Transform head = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Head) : null;
            Transform neck = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Neck) : null;
            var excluded = new bool[source.bones.Length];
            for (int i = 0; i < excluded.Length; i++)
            {
                var bone = source.bones[i];
                excluded[i] = bone != null && ((head != null && (bone == head || bone.IsChildOf(head))) || bone == neck
                    || (head == null && (bone.name.EndsWith("Head") || bone.name.EndsWith("Neck"))));
            }
            var mesh = Instantiate(source.sharedMesh); mesh.name = source.sharedMesh.name + " • FPS below neck";
            mesh.hideFlags = HideFlags.DontSave;
            var weights = mesh.boneWeights; var remove = new bool[mesh.vertexCount];
            float Weight(int index, float value) => index >= 0 && index < excluded.Length && excluded[index] ? value : 0f;
            for (int i=0;i<remove.Length;i++)
            {
                var w=weights[i];
                remove[i]=Weight(w.boneIndex0,w.weight0)+Weight(w.boneIndex1,w.weight1)+Weight(w.boneIndex2,w.weight2)+Weight(w.boneIndex3,w.weight3)>.5f;
            }
            int total = 0;
            for (int sub=0;sub<mesh.subMeshCount;sub++)
            {
                var original = mesh.GetTriangles(sub); var kept = new List<int>(original.Length);
                for (int i=0;i<original.Length;i+=3)
                    if (!remove[original[i]] && !remove[original[i+1]] && !remove[original[i+2]])
                    { kept.Add(original[i]); kept.Add(original[i+1]); kept.Add(original[i+2]); }
                mesh.SetTriangles(kept,sub,false); total+=kept.Count;
            }
            if (total==0) { Release(mesh); continue; }
            var go = new GameObject(source.name + " • First-person body"); go.hideFlags = HideFlags.DontSave;
            go.layer = source.gameObject.layer; go.transform.SetParent(source.transform,false);
            var copy = go.AddComponent<SkinnedMeshRenderer>();
            copy.sharedMesh=mesh; copy.sharedMaterials=source.sharedMaterials; copy.bones=source.bones; copy.rootBone=source.rootBone;
            copy.localBounds=source.localBounds; copy.quality=source.quality; copy.updateWhenOffscreen=true;
            copy.shadowCastingMode=ShadowCastingMode.Off; copy.receiveShadows=source.receiveShadows; copy.forceRenderingOff=true;
            sources.Add(source); firstPersonRenderers.Add(copy); meshes.Add(mesh);
        }
    }
    private void LateUpdate()
    {
        for (int i=0;i<sources.Count;i++)
            if (sources[i]!=null && firstPersonRenderers[i]!=null)
                for (int j=0;j<sources[i].sharedMesh.blendShapeCount;j++)
                    firstPersonRenderers[i].SetBlendShapeWeight(j,sources[i].GetBlendShapeWeight(j));
    }
    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= Begin; RenderPipelineManager.endCameraRendering -= End;
        Camera.onPreCull -= BeforeCamera; Camera.onPostRender -= AfterCamera; Restore();
        foreach (var renderer in firstPersonRenderers) if (renderer != null) { renderer.forceRenderingOff=true; Release(renderer.gameObject); }
        foreach (var mesh in meshes) if (mesh != null) Release(mesh);
        firstPersonRenderers.Clear(); sources.Clear(); meshes.Clear();
    }
    private static void Release(Object value) { if (Application.isPlaying) Destroy(value); else DestroyImmediate(value); }
}
