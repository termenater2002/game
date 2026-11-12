using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
public static class MansionRuntimeProof
{
    const string ScenePath="Assets/Game/Scenes/MansionPrototype.unity";
    public static object Prepare()
    {
        if(Application.isPlaying)throw new Exception("Use Edit Mode");
        var movement=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();movement.enabled=false;
        var look=movement.GetComponentInChildren<FirstPersonCamera>();look.captureOnStart=false;look.enabled=false;
        look.GetComponent<ObjectGrabber>().enabled=false;look.GetComponent<MansionInteraction>().enabled=false;
        return new{prepared=true,position=movement.transform.position.ToString("F3"),minimum=movement.GetComponent<CapsuleCollider>().bounds.min.y};
    }
    public static async Task<object> Settle()
    {
        if(!Application.isPlaying)throw new Exception("Enter Play Mode");Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
        int startFrame=Time.frameCount;var original=UnityEngine.Object.FindObjectsByType<MansionProp>(FindObjectsSortMode.None).ToDictionary(p=>p.name,p=>p.transform.position);
        await Task.Delay(6000);
        var props=UnityEngine.Object.FindObjectsByType<MansionProp>(FindObjectsSortMode.None);var body=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>().GetComponent<Rigidbody>();
        var expected=JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(File.ReadAllText("Documentation/Mansion/placements.json")).ToDictionary(x=>(string)x["name"],x=>{var v=((string)x["position"]).Trim('(',')').Split(',');return new Vector3(float.Parse(v[0],System.Globalization.CultureInfo.InvariantCulture),float.Parse(v[1],System.Globalization.CultureInfo.InvariantCulture),float.Parse(v[2],System.Globalization.CultureInfo.InvariantCulture));});
        var moved=props.Select(p=>new{name=p.name,distance=expected.TryGetValue(p.name,out var pos)?Vector3.Distance(pos,p.transform.position):0,velocity=p.GetComponent<Rigidbody>().linearVelocity.magnitude,damaged=p.GetComponent<DamageOnImpact>().State.HealthFraction()}).ToArray();
        var result=new{frames=Time.frameCount-startFrame,original=original.Count,remaining=props.Length,broken=143-props.Length,unfastened=props.Count(p=>p.WasUnfastened),moving=moved.Where(p=>p.velocity>.08f).ToArray(),damaged=moved.Where(p=>p.damaged<.999f).ToArray(),largeMoves=moved.Where(p=>p.distance>.15f).ToArray(),player=body.position.ToString("F3"),foot=body.GetComponent<CapsuleCollider>().bounds.min.y};
        File.WriteAllText("Documentation/Mansion/controlled_settle.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
    }
}
public static class MansionHealthProofExtension
{
    public static float HealthFraction(this DamageState state)=>state.CurrentHealth/state.MaxHealth;
}
