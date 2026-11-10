using NUnit.Framework;

public class FoundationStateTests
{
    [Test] public void OverkillPaysOnlyRemainingValue()
    {
        var health = new DamageState(100f, 10000m);
        Assert.That(health.Apply(25f).Reward, Is.EqualTo(2500m));
        var lethal = health.Apply(1000f);
        Assert.That(lethal.AppliedDamage, Is.EqualTo(75f));
        Assert.That(lethal.Reward, Is.EqualTo(7500m));
        Assert.That(health.Apply(1000f).Reward, Is.Zero);
        Assert.That(health.Rewarded, Is.EqualTo(10000m));
    }
    [Test] public void SmallHitsCannotFarmRoundingRewards()
    {
        var health = new DamageState(100f, 100m);
        for (int i = 0; i < 2000; i++) health.Apply(0.1f);
        Assert.That(health.IsDestroyed, Is.True);
        Assert.That(health.Rewarded, Is.EqualTo(100m));
    }
    [TestCase(-10f)] [TestCase(0f)] [TestCase(float.NaN)] [TestCase(float.PositiveInfinity)]
    public void InvalidDamageDoesNotChangeHealth(float amount)
    {
        var health = new DamageState(100f, 100m);
        Assert.That(health.Apply(amount).AppliedDamage, Is.Zero);
        Assert.That(health.CurrentHealth, Is.EqualTo(100f));
    }
    [Test] public void ImpactPreservesOriginalThresholdAndMultiplier()
    {
        Assert.That(DamageState.ImpactDamage(2.99f, 3f, 2f), Is.Zero);
        Assert.That(DamageState.ImpactDamage(3f, 3f, 2f), Is.EqualTo(30f));
    }
    [Test] public void JumpPressIsBufferedAndConsumedOnce()
    {
        var jump = new JumpRequest();
        jump.Queue(10f, 0.12f);
        Assert.That(jump.TryConsume(false, 10.02f), Is.False);
        Assert.That(jump.TryConsume(true, 10.04f), Is.True);
        Assert.That(jump.TryConsume(true, 10.06f), Is.False);
    }
    [Test] public void ExpiredJumpDoesNotTriggerOnLaterLanding()
    {
        var jump = new JumpRequest();
        jump.Queue(10f, 0.12f);
        Assert.That(jump.TryConsume(true, 10.2f), Is.False);
    }
    [Test] public void EarningsAccumulateAndResetWithoutNegativeRewards()
    {
        var earnings = new SessionEarnings();
        int notifications = 0;
        earnings.Changed += _ => notifications++;
        earnings.Add(12.34m); earnings.Add(-100m); earnings.Add(0.01m);
        Assert.That(earnings.Balance, Is.EqualTo(12.35m));
        earnings.Reset();
        Assert.That(earnings.Balance, Is.Zero);
        Assert.That(notifications, Is.EqualTo(3));
    }
}
