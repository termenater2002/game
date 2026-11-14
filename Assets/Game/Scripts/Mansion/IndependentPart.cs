using UnityEngine;
using System.Collections;

/// <summary>An individually targetable book or subassembly that survives its container's destruction.</summary>
public sealed class IndependentPart : MonoBehaviour
{
    public DamageOnImpact owner;
    public bool contained;
    [Min(.01f)] public float partMass = 1f;
    public string role = "attachment";
    private static readonly WaitForFixedUpdate SeparationStep = new WaitForFixedUpdate();
    private DamageOnImpact ownDamage;
    private Rigidbody body;
    public bool Released { get; private set; }
    private void Awake()
    {
        body = GetComponent<Rigidbody>(); ownDamage = GetComponent<DamageOnImpact>();
        if (owner != null) owner.Damaged += OwnerDamaged;
        if (ownDamage != null) ownDamage.Damaged += SelfDamaged;
    }
    private void SelfDamaged(DamageReceipt receipt) { if (receipt.Destroyed && ownDamage.retainOnDestruction) Release(); }
    private void OnJointBreak(float force) { Release(); }
    private void OwnerDamaged(DamageReceipt receipt) { if (receipt.Destroyed) Release(); }
    public void Release()
    {
        if (Released) return; Released = true;
        var parentBody = owner != null ? owner.GetComponentInParent<Rigidbody>() : null;
        Vector3 inheritedVelocity = parentBody != null ? parentBody.GetPointVelocity(transform.position) : Vector3.zero;
        Vector3 inheritedSpin = parentBody != null ? parentBody.angularVelocity : Vector3.zero;
        if (body == null) body = GetComponent<Rigidbody>();
        bool newBody = body == null;
        if (newBody)
        {
            body = gameObject.AddComponent<Rigidbody>();body.mass = Mathf.Max(.01f, partMass);
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            body.maxDepenetrationVelocity = 1.5f;body.maxAngularVelocity = 14;body.angularDamping = .6f;
        }
        body.maxDepenetrationVelocity = .5f; // Release touching mounts without a false high-speed impact.
        foreach (var joint in GetComponents<Joint>()) Destroy(joint);
        transform.SetParent(null, true);
        if (body != null) { bool wasKinematic = body.isKinematic; body.isKinematic = false; if (newBody || wasKinematic) { body.linearVelocity = inheritedVelocity; body.angularVelocity = inheritedSpin; } body.WakeUp(); }
        var formerShapes = owner != null ? owner.GetComponentsInChildren<Collider>() : new Collider[0];
        var group = new GameObject("Released " + name); group.SetActive(false); transform.SetParent(group.transform, true); group.AddComponent<MansionDebris>(); group.SetActive(true); Destroy(group, 75);
        foreach (var a in GetComponentsInChildren<Collider>()) foreach (var b in formerShapes)
        {
            if (a == b || b == null || b.transform.IsChildOf(transform)) continue;
            if (a.enabled && b.enabled && Physics.ComputePenetration(a,a.transform.position,a.transform.rotation,b,b.transform.position,b.transform.rotation,out _,out var depth) && depth > .001f)
            { Physics.IgnoreCollision(a,b,true);StartCoroutine(RestoreContactAfterSeparation(a,b)); }
            else Physics.IgnoreCollision(a,b,false);
        }
    }
    private IEnumerator RestoreContactAfterSeparation(Collider a, Collider b)
    {
        while (a != null && b != null && a.enabled && b.enabled && a.gameObject.activeInHierarchy && b.gameObject.activeInHierarchy)
        {
            if (!Physics.ComputePenetration(a,a.transform.position,a.transform.rotation,b,b.transform.position,b.transform.rotation,out _,out var depth) || depth <= .001f)
            { Physics.IgnoreCollision(a,b,false);yield break; }
            Physics.IgnoreCollision(a,b,true);yield return SeparationStep;
        }
        if (a != null && b != null) Physics.IgnoreCollision(a,b,false);
    }
    private void OnDestroy() { if (owner != null) owner.Damaged -= OwnerDamaged; if (ownDamage != null) ownDamage.Damaged -= SelfDamaged; }
}
