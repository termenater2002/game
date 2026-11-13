#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

public class GroundSupportAnimationTests
{
    readonly List<GameObject> created = new List<GameObject>();
    Rigidbody body;
    PlayerAnimationDriver driver;
    Animator animator;
    FunnyMovement motor;
    GameObject rightFloor;
    static readonly Vector3 Origin = new Vector3(1200, 0, 1200);

    GameObject New(string name, Vector3 position)
    {
        var go = new GameObject(name); go.transform.position = position; created.Add(go); return go;
    }
    void Create(float startZ = 0)
    {
        // A real 10 cm seam: the capsule is supported by both edges, but its centre ray misses.
        foreach (int sign in new[] { -1, 1 })
        {
            var floor = New("Floor edge", Origin + new Vector3(0, -.1f, sign * 1.55f));
            floor.AddComponent<BoxCollider>().size = new Vector3(8, .2f, 3);
            if (sign > 0) rightFloor = floor;
        }
        var actor = New("Seam animation actor", Origin + new Vector3(0, .02f, startZ)); actor.SetActive(false);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Characters/StyleV3/RiderV3/RiderV3.prefab");
        animator = Object.Instantiate(prefab, actor.transform, false).GetComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Game/Animations/PlayerMoveAnimation.controller");
        body = actor.AddComponent<Rigidbody>(); body.mass = 75; body.constraints = RigidbodyConstraints.FreezeRotation;
        var capsule = actor.AddComponent<CapsuleCollider>(); capsule.height = 1.78f; capsule.radius = .25f; capsule.center = Vector3.up * .89f;
        motor = actor.AddComponent<FunnyMovement>(); motor.groundMask = ~0; motor.maxSpeed = 5.4f; motor.speedMultiplier = 2; motor.moveForce = 45;
        driver = actor.AddComponent<PlayerAnimationDriver>(); driver.characterAnimator = animator; driver.walkReferenceSpeed = 1.1f; driver.runReferenceSpeed = 2.7f; driver.maximumPlaybackRate = 1;
        actor.SetActive(true); motor.enabled = false; Physics.SyncTransforms();
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        foreach (var go in created) if (go != null) Object.Destroy(go);
        created.Clear(); yield return null;
    }
    void AssertLocomotion()
    {
        Assert.That(driver.Grounded, Is.True, "A supported capsule must not report airborne over the seam. velocity=" + body.linearVelocity + ", motor=" + motor.IsGrounded);
        var state = animator.GetCurrentAnimatorStateInfo(0);
        Assert.That(state.IsName("Locomotion"), Is.True,
            $"state={state.shortNameHash} time={state.normalizedTime:F2}, transition={animator.IsInTransition(0)}, grounded={animator.GetBool("Grounded")}, clips={string.Join(",", System.Array.ConvertAll(animator.GetCurrentAnimatorClipInfo(0), c=>c.clip.name))}, velocity={body.linearVelocity}, y={body.position.y:F4}");
        if (animator.IsInTransition(0)) Assert.That(animator.GetNextAnimatorStateInfo(0).IsName("Locomotion"), Is.True);
    }
    [UnityTest] public IEnumerator StandingAcrossNarrowGapKeepsLocomotion()
    {
        Create(); yield return new WaitForSeconds(.65f);
        Assert.That(body.position.y, Is.GreaterThan(Origin.y - .03f), "Physics must actually support the capsule.");
        for (int i = 0; i < 20; i++) { yield return null; AssertLocomotion(); }
        Assert.That(driver.LandingCount, Is.Zero, "The seam must not produce a fake landing.");
    }
    [UnityTest] public IEnumerator WalkingAndRunningAcrossGapDoNotTriggerAirborneAnimation()
    {
        Create(-1.1f); yield return new WaitForSeconds(.65f);
        foreach (float speed in new[] { 2.2f, 5.4f })
        {
            body.position = Origin + new Vector3(0, .02f, -1.1f); body.linearVelocity = Vector3.zero; Physics.SyncTransforms();
            yield return new WaitForSeconds(.25f); int initialLandings = driver.LandingCount;
            float deadline = Time.time + 2;
            while (body.position.z < Origin.z + 1.1f && Time.time < deadline)
            {
                motor.SetMoveInput(Vector2.up, speed > 3); motor.SendMessage("FixedUpdate");
                yield return new WaitForFixedUpdate(); yield return null; AssertLocomotion();
            }
            Assert.That(body.position.z, Is.GreaterThan(Origin.z + 1));
            Assert.That(driver.LandingCount, Is.EqualTo(initialLandings));
        }
    }
    [UnityTest] public IEnumerator RealJumpOverGapStillPlaysJumpFallAndLanding()
    {
        Create(); yield return new WaitForSeconds(.65f); AssertLocomotion();
        body.AddForce(Vector3.up * 4.2f, ForceMode.VelocityChange);
        yield return new WaitForFixedUpdate(); yield return null;
        Assert.That(driver.Grounded, Is.False, "Real takeoff must bypass any grounding grace period.");
        var seen = new HashSet<string>(); float deadline = Time.time + 2.2f;
        while (Time.time < deadline)
        {
            foreach (string state in new[] { "Jump", "Fall", "Landing", "Locomotion" })
                if (animator.GetCurrentAnimatorStateInfo(0).IsName(state)) seen.Add(state);
            yield return null;
        }
        CollectionAssert.AreEquivalent(new[] { "Jump", "Fall", "Landing", "Locomotion" }, seen);
        Assert.That(driver.Grounded, Is.True); Assert.That(driver.LandingCount, Is.EqualTo(1));
    }
    [UnityTest] public IEnumerator BriefContactLossDoesNotProduceAFakeLanding()
    {
        Create(); yield return new WaitForSeconds(.65f); AssertLocomotion();
        created[0].SetActive(false); created[1].SetActive(false);
        float until = Time.time + .04f;
        while (Time.time < until) { yield return null; AssertLocomotion(); }
        created[0].SetActive(true); created[1].SetActive(true); Physics.SyncTransforms();
        yield return new WaitForSeconds(.15f); AssertLocomotion();
        Assert.That(driver.LandingCount, Is.Zero);
    }
    [UnityTest] public IEnumerator SmallRequestedJumpBypassesContactGraceImmediately()
    {
        Create(); yield return new WaitForSeconds(.65f); AssertLocomotion();
        motor.jumpForce = body.mass * .8f;
        var request = (JumpRequest)typeof(FunnyMovement).GetField("jump", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(motor);
        request.Queue(Time.time, .12f); motor.SendMessage("FixedUpdate");
        yield return new WaitForFixedUpdate(); yield return null;
        Assert.That(body.linearVelocity.y, Is.GreaterThan(.2f));
        Assert.That(driver.Grounded, Is.False, "Explicit jumps must not be hidden as tiny floor bumps.");
    }
    [UnityTest] public IEnumerator LeavingActualLedgeStillPlaysFallWithoutTakeoff()
    {
        Create(-.6f); rightFloor.SetActive(false); yield return new WaitForSeconds(.65f); AssertLocomotion();
        float deadline = Time.time + .7f;
        while (Time.time < deadline)
        {
            body.linearVelocity = new Vector3(0, body.linearVelocity.y, 2.2f); yield return null;
            Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"), Is.False);
        }
        Assert.That(driver.Grounded, Is.False); Assert.That(body.position.y, Is.LessThan(-.2f));
        Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Fall"), Is.True);
    }
}
#endif
