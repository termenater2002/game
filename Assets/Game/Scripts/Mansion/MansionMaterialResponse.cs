using System.Collections.Generic;
using UnityEngine;

/// <summary>Localized dents and cloth compression on private mesh copies; imported meshes and UVs stay intact.</summary>
public sealed class MansionMaterialResponse : MonoBehaviour
{
    public DestructionProfile profile;
    public MeshFilter[] deformableSurfaces = new MeshFilter[0];
    private DamageOnImpact owner;
    private sealed class Surface { public MeshFilter filter; public Mesh mesh; public Vector3[] rest, current; }
    private readonly List<Surface> surfaces = new List<Surface>();
    private bool relaxing;
    public int DeformationRevision { get; private set; }
    public bool Failed { get; private set; }
    private void Awake()
    {
        owner = GetComponent<DamageOnImpact>();
        if (owner != null) owner.Damaged += OnDamage;
    }
    private void Prepare()
    {
        if (surfaces.Count > 0) return;
        foreach (var f in deformableSurfaces)
        {
            if (f == null || f.sharedMesh == null) continue;
            var mesh = Instantiate(f.sharedMesh); mesh.name = f.sharedMesh.name + " live deformation"; f.sharedMesh = mesh;
            var rest = mesh.vertices; surfaces.Add(new Surface { filter = f, mesh = mesh, rest = rest, current = (Vector3[])rest.Clone() });
        }
    }
    private void OnDamage(DamageReceipt receipt)
    {
        if (profile == null || profile.failure == DestructionFailure.Fracture) return;
        Prepare(); Vector3 point = owner.LastHitPoint, normal = owner.LastHitNormal;
        if (normal.sqrMagnitude < .1f) { point = transform.position + Vector3.up * .5f; normal = -transform.forward; }
        float radius = profile.deformationRadius;
        foreach (var s in surfaces)
        {
            float closest = float.MaxValue; foreach (var v in s.current) closest = Mathf.Min(closest, Vector3.Distance(s.filter.transform.TransformPoint(v), point));
            if (closest > radius * 1.2f) continue;
            float influence = radius;
            for (int i = 0; i < s.current.Length; i++)
            {
                Vector3 world = s.filter.transform.TransformPoint(s.current[i]); float weight = Mathf.Clamp01(1 - Vector3.Distance(world, point) / influence);
                Vector3 delta = -normal * (profile.maximumDent * weight * Mathf.Clamp01(receipt.AppliedDamage / Mathf.Max(1, owner.maxHP) + .15f));
                Vector3 next = s.filter.transform.InverseTransformPoint(world + delta);
                float localLimit = profile.maximumDent / Mathf.Max(.0001f, s.filter.transform.lossyScale.magnitude / 1.732f);
                s.current[i] = s.rest[i] + Vector3.ClampMagnitude(next - s.rest[i], localLimit);
            }
            s.mesh.vertices = s.current; s.mesh.RecalculateNormals(); s.mesh.RecalculateBounds();
            if (receipt.Destroyed && profile.failure == DestructionFailure.TearAndSag)
            {
                // A bounded patch is actually removed, rather than spawned as rigid cubes.
                for (int sub = 0; sub < s.mesh.subMeshCount; sub++)
                {
                    var old = s.mesh.GetTriangles(sub); var keep = new List<int>();
                    for (int t = 0; t < old.Length; t += 3)
                    { var center = (s.current[old[t]] + s.current[old[t + 1]] + s.current[old[t + 2]]) / 3; if (Vector3.Distance(s.filter.transform.TransformPoint(center), point) > radius * .5f) { keep.Add(old[t]); keep.Add(old[t + 1]); keep.Add(old[t + 2]); } }
                    s.mesh.SetTriangles(keep, sub);
                }
            }
        }
        relaxing = profile.failure == DestructionFailure.TearAndSag && !receipt.Destroyed;
        if (receipt.Destroyed)
        {
            Failed = true;
            foreach (var joint in GetComponents<Joint>()) Destroy(joint);
        }
        DeformationRevision++;
    }
    private void LateUpdate()
    {
        if (!relaxing) return; bool moving = false;
        foreach (var s in surfaces)
        {
            for (int i = 0; i < s.current.Length; i++) { s.current[i] = Vector3.Lerp(s.current[i], s.rest[i], Time.deltaTime * 3); moving |= (s.current[i] - s.rest[i]).sqrMagnitude > .000001f; }
            s.mesh.vertices = s.current; s.mesh.RecalculateBounds();
        }
        relaxing = moving;
    }
    private void OnDestroy()
    {
        if (owner != null) owner.Damaged -= OnDamage;
        foreach (var s in surfaces) if (s.mesh != null) Destroy(s.mesh);
    }
}
