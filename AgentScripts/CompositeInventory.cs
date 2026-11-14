#pragma warning disable CS0618
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
public static class CompositeInventory
{
 public static object Scan()
 {
  const string root="Assets/Game/Locations/Mansion/";
  var rows=Directory.GetFiles(root+"Art/Models","*.fbx").Select(path=>{
   var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);
   return new{path,name=model.name,parts=model.GetComponentsInChildren<MeshFilter>().Select(f=>new{name=f.name,parent=f.transform.parent.name,materials=f.GetComponent<Renderer>().sharedMaterials.Select(m=>m.name).ToArray(),size=f.sharedMesh.bounds.size.ToString("F3")}).ToArray()};}).ToArray();
  Directory.CreateDirectory("Documentation/Mansion/Composites");File.WriteAllText("Documentation/Mansion/Composites/inventory.json",JsonConvert.SerializeObject(rows,Formatting.Indented));
  return new{models=rows.Length,parts=rows.Sum(r=>r.parts.Length)};
 }
}
