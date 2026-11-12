using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
public static class MansionInteractionProof
{
 public static async Task<object> Movement()
 {
  var p=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();p.enabled=false;var rb=p.GetComponent<Rigidbody>();rb.isKinematic=false;UnityEngine.Object.FindAnyObjectByType<MansionSession>().Respawn();await Task.Delay(400);var start=rb.position;
  p.SetMoveInput(Vector2.up,true);for(int i=0;i<35;i++){p.SendMessage("FixedUpdate");await Task.Delay(22);}var moved=Vector3.Distance(start,rb.position);
  p.SetMoveInput(Vector2.zero);for(int i=0;i<25;i++){p.SendMessage("FixedUpdate");await Task.Delay(22);}var speed=Vector3.ProjectOnPlane(rb.linearVelocity,Vector3.up).magnitude;var result=new{moved,stoppedSpeed=speed,foot=p.GetComponent<CapsuleCollider>().bounds.min.y,grounded=p.IsGrounded,pass=moved>1&&speed<.12f};File.WriteAllText("Documentation/Mansion/movement_proof.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
 }
 public static async Task<object> Run()
 {
  if(!Application.isPlaying)throw new Exception("Requires real Play Mode");
  var player=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();player.enabled=false;var body=player.GetComponent<Rigidbody>();body.isKinematic=true;
  var look=player.GetComponentInChildren<FirstPersonCamera>();look.enabled=false;look.ReleaseCursor();var grab=look.GetComponent<ObjectGrabber>();grab.enabled=false;var interact=look.GetComponent<MansionInteraction>();interact.enabled=false;
  var props=UnityEngine.Object.FindObjectsByType<MansionProp>(FindObjectsSortMode.None);
  var cup=props.First(p=>p.assetId==60&&p.room=="Lounge");var rb=cup.GetComponent<Rigidbody>();var start=rb.position;var damage=cup.GetComponent<DamageOnImpact>();decimal wallet=SessionWallet.Current.Earnings.Balance;
  var point=cup.GetComponentsInChildren<Renderer>().Select(r=>r.bounds).Aggregate((a,b)=>{a.Encapsulate(b);return a;}).center;
  look.transform.position=point+new Vector3(0,.65f,-1.25f);look.transform.LookAt(point);Physics.SyncTransforms();bool grabbed=grab.TryGrab();float heldDistance=grab.HeldDistance;
  look.transform.position+=Vector3.up*.35f;
  for(int i=0;i<16;i++){grab.SendMessage("FixedUpdate");await Task.Delay(22);}
  float moved=rb!=null?Vector3.Distance(start,rb.position):0;
  look.transform.rotation=Quaternion.LookRotation(new Vector3(0,.1f,1));bool thrown=interact.ThrowHeld();await Task.Delay(1400);
  bool cupBroken=damage==null||damage.State.IsDestroyed;decimal earned=SessionWallet.Current.Earnings.Balance-wallet;
  var chair=UnityEngine.Object.FindObjectsByType<MansionProp>(FindObjectsSortMode.None).First(p=>p.assetId==42&&p.room=="Dining"&&p.transform.position.z<-8.5f);var chairDamage=chair.GetComponent<DamageOnImpact>();point=chair.GetComponentsInChildren<Renderer>().Select(r=>r.bounds).Aggregate((a,b)=>{a.Encapsulate(b);return a;}).center;look.transform.position=point+new Vector3(0,.25f,-1.2f);look.transform.LookAt(point);Physics.SyncTransforms();bool strike1=interact.Strike();await Task.Delay(350);point=chair.GetComponentsInChildren<Renderer>().Select(r=>r.bounds).Aggregate((a,b)=>{a.Encapsulate(b);return a;}).center;look.transform.LookAt(point);bool strike2=interact.Strike();await Task.Delay(700);bool chairBroken=chairDamage==null||chairDamage.State.IsDestroyed;
  var hinge=UnityEngine.Object.FindObjectsByType<MansionMechanism>(FindObjectsSortMode.None).First(m=>m.hinge!=null&&m.owner!=null&&m.owner.GetComponent<MansionProp>().assetId==8);var angle0=hinge.transform.localRotation;
  point=hinge.GetComponent<Renderer>().bounds.center;var doorRoot=hinge.owner.transform;look.transform.position=point+doorRoot.forward*1.25f;look.transform.LookAt(point);Physics.SyncTransforms();bool usedDoor=interact.Use();await Task.Delay(1200);float doorAngle=Quaternion.Angle(angle0,hinge.transform.localRotation);
  var drawer=UnityEngine.Object.FindObjectsByType<MansionMechanism>(FindObjectsSortMode.None).First(m=>m.drawer!=null&&m.owner!=null&&m.owner.GetComponent<MansionProp>().assetId==111);var drawerStart=drawer.transform.position;drawer.Toggle();await Task.Delay(1000);float drawerMove=Vector3.Distance(drawerStart,drawer.transform.position);
  var fragments=UnityEngine.Object.FindObjectsByType<MansionFragment>(FindObjectsSortMode.None);float maxSize=fragments.Length==0?0:fragments.Max(f=>Mathf.Max(f.GetComponent<MeshFilter>().sharedMesh.bounds.size.x,f.GetComponent<MeshFilter>().sharedMesh.bounds.size.y,f.GetComponent<MeshFilter>().sharedMesh.bounds.size.z));
  var result=new{grabbed,heldDistance,moved,thrown,cupBroken,earned,strike1,strike2,chairBroken,usedDoor,doorAngle,drawerMove,fragments=fragments.Length,maxFragmentAxis=maxSize,pass=grabbed&&moved>.06f&&thrown&&cupBroken&&earned>0&&chairBroken&&usedDoor&&doorAngle>15&&drawerMove>.10f&&maxSize<.70f};
  File.WriteAllText("Documentation/Mansion/interaction_proof.json",JsonConvert.SerializeObject(result,Formatting.Indented));
  body.isKinematic=false;UnityEngine.Object.FindAnyObjectByType<MansionSession>().Respawn();return result;
 }
}
