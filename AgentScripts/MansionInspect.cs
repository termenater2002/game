using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
public static class MansionInspect
{
 public static object State()
 {
  var scene=SceneManager.GetActiveScene();var props=UnityEngine.Object.FindObjectsByType<MansionProp>(FindObjectsSortMode.None);var bodies=UnityEngine.Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);var p=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();
  var result=new {scene=scene.path,playing=Application.isPlaying,frame=Time.frameCount,props=props.Length,damage=props.Count(x=>x.GetComponent<DamageOnImpact>().State!=null&&x.GetComponent<DamageOnImpact>().State.CurrentHealth<x.GetComponent<DamageOnImpact>().maxHP-.001f),unfastened=props.Count(x=>x.WasUnfastened),underfloor=bodies.Where(x=>x.position.y<-.5f).Select(x=>new{x.name,pos=x.position.ToString("F2")}).ToArray(),fast=bodies.Count(x=>x.linearVelocity.magnitude>3),player=p?.transform.position.ToString("F3"),grounded=p?.IsGrounded,bodyHeight=p?.GetComponent<CapsuleCollider>().bounds.size.y};
  Directory.CreateDirectory("Documentation/Mansion");File.WriteAllText("Documentation/Mansion/settling.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
 }
 public static object PlanView(bool cutaway=true)
 {
  var roof=GameObject.Find("Ceilings — hide in editor for plan view");if(roof!=null)foreach(var r in roof.GetComponentsInChildren<Renderer>())r.shadowCastingMode=cutaway?UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly:UnityEngine.Rendering.ShadowCastingMode.On;return new{roof=cutaway?"cutaway":"normal"};
 }
}
