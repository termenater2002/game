#pragma warning disable CS0618
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Object=UnityEngine.Object;

public static class DestructionReworkAudit
{
 public static object Check()
 {
  if(Application.isPlaying)throw new Exception("Audit the saved intact scene in Edit Mode");
  if(SceneManager.GetActiveScene().name!="MansionPrototype")throw new Exception("Open MansionPrototype first");
  var roots=SceneManager.GetActiveScene().GetRootGameObjects();var transforms=roots.SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
  int missingScripts=transforms.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
  var walls=Object.FindObjectsByType<VoxelWall>(FindObjectsSortMode.None);var chunks=Object.FindObjectsByType<VoxelWallChunk>(FindObjectsSortMode.None);
  var floor=GameObject.Find("MD_6_ParquetFloor_continuous");var ceiling=GameObject.Find("Continuous ceiling");var shape=floor.GetComponent<BoxCollider>();
  int floorSamples=0,misses=0;for(float x=-11.5f;x<=11.5f;x+=.5f)for(float z=-11.5f;z<=11.5f;z+=.5f){floorSamples++;if(!shape.Raycast(new Ray(new Vector3(x,.5f,z),Vector3.down),out var hit,1)||Mathf.Abs(hit.point.y)>.001f)misses++;}
  var missingMaterials=transforms.SelectMany(t=>t.GetComponents<Renderer>()).Where(r=>r.sharedMaterials.Any(m=>m==null||m.shader==null||m.shader.name=="Hidden/InternalErrorShader")).Select(r=>r.name).ToArray();
  var files=Directory.GetFiles("Assets/Game/Locations/Mansion","*",SearchOption.AllDirectories).Where(f=>new[]{".prefab",".asset",".mat",".unity"}.Contains(Path.GetExtension(f))).Concat(new[]{"Assets/Game/Scenes/MansionPrototype.unity"});
  var missingGuids=new HashSet<string>();var checkedGuids=new HashSet<string>();var expression=new Regex(@"guid: ([0-9a-f]{32})");
  foreach(var path in files)foreach(Match match in expression.Matches(File.ReadAllText(path)))
  {
   string guid=match.Groups[1].Value;if(guid.StartsWith("00000000")||!checkedGuids.Add(guid))continue;
   if(string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid)))missingGuids.Add(guid);
  }
  var props=Object.FindObjectsByType<MansionProp>(FindObjectsSortMode.None);var failures=new List<string>();
  foreach(var p in props)if(p.GetComponent<DamageOnImpact>().materialProfile==null)failures.Add(p.name);
  int prefabMissing=0;foreach(var path in Directory.GetFiles("Assets/Game/Locations/Mansion/Props","*.prefab"))foreach(var t in AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponentsInChildren<Transform>(true))prefabMissing+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
  var result=new{scene=SceneManager.GetActiveScene().path,props=props.Length,walls=walls.Length,chunks=chunks.Length,intactWallRigidbodies=walls.Sum(w=>w.GetComponentsInChildren<Rigidbody>().Length),intactWallMeshColliders=walls.Sum(w=>w.GetComponentsInChildren<MeshCollider>().Length),wallBoxColliders=walls.Sum(w=>w.GetComponentsInChildren<BoxCollider>().Length),floorMeshes=floor.GetComponentsInChildren<MeshFilter>().Length,ceilingMeshes=ceiling.GetComponentsInChildren<MeshFilter>().Length,floorSamples,floorColliderMisses=misses,missingScripts,prefabMissingScripts=prefabMissing,missingMaterials,checkedAssetGuids=checkedGuids.Count,missingGuids=missingGuids.ToArray(),unconfiguredProps=failures,flexibleRugs=Object.FindObjectsByType<FlexibleFabric>(FindObjectsSortMode.None).Length,profiles=Directory.GetFiles("Assets/Game/Locations/Mansion/DestructionProfiles","*.asset").Length,pass=missingScripts==0&&prefabMissing==0&&missingMaterials.Length==0&&missingGuids.Count==0&&failures.Count==0&&misses==0&&walls.Length==10};
  Directory.CreateDirectory("Documentation/Mansion/DestructionRework");File.WriteAllText("Documentation/Mansion/DestructionRework/scene_audit.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
 }
}
