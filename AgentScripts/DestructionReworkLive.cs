#pragma warning disable CS0618
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Object=UnityEngine.Object;

/// <summary>Repeatable live-scene demonstrations and a bounded three-minute destruction soak.</summary>
public static class DestructionReworkLive
{
 const string Folder="Documentation/Mansion/DestructionRework";
 static FunnyMovement Player=>Object.FindAnyObjectByType<FunnyMovement>();
 static FirstPersonCamera Look=>Player.GetComponentInChildren<FirstPersonCamera>();
 static Camera Camera=>Look.GetComponent<Camera>();
 static MansionInteraction Interaction=>Look.GetComponent<MansionInteraction>();
 static ObjectGrabber Grab=>Look.GetComponent<ObjectGrabber>();
 static float[] V(Vector3 p)=>new[]{p.x,p.y,p.z};
 static void Save(string name,object value){Directory.CreateDirectory(Folder);File.WriteAllText(Folder+"/"+name+".json",JsonConvert.SerializeObject(value,Formatting.Indented));}
 static MansionProp Prop(int id,string room=null)=>Object.FindObjectsByType<MansionProp>(FindObjectsSortMode.None).First(p=>p.assetId==id&&(room==null||p.room==room));
 static void Controlled()
 {
  if(!Application.isPlaying)throw new Exception("Requires real Play Mode");if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="MansionPrototype")throw new Exception("Open MansionPrototype first");Player.enabled=false;Look.enabled=false;Grab.enabled=false;Interaction.enabled=false;Look.ReleaseCursor();
  var side=Player.GetComponent<SideViewTestCamera>();if(side.SideViewActive)side.SetSideView(false);side.sideCamera.enabled=false;Camera.enabled=true;
  var body=Player.GetComponent<Rigidbody>();body.isKinematic=true;
 }
 static void Aim(Vector3 from,Vector3 target){Look.transform.position=from;Look.transform.LookAt(target);Physics.SyncTransforms();}
 public static async Task<object> WallExample()
 {
  Controlled();var wall=Object.FindObjectsByType<VoxelWall>(FindObjectsSortMode.None).First(w=>w.name.Contains("East exterior"));
  var toaster=Prop(58);var body=toaster.GetComponent<Rigidbody>();body.position=new Vector3(10.2f,2.1f,-1.7f);body.rotation=Quaternion.identity;body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;Physics.SyncTransforms();
  Aim(new Vector3(8.7f,2.2f,-1.7f),body.worldCenterOfMass);
  bool grabbed=Grab.TryGrab();bool thrown=grabbed&&Interaction.ThrowHeld();await Task.Delay(1100);
  if(wall.DamageRevision==0)throw new Exception("The actual toaster collision did not carve the wall");
  var contact=wall.LastDamagePoint;var normal=wall.LastDamageNormal;float initial=wall.RemovedVolume;int revision=wall.DamageRevision;
  Aim(contact+normal*1.4f+Vector3.up*.15f,contact-normal*.15f);bool strike1=Interaction.Strike();await Task.Delay(340);bool strike2=Interaction.Strike();await Task.Delay(340);
  float repeated=wall.RemovedVolume;var second=contact+wall.transform.right*.65f+Vector3.up*.12f;
  Aim(second+normal*1.4f,second-normal*.15f);bool other=Interaction.Strike();await Task.Delay(220);
  Aim(new Vector3(9.35f,2.0f,-3.2f),new Vector3(11.9f,1.6f,-1.35f));
  var response=toaster.GetComponent<MansionMaterialResponse>();
  var result=new{grabbed,thrown,contact=V(contact),normal=V(normal),initialVolume=initial,repeatedVolume=repeated,finalVolume=wall.RemovedVolume,wall.DamageRevision,wall.DamagedCells,wall.DeepestDamage,wall.LastRebuiltChunks,wall.TotalChunkRebuilds,chunks=wall.ChunkCount,damagedMeshColliders=wall.GetComponentsInChildren<MeshCollider>().Length,toasterDented=response.DeformationRevision>0,strike1,strike2,other,pass=grabbed&&thrown&&repeated>initial&&wall.DamageRevision>revision&&other&&wall.LastRebuiltChunks<=2};Save("wall_live",result);return result;
 }
 public static object CraterCloseup()
 {
  Controlled();var wall=Object.FindObjectsByType<VoxelWall>(FindObjectsSortMode.None).First(w=>w.name.Contains("East exterior"));
  if(wall.DamageRevision==0)throw new Exception("Create wall damage with examples/stress before the close-up");
  var point=wall.LastDamagePoint;var normal=wall.LastDamageNormal;
  Aim(point+normal*.72f+wall.transform.right*.38f+Vector3.up*.12f,point-normal*.045f);Camera.fieldOfView=55;
  return new{wall.DamagedCells,wall.DeepestDamage,screenWidth=Screen.width,screenHeight=Screen.height};
 }
 public static async Task<object> UpholsteryExample()
 {
  Controlled();Grab.Release();var sofa=Prop(21,"Lounge");var damage=sofa.GetComponent<DamageOnImpact>();var response=sofa.GetComponent<MansionMaterialResponse>();
  var cushion=sofa.GetComponentsInChildren<MeshFilter>().First(f=>f.name.Contains("seat_cushion"));int before=cushion.sharedMesh.triangles.Length;int hits=0;
  for(int i=0;i<12&&!damage.State.IsDestroyed;i++)
  {
   var b=cushion.GetComponent<Renderer>().bounds;var point=b.center+Vector3.up*b.extents.y;Aim(point+Vector3.up*1.5f,point);
   if(Interaction.Strike())hits++;await Task.Delay(320);
  }
  var bounds=cushion.GetComponent<Renderer>().bounds;Aim(bounds.center+sofa.transform.forward*1.1f+Vector3.up*.85f,bounds.center);Camera.fieldOfView=65;
  var result=new{hits,retained=sofa!=null,damage.State.IsDestroyed,response.DeformationRevision,trianglesBefore=before,trianglesAfter=cushion.sharedMesh.triangles.Length,pass=sofa!=null&&damage.State.IsDestroyed&&cushion.sharedMesh.triangles.Length<before};Save("upholstery_live",result);return result;
 }
 public static async Task<object> TelevisionExample()
 {
  Controlled();var tv=Prop(27,"Lounge");var root=tv.GetComponent<DamageOnImpact>();var screen=tv.GetComponentsInChildren<IndependentPart>().Single(p=>p.GetComponent<DamageOnImpact>().materialProfile.substance==DestructionSubstance.Glass);
  var screenDamage=screen.GetComponent<DamageOnImpact>();var bounds=screen.GetComponent<Renderer>().bounds;var front=tv.transform.forward;
  Aim(bounds.center+front*1.6f+Vector3.up*.1f,bounds.center);bool struck=Interaction.Strike();bool correct=Interaction.LastStruck==screenDamage;await Task.Delay(180);
  var result=new{struck,correctTarget=correct,screenBroken=screen==null,housingIntact=tv!=null&&!root.State.IsDestroyed,partsRemaining=tv.GetComponentsInChildren<IndependentPart>().Length,pass=struck&&correct&&screen==null&&tv!=null&&!root.State.IsDestroyed};Save("television_live",result);return result;
 }
 public static async Task<object> BookcaseExample()
 {
  Controlled();var cabinet=Object.FindObjectsByType<MansionProp>(FindObjectsSortMode.None).Where(p=>p.assetId==29&&p.room=="Study").OrderBy(p=>Mathf.Abs(p.transform.position.x+8.5f)).First();
  var damage=cabinet.GetComponent<DamageOnImpact>();var frame=cabinet.GetComponentsInChildren<MeshFilter>().First(f=>f.name.Contains("upright"));var books=cabinet.GetComponentsInChildren<IndependentPart>();var before=books.Select(b=>b.transform.position).ToArray();int hits=0;
  for(int i=0;i<10&&cabinet!=null;i++)
  {
   var center=frame.GetComponent<Renderer>().bounds.center;Aim(new Vector3(center.x,1.35f,1.2f),center);
   if(Interaction.Strike())hits++;await Task.Delay(320);
  }
  await Task.Delay(130);Aim(new Vector3(-8.3f,1.65f,.6f),new Vector3(-8.5f,.5f,3.25f));
  int released=books.Count(b=>b!=null&&b.Released);int moved=books.Where((b,i)=>b!=null&&Vector3.Distance(b.transform.position,before[i])>.03f).Count();
  var result=new{hits,frameBroken=cabinet==null,books=books.Length,released,moved,independentDynamic=books.Count(b=>b!=null&&!b.GetComponent<Rigidbody>().isKinematic),pass=cabinet==null&&released==32&&moved>0};Save("books_live",result);return result;
 }
 public static async Task<object> RugExample()
 {
  Controlled();var cloth=Prop(26,"Hall").GetComponent<FlexibleFabric>();cloth.Wake();var body=Player.GetComponent<Rigidbody>();body.position=new Vector3(16,5,16);Physics.SyncTransforms();await Task.Delay(150);
  Aim(new Vector3(0,1.3f,-10.65f),new Vector3(0,.025f,-9.95f));Grab.Release();bool grabbed=Grab.TryGrab();string firstHeld=Grab.HeldBody!=null?Grab.HeldBody.name:"none";bool firstCorrect=Grab.HeldBody!=null&&cloth.segments.Contains(Grab.HeldBody);Look.transform.position+=Vector3.up*.65f;
  for(int i=0;i<60;i++){Grab.SendMessage("FixedUpdate");await Task.Delay(22);}
  var heights=cloth.segments.Select(b=>b.worldCenterOfMass.y).ToArray();var rotations=cloth.segments.Select(b=>Quaternion.Angle(b.rotation,Quaternion.identity)).ToArray();
  var side=Player.GetComponent<SideViewTestCamera>();side.enabled=false;side.sideCamera.transform.position=new Vector3(1.55f,1.8f,-11.45f);side.sideCamera.transform.LookAt(new Vector3(0,.18f,-7.9f));side.sideCamera.orthographic=false;side.sideCamera.fieldOfView=72;side.sideCamera.enabled=true;Camera.enabled=false;
  EditorApplication.isPaused=true;
  var result=new{grabbed,segments=heights.Length,heights,angleSpread=rotations.Max()-rotations.Min(),heightSpread=heights.Max()-heights.Min(),firstHeld,firstCorrect,grabbers=Player.GetComponentsInChildren<ObjectGrabber>().Select(g=>new{g.name,holding=g.IsHolding}).ToArray(),heldBody=Grab.HeldBody!=null?Grab.HeldBody.name:"none",heldCorrectCloth=Grab.HeldBody!=null&&cloth.segments.Contains(Grab.HeldBody),frozen=cloth.IsFrozen,kinematic=cloth.segments.Select(b=>b.isKinematic).ToArray(),pausedForScreenshot=true,pass=grabbed&&heights.Max()-heights.Min()>.12f&&rotations.Max()-rotations.Min()>8};Save("rug_live",result);return result;
 }
 public static object Resume()
 {
  Grab.Release();EditorApplication.isPaused=false;var side=Player.GetComponent<SideViewTestCamera>();side.sideCamera.enabled=false;side.enabled=true;Camera.enabled=true;var body=Player.GetComponent<Rigidbody>();body.isKinematic=false;Object.FindAnyObjectByType<MansionSession>().Respawn();Look.FitBodyView();return new{resumed=true};
 }
 public static async Task<object> Routes()
 {
  Controlled();Grab.Release();var body=Player.GetComponent<Rigidbody>();body.isKinematic=false;
  foreach(var mechanism in Object.FindObjectsByType<MansionMechanism>(FindObjectsSortMode.None).Where(m=>m.owner!=null&&m.owner.GetComponent<MansionProp>()!=null&&new[]{8,9}.Contains(m.owner.GetComponent<MansionProp>().assetId)))if(!mechanism.IsOpen)mechanism.Toggle();
  await Task.Delay(1400);var results=new List<object>();bool all=true;var driver=Player.GetComponent<PlayerAnimationDriver>();float y=Object.FindAnyObjectByType<MansionSession>().spawnPosition.y;
  foreach(float z in new[]{-8f,0,8})foreach(int sign in new[]{-1,1})
  {
   var start=new Vector3(sign*.7f,y,z);var direction=Vector3.right*sign;body.position=start;body.rotation=Quaternion.LookRotation(direction);body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;Physics.SyncTransforms();await Task.Delay(180);
   Look.FitBodyView();Look.SetLookAngles(0,sign>0?90:-90);float deadline=Time.time+2.5f;int ungrounded=0;
   while(Vector3.Dot(body.position-start,direction)<3.8f&&Time.time<deadline){Player.SetMoveInput(Vector2.up);Player.SendMessage("FixedUpdate");await Task.Delay(22);if(!driver.Grounded)ungrounded++;}
   Player.SetMoveInput(Vector2.zero);for(int i=0;i<18;i++){Player.SendMessage("FixedUpdate");await Task.Delay(22);}
   float travelled=Vector3.Dot(body.position-start,direction);bool pass=travelled>3.5f&&body.position.y>-.1f;all&=pass;results.Add(new{side=sign,z,travelled,ungrounded,end=V(body.position),pass});
  }
  Object.FindAnyObjectByType<MansionSession>().Respawn();Look.FitBodyView();var result=new{pass=all,routes=results};Save("room_routes",result);return result;
 }
 public static async Task<object> Stress(int seconds=180)
 {
  Controlled();if(EditorApplication.isPaused)EditorApplication.isPaused=false;
  var wall=Object.FindObjectsByType<VoxelWall>(FindObjectsSortMode.None).First(w=>w.name.Contains("East exterior"));
  var prefabs=new[]{60,59,42,58,21}.Select(id=>AssetDatabase.LoadAssetAtPath<GameObject>(Directory.GetFiles("Assets/Game/Locations/Mansion/Props","MD_"+id+"_*.prefab").Single())).ToArray();
  var watch=Stopwatch.StartNew();int count=0,peakBodies=0,peakGroups=0,peakMeshes=0,startFrame=Time.frameCount,baselineMeshes=Resources.FindObjectsOfTypeAll<Mesh>().Length;long baselineManaged=GC.GetTotalMemory(false);var samples=new List<object>();var live=new List<GameObject>();var rng=new System.Random(73821);var frameTimes=new List<float>();double nextSample=0;
  while(watch.Elapsed.TotalSeconds<seconds)
  {
   var position=new Vector3(14+(float)rng.NextDouble(),1.5f,(float)rng.NextDouble()*6-3);int which=count%prefabs.Length;
   var go=Object.Instantiate(prefabs[which],position,Quaternion.identity);live.Add(go);var damage=go.GetComponent<DamageOnImpact>();damage.ApplyDamageAt(10000,position+Vector3.up*.3f,Vector3.up);if(go!=null)Object.Destroy(go,2.2f);
   var point=wall.transform.TransformPoint(new Vector3(-9+(float)rng.NextDouble()*18,1.3f+(float)rng.NextDouble()*.9f,.14f));wall.DamageAt(point,wall.transform.forward,15+(float)rng.NextDouble()*70);
   count++;peakBodies=Mathf.Max(peakBodies,MansionDebris.LiveBodyCount);peakGroups=Mathf.Max(peakGroups,MansionDebris.LiveGroupCount);frameTimes.Add(Time.unscaledDeltaTime*1000);
   if(watch.Elapsed.TotalSeconds>=nextSample)
   {
    int meshes=Resources.FindObjectsOfTypeAll<Mesh>().Length;peakMeshes=Mathf.Max(peakMeshes,meshes);live.RemoveAll(x=>x==null);
    var sample=new{seconds=watch.Elapsed.TotalSeconds,iterations=count,bodies=MansionDebris.LiveBodyCount,groups=MansionDebris.LiveGroupCount,meshes,managedBytes=GC.GetTotalMemory(false),frame=Time.frameCount};samples.Add(sample);Save("stress_progress",sample);nextSample+=10;
   }
   await Task.Delay(200);
  }
  foreach(var go in live)if(go!=null)Object.Destroy(go);await Task.Delay(2600);GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();
  int finalMeshes=Resources.FindObjectsOfTypeAll<Mesh>().Length;long finalManaged=GC.GetTotalMemory(false);frameTimes.Sort();float p95=frameTimes[(int)(frameTimes.Count*.95f)];
  var result=new{seconds=watch.Elapsed.TotalSeconds,iterations=count,frames=Time.frameCount-startFrame,peakBodies,peakGroups,bodyBudget=MansionDebris.BodyBudget,baselineMeshes,peakMeshes,finalMeshes,baselineManagedBytes=baselineManaged,finalManagedBytes=finalManaged,frameTimeP95Milliseconds=p95,wall.DamagedCells,wall.TotalChunkRebuilds,samples,pass=peakBodies<=MansionDebris.BodyBudget&&peakGroups<=MansionDebris.BodyBudget&&finalMeshes<=baselineMeshes+100};Save("stress_180s",result);return result;
 }
}
