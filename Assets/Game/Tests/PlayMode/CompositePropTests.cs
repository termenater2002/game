#if UNITY_EDITOR
#pragma warning disable CS0618
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;

public class CompositePropTests
{
 static readonly Vector3 Origin=new Vector3(1800,8,1800);
 readonly List<GameObject> created=new List<GameObject>();HashSet<MansionDebris> before;
 [SetUp] public void Setup(){before=new HashSet<MansionDebris>(Object.FindObjectsByType<MansionDebris>(FindObjectsSortMode.None));}
 GameObject Spawn(int id)
 {
  var path=Directory.GetFiles("Assets/Game/Locations/Mansion/Props","MD_"+id+"_*.prefab").Single();
  var go=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),Origin,Quaternion.identity);created.Add(go);go.GetComponent<Rigidbody>().isKinematic=true;return go;
 }
 void ClearCaseDebris()
 {
  foreach(var group in Object.FindObjectsByType<MansionDebris>(FindObjectsSortMode.None))if(!before.Contains(group))Object.Destroy(group.gameObject);
 }
 [UnityTearDown] public IEnumerator Cleanup()
 {
  foreach(var go in created)if(go!=null)Object.Destroy(go);created.Clear();
  foreach(var group in Object.FindObjectsByType<MansionDebris>(FindObjectsSortMode.None))if(!before.Contains(group))Object.Destroy(group.gameObject);
  yield return null;yield return null;
 }
 static Bounds BoundsOf(IndependentPart part)
 {
  var renderers=part.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);return bounds;
 }
 [UnityTest] public IEnumerator GlazingAndDisplaysBreakWithoutDestroyingTheirFramesAcrossLibrary()
 {
  foreach(int id in new[]{10,11,12,27,36,55,81,82,90,92,96,115,118,129,130})
  {
   var go=Spawn(id);var frame=go.GetComponent<DamageOnImpact>();var glass=go.GetComponentsInChildren<IndependentPart>().First(p=>p.GetComponent<DamageOnImpact>().materialProfile.substance==DestructionSubstance.Glass);
   float frameHealth=frame.State.CurrentHealth;var bounds=BoundsOf(glass);var partDamage=glass.GetComponent<DamageOnImpact>();partDamage.ApplyDamageAt(10000,bounds.center,Vector3.forward);
   yield return null;Assert.That(glass==null,Is.True,"Glass must break: "+id);Assert.That(go!=null,Is.True);Assert.That(frame.State.CurrentHealth,Is.EqualTo(frameHealth),"Frame damaged by panel strike: "+id);
   Object.Destroy(go);ClearCaseDebris();yield return null;
  }
 }
 [UnityTest] public IEnumerator EveryMovingAssemblyHasLocalDamageAndSurvivesDestructionOfItsOwner()
 {
  foreach(int id in new[]{8,9,10,28,30,44,45,47,48,49,50,51,55,63,70,76,77,79,80,87,92,93,95,111,113,114,118,127})
  {
   var go=Spawn(id);var owner=go.GetComponent<DamageOnImpact>();var part=go.GetComponentsInChildren<IndependentPart>().First(p=>p.role=="mechanism");
   var damage=part.GetComponent<DamageOnImpact>();damage.ApplyDamageAt(3,BoundsOf(part).center,Vector3.forward);float remaining=damage.State.CurrentHealth;
   Assert.That(owner.State.CurrentHealth,Is.EqualTo(owner.State.MaxHealth),"Leaf hit forwarded to owner: "+id);
   owner.ApplyDamage(10000);Assert.That(damage.State.CurrentHealth,Is.EqualTo(remaining),"Owner damage forwarded to leaf: "+id);yield return null;
   Assert.That(part!=null,Is.True,"Owner erased its leaf: "+id);Assert.That(part.Released,Is.True,"Leaf stayed attached: "+id);Assert.That(part.GetComponent<Rigidbody>().isKinematic,Is.False);Assert.That(damage.State.CurrentHealth,Is.GreaterThan(0),"Released part did not survive the physical breakup: "+id);
   if(go!=null)Object.Destroy(go);ClearCaseDebris();yield return null;
  }
 }
 [UnityTest] public IEnumerator LooseContentsCanBeSelectedWithoutReleasingThemUntilTheActualGrab()
 {
  foreach(int id in new[]{20,29,69,71,72,106,110,119,120,122})
  {
   var go=Spawn(id);Physics.SyncTransforms();var candidates=go.GetComponentsInChildren<IndependentPart>().Where(p=>p.contained).ToArray();Assert.That(candidates.Length,Is.GreaterThan(0),"Missing contents: "+id);
   IndependentPart selected=null;Vector3 from=Vector3.zero,target=Vector3.zero;
   foreach(var part in candidates)
   {
    var bounds=BoundsOf(part);
    foreach(var direction in new[]{Vector3.up,Vector3.forward,Vector3.back,Vector3.right,Vector3.left,new Vector3(0,1,1).normalized})
    {
     var origin=bounds.center+direction*(bounds.extents.magnitude+1);
     if(Physics.Raycast(origin,-direction,out var hit,3)&&hit.collider.GetComponentInParent<IndependentPart>()==part){selected=part;from=origin;target=bounds.center;break;}
    }
    if(selected!=null)break;
   }
   Assert.That(selected,Is.Not.Null,"No unobstructed content target: "+id);
   var hand=new GameObject("Composite test hand");created.Add(hand);hand.transform.position=from;hand.transform.LookAt(target);var grab=hand.AddComponent<ObjectGrabber>();grab.enabled=false;grab.grabbableMask=~0;
   for(int i=0;i<12;i++){Assert.That(grab.TryGetTarget(out var body,out _),Is.True);Assert.That(body.GetComponent<IndependentPart>(),Is.SameAs(selected));}
   Assert.That(candidates.All(p=>!p.Released),Is.True);Assert.That(grab.TryGrab(),Is.True);Assert.That(selected.Released,Is.True);Assert.That(selected.GetComponent<Rigidbody>().isKinematic,Is.False);Assert.That(candidates.Where(p=>p!=selected).All(p=>!p.Released),Is.True);
   grab.Release();Object.Destroy(go);Object.Destroy(hand);ClearCaseDebris();yield return null;
  }
 }
 [UnityTest] public IEnumerator AStrikeOnFragileMountedGlassDoesNotRipItsWholeFrameOffTheWall()
 {
  var go=Spawn(96);var body=go.GetComponent<Rigidbody>();body.isKinematic=false;body.useGravity=false;
  var mount=go.AddComponent<FixedJoint>();mount.breakForce=220;mount.breakTorque=154;
  var glass=go.GetComponentsInChildren<IndependentPart>().Single(p=>p.role=="mirror");var bounds=BoundsOf(glass);Vector3 from=Vector3.zero;bool reachable=false;Physics.SyncTransforms();
  foreach(var d in new[]{Vector3.forward,Vector3.back,Vector3.right,Vector3.left})
  {var start=bounds.center+d*1.1f;if(Physics.Raycast(start,-d,out var hit,2.4f)&&hit.collider.GetComponentInParent<IndependentPart>()==glass){from=start;reachable=true;break;}}
  Assert.That(reachable,Is.True);var hand=new GameObject("Local strike hand");created.Add(hand);hand.transform.position=from;hand.transform.LookAt(bounds.center);var tool=hand.AddComponent<MansionInteraction>();tool.enabled=false;hand.GetComponent<ObjectGrabber>().enabled=false;
  Assert.That(tool.Strike(),Is.True);for(int i=0;i<3;i++)yield return new WaitForFixedUpdate();
  Assert.That(glass==null,Is.True);Assert.That(go.GetComponent<FixedJoint>(),Is.Not.Null);Assert.That(go.GetComponent<MansionProp>().WasUnfastened,Is.False);Assert.That(go.GetComponent<DamageOnImpact>().State.CurrentHealth,Is.EqualTo(go.GetComponent<DamageOnImpact>().State.MaxHealth));
 }
 [UnityTest] public IEnumerator MountedAttachmentsReceiveBodiesOnlyOnReleaseAndKeepParentMotion()
 {
  var go=Spawn(57);var root=go.GetComponent<Rigidbody>();root.isKinematic=false;root.useGravity=false;root.linearVelocity=Vector3.right*2;
  var part=go.GetComponentsInChildren<IndependentPart>().First(p=>p.role=="handle");Assert.That(part.GetComponent<Rigidbody>(),Is.Null);
  part.Release();var body=part.GetComponent<Rigidbody>();Assert.That(body,Is.Not.Null);Assert.That(body.mass,Is.EqualTo(part.partMass));Assert.That(body.linearVelocity.x,Is.EqualTo(2).Within(.01f));Assert.That(part.GetComponents<Joint>().Length,Is.Zero);yield return new WaitForFixedUpdate();Assert.That(body.linearVelocity.magnitude,Is.LessThan(5),"Release caused an explosive contact impulse");
 }
 [UnityTest] public IEnumerator FridgeDoorDentsAndDetachesWhileCabinetStaysIntact()
 {
  var go=Spawn(48);var part=go.GetComponentsInChildren<IndependentPart>().First(p=>p.role=="mechanism");var damage=part.GetComponent<DamageOnImpact>();var bounds=BoundsOf(part);
  damage.ApplyDamageAt(10,bounds.center,Vector3.forward);Assert.That(part.GetComponent<MansionMaterialResponse>().DeformationRevision,Is.EqualTo(1));
  damage.ApplyDamageAt(10000,bounds.center,Vector3.forward);yield return null;
  Assert.That(part!=null&&part.Released,Is.True);Assert.That(go.GetComponent<DamageOnImpact>().State.IsDestroyed,Is.False);Assert.That(part.GetComponents<Joint>().Length,Is.Zero);
 }
 [UnityTest] public IEnumerator RealProjectileHitsWindowGlassInsteadOfDamagingWholeFrame()
 {
  var go=Spawn(11);var frame=go.GetComponent<DamageOnImpact>();var glass=go.GetComponentsInChildren<IndependentPart>().First(p=>p.role=="pane");var b=BoundsOf(glass);var s=b.size;
  var normal=s.x<s.y&&s.x<s.z?Vector3.right:s.y<s.z?Vector3.up:Vector3.forward;
  var ball=new GameObject("Window impact test projectile");created.Add(ball);ball.transform.position=b.center+normal*.8f;ball.AddComponent<SphereCollider>().radius=.045f;var rb=ball.AddComponent<Rigidbody>();rb.mass=2;rb.useGravity=false;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;rb.linearVelocity=-normal*7;
  float end=Time.time+1.5f;while(glass!=null&&Time.time<end)yield return new WaitForFixedUpdate();Assert.That(glass==null,Is.True,"Contact did not break the actual pane");Assert.That(frame.State.CurrentHealth,Is.EqualTo(frame.State.MaxHealth));
 }
 [UnityTest] public IEnumerator AnInitiallyIntersectingPackedPartCanBePulledFreeAndRestoresContainerCollision()
 {
  var root=new GameObject("Packed intersection fixture");created.Add(root);root.transform.position=Origin;root.SetActive(false);var support=root.AddComponent<BoxCollider>();root.AddComponent<Rigidbody>().isKinematic=true;var owner=root.AddComponent<DamageOnImpact>();
  var item=GameObject.CreatePrimitive(PrimitiveType.Cube);item.transform.SetParent(root.transform,false);item.transform.localPosition=new Vector3(.48f,.3f,0);item.transform.localScale=Vector3.one*.2f;var body=item.AddComponent<Rigidbody>();body.mass=.4f;body.isKinematic=true;var part=item.AddComponent<IndependentPart>();part.owner=owner;part.contained=true;part.partMass=.4f;root.AddComponent<CompositeContents>();root.SetActive(true);Physics.SyncTransforms();
  var hand=new GameObject("Overlapping content hand");created.Add(hand);hand.transform.position=Origin+new Vector3(1.6f,.3f,0);hand.transform.LookAt(item.transform.position);var grab=hand.AddComponent<ObjectGrabber>();grab.enabled=false;grab.grabbableMask=~0;grab.grabForce=650;
  Assert.That(grab.TryGrab(),Is.True);Assert.That(grab.HeldBody,Is.SameAs(body));hand.transform.position+=Vector3.right*.8f;
  for(int i=0;i<70;i++){grab.SendMessage("FixedUpdate");yield return new WaitForFixedUpdate();}
  Assert.That(body.position.x-Origin.x,Is.GreaterThan(.8f));Assert.That(Physics.GetIgnoreCollision(item.GetComponent<Collider>(),support),Is.False,"Original container collision must be restored after clearance");Assert.That(body.useGravity,Is.True);grab.Release();
 }
 [UnityTest] public IEnumerator PackedPaperAndRollsHavePhysicalFloorSupport()
 {
  foreach(int id in new[]{106,122})
  {
   var floor=new GameObject("Packed contents floor");created.Add(floor);floor.transform.position=Origin+Vector3.down*.1f;floor.AddComponent<BoxCollider>().size=new Vector3(3,.2f,3);
   var go=Spawn(id);go.GetComponent<Rigidbody>().isKinematic=false;yield return new WaitForSeconds(.5f);
   Assert.That(go.transform.position.y,Is.GreaterThan(Origin.y-.1f),"Packed contents fell through floor: "+id);Assert.That(go.GetComponent<Rigidbody>().linearVelocity.magnitude,Is.LessThan(.1f));Assert.That(go.GetComponentsInChildren<IndependentPart>().All(p=>!p.Released),Is.True);
   Object.Destroy(go);Object.Destroy(floor);ClearCaseDebris();yield return null;
  }
 }
 [UnityTest] public IEnumerator ShoePairHasIndependentFeetAndItsEmptyWrapperIsRemoved()
 {
  var go=Spawn(88);var shoes=go.GetComponentsInChildren<IndependentPart>().Where(p=>p.role=="shoe").ToArray();Assert.That(shoes.Length,Is.EqualTo(2));
  shoes[0].GetComponent<DamageOnImpact>().ApplyDamage(10000);yield return null;Assert.That(shoes[0].Released,Is.True);Assert.That(shoes[1].Released,Is.False);
  shoes[1].Release();yield return new WaitForSeconds(.65f);Assert.That(go==null,Is.True);Assert.That(shoes.All(p=>p!=null),Is.True);
 }
 [UnityTest] public IEnumerator NestedLaptopLcdHasItsOwnGlassDamageAndThinSolidGeometry()
 {
  var go=Spawn(118);var screen=go.GetComponentsInChildren<IndependentPart>().Single(p=>p.role=="display");var leaf=screen.owner;var mesh=screen.GetComponent<MeshFilter>().sharedMesh;
  Assert.That(leaf.GetComponent<IndependentPart>().role,Is.EqualTo("mechanism"));Assert.That(mesh.bounds.size.magnitude,Is.GreaterThan(.1f));Assert.That(mesh.triangles.Length,Is.GreaterThan(12));
  screen.GetComponent<DamageOnImpact>().ApplyDamage(10000);yield return null;Assert.That(screen==null,Is.True);Assert.That(leaf.State.IsDestroyed,Is.False);Assert.That(go.GetComponent<DamageOnImpact>().State.IsDestroyed,Is.False);
 }
}
#endif
