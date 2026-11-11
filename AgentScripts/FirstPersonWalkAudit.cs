using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FirstPersonWalkAudit
{
    static float[] V(Vector3 v)=>new[]{v.x,v.y,v.z};
    public static object Inspect()
    {
        var movement=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();
        if(movement==null)throw new Exception("No physical player in the open scene.");
        var driver=movement.GetComponent<PlayerAnimationDriver>();var animator=driver.CharacterAnimator;
        var look=movement.GetComponentInChildren<FirstPersonCamera>();var camera=look.GetComponent<Camera>();
        var capsule=movement.GetComponent<CapsuleCollider>();var body=movement.GetComponent<Rigidbody>();
        var visibility=movement.GetComponent<FirstPersonBodyVisibility>();
        return new{scene=SceneManager.GetActiveScene().path,playing=Application.isPlaying,frame=Time.frameCount,
            movement=new{movement.moveForce,movement.maxSpeed,movement.WalkSpeed,movement.SprintSpeed,movement.IsGrounded,body.linearDamping,velocity=V(body.linearVelocity),position=V(body.position),scale=V(movement.transform.lossyScale)},
            capsule=new{capsule.height,capsule.radius,center=V(capsule.center),worldHeight=capsule.bounds.size.y},
            camera=new{localPosition=V(camera.transform.localPosition),worldPosition=V(camera.transform.position),localAngles=V(camera.transform.localEulerAngles),camera.fieldOfView,camera.nearClipPlane},
            firstPerson=new{visibility.showFirstPersonBody,renderers=visibility.FirstPersonRenderers.Count,gaitOffset=V(look.GaitOffset)},
            animation=new{driver.walkReferenceSpeed,driver.runReferenceSpeed,driver.blendSpaceScale,driver.maximumPlaybackRate,animator.humanScale,animator.applyRootMotion,
                parameters=animator.parameters.Where(p=>p.type==AnimatorControllerParameterType.Float).Select(p=>new{p.name,value=animator.GetFloat(p.name)}).ToArray(),
                clips=animator.runtimeAnimatorController.animationClips.Select(c=>new{c.name,c.length,averageSpeed=V(c.averageSpeed),c.isLooping}).ToArray()},
            meshes=visibility.bodyRenderers.OfType<SkinnedMeshRenderer>().Select(r=>new{r.name,r.sharedMesh.isReadable,vertices=r.sharedMesh.vertexCount,triangles=r.sharedMesh.triangles.Length/3,boundsCenter=V(r.bounds.center),boundsSize=V(r.bounds.size),bones=r.bones.Length}).ToArray()};
    }
}
