#pragma warning disable CS0618
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Object=UnityEngine.Object;
public static class MansionCompositeLive
{
 const string Folder="Documentation/Mansion/Composites";
 static FunnyMovement Player=>Object.FindAnyObjectByType<FunnyMovement>();
 static FirstPersonCamera Look=>Player.GetComponentInChildren<FirstPersonCamera>();
 static MansionInteraction Tool=>Look.GetComponent<MansionInteraction>();
 static ObjectGrabber Grab=>Look.GetComponent<ObjectGrabber>();
 static void Save(string name,object r){Directory.CreateDirectory(Folder);File.WriteAllText(Folder+"/"+name+".json",JsonConvert.SerializeObject(r,Formatting.Indented));}
 static Bounds BoundsOf(IndependentPart part){var rr=part.GetComponentsInChildren<Renderer>();var b=rr[0].bounds;foreach(var r in rr)b.Encapsulate(r.bounds);return b;}
 static async Task Prepare()
 {
  if(!Application.isPlaying)throw new Exception("Requires the real Mansion Play session");Player.enabled=false;Look.enabled=false;Grab.enabled=false;Tool.enabled=false;Look.ReleaseCursor();Grab.Release();
  var side=Player.GetComponent<SideViewTestCamera>();if(side.SideViewActive)side.SetSideView(false);side.sideCamera.enabled=false;Look.GetComponent<Camera>().enabled=true;
  Player.GetComponent<Rigidbody>().isKinematic=true;Player.GetComponent<Rigidbody>().position=new Vector3(16,5,16);Physics.SyncTransforms();await Task.Delay(150);
 }
 static bool AimAt(IndependentPart part)
 {
  var damage=part.GetComponent<DamageOnImpact>();
  foreach(var shape in part.GetComponentsInChildren<Collider>().Where(c=>c.GetComponentInParent<DamageOnImpact>()==damage))
  {
   var b=shape.bounds;
   foreach(var target in new[]{b.center,b.center+Vector3.right*b.extents.x*.9f,b.center-Vector3.right*b.extents.x*.9f,b.center+Vector3.up*b.extents.y*.85f,b.center-Vector3.up*b.extents.y*.85f,b.center+Vector3.forward*b.extents.z*.9f,b.center-Vector3.forward*b.extents.z*.9f})
   foreach(float distance in new[]{.8f,1.25f,1.8f})foreach(var d in new[]{Vector3.forward,Vector3.back,Vector3.right,Vector3.left,Vector3.up,new Vector3(0,1,1).normalized,new Vector3(0,1,-1).normalized})
   {
    var origin=target+d*distance;if(Physics.CheckSphere(origin,.045f,~0,QueryTriggerInteraction.Ignore))continue;if(!Physics.Raycast(origin,-d,out var hit,2.4f)||hit.collider.GetComponentInParent<DamageOnImpact>()!=damage)continue;
    Look.transform.position=origin;Look.transform.LookAt(target);Look.GetComponent<Camera>().fieldOfView=65;Physics.SyncTransforms();return true;
   }
  }
  return false;
 }
 public static async Task<object> Strike(string example)
 {
  await Prepare();int id=example=="window"?11:example=="fridge"?48:example=="drawer"?30:example=="lamp"?31:81;
  string role=example=="window"?"pane":example=="lamp"?"shade":example=="mirror"?"mirror":"mechanism";
  var prop=Object.FindObjectsByType<MansionProp>(FindObjectsSortMode.None).First(p=>p.assetId==id);var root=prop.GetComponent<DamageOnImpact>();float health=root.State.CurrentHealth;
  var part=prop.GetComponentsInChildren<IndependentPart>().First(p=>p.role==role);var damage=part.GetComponent<DamageOnImpact>();var bounds=BoundsOf(part);int hits=0;
  for(int i=0;i<8&&part!=null&&!damage.State.IsDestroyed;i++){if(!AimAt(part))throw new Exception("No reachable local part: "+example);if(Tool.Strike())hits++;await Task.Delay(310);}
  bool intact=prop!=null&&root.State.CurrentHealth==health;bool local=part==null||damage.State.IsDestroyed;
  // Show the whole surviving item, rather than only its vanished glass surface.
  var target=example=="fridge"?prop.transform.position+Vector3.up*1:example=="drawer"?prop.transform.position+Vector3.up*.6f:bounds.center;
  Look.transform.position=target+prop.transform.forward*(example=="fridge"?2:1.2f)+Vector3.up*.2f;Look.transform.LookAt(target);Look.GetComponent<Camera>().fieldOfView=65;
  var result=new{example,id,hits,localPartDestroyed=local,frameIntact=intact,partRetained=part!=null,partReleased=part!=null&&part.Released,remainingParts=prop.GetComponentsInChildren<IndependentPart>().Length,pass=hits>0&&local&&intact};Save(example,result);return result;
 }
 public static async Task<object> Take(string example)
 {
  await Prepare();int id=example=="fruit"?69:71;string role=example=="fruit"?"fruit":"pillow";
  var prop=Object.FindObjectsByType<MansionProp>(FindObjectsSortMode.None).First(p=>p.assetId==id);var choices=prop.GetComponentsInChildren<IndependentPart>().Where(p=>p.role==role).ToArray();var part=choices.FirstOrDefault(p=>AimAt(p));if(part==null)throw new Exception("No visible content: "+example);
  var start=BoundsOf(part).center;bool grabbed=Grab.TryGrab();string initiallyHeld=Grab.HeldBody!=null?Grab.HeldBody.name:"none";Vector3 handStart=Look.transform.position;float side=Mathf.Sign(Vector3.Dot(start-prop.transform.position,prop.transform.right));if(side==0)side=1;
  Look.transform.position+=Vector3.up*(example=="pillow"?1.05f:.45f)+prop.transform.right*side*.65f;
  for(int i=0;i<70;i++){Grab.SendMessage("FixedUpdate");await Task.Delay(22);}
  var held=Grab.HeldBody;var requested=Look.transform.position+Look.transform.forward*Grab.HeldDistance;
  Vector3 point=held!=null?held.transform.TransformPoint((Vector3)typeof(ObjectGrabber).GetField("localPoint",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(Grab)):Vector3.zero;
  var blockers=Physics.RaycastAll(Look.transform.position,Look.transform.forward,Grab.HeldDistance).Where(h=>h.rigidbody!=held&&h.rigidbody!=Player.GetComponent<Rigidbody>()).Select(h=>new{h.collider.name,h.distance}).ToArray();
  var handAtPull=Look.transform.position;var centerOfMass=held!=null?held.worldCenterOfMass:Vector3.zero;float heldMass=held!=null?held.mass:0;
  float raised=BoundsOf(part).center.y-start.y;bool holdingAtEnd=Grab.IsHolding;string finallyHeld=Grab.HeldBody!=null?Grab.HeldBody.name:"none";EditorApplication.isPaused=true;Grab.Release();var focus=(start+BoundsOf(part).center)*.5f+Vector3.up*.15f;Look.transform.position=focus+prop.transform.forward*1.5f+Vector3.up*.5f;Look.transform.LookAt(focus);
  var result=new{example,grabbed,part.Released,raised,heldMass,handAtPull=handAtPull.ToString("F3"),point=point.ToString("F3"),requested=requested.ToString("F3"),centerOfMass=centerOfMass.ToString("F3"),blockers,initiallyHeld,finallyHeld,holdingAtEnd,handStart=handStart.ToString("F3"),handEnd=Look.transform.position.ToString("F3"),partPosition=BoundsOf(part).center.ToString("F3"),remainingAttached=choices.Count(p=>p!=part&&!p.Released),pass=grabbed&&part.Released&&raised>.12f&&prop.GetComponent<DamageOnImpact>().State.CurrentHealth==prop.GetComponent<DamageOnImpact>().State.MaxHealth};Save(example,result);return result;
 }
 public static object ResumeCapture(){EditorApplication.isPaused=false;return new{resumed=true};}
 public static async Task<object> Stress(int seconds=60)
 {
  await Prepare();var before=new HashSet<MansionDebris>(Object.FindObjectsByType<MansionDebris>(FindObjectsSortMode.None));var prefabs=new[]{11,28,48,69,71,88,106,118,122}.Select(id=>AssetDatabase.LoadAssetAtPath<GameObject>(Directory.GetFiles("Assets/Game/Locations/Mansion/Props","MD_"+id+"_*.prefab").Single())).ToArray();
  var floor=new GameObject("Temporary isolated composite stress floor");floor.transform.position=new Vector3(40,-.1f,0);floor.AddComponent<BoxCollider>().size=new Vector3(12,.2f,14);
  int baseline=Resources.FindObjectsOfTypeAll<Mesh>().Length,baselinePrivate=Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>!EditorUtility.IsPersistent(m)),peak=0,count=0,frame=Time.frameCount;var watch=Stopwatch.StartNew();var spawned=new List<GameObject>();
  while(watch.Elapsed.TotalSeconds<seconds)
  {
   var go=Object.Instantiate(prefabs[count%prefabs.Length],new Vector3(40+(count%3)*.7f,1.5f,(count%7)-3),Quaternion.identity);spawned.Add(go);go.GetComponent<DamageOnImpact>().ApplyDamageAt(10000,go.transform.position+Vector3.up*.5f,Vector3.forward);if(go!=null)Object.Destroy(go,1.5f);
   peak=Mathf.Max(peak,MansionDebris.LiveBodyCount);count++;if(count%40==0)Save("stress_progress",new{seconds=watch.Elapsed.TotalSeconds,count,peak,current=MansionDebris.LiveBodyCount,meshes=Resources.FindObjectsOfTypeAll<Mesh>().Length});await Task.Delay(200);
  }
  foreach(var go in spawned)if(go!=null)Object.Destroy(go);foreach(var group in Object.FindObjectsByType<MansionDebris>(FindObjectsSortMode.None))if(!before.Contains(group))Object.Destroy(group.gameObject);
  Object.Destroy(floor);await Task.Delay(1800);GC.Collect();int final=Resources.FindObjectsOfTypeAll<Mesh>().Length;int finalPrivate=Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>!EditorUtility.IsPersistent(m));
  var result=new{seconds=watch.Elapsed.TotalSeconds,iterations=count,frames=Time.frameCount-frame,peakBodies=peak,budget=MansionDebris.BodyBudget,baselineMeshes=baseline,finalMeshes=final,baselinePrivateMeshes=baselinePrivate,finalPrivateMeshes=finalPrivate,pass=peak<=MansionDebris.BodyBudget&&final<=baseline+8&&finalPrivate<=baselinePrivate+2};Save("stress",result);return result;
 }
 public static object MeshAudit()
 {
  var meshes=Resources.FindObjectsOfTypeAll<Mesh>();var temporary=meshes.Where(m=>!EditorUtility.IsPersistent(m)).Select(m=>new{m.name,m.vertexCount}).ToArray();
  var result=new{total=meshes.Length,persistent=meshes.Count(m=>EditorUtility.IsPersistent(m)),temporary};Save("mesh_audit_after_cold_stress",result);return result;
 }
 public static async Task<object> HealthyStart()
 {
  if(!Application.isPlaying)throw new Exception("Requires a clean Play session");int frame=Time.frameCount;await Task.Delay(5000);
  var props=Object.FindObjectsByType<MansionProp>(FindObjectsSortMode.None);var parts=Object.FindObjectsByType<IndependentPart>(FindObjectsSortMode.None);
  int expected=props.Sum(p=>AssetDatabase.LoadAssetAtPath<GameObject>(Directory.GetFiles("Assets/Game/Locations/Mansion/Props","MD_"+p.assetId+"_*.prefab").Single()).GetComponentsInChildren<IndependentPart>().Length);
  var damaged=parts.Where(p=>p.GetComponent<DamageOnImpact>().State.CurrentHealth<p.GetComponent<DamageOnImpact>().State.MaxHealth-.001f).Select(p=>p.name).ToArray();
  var result=new{frames=Time.frameCount-frame,props=props.Length,expectedParts=expected,actualParts=parts.Length,damagedParts=damaged,releasedParts=parts.Count(p=>p.Released),damagedFrames=props.Count(p=>p.GetComponent<DamageOnImpact>().State.CurrentHealth<p.GetComponent<DamageOnImpact>().State.MaxHealth-.001f),pass=props.Length==143&&parts.Length==expected&&damaged.Length==0&&!parts.Any(p=>p.Released)};Save("healthy_parts_start",result);return result;
 }
 public static object Audit()
 {
  var prefabs=Directory.GetFiles("Assets/Game/Locations/Mansion/Props","*.prefab").Select(p=>AssetDatabase.LoadAssetAtPath<GameObject>(p)).ToArray();
  var rows=prefabs.Select(p=>new{p.name,parts=p.GetComponentsInChildren<IndependentPart>().Length,lazyBodies=p.GetComponentsInChildren<IndependentPart>().Count(x=>x.GetComponent<Rigidbody>()==null),roles=p.GetComponentsInChildren<IndependentPart>().GroupBy(x=>x.role).ToDictionary(g=>g.Key,g=>g.Count())}).ToArray();
  var result=new{prefabs=rows.Length,compoundTypes=rows.Count(r=>r.parts>0),independentAssemblies=rows.Sum(r=>r.parts),mountedPartsWithoutIdleRigidbodies=rows.Sum(r=>r.lazyBodies),rows};Save("coverage",result);return new{result.prefabs,result.compoundTypes,result.independentAssemblies,result.mountedPartsWithoutIdleRigidbodies};
 }
}
