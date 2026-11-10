using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FoundationPhysicsTests
{
    private readonly List<GameObject> created = new List<GameObject>();
    private GameObject New(string name, Vector3 position)
    {
        var go = new GameObject(name); go.transform.position = position; created.Add(go); return go;
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        foreach (var go in created) if (go != null) Object.Destroy(go);
        created.Clear();
        yield return null;
    }
    [UnityTest] public IEnumerator DamagePaysSessionOnceAndActuallyDestroysObject()
    {
        var wallet = SessionWallet.Current;
        if (wallet == null) wallet = New("Test Wallet", Vector3.zero).AddComponent<SessionWallet>();
        decimal initial = wallet.Earnings.Balance;
        var go = New("Breakable", Vector3.one * 1000f);
        go.SetActive(false);
        var damage = go.AddComponent<DamageOnImpact>();
        damage.maxHP = 10f; damage.objectValue = 40f; damage.wallet = wallet;
        go.SetActive(true);
        Assert.That(damage.ApplyDamage(4f).Reward, Is.EqualTo(16m));
        damage.ApplyDamage(1000f); damage.ApplyDamage(1000f);
        Assert.That(wallet.Earnings.Balance - initial, Is.EqualTo(40m));
        yield return null;
        Assert.That(go == null, Is.True);
    }
    [UnityTest] public IEnumerator RealCollisionDamagesObjectAndPaysWallet()
    {
        var wallet = SessionWallet.Current;
        if (wallet == null) wallet = New("Collision Wallet", Vector3.zero).AddComponent<SessionWallet>();
        decimal initial = wallet.Earnings.Balance;
        var floor = New("Impact Floor", new Vector3(1000, 0, 1000));
        floor.transform.localScale = new Vector3(10f, 0.2f, 10f);
        floor.AddComponent<BoxCollider>();
        var go = New("Falling Damage Target", new Vector3(1000, 3, 1000));
        go.transform.localScale = Vector3.one * 0.5f;
        go.AddComponent<BoxCollider>();
        go.AddComponent<Rigidbody>();
        var damage = go.AddComponent<DamageOnImpact>();
        damage.impactThreshold = 1f;
        Physics.SyncTransforms();
        float deadline = Time.time + 3f;
        while (damage != null && damage.State.CurrentHealth >= damage.State.MaxHealth && Time.time < deadline)
            yield return new WaitForFixedUpdate();
        Assert.That(damage != null, Is.True);
        Assert.That(damage.State.CurrentHealth, Is.LessThan(damage.State.MaxHealth));
        Assert.That(wallet.Earnings.Balance - initial, Is.EqualTo(damage.State.Rewarded));
        Assert.That(damage.State.Rewarded, Is.GreaterThan(0m));
    }
    [UnityTest] public IEnumerator FallingDoesNotDisableHorizontalAcceleration()
    {
        var go = New("Falling Player", Vector3.one * 1000f);
        var body = go.AddComponent<Rigidbody>(); body.useGravity = false;
        var movement = go.AddComponent<FunnyMovement>(); movement.enabled = false;
        body.linearVelocity = new Vector3(0f, -10f, 0f);
        typeof(FunnyMovement).GetField("movement", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(movement, Vector2.right);
        movement.SendMessage("FixedUpdate");
        yield return new WaitForFixedUpdate();
        Assert.That(body.linearVelocity.x, Is.GreaterThan(0.1f));
        Assert.That(body.linearVelocity.y, Is.EqualTo(-10f).Within(0.01f));
    }
    [UnityTest] public IEnumerator OneQueuedJumpProducesOnePhysicsImpulse()
    {
        var floor = New("Jump Floor", new Vector3(1000, 0, 1000));
        floor.AddComponent<BoxCollider>();
        var go = New("Jump Player", new Vector3(1000, 1, 1000));
        var body = go.AddComponent<Rigidbody>(); body.useGravity = false;
        var movement = go.AddComponent<FunnyMovement>(); movement.enabled = false;
        movement.groundCheck = New("Ground Probe", new Vector3(1000, 0.6f, 1000)).transform;
        movement.groundMask = 1; movement.jumpForce = 5f;
        Physics.SyncTransforms();
        var jump = (JumpRequest)typeof(FunnyMovement).GetField("jump", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(movement);
        jump.Queue(Time.time, 1f);
        movement.SendMessage("FixedUpdate"); movement.SendMessage("FixedUpdate");
        yield return new WaitForFixedUpdate();
        Assert.That(body.linearVelocity.y, Is.EqualTo(5f).Within(0.05f));
    }
    [UnityTest] public IEnumerator GrabForceIsBoundedAndDestroyedBodyReleasesSafely()
    {
        var hand = New("Grab Camera", new Vector3(1000, 10, 1000));
        var grabber = hand.AddComponent<ObjectGrabber>(); grabber.enabled = false;
        grabber.grabbableMask = ~0; grabber.grabDistance = 2f; grabber.grabForce = 10000f;
        var go = New("Grabbable", hand.transform.position + Vector3.forward);
        go.AddComponent<BoxCollider>();
        var body = go.AddComponent<Rigidbody>(); body.useGravity = false; body.mass = 0.01f;
        Physics.SyncTransforms();
        Assert.That(grabber.TryGrab(), Is.True);
        for (int i = 0; i < 10; i++)
        {
            grabber.SendMessage("FixedUpdate");
            yield return new WaitForFixedUpdate();
        }
        Assert.That(body.linearVelocity.magnitude, Is.LessThan(7f));
        grabber.Release(); Assert.That(grabber.IsHolding, Is.False);
        body.position = hand.transform.position + Vector3.forward; body.linearVelocity = Vector3.zero;
        Physics.SyncTransforms(); Assert.That(grabber.TryGrab(), Is.True);
        Object.Destroy(go); yield return null;
        Assert.DoesNotThrow(() => grabber.SendMessage("FixedUpdate"));
        Assert.That(grabber.IsHolding, Is.False);
        LogAssert.NoUnexpectedReceived();
    }
}
