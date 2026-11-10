using System;

public readonly struct DamageReceipt
{
    public readonly float AppliedDamage;
    public readonly decimal Reward;
    public readonly float HealthFraction;
    public readonly bool Destroyed;
    public DamageReceipt(float applied, decimal reward, float fraction, bool destroyed)
    { AppliedDamage = applied; Reward = reward; HealthFraction = fraction; Destroyed = destroyed; }
}

/// <summary>Health and proportional rewards, independent of rendering and Unity physics.</summary>
public sealed class DamageState
{
    public float MaxHealth { get; }
    public float CurrentHealth { get; private set; }
    public decimal Value { get; }
    public decimal Rewarded { get; private set; }
    public bool IsDestroyed => CurrentHealth <= 0f;
    public DamageState(float maxHealth, decimal value)
    {
        if (float.IsNaN(maxHealth) || float.IsInfinity(maxHealth) || maxHealth <= 0f) throw new ArgumentOutOfRangeException(nameof(maxHealth));
        MaxHealth = CurrentHealth = maxHealth;
        Value = Math.Max(0m, value);
    }
    public DamageReceipt Apply(float damage)
    {
        if (float.IsNaN(damage) || float.IsInfinity(damage) || damage <= 0f || IsDestroyed)
            return new DamageReceipt(0f, 0m, CurrentHealth / MaxHealth, IsDestroyed);
        float applied = Math.Min(damage, CurrentHealth);
        CurrentHealth = Math.Max(0f, CurrentHealth - applied);
        decimal reward = IsDestroyed ? Value - Rewarded : Math.Min(Value - Rewarded, Value * (decimal)(applied / MaxHealth));
        Rewarded += reward;
        return new DamageReceipt(applied, reward, CurrentHealth / MaxHealth, IsDestroyed);
    }
    public static float ImpactDamage(float speed, float threshold, float multiplier)
    {
        if (float.IsNaN(speed) || float.IsInfinity(speed) || speed < Math.Max(0f, threshold) || speed <= 0f) return 0f;
        return speed * 5f * Math.Max(0f, multiplier);
    }
}
