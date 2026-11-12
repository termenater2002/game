using System.Collections.Generic;
using UnityEngine;

/// <summary>Metadata and live moving-part alignment for the mansion's reusable physical props.</summary>
public sealed class MansionProp : MonoBehaviour
{
    public int assetId;
    public string displayName;
    public string room;
    public string substance;
    public bool wallMounted;
    public AudioClip hitSound;
    public AudioClip breakSound;
    public Collider[] mountSupports = new Collider[0];
    private DamageOnImpact damageSource;
    private readonly List<Collider> ignoredMountShapes = new List<Collider>();
    public bool WasUnfastened { get; private set; }
    public void PrepareFracture(GameObject debris)
    {
        var sources = new Dictionary<string, Transform>();
        foreach (var source in GetComponentsInChildren<Transform>())
            if (!sources.ContainsKey(source.name)) sources.Add(source.name, source.transform);
        foreach (var shard in debris.GetComponentsInChildren<MansionFragment>(true))
        {
            var delta = Matrix4x4.identity;
            if (!string.IsNullOrEmpty(shard.sourceName) && sources.TryGetValue(shard.sourceName, out var source))
                delta = transform.worldToLocalMatrix * source.localToWorldMatrix * shard.sourceRestInverse;
            var pose = delta * Matrix4x4.Translate(shard.restCenter);
            shard.transform.localPosition = pose.GetColumn(3);
            shard.transform.localRotation = pose.rotation;
            shard.transform.localScale = pose.lossyScale;
        }
    }
    private void OnEnable()
    {
        damageSource = GetComponent<DamageOnImpact>();
        if (damageSource != null) damageSource.Damaged += PlayImpact;
        if (wallMounted)
        {
            var shapes = GetComponentsInChildren<Collider>();
            if (mountSupports != null && mountSupports.Length > 0)
            {
                foreach (var mount in mountSupports)
                {
                    if (mount == null) continue;
                    foreach (var shape in shapes) Physics.IgnoreCollision(shape, mount, true);
                    ignoredMountShapes.Add(mount);
                }
                return;
            }
            var bounds = new Bounds(transform.position, Vector3.zero);
            foreach (var shape in shapes) bounds.Encapsulate(shape.bounds);
            foreach (var mount in Physics.OverlapBox(bounds.center, bounds.extents + Vector3.one * .04f))
            {
                if (mount.transform.IsChildOf(transform) || mount.attachedRigidbody != null) continue;
                // Trim overlaps its supporting wall/ceiling by design. Floors still support all furniture.
                if (mount.bounds.size.y < 1.5f && mount.bounds.center.y < 2.8f) continue;
                foreach (var shape in shapes) Physics.IgnoreCollision(shape, mount, true);
                ignoredMountShapes.Add(mount);
            }
        }
    }
    private void PlayImpact(DamageReceipt receipt)
    {
        var clip = receipt.Destroyed ? breakSound : hitSound;
        if (clip != null) AudioSource.PlayClipAtPoint(clip, transform.position, receipt.Destroyed ? .45f : .22f);
    }
    private void OnDisable() { if (damageSource != null) damageSource.Damaged -= PlayImpact; }
    private void OnJointBreak(float force)
    {
        WasUnfastened = true;
        foreach (var mount in ignoredMountShapes)
            if (mount != null) foreach (var shape in GetComponentsInChildren<Collider>()) Physics.IgnoreCollision(shape, mount, false);
        ignoredMountShapes.Clear();
        var damage = GetComponent<DamageOnImpact>();
        if (damage != null && damage.State != null && !damage.State.IsDestroyed)
            damage.ApplyDamage(damage.maxHP * .12f);
    }
}
