using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerAnimationPhysicsTests
{
    readonly List<GameObject> created = new List<GameObject>();
    GameObject actor;
    Rigidbody body;
    Animator animator;
    PlayerAnimationDriver driver;
    void CreateActor()
    {
#if UNITY_EDITOR
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube); created.Add(floor);
        floor.transform.position = new Vector3(1000, -.1f, 1000); floor.transform.localScale = new Vector3(30, .2f, 30);
        var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Characters/Rider/Rider.prefab");
        actor = new GameObject("Rider Physics Test Actor"); created.Add(actor); actor.SetActive(false);
        actor.transform.position = new Vector3(1000, .025f, 1000); actor.transform.localScale = Vector3.one * .5f;
        var visual = Object.Instantiate(model,actor.transform,false);
        animator = visual.GetComponent<Animator>(); animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Game/Animations/PlayerMoveAnimation.controller");
        body = actor.AddComponent<Rigidbody>(); body.constraints = RigidbodyConstraints.FreezeRotation;
        var capsule = actor.AddComponent<CapsuleCollider>(); capsule.height = 3.5f; capsule.radius = .5f; capsule.center = new Vector3(0, 1.8f, 0);
        driver = actor.AddComponent<PlayerAnimationDriver>(); driver.characterAnimator=animator; driver.walkReferenceSpeed = .8f; driver.runReferenceSpeed = 2f; driver.blendSpaceScale = .2f;
        actor.SetActive(true); Physics.SyncTransforms();
#else
        Assert.Ignore("Requires Editor assets; exercise built-player animation separately.");
#endif
    }
    [UnityTearDown] public IEnumerator Cleanup() { foreach (var go in created) if (go != null) Object.Destroy(go); created.Clear(); yield return null; }
    [UnityTest] public IEnumerator MeasuredVelocitySelectsForwardBackwardAndStrafeWithoutMovingTheRoot()
    {
        CreateActor(); body.useGravity = false;
        var vectors = new[] { Vector3.forward * 2f, Vector3.back * .8f, Vector3.left * .8f, Vector3.right * .8f };
        var expected = new[] { "running.fbx", "Backward Walk.anim", "left strafe walking.fbx", "right strafe walking.fbx" };
        for (int i = 0; i < vectors.Length; i++)
        {
            body.linearVelocity = vectors[i]; var start = body.position; float elapsed = 0;
            var initialFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot).localRotation;
            while (elapsed < .4f) { yield return new WaitForFixedUpdate(); elapsed += Time.fixedDeltaTime; }
            yield return null;
            Assert.That(animator.applyRootMotion, Is.False);
            Assert.That(Vector3.Distance(body.linearVelocity, vectors[i]), Is.LessThan(.02f));
            Assert.That(Vector3.Dot(body.position - start, vectors[i].normalized), Is.GreaterThan(.1f));
#if UNITY_EDITOR
            Assert.That(animator.GetCurrentAnimatorClipInfo(0).Any(c => AssetDatabase.GetAssetPath(c.clip).EndsWith(expected[i]) && c.weight > .5f), Is.True, expected[i]);
#endif
            Assert.That(Quaternion.Angle(initialFoot, animator.GetBoneTransform(HumanBodyBones.LeftFoot).localRotation), Is.GreaterThan(.2f), "The rig must actually animate.");
        }
        body.linearVelocity = Vector3.zero;
        yield return new WaitForSeconds(.6f);
        Assert.That(animator.GetFloat("Speed"), Is.LessThan(.02f));
#if UNITY_EDITOR
        Assert.That(animator.GetCurrentAnimatorClipInfo(0).Any(c => AssetDatabase.GetAssetPath(c.clip).EndsWith("idle.fbx") && c.weight > .95f), Is.True);
