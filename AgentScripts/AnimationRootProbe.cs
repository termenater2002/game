using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Threading.Tasks;

public static class AnimationRootProbe
{
    public static async Task<object> SampleLive()
    {
        if(!Application.isPlaying) throw new Exception("Enter Play Mode first.");
        var driver=UnityEngine.Object.FindAnyObjectByType<PlayerAnimationDriver>();var animator=driver.CharacterAnimator;
        var hips=animator.GetBoneTransform(HumanBodyBones.Hips);var body=driver.GetComponent<Rigidbody>();
        var samples=new List<object>();float start=Time.time,maximum=0,largestStep=0;Vector2 previous=Vector2.zero;bool first=true;
        while(Time.time-start<1.2f) {
            await Task.Delay(16);var p=driver.transform.InverseTransformPoint(hips.position);var horizontal=new Vector2(p.x,p.z);
            maximum=Mathf.Max(maximum,horizontal.magnitude);if(!first) largestStep=Mathf.Max(largestStep,Vector2.Distance(previous,horizontal));first=false;previous=horizontal;
            samples.Add(new{frame=Time.frameCount,time=Time.time,localHipX=p.x,localHipZ=p.z,localVelocityZ=driver.LocalVelocity.z,physicalPosition=body.position.ToString("F4")});
        }
        return Newtonsoft.Json.JsonConvert.SerializeObject(new{maximumLocalHipOffset=maximum,largestLocalStep=largestStep,worldScale=driver.transform.lossyScale.x,animator.applyRootMotion,samples});
    }
    public static object Inspect()
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Characters/Rider/Rider.prefab");
        var actor=UnityEngine.Object.Instantiate(prefab);actor.name="Animation root probe (temporary)";actor.transform.position=Vector3.one*1000;
        var animator=actor.GetComponent<Animator>();animator.runtimeAnimatorController=null;animator.applyRootMotion=false;
        var graph=PlayableGraph.Create("Animation root position probe");graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        try {
            var output=AnimationPlayableOutput.Create(graph,"Probe",animator);graph.Play();
            var paths=new[]{"Assets/Game/Animations/Pack/walking.fbx","Assets/Game/Animations/Backward Walk.anim","Assets/Game/Animations/Pack/running.fbx","Assets/Game/Animations/Pack/left strafe walking.fbx"};
            var result=new List<object>();
            foreach(string path in paths) {
                var clip=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().First(c=>!c.name.StartsWith("__preview"));
                var p=AnimationClipPlayable.Create(graph,clip);p.SetApplyFootIK(false);output.SetSourcePlayable(p);
                var samples=new List<object>();float maximum=0;
                foreach(float u in new[]{0f,.01f,.1f,.25f,.5f,.75f,.99f,1.01f,1.5f,1.99f}) {
                    p.SetTime(clip.length*u);graph.Evaluate(0);
                    var hips=actor.transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.Hips).position);
                    maximum=Mathf.Max(maximum,new Vector2(hips.x,hips.z).magnitude);samples.Add(new{u,x=hips.x,z=hips.z});
                }
                var settings=AnimationUtility.GetAnimationClipSettings(clip);
                result.Add(new{path,settings.loopBlendPositionXZ,settings.keepOriginalPositionXZ,maximumHipOffset=maximum,samples});
                output.SetSourcePlayable(Playable.Null);graph.DestroyPlayable(p);
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(result);
        } finally {graph.Destroy();UnityEngine.Object.DestroyImmediate(actor);}
    }
}
