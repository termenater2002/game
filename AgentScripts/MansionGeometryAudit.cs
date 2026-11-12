using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
public static class MansionGeometryAudit
{
 public static object Check()
 {
  if(Application.isPlaying)throw new Exception("Run the spawn audit in Edit Mode before physics");
  Physics.SyncTransforms();var props=UnityEngine.Object.FindObjectsByType<MansionProp>(FindObjectsSortMode.None);var walls=GameObject.Find("Mansion shell — structural surfaces").GetComponentsInChildren<Renderer>();var floors=walls.Where(r=>r.name.IndexOf("Parquet",StringComparison.OrdinalIgnoreCase)>=0).Select(r=>new{name=r.name,size=r.bounds.size.ToString("F3"),center=r.bounds.center.ToString("F3")}).Take(2).ToArray();var actor=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();var cap=actor.GetComponent<CapsuleCollider>();var p=actor.transform.TransformPoint(cap.center);float radius=cap.radius*Mathf.Abs(actor.transform.lossyScale.x);float h=cap.height*Mathf.Abs(actor.transform.lossyScale.y);var overlaps=Physics.OverlapCapsule(p+Vector3.up*(h*.5f-radius),p-Vector3.up*(h*.5f-radius),radius*.97f).Where(c=>!c.transform.IsChildOf(actor.transform)).Select(c=>c.name).ToArray();
  var result=new{floors,playerBounds=cap.bounds.ToString(),spawnOverlaps=overlaps,props=props.Length,anchors=props.Where(m=>m.wallMounted).Select(m=>new{name=m.name,error=Vector3.Distance(m.GetComponent<FixedJoint>().connectedAnchor,m.transform.position)}).ToArray()};File.WriteAllText("Documentation/Mansion/geometry_audit.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
 }
}
