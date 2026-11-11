using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public static class AutoWalkRegression
{
    static object Inspect(FunnyMovement m)
    {
        var body=m.GetComponent<Rigidbody>();var c=m.GetComponent<CapsuleCollider>();var bounds=c.bounds;
        float skin=Mathf.Max(.006f,bounds.size.y*.08f);var start=new Vector3(bounds.center.x,bounds.min.y+skin,bounds.center.z);
        var field=typeof(FunnyMovement).GetField("movement",BindingFlags.Instance|BindingFlags.NonPublic);
        return new{frame=Time.frameCount,position=body.position.ToString("F6"),velocity=body.linearVelocity.ToString("F6"),speed=Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up).magnitude,
            m.IsGrounded,animationGrounded=m.GetComponent<PlayerAnimationDriver>().Grounded,groundMask=m.groundMask.value,storedInput=((Vector2)field.GetValue(m)).ToString("F3"),
            rawInput=new[]{Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")},shift=Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift),focused=Application.isFocused,
            hits=Physics.RaycastAll(start,Vector3.down,skin*2f,~0,QueryTriggerInteraction.Ignore).Select(h=>new{h.collider.name,layer=h.collider.gameObject.layer,layerName=LayerMask.LayerToName(h.collider.gameObject.layer),h.distance,normal=h.normal.ToString("F3"),selectedByGroundMask=(m.groundMask.value&(1<<h.collider.gameObject.layer))!=0}).ToArray()};
    }
    public static async Task<object> Before()
    {
        var m=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();var rows=new List<object>();
        for(int i=0;i<4;i++){rows.Add(Inspect(m));await Task.Delay(400);}
        var result=new{samples=rows};Directory.CreateDirectory("Documentation/FirstPersonWalk");
        File.WriteAllText("Documentation/FirstPersonWalk/AutoWalkBefore.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
    }
    public static async Task<object> OnTheActualRoad()
    {
        if(!Application.isPlaying)throw new Exception("Use the running SampleScene player.");
        var m=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();var body=m.GetComponent<Rigidbody>();var look=m.GetComponentInChildren<FirstPersonCamera>();
        var originalPosition=body.position;var originalVelocity=body.linearVelocity;float pitch=look.Pitch,yaw=look.Yaw;bool enabled=m.enabled;
        var path="Documentation/FirstPersonWalk/AutoWalkBefore.json";
        var before=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(path));
        var xyz=((string)before["samples"][0]["position"]).Trim('(',')').Split(',').Select(x=>float.Parse(x,CultureInfo.InvariantCulture)).ToArray();
        var stages=new List<object>();int startFrame=Time.frameCount;
        try
        {
            look.ReleaseCursor();m.enabled=false;body.position=new Vector3(xyz[0],xyz[1],xyz[2]);body.linearVelocity=Vector3.zero;look.SetLookAngles(pitch,0);Physics.SyncTransforms();
            await Task.Delay(250);var road=Inspect(m);
            foreach(var stage in new[]{("Sprint",Vector2.up,true,.6f),("ReleaseShift_WStillHeld",Vector2.up,false,.4f),("ReleaseAllKeys",Vector2.zero,false,1.2f),("RemainIdle",Vector2.zero,false,.8f)})
            {
                float until=Time.time+stage.Item4;Vector3 start=body.position;
                while(Time.time<until){m.SetMoveInput(stage.Item2,stage.Item3);m.SendMessage("FixedUpdate");await Task.Delay(20);}
                float speed=Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up).magnitude;
                float expected=stage.Item2==Vector2.zero?0:stage.Item3?m.SprintSpeed:m.WalkSpeed;
                if(Mathf.Abs(speed-expected)>.015f)throw new Exception(stage.Item1+": unexpected speed "+speed+", expected "+expected);
                float travel=Vector3.Distance(body.position,start);
                float playbackRate=m.GetComponent<PlayerAnimationDriver>().characterAnimator.GetFloat("PlaybackRate");
                if(playbackRate>1.01f)throw new Exception("Animation cadence accelerated: "+playbackRate);
                if(stage.Item1=="RemainIdle" && (travel>.0005f || speed>.0001f))throw new Exception("Player kept drifting after release: "+travel+", speed "+speed);
                stages.Add(new{stage=stage.Item1,speed,expected,travel,playbackRate,m.speedMultiplier,m.IsGrounded,frame=Time.frameCount});
            }
            var result=new{road,startFrame,lastFrame=Time.frameCount,framesAdvanced=Time.frameCount-startFrame,stages,stopped=true};
            Directory.CreateDirectory("Documentation/FirstPersonWalk/AutoWalkFix");
            File.WriteAllText("Documentation/FirstPersonWalk/AutoWalkFix/ActualRoad.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
        }
        finally
        {
            m.SetMoveInput(Vector2.zero);body.position=originalPosition;body.linearVelocity=originalVelocity;look.SetLookAngles(pitch,yaw);m.enabled=enabled;Physics.SyncTransforms();
        }
    }
}
