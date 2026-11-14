#if UNITY_EDITOR
#pragma warning disable CS0618, CS0649
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System;
public class CompositeCoverageTests
{
 [Serializable] public class Recipe {public string name,role;public string[] meshes;}
 [Serializable] public class AssetRecipe {public int id;public Recipe[] parts;public string[] moving_parts;}
 [Serializable] public class Manifest {public AssetRecipe[] assets;}
 const string Folder="Assets/Game/Locations/Mansion/Props";
 static GameObject Prefab(int id)=>AssetDatabase.LoadAssetAtPath<GameObject>(Directory.GetFiles(Folder,"MD_"+id+"_*.prefab").Single());
 [Test] public void EntireReusableLibraryAndEveryAuthoredCompositeRecipeAreBuilt()
 {
  Assert.That(Directory.GetFiles(Folder,"*.prefab").Length,Is.EqualTo(116));
  foreach(var asset in JsonUtility.FromJson<Manifest>(File.ReadAllText("ArtSource/Mansion/composite_parts.json")).assets)
  {
   int id=asset.id;var parts=Prefab(id).GetComponentsInChildren<IndependentPart>();
   foreach(var recipe in asset.parts)Assert.That(parts.Any(p=>p.role==recipe.role&&p.GetComponentsInChildren<MeshFilter>().Any(f=>f.name.EndsWith("_"+recipe.meshes[0]))),Is.True,"Missing authored component: "+id+" / "+recipe.name);
  }
 }
 [Test] public void AllPartsHaveLocalMaterialDamageAndAValidOwnerWithNoUnnecessaryMountedBodies()
 {
  foreach(var file in Directory.GetFiles(Folder,"*.prefab"))
  {
   var root=AssetDatabase.LoadAssetAtPath<GameObject>(file);
   foreach(var part in root.GetComponentsInChildren<IndependentPart>())
   {
    var damage=part.GetComponent<DamageOnImpact>();Assert.That(damage,Is.Not.Null,file+part.name);Assert.That(part.owner,Is.Not.Null,file+part.name);Assert.That(part.owner.transform.IsChildOf(root.transform),Is.True);
    Assert.That(damage.materialProfile,Is.Not.Null);Assert.That(part.partMass,Is.GreaterThan(0));
    if(damage.materialProfile.failure==DestructionFailure.Fracture)Assert.That(damage.brokenPrefab,Is.Not.Null,file+part.name);
    if(part.contained)Assert.That(part.GetComponent<Rigidbody>().isKinematic,Is.True);
    if(new[]{"pane","bulb","mirror","clockface","cap"}.Contains(part.role))Assert.That(part.GetComponent<Rigidbody>(),Is.Null,"Mounted part should not create an idle body: "+file+part.name);
   }
  }
 }
 [Test] public void HollowContainersOpenUpwardsInTheImportedFbxAxes()
 {
  foreach(int id in new[]{69,110,120})
  {
   var go=UnityEngine.Object.Instantiate(Prefab(id));go.transform.position=new Vector3(5000,10,5000);Physics.SyncTransforms();
   try
   {
    string suffix=id==69?"_basket":id==110?"_hollow_bucket":"_pen_cup";var shell=go.GetComponentsInChildren<MeshFilter>().Single(f=>f.name.EndsWith(suffix));var b=shell.GetComponent<Renderer>().bounds;
    float highest=float.NegativeInfinity;var ray=new Ray(b.center+Vector3.up*(b.extents.y+.5f),Vector3.down);
    foreach(var collider in shell.GetComponents<Collider>())if(collider.Raycast(ray,out var hit,b.size.y+1))highest=Mathf.Max(highest,hit.point.y);
    Assert.That(highest,Is.GreaterThan(b.min.y-.02f));Assert.That(highest,Is.LessThan(b.min.y+b.size.y*.3f),"Container mouth is sealed by a box: "+id);
    if(id==69)
    {
     var handle=go.GetComponentsInChildren<MeshFilter>().Single(f=>f.name.EndsWith("_basket_handle"));var h=handle.GetComponent<Renderer>().bounds;var normal=h.size.x<h.size.z?Vector3.right:Vector3.forward;var through=new Ray(h.center+normal,-normal);
     Assert.That(handle.GetComponents<Collider>().Any(c=>c.Raycast(through,out _,2)),Is.False,"The handle opening is filled by its bounding box");
    }
   }
   finally{UnityEngine.Object.DestroyImmediate(go);}
  }
 }
 [Test] public void MovingLeavesAndDrawersAreIndividuallyDamageableAcrossAllAuthoredAssets()
 {
  foreach(var asset in JsonUtility.FromJson<Manifest>("{\"assets\":"+File.ReadAllText("Assets/Game/Locations/Mansion/Art/Data/assets.json")+"}").assets)
  {
   int id=asset.id;if(id<8||id>=13&&id<=19)continue;
   foreach(string name in asset.moving_parts)
   {
    var leaf=Prefab(id).GetComponentsInChildren<Transform>().Single(t=>t.name==name);Assert.That(leaf.GetComponent<IndependentPart>(),Is.Not.Null,name);
    Assert.That(leaf.GetComponent<DamageOnImpact>(),Is.Not.Null,name);Assert.That(leaf.GetComponent<MansionMechanism>().owner,Is.SameAs(leaf.GetComponent<DamageOnImpact>()),name);
   }
  }
 }
}
#endif
