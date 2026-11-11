using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FirstPersonGaitTests
{
    readonly List<GameObject> created=new List<GameObject>();
    GameObject actor;Rigidbody body;FunnyMovement motor;PlayerAnimationDriver driver;FirstPersonCamera look;Camera camera;FirstPersonBodyVisibility visibility;
    void Create()
    {
#if UNITY_EDITOR
        var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);created.Add(floor);
        floor.transform.position=new Vector3(1000,-.1f,1000);floor.transform.localScale=new Vector3(10,.2f,10);
        actor=new GameObject("FPS walking test");created.Add(actor);actor.SetActive(false);
        actor.transform.position=new Vector3(1000,.025f,1000);actor.transform.localScale=Vector3.one*.1f;
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Characters/StyleV3/RiderV3/RiderV3.prefab");
        var visual=Object.Instantiate(prefab,actor.transform,false);var animator=visual.GetComponent<Animator>();
        body=actor.AddComponent<Rigidbody>();body.constraints=RigidbodyConstraints.FreezeRotation;
        var capsule=actor.AddComponent<CapsuleCollider>();capsule.height=3.5152755f;capsule.radius=.5f;capsule.center=new Vector3(0,1.7854726f,0);
        motor=actor.AddComponent<FunnyMovement>();motor.groundMask=~0;
        driver=actor.AddComponent<PlayerAnimationDriver>();driver.characterAnimator=animator;driver.walkReferenceSpeed=.2f;driver.runReferenceSpeed=.5f;driver.blendSpaceScale=.8f;
        var eye=new GameObject("FPS test eye");eye.transform.SetParent(actor.transform,false);camera=eye.AddComponent<Camera>();
        camera.enabled=false;camera.fieldOfView=60;camera.farClipPlane=30;
        look=eye.AddComponent<FirstPersonCamera>();look.playerBody=actor.transform;look.captureOnStart=false;
        visibility=actor.AddComponent<FirstPersonBodyVisibility>();visibility.firstPersonCamera=camera;visibility.bodyRenderers=visual.GetComponentsInChildren<Renderer>();
        actor.SetActive(true);motor.enabled=false;Physics.SyncTransforms();
#else
        Assert.Ignore("Requires Editor character assets.");
#endif
    }
    IEnumerator Drive(Vector2 input,bool sprint,float seconds)
    {
        float end=Time.time+seconds;
        do{motor.SetMoveInput(input,sprint);motor.SendMessage("FixedUpdate");yield return new WaitForFixedUpdate();}while(Time.time<end);
    }
    [UnityTearDown]public IEnumerator Cleanup(){foreach(var go in created)if(go!=null)Object.Destroy(go);created.Clear();yield return null;}
    [UnityTest]public IEnumerator WalkSprintAndReleaseUseRealPhysicsAtTheAnimationSpeeds()
    {
        Create();yield return Drive(Vector2.zero,false,.2f);Assert.That(driver.Grounded,Is.True);
        yield return Drive(Vector2.up,false,.5f);
        Assert.That(Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up).magnitude,Is.EqualTo(.2f).Within(.025f));
        yield return Drive(Vector2.up,true,.5f);
        Assert.That(Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up).magnitude,Is.EqualTo(.5f).Within(.025f));
        float y=body.position.y;
        yield return Drive(Vector2.zero,false,.2f);
        Assert.That(Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up).magnitude,Is.LessThan(.01f),"Released movement must not coast.");
        Assert.That(body.position.y,Is.EqualTo(y).Within(.025f));
        Assert.That(driver.CharacterAnimator.applyRootMotion,Is.False);
    }
    [UnityTest]public IEnumerator CameraHasGroundedStepMotionAndSettlesAtRestAndInAir()
    {
        Create();yield return Drive(Vector2.up,false,.5f);
        float maximum=0;
        for(int i=0;i<24;i++)
        {
            motor.SetMoveInput(Vector2.up);motor.SendMessage("FixedUpdate");yield return new WaitForFixedUpdate();
            maximum=Mathf.Max(maximum,actor.transform.TransformVector(look.GaitOffset).magnitude);
        }
        Assert.That(maximum,Is.GreaterThan(.0002f),"A moving grounded player needs actual camera step motion.");
        yield return Drive(Vector2.zero,false,.7f);
        Assert.That(actor.transform.TransformVector(look.GaitOffset).magnitude,Is.LessThan(.0001f));
        body.position+=Vector3.up;Physics.SyncTransforms();yield return new WaitForSeconds(.3f);
        Assert.That(driver.Grounded,Is.False);
        Assert.That(actor.transform.TransformVector(look.GaitOffset).magnitude,Is.LessThan(.0002f));
    }
    [UnityTest]public IEnumerator AirInputDoesNotBoostPastWalkingSpeedOrCancelAnExternalImpulse()
    {
        Create();body.useGravity=false;body.position+=Vector3.up*2f;Physics.SyncTransforms();
        body.linearVelocity=new Vector3(0,-1,.2f);
        yield return Drive(Vector2.up,false,.25f);
        Assert.That(driver.Grounded,Is.False);
        Assert.That(body.linearVelocity.z,Is.EqualTo(.2f).Within(.01f));
        Assert.That(body.linearVelocity.y,Is.EqualTo(-1f).Within(.01f));
        body.linearVelocity=new Vector3(0,-1,1f);
        yield return Drive(Vector2.zero,false,.25f);
        Assert.That(body.linearVelocity.z,Is.EqualTo(1f).Within(.01f),"Airborne external momentum must not receive ground braking.");
        Assert.That(body.linearVelocity.y,Is.EqualTo(-1f).Within(.01f));
    }
    [UnityTest]public IEnumerator FPSActuallyDrawsTheAnimatedBodyWithoutHeadAndRestoresOtherCameras()
    {
        Create();yield return Drive(Vector2.zero,false,.3f);look.SetLookAngles(80,0);
        Assert.That(camera.nearClipPlane,Is.LessThan(actor.GetComponent<CapsuleCollider>().bounds.size.y*.04f));
        Assert.That(visibility.FirstPersonRenderers.Count,Is.GreaterThan(0));
        var original=visibility.bodyRenderers.OfType<SkinnedMeshRenderer>().First();
        var copy=visibility.FirstPersonRenderers.First();var bones=copy.bones;var weights=copy.sharedMesh.boneWeights;
        var head=driver.CharacterAnimator.GetBoneTransform(HumanBodyBones.Head);var neck=driver.CharacterAnimator.GetBoneTransform(HumanBodyBones.Neck);
        float Weight(int i,float w)=>bones[i]==head||bones[i]==neck||bones[i].IsChildOf(head)?w:0;
        foreach(int index in copy.sharedMesh.triangles)
        {
            var w=weights[index];Assert.That(Weight(w.boneIndex0,w.weight0)+Weight(w.boneIndex1,w.weight1)+Weight(w.boneIndex2,w.weight2)+Weight(w.boneIndex3,w.weight3),Is.LessThanOrEqualTo(.5f));
        }
        Assert.That(copy.sharedMesh.triangles.Length,Is.LessThan(original.sharedMesh.triangles.Length));
        bool passSawBody=false;
        void Inspect(ScriptableRenderContext context,Camera current){if(current==camera)passSawBody|=visibility.FirstPersonRenderers.Any(r=>!r.forceRenderingOff)&&visibility.bodyRenderers.All(r=>r.shadowCastingMode==ShadowCastingMode.ShadowsOnly);}
        var rt=RenderTexture.GetTemporary(128,128,24);var on=new Texture2D(128,128,TextureFormat.RGB24,false);var off=new Texture2D(128,128,TextureFormat.RGB24,false);var old=RenderTexture.active;
        RenderPipelineManager.beginCameraRendering+=Inspect;
        try
        {
            camera.targetTexture=rt;visibility.showFirstPersonBody=true;camera.Render();RenderTexture.active=rt;on.ReadPixels(new Rect(0,0,128,128),0,0);on.Apply();
            visibility.showFirstPersonBody=false;camera.Render();RenderTexture.active=rt;off.ReadPixels(new Rect(0,0,128,128),0,0);off.Apply();
            var a=on.GetPixels32();var b=off.GetPixels32();int changed=0;
            for(int i=0;i<a.Length;i++)if(Mathf.Abs(a[i].r-b[i].r)+Mathf.Abs(a[i].g-b[i].g)+Mathf.Abs(a[i].b-b[i].b)>12)changed++;
            Assert.That(changed,Is.GreaterThan(150),"FPS body must be visible in actual rendered pixels.");
            Assert.That(passSawBody,Is.True);
            Assert.That(visibility.FirstPersonRenderers.All(r=>r.forceRenderingOff),Is.True);
            Assert.That(visibility.bodyRenderers.All(r=>r.shadowCastingMode==ShadowCastingMode.On),Is.True);
        }
        finally
        {
            RenderPipelineManager.beginCameraRendering-=Inspect;camera.targetTexture=null;RenderTexture.active=old;
            RenderTexture.ReleaseTemporary(rt);Object.Destroy(on);Object.Destroy(off);
        }
    }
}
