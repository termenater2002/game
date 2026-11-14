using System;
using UnityEngine;

public class DamageOnImpact : MonoBehaviour
{
    [Header("Health")]
    public float maxHP = 100f;
    public float damageMultiplier = 1f;
    public float impactThreshold = 5f;
    [Header("Destruction")]
    public GameObject brokenPrefab;
    public float explosionForce = 300f;
    public float explosionRadius = 2f;
    public float debrisLifetime = 5f;
    [Header("Session reward")]
    public float objectValue = 100f;
    public SessionWallet wallet;
    public DestructionProfile materialProfile;
    [Min(.1f)] public float materialToughnessScale = 1;
    public bool retainOnDestruction;
    public Vector3 LastHitPoint { get; private set; }
    public Vector3 LastHitNormal { get; private set; }
    public DamageState State { get; private set; }
    public event Action<DamageReceipt> Damaged;
    private DamageFeedback feedback;
    private void Awake()
    {
        float hp = float.IsNaN(maxHP) || float.IsInfinity(maxHP) ? 100f : Mathf.Max(0.001f, maxHP);
        float price = float.IsNaN(objectValue) || float.IsInfinity(objectValue) ? 0f : Mathf.Clamp(objectValue, 0f, 1e20f);
        State = new DamageState(hp, (decimal)price);
        feedback = GetComponent<DamageFeedback>();
        if (feedback == null) feedback = gameObject.AddComponent<DamageFeedback>();
        feedback.Initialize(this);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contactCount == 0) return;
        var contact = collision.GetContact(0);
        var part = contact.thisCollider.GetComponentInParent<DamageOnImpact>();
        if (part != null && part != this) { part.ReceiveCollision(collision); return; }
        ReceiveCollision(collision);
    }
    public void ReceiveCollision(Collision collision)
    {
        if (State == null || State.IsDestroyed || collision.contactCount == 0) return;
        var c = collision.GetContact(0); float amount;
        if (materialProfile != null)
        {
            if (collision.rigidbody != null && collision.rigidbody.GetComponent<FunnyMovement>() != null) return;
            var ownBody = c.thisCollider.attachedRigidbody ?? GetComponentInParent<Rigidbody>(); float ownMass = ownBody != null ? ownBody.mass : 1000;
            float otherMass = collision.rigidbody != null ? collision.rigidbody.mass : float.PositiveInfinity;
            float effectiveMass = 1f / (1f / Mathf.Max(.01f, ownMass) + 1f / Mathf.Max(.01f, otherMass));
            float normalSpeed = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, c.normal));
            if (normalSpeed < materialProfile.minimumImpactSpeed) return;
            amount = materialProfile.ImpactDamage(.5f * effectiveMass * normalSpeed * normalSpeed, maxHP) / Mathf.Max(.1f, materialToughnessScale);
        }
        else amount = DamageState.ImpactDamage(collision.relativeVelocity.magnitude, impactThreshold, damageMultiplier);
        var own = GetComponentInParent<Rigidbody>(); var center = own != null ? own.worldCenterOfMass : transform.position;
        var normal = Vector3.Dot(c.normal, c.point - center) < 0 ? -c.normal : c.normal;
        ApplyDamageAt(amount, c.point, normal);
    }
    public DamageReceipt ApplyDamageAt(float amount, Vector3 point, Vector3 normal)
    {
        LastHitPoint = point; LastHitNormal = normal; return ApplyDamage(amount);
    }
    public DamageReceipt ApplyDamage(float amount)
    {
        DamageReceipt receipt = State.Apply(amount);
        if (receipt.AppliedDamage <= 0f) return receipt;
        var destination = wallet != null ? wallet : SessionWallet.Current;
        if (destination != null) destination.Earnings.Add(receipt.Reward);
        Damaged?.Invoke(receipt);
        if (receipt.Destroyed && !retainOnDestruction) Destroy(gameObject);
        return receipt;
    }
}
