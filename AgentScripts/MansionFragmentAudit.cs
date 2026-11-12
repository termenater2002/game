using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
public static class MansionFragmentAudit
{
 public static object Check()
 {
  var live=UnityEngine.Object.FindObjectsByType<MansionFragment>(FindObjectsSortMode.None).Select(f=>new {name=f.name,source=f.sourceName,mesh=f.GetComponent<MeshFilter>().sharedMesh.bounds.size.ToString("F4"),world=f.GetComponent<Renderer>().bounds.size.ToString("F4"),scale=f.transform.lossyScale.ToString("F4"),pos=f.transform.position.ToString("F3")}).ToArray();
  var prefab=Directory.GetFiles("Assets/Game/Locations/Mansion/Fragments","*.prefab").Select(path=>{
   var go=AssetDatabase.LoadAssetAtPath<GameObject>(path);var pieces=go.GetComponentsInChildren<MansionFragment>(true);var biggest=pieces.OrderByDescending(p=>p.GetComponent<MeshFilter>().sharedMesh.bounds.size.magnitude).FirstOrDefault();return new{path,parts=pieces.Length,max=biggest==null?"":biggest.GetComponent<MeshFilter>().sharedMesh.bounds.size.ToString("F3")};}).ToArray();
  var result=new{live,prefab};File.WriteAllText("Documentation/Mansion/fragment_audit.json",JsonConvert.SerializeObject(result,Formatting.Indented));return new{live=live.Take(25),prefabs=prefab.Where(p=>p.path.Contains("Plate")||p.path.Contains("Teacup")||p.path.Contains("Glass"))};
 }
}
