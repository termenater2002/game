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
        ApplyDamage(DamageState.ImpactDamage(collision.relativeVelocity.magnitude, impactThreshold, damageMultiplier));
    }
    public DamageReceipt ApplyDamage(float amount)
    {
        DamageReceipt receipt = State.Apply(amount);
        if (receipt.AppliedDamage <= 0f) return receipt;
        var destination = wallet != null ? wallet : SessionWallet.Current;
        if (destination != null) destination.Earnings.Add(receipt.Reward);
        Damaged?.Invoke(receipt);
        if (receipt.Destroyed) Destroy(gameObject);
        return receipt;
    }
}
