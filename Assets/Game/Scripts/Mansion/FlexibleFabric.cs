using System.Collections.Generic;
using UnityEngine;

/// <summary>A short chain of sleeping hinge-connected strips, capped by a shared active-segment budget.</summary>
public sealed class FlexibleFabric : MonoBehaviour
{
    public Rigidbody[] segments = new Rigidbody[0];
    public float sleepDistance = 14f;
    public int activeSegmentBudget = 64;
    private static readonly List<FlexibleFabric> all = new List<FlexibleFabric>();
    private static float nextCheck;
    private bool frozen;
    private DamageOnImpact owner;
    public bool Torn { get; private set; }
    public bool IsFrozen => frozen;
    public void Wake()
    {
        foreach (var body in segments) if (body != null) { body.isKinematic = false; body.WakeUp(); }
        frozen = false;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() { all.Clear(); nextCheck = 0; }
    private void OnEnable() { all.Add(this); }
    public static FlexibleFabric ForBody(Rigidbody body)
    {
        if (body == null) return null;
        var root = body.GetComponent<FlexibleFabric>();
        if (root != null) return root;
        var strip = body.GetComponent<FabricSegment>();
        return strip != null ? strip.fabric : null;
    }
    private void Awake()
    {
        owner = GetComponent<DamageOnImpact>(); if (owner != null) owner.Damaged += Damaged;
        // Nested dynamic bodies under a nonuniformly scaled moving Rigidbody shear the strips.
        // Keep their initial world scale/pose and joints, then simulate each as a sibling.
        for (int i = 1; i < segments.Length; i++) if (segments[i] != null)
        {
            var strip = segments[i].gameObject.GetComponent<FabricSegment>();
            if (strip == null) strip = segments[i].gameObject.AddComponent<FabricSegment>();
            strip.fabric = this;
            segments[i].transform.SetParent(transform.parent, true);
        }
    }
    private void OnDisable() { all.Remove(this); }
    private void Damaged(DamageReceipt receipt)
    {
        if (!receipt.Destroyed || Torn) return;
        HingeJoint nearest = null; float distance = float.MaxValue;
        foreach (var body in segments)
        { var joint = body != null ? body.GetComponent<HingeJoint>() : null; if (joint == null) continue;
        float d = (joint.transform.position - owner.LastHitPoint).sqrMagnitude; if (d < distance) { nearest = joint; distance = d; } }
        if (nearest != null) Destroy(nearest); Torn = true;
    }
    private void Update()
    {
        if (all.Count == 0 || all[0] != this || Time.time < nextCheck) return; nextCheck = Time.time + .5f;
        var player = Object.FindAnyObjectByType<FunnyMovement>(); if (player == null) return;
        var grab = player.GetComponentInChildren<ObjectGrabber>(); int used = 0;
        all.Sort((a, b) => (a.transform.position - player.transform.position).sqrMagnitude.CompareTo((b.transform.position - player.transform.position).sqrMagnitude));
        foreach (var cloth in all)
        {
            bool held = false, moving = false;
            foreach (var body in cloth.segments) if (body != null)
            { held |= grab != null && grab.HeldBody == body; moving |= body.linearVelocity.sqrMagnitude > .02f || body.angularVelocity.sqrMagnitude > .05f; }
            bool wantsActive = held || moving || Vector3.Distance(cloth.transform.position, player.transform.position) < cloth.sleepDistance;
            bool activate = wantsActive && used + cloth.segments.Length <= Mathf.Clamp(activeSegmentBudget, 8, 128);
            if (activate) used += cloth.segments.Length;
            if (cloth.frozen == !activate) continue;
            foreach (var body in cloth.segments) if (body != null) { body.isKinematic = !activate; if (activate) body.WakeUp(); }
            cloth.frozen = !activate;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!frozen) return; Wake();
    }
    private void OnDestroy()
    {
        if (owner != null) owner.Damaged -= Damaged;
        for (int i = 1; i < segments.Length; i++) if (segments[i] != null) Destroy(segments[i].gameObject);
    }
}
