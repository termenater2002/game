using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public static class FirstPersonWalkProof
{
    const string Output="Documentation/FirstPersonWalk/";
    static readonly List<string> Captures=new List<string>();
    static int Capture(Camera camera,string name)
    {
        Directory.CreateDirectory(Output);var old=camera.targetTexture;var active=RenderTexture.active;float aspect=camera.aspect;
        var rt=RenderTexture.GetTemporary(1280,720,24);var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
        try{camera.targetTexture=rt;camera.aspect=16f/9f;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Output+name+".png",image.EncodeToPNG());}
        finally{camera.targetTexture=old;camera.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.Destroy(image);}
        Captures.Add(Output+name+".png");return Time.frameCount;
    }
    public static object Views()
    {
        if(!Application.isPlaying)throw new Exception("Use Play Mode.");
        var motor=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();var look=motor.GetComponentInChildren<FirstPersonCamera>();
        var side=motor.GetComponent<SideViewTestCamera>();side.SetSideView(false);look.ReleaseCursor();
        float pitch=look.Pitch,yaw=look.Yaw;var camera=look.GetComponent<Camera>();
        foreach(float angle in new[]{0f,60f,75f,85f}){look.SetLookAngles(angle,yaw);Capture(camera,"FPS_Down_"+angle.ToString("00"));}
        look.SetLookAngles(pitch,yaw);
        return new{frame=Time.frameCount,nearClip=camera.nearClipPlane,motor.WalkSpeed,motor.SprintSpeed,firstPersonRenderers=motor.GetComponent<FirstPersonBodyVisibility>().FirstPersonRenderers.Count};
    }
    public static async Task<object> Motion()
    {
        if(!Application.isPlaying)throw new Exception("Use Play Mode.");
        Directory.CreateDirectory(Output);var motor=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();var body=motor.GetComponent<Rigidbody>();
        var driver=motor.GetComponent<PlayerAnimationDriver>();var look=motor.GetComponentInChildren<FirstPersonCamera>();
        var camera=look.GetComponent<Camera>();bool wasEnabled=motor.enabled;float pitch=look.Pitch,yaw=look.Yaw;
        var start=body.position;var oldVelocity=body.linearVelocity;look.ReleaseCursor();motor.enabled=false;
        var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.name="Temporary gait verification floor";
        floor.transform.position=new Vector3(1000,-.1f,1000);floor.transform.localScale=new Vector3(10,.2f,10);
        for(int layer=0;layer<32;layer++)if((motor.groundMask.value&(1<<layer))!=0){floor.layer=layer;break;}
        body.position=new Vector3(1000,.025f,1000);body.linearVelocity=Vector3.zero;Physics.SyncTransforms();
        var rows=new List<object>();var stageReports=new List<object>();int firstFrame=Time.frameCount;
        try
        {
            look.SetLookAngles(75,0);
            foreach(var stage in new[]{("Walk",Vector2.up,false,1.2f),("Sprint",Vector2.up,true,1.0f),("Stop",Vector2.zero,false,.6f),("Backward",Vector2.down,false,1.0f)})
            {
                float until=Time.time+stage.Item4;int count=0;float maximumBob=0;Vector3 before=body.position;
                while(Time.time<until)
                {
                    motor.SetMoveInput(stage.Item2,stage.Item3);motor.SendMessage("FixedUpdate");await Task.Delay(20);
                    maximumBob=Mathf.Max(maximumBob,look.transform.parent.TransformVector(look.GaitOffset).magnitude);
                    if(count%6==0)rows.Add(new{stage=stage.Item1,frame=Time.frameCount,time=Time.time,position=body.position.ToString("F5"),velocity=body.linearVelocity.ToString("F5"),bob=look.GaitOffset.ToString("F5"),driver.Grounded,phase=driver.CharacterAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime,playbackRate=driver.CharacterAnimator.GetFloat("PlaybackRate")});
                    if(stage.Item1=="Walk" && count%12==0)Capture(camera,"Walking_"+count.ToString("000"));
                    count++;
                }
                Capture(camera,stage.Item1);
                float measured=Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up).magnitude;
                float expected=stage.Item1=="Stop"?0:stage.Item3?motor.SprintSpeed:motor.WalkSpeed;
                if(Mathf.Abs(measured-expected)>.03f)throw new Exception(stage.Item1+" did not reach the expected physical speed: "+measured+" vs "+expected);
                stageReports.Add(new{stage=stage.Item1,frames=Time.frameCount-firstFrame,measuredSpeed=measured,travel=Vector3.Distance(before,body.position),maximumBob});
            }
            var report=new{firstFrame,lastFrame=Time.frameCount,frameCountAdvanced=Time.frameCount-firstFrame,motor.WalkSpeed,motor.SprintSpeed,stages=stageReports,samples=rows,images=Captures};
            File.WriteAllText(Output+"Motion.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
        }
        finally
        {
            motor.SetMoveInput(Vector2.zero);body.position=start;body.linearVelocity=oldVelocity;Physics.SyncTransforms();motor.enabled=wasEnabled;
            look.SetLookAngles(pitch,yaw);
            UnityEngine.Object.Destroy(floor);
        }
    }
}
