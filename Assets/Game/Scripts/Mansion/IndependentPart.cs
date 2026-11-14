using UnityEngine;

/// <summary>An individually targetable book or subassembly that survives its container's destruction.</summary>
public sealed class IndependentPart : MonoBehaviour
{
    public DamageOnImpact owner;
    public bool contained;
    private Rigidbody body;
    public bool Released { get; private set; }
    private void Awake() { body = GetComponent<Rigidbody>(); if (owner != null) owner.Damaged += OwnerDamaged; }
    private void OwnerDamaged(DamageReceipt receipt) { if (receipt.Destroyed) Release(); }
    public void Release()
    {
        if (Released) return; Released = true;
        if (body == null) body = GetComponent<Rigidbody>();
        foreach (var joint in GetComponents<Joint>()) Destroy(joint);
        transform.SetParent(null, true);
        if (body != null) { body.isKinematic = false; body.WakeUp(); }
        if (owner != null)
            foreach (var a in GetComponentsInChildren<Collider>()) foreach (var b in owner.GetComponentsInChildren<Collider>())
                if (a != b && !b.transform.IsChildOf(transform)) Physics.IgnoreCollision(a, b, false);
        var group = new GameObject("Released " + name); group.SetActive(false); transform.SetParent(group.transform, true); group.AddComponent<MansionDebris>(); group.SetActive(true); Destroy(group, 75);
    }
    private void OnDestroy() { if (owner != null) owner.Damaged -= OwnerDamaged; }
}