#endif
    }
    [UnityTest] public IEnumerator ActualImpulsePassesThroughJumpFallLandingAndGroundedLocomotion()
    {
        CreateActor(); yield return new WaitForSeconds(.3f);
        Assert.That(driver.Grounded, Is.True);
        var observed = new HashSet<string>(); var states = new[] { "Locomotion", "Jump", "Fall", "Landing" };
        body.AddForce(Vector3.up * 4.2f, ForceMode.Impulse);
        float maxHeight = body.position.y;
        float until = Time.time + 2.2f;
        while (Time.time < until)
        {
            yield return null;
            maxHeight = Mathf.Max(maxHeight, body.position.y);
            foreach (var state in states) if (animator.GetCurrentAnimatorStateInfo(0).IsName(state)) observed.Add(state);
        }
        CollectionAssert.IsSubsetOf(states, observed); Assert.That(maxHeight, Is.GreaterThan(.5f));
        Assert.That(driver.Grounded, Is.True); Assert.That(driver.LandingCount, Is.GreaterThanOrEqualTo(1));
        Assert.That(animator.applyRootMotion, Is.False);
    }
    [UnityTest] public IEnumerator FallingWithoutJumpDoesNotPlayTakeoff()
    {
        CreateActor(); body.position += Vector3.up * 2f; body.linearVelocity = Vector3.zero;
        yield return new WaitForSeconds(.25f);
        Assert.That(driver.Grounded, Is.False); Assert.That(body.linearVelocity.y, Is.LessThan(-1f));
        Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Fall"), Is.True);
    }
    [UnityTest] public IEnumerator BackwardLoopsAndDirectionChangesKeepBodyAtRigidbody()
    {
        CreateActor();body.useGravity=false;
        var hips=animator.GetBoneTransform(HumanBodyBones.Hips);
        var directions=new[]{Vector3.forward*.8f,Vector3.back*.8f,Vector3.forward*2f,Vector3.back*.8f};
        float largestOffset=0,largestStep=0;Vector2 previous=Vector2.zero;bool sampled=false;
        foreach(var velocity in directions) {
            body.linearVelocity=velocity;float until=Time.time+1.35f;
            while(Time.time<until) {
                yield return null;
                var relative=actor.transform.InverseTransformPoint(hips.position);var horizontal=new Vector2(relative.x,relative.z);
                largestOffset=Mathf.Max(largestOffset,horizontal.magnitude);
                if(sampled) largestStep=Mathf.Max(largestStep,Vector2.Distance(horizontal,previous));previous=horizontal;sampled=true;
            }
        }
        // The regression put the pelvis ~3 local units forward at backward-state entry.
        // Allow normal gait sway, while checking multiple loops and both direction changes.
        float allowedOffset=actor.GetComponent<CapsuleCollider>().radius*.85f;
        Assert.That(largestOffset,Is.LessThan(allowedOffset),$"Pelvis must stay within the capsule radius; maximum single-frame step={largestStep:F4}.");
        Assert.That(largestStep,Is.LessThan(.12f),$"The body jumped at an animation loop / blend boundary; maximum offset={largestOffset:F4}.");
        Assert.That(animator.applyRootMotion,Is.False);
    }
    [UnityTest] public IEnumerator FPSKeepsShadowPassAndSideViewRestoresTheOriginalCamera()
    {
        CreateActor(); actor.SetActive(false);
        var fpsObject = new GameObject("Test FPS"); fpsObject.transform.SetParent(actor.transform,false); var fps = fpsObject.AddComponent<Camera>();
        var sideObject = new GameObject("Test Side"); sideObject.transform.SetParent(actor.transform,false); var side = sideObject.AddComponent<Camera>(); side.enabled = false;
        var renderers = actor.GetComponentsInChildren<SkinnedMeshRenderer>(true);Assert.That(renderers.Length,Is.GreaterThan(0));
        var visibility = actor.AddComponent<FirstPersonBodyVisibility>(); visibility.firstPersonCamera = fps; visibility.bodyRenderers = renderers;
        var cameraControl = actor.AddComponent<SideViewTestCamera>(); cameraControl.firstPersonCamera = fps; cameraControl.sideCamera = side;
        actor.SetActive(true); yield return null;
        bool sawShadowsOnly = false;
        void InspectPass(ScriptableRenderContext context, Camera camera) {
            if(camera==fps) sawShadowsOnly |= renderers.All(r=>r.shadowCastingMode==ShadowCastingMode.ShadowsOnly && !r.forceRenderingOff);
        }
        var rt = RenderTexture.GetTemporary(64,64,24);
        RenderPipelineManager.beginCameraRendering += InspectPass;
        try { fps.targetTexture = rt; fps.Render(); }
        finally { fps.targetTexture = null; RenderTexture.ReleaseTemporary(rt); RenderPipelineManager.beginCameraRendering -= InspectPass; }
        Assert.That(sawShadowsOnly, Is.True, "The actual FPS render must retain shadow casters.");
        Assert.That(renderers.All(r=>r.shadowCastingMode==ShadowCastingMode.On && !r.forceRenderingOff), Is.True, "Other cameras must see the body after the FPS pass.");
        cameraControl.SetSideView(true); Assert.That(side.enabled, Is.True); Assert.That(fps.enabled, Is.False);
        Assert.That(cameraControl.SideViewActive, Is.True);
        cameraControl.SetSideView(false); Assert.That(side.enabled, Is.False); Assert.That(fps.enabled, Is.True);
        Assert.That(cameraControl.SideViewActive, Is.False);
    }
}
