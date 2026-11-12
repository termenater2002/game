using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
public static class MansionCaptures
{
 public static object Set(string room)
 {
  if(SceneManager.GetActiveScene().name!="MansionPrototype")throw new Exception("Open MansionPrototype first");
  var player=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();var look=player.GetComponentInChildren<FirstPersonCamera>();look.enabled=false;player.enabled=false;look.GetComponent<ObjectGrabber>().enabled=false;look.GetComponent<MansionInteraction>().enabled=false;look.ReleaseCursor();
  Vector3 pos=new Vector3(0,1.63f,-10.4f),target=new Vector3(0,1.5f,-4);
  if(room=="Lounge"){pos=new Vector3(-3.15f,1.7f,-5.0f);target=new Vector3(-7,.9f,-9);}
  if(room=="Kitchen"){pos=new Vector3(3.0f,1.7f,-2.6f);target=new Vector3(7.3f,1.1f,2.4f);}
  if(room=="Study"){pos=new Vector3(-3.2f,1.7f,-2.8f);target=new Vector3(-7,1.0f,1.8f);}
  if(room=="Bedroom"){pos=new Vector3(-3.1f,1.7f,5.0f);target=new Vector3(-7,1.0f,10.3f);}
  if(room=="Bath"){pos=new Vector3(2.8f,1.7f,5.2f);target=new Vector3(7,.9f,10);}
  if(room=="Dining"){pos=new Vector3(3.0f,1.7f,-5.4f);target=new Vector3(7,.7f,-8.8f);}
  var side=player.GetComponent<SideViewTestCamera>();if(side!=null&&side.SideViewActive)side.SetSideView(false);
  look.transform.position=pos;look.transform.LookAt(target);return new{room};
 }
 public static object Character()
 {
  Set("Hall");var side=UnityEngine.Object.FindAnyObjectByType<SideViewTestCamera>();side.SetSideView(true);
  return new{camera=side.sideCamera.name};
 }
 public static async Task<object> Feedback()
 {
  Set("Dining");var camera=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>().GetComponentInChildren<FirstPersonCamera>().transform;
  camera.position=new Vector3(7,2.05f,-10.2f);camera.LookAt(new Vector3(7,.9f,-8));
  var plate=UnityEngine.Object.FindObjectsByType<MansionProp>(FindObjectsSortMode.None).First(p=>p.room=="Dining"&&p.assetId==59);
  var body=plate.GetComponent<Rigidbody>();body.position+=Vector3.up*.55f;body.linearVelocity=Vector3.down*7;
  await Task.Delay(450);
  var pieces=UnityEngine.Object.FindObjectsByType<MansionFragment>(FindObjectsSortMode.None);
  var result=new{plateBroken=plate==null,pieces=pieces.Length,maxFragmentAxis=pieces.Length==0?0:pieces.Max(f=>Mathf.Max(f.GetComponent<MeshFilter>().sharedMesh.bounds.size.x,f.GetComponent<MeshFilter>().sharedMesh.bounds.size.y,f.GetComponent<MeshFilter>().sharedMesh.bounds.size.z))};
  File.WriteAllText("Documentation/Mansion/feedback_proof.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
 }
 public static object PaletteAudit()
 {
  var defaults=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).SelectMany(r=>r.sharedMaterials).Where(m=>m==null||m.name=="Default-Material").Select(m=>m==null?"missing":m.name).ToArray();
  var panels=UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Image>(FindObjectsSortMode.None).Select(i=>new{i.name,color=i.color.ToString()}).ToArray();
  var labels=UnityEngine.Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsSortMode.None).Select(t=>new{t.name,t.fontSize,color=t.color.ToString(),font=t.font?.name}).ToArray();
  var particles=UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).Select(p=>new{p.name,startColor=p.main.startColor.color.ToString()}).ToArray();
  var result=new{defaultMaterials=defaults,panels,labels,particles};File.WriteAllText("Documentation/Mansion/palette_audit.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
 }
}
