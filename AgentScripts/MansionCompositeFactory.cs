using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

/// <summary>Authored semantic recipes for every compound asset, independent of texture-name guesses.</summary>
public static class MansionCompositeFactory
{
 const string Folder="Assets/Game/Locations/Mansion";
 public sealed class Recipe { public string name,kind,role,mode;public float mass,hp;public string[] meshes; }
 public sealed class AssetRecipe {public int id;public Recipe[] parts;}
 public sealed class Manifest {public AssetRecipe[] assets;}
 static Dictionary<int,Recipe[]> recipes;
 static string Tail(GameObject root,string name)=>name.StartsWith(root.name+"_")?name.Substring(root.name.Length+1):name;
 public static void Configure(GameObject go,int id,string[] movingNames)
 {
  if(recipes==null)recipes=JsonConvert.DeserializeObject<Manifest>(File.ReadAllText("ArtSource/Mansion/composite_parts.json")).assets.ToDictionary(a=>a.id,a=>a.parts);
  var owner=go.GetComponent<DamageOnImpact>();int index=0;
  foreach(var name in movingNames)
  {
   var child=go.GetComponentsInChildren<Transform>().Single(t=>t.name==name).gameObject;
   if(child.GetComponent<IndependentPart>()!=null)continue;
   string kind=id==87?"fabric":(id==92)?"glass":(id==93)?"plastic":(id==63||id==48||id==49||id==50||id==51||id==55||id==70||id==113)?"metal":id==118?"plastic":"wood";
   Add(child,owner,new Recipe{name="mechanism_"+Tail(go,name),kind=kind,role="mechanism",mode="jointed",mass=child.GetComponent<Rigidbody>().mass,hp=kind=="glass"?24:kind=="metal"?90:60});
  }
  foreach(var mechanism in go.GetComponentsInChildren<MansionMechanism>())
  {
   var hardware=mechanism.GetComponentsInChildren<MeshFilter>().Where(f=>f.name.Contains("_handle_")||f.name.Contains("_lever_")||f.name.Contains("_drawer_pull_")||f.name.Contains("_hinge_pin")).ToArray();
   if(hardware.Length==0)continue;
   var group=new GameObject("Assembly_metal_hardware");group.transform.SetParent(mechanism.transform,false);group.transform.position=hardware.Select(f=>f.GetComponent<Renderer>().bounds.center).Aggregate(Vector3.zero,(a,b)=>a+b)/hardware.Length;
   foreach(var f in hardware)f.transform.SetParent(group.transform,true);
   Add(group,mechanism.GetComponent<DamageOnImpact>(),new Recipe{name="hardware",kind="metal",role="hardware",mode="mounted",mass=.15f,hp=45});
  }
  if(id==63)
  {
   var lid=go.GetComponentsInChildren<Transform>().Single(t=>t.name.EndsWith("_separate_pot_lid"));
   var grip=go.GetComponentsInChildren<Transform>().Single(t=>t.name.EndsWith("_lid_grip"));grip.SetParent(lid,true);
  }
  if(recipes.TryGetValue(id,out var list))foreach(var rule in list)
  {
   var all=go.GetComponentsInChildren<MeshFilter>();
   var selected=all.Where(f=>rule.meshes.Contains(Tail(go,f.name))).ToArray();
   if(selected.Length!=rule.meshes.Length)throw new Exception("Composite recipe mismatch: "+go.name+" / "+rule.name);
   GameObject child;
   if(selected.Length==1)child=selected[0].gameObject;
   else
   {
    child=new GameObject("Assembly_"+rule.name);child.transform.SetParent(go.transform,false);child.transform.position=selected.Select(f=>f.GetComponent<Renderer>().bounds.center).Aggregate(Vector3.zero,(a,b)=>a+b)/selected.Length;
    foreach(var f in selected)f.transform.SetParent(child.transform,true);
   }
   if(id==69&&rule.role=="fruit")
   {
    string stem=selected[0].name.Replace("_fruit","_fruit_stem");var f=all.FirstOrDefault(m=>m.name==stem);if(f!=null)f.transform.SetParent(child.transform,true);
   }
   // The containing leaf, rather than the furniture root, owns nested glazing.
   var ancestor=child.transform.parent!=null?child.transform.parent.GetComponentInParent<DamageOnImpact>():null;
   Add(child,ancestor!=null?ancestor:owner,rule);
  }
  if(id==69||id==120||id==110)
  {
   string suffix=id==69?"_basket":id==120?"_pen_cup":"_hollow_bucket";
   var shell=go.GetComponentsInChildren<MeshFilter>().Single(f=>f.name.EndsWith(suffix));HollowContainer(shell);
   if(id==69)OpenHandle(go.GetComponentsInChildren<MeshFilter>().Single(f=>f.name.EndsWith("_basket_handle")));
  }
  // A notebook's painted LCD is a separate surface inside its hinged casing.
  if(id==118)
  {
   var leaf=go.GetComponentsInChildren<MeshFilter>().Single(f=>Tail(go,f.name)=="hinged_screen");
   var panel=ExtractPanel(leaf,1,id);Add(panel,leaf.GetComponent<DamageOnImpact>(),new Recipe{name="LCD",kind="glass",role="display",mode="mounted",mass=.1f,hp=22});
  }
  if(id==88)
  {
   foreach(int sign in new[]{-1,1})
   {
    var meshes=go.GetComponentsInChildren<MeshFilter>().Where(f=>f.GetComponentInParent<IndependentPart>()==null&&Mathf.Sign(go.transform.InverseTransformPoint(f.GetComponent<Renderer>().bounds.center).x)==sign).ToArray();
    if(meshes.Length==0)throw new Exception("Shoe pair partition failed");
    var shoe=new GameObject(sign<0?"Left_shoe":"Right_shoe");shoe.transform.SetParent(go.transform,false);shoe.transform.position=meshes.Select(f=>f.GetComponent<Renderer>().bounds.center).Aggregate(Vector3.zero,(a,b)=>a+b)/meshes.Length;foreach(var f in meshes)f.transform.SetParent(shoe.transform,true);
    Add(shoe,owner,new Recipe{name=shoe.name,kind="fabric",role="shoe",mode="mounted",mass=.4f,hp=60});
   }
  }
  var parts=go.GetComponentsInChildren<IndependentPart>();
  if(parts.Length>0&&go.GetComponent<CompositeContents>()==null)go.AddComponent<CompositeContents>();
  bool noShell=!go.GetComponentsInChildren<MeshFilter>().Any(f=>f.GetComponentInParent<DamageOnImpact>()==owner);
  if(parts.Length>0)go.GetComponent<CompositeContents>().removeWhenEmpty=noShell;
  if(noShell&&parts.All(p=>p.contained))
  {
   var renderers=go.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
   // A thin support under a packed stack transfers its weight to the root without hiding its contents.
   var support=go.AddComponent<BoxCollider>();support.center=go.transform.InverseTransformPoint(new Vector3(bounds.center.x,bounds.min.y+.002f,bounds.center.z));support.size=new Vector3(bounds.size.x,.004f,bounds.size.z);support.contactOffset=.002f;support.sharedMaterial=go.GetComponentInChildren<Collider>().sharedMaterial;
  }
  // Reserve part of each existing object's price for its own parts, avoiding duplicate payouts.
  foreach(var damage in go.GetComponentsInChildren<DamageOnImpact>())
  {
   var children=parts.Where(p=>p.owner==damage&&p.role!="book").ToArray();
   if(children.Length==0||damage==owner&&(id==27||id==115))continue;float pool=damage.objectValue*(damage==owner&&noShell?1:.35f);damage.objectValue-=pool;
   foreach(var part in children)part.GetComponent<DamageOnImpact>().objectValue=pool/children.Length;
  }
  // Freeze recipe bodies until selected/released; rigidbodies are not required for mounted parts.
  foreach(var damage in go.GetComponentsInChildren<DamageOnImpact>())
  {
   if(id==26&&damage.gameObject==go)continue;
   var profile=damage.materialProfile;if(profile==null||profile.failure==DestructionFailure.Fracture)continue;
   var surfaces=damage.GetComponentsInChildren<MeshFilter>().Where(f=>f.GetComponentInParent<DamageOnImpact>()==damage).ToArray();
   if(damage.GetComponent<MansionMaterialResponse>()==null)MansionPropFactory.Response(damage.gameObject,profile,surfaces,id);
   else damage.GetComponent<MansionMaterialResponse>().deformableSurfaces=surfaces;
  }
  foreach(var part in parts)
  {
   var damage=part.GetComponent<DamageOnImpact>();if(damage.retainOnDestruction||damage.brokenPrefab!=null)continue;
   if(!part.GetComponentsInChildren<MeshFilter>().Any(f=>f.GetComponentInParent<IndependentPart>()==part))continue;
   var profile=damage.materialProfile;string kind=profile.substance.ToString().ToLowerInvariant();
   var cut=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/Cut_"+kind+".mat");
   if(cut==null){cut=new Material(Shader.Find("Universal Render Pipeline/Lit"));cut.color=new Color(.34f,.29f,.24f);cut.SetFloat("_Smoothness",.05f);AssetDatabase.CreateAsset(cut,Folder+"/Materials/Cut_"+kind+".mat");}
   string key="Part_"+id+"_"+(index++).ToString("D2");
   var broken=MansionFracture.Build(part.gameObject,Folder+"/FragmentMeshes/"+key,part.partMass,cut,new string[0],id*100+index,kind);
   damage.brokenPrefab=PrefabUtility.SaveAsPrefabAsset(broken,Folder+"/Fragments/"+key+".prefab");UnityEngine.Object.DestroyImmediate(broken);
  }
 }
 static void Add(GameObject child,DamageOnImpact owner,Recipe recipe)
 {
  if(child.GetComponent<IndependentPart>()!=null)throw new Exception("Duplicate part: "+child.name);
  var part=child.AddComponent<IndependentPart>();part.owner=owner;part.contained=recipe.mode=="contained";part.partMass=recipe.mass;part.role=recipe.role;
  if(part.contained&&child.GetComponent<Rigidbody>()==null)
  {var rb=child.AddComponent<Rigidbody>();rb.mass=recipe.mass;rb.isKinematic=true;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;rb.maxDepenetrationVelocity=1;}
  var damage=child.AddComponent<DamageOnImpact>();damage.maxHP=recipe.hp;damage.materialProfile=MansionPropFactory.Profile(recipe.kind);damage.retainOnDestruction=damage.materialProfile.failure!=DestructionFailure.Fracture;damage.objectValue=1;damage.explosionForce=.15f;damage.explosionRadius=.5f;damage.debrisLifetime=70;
  var mechanism=child.GetComponent<MansionMechanism>();if(mechanism!=null)mechanism.owner=damage;
 }
 static void HollowContainer(MeshFilter shell)
 {
  var b=shell.sharedMesh.bounds;var up=shell.transform.InverseTransformDirection(Vector3.up);int vertical=Mathf.Abs(up.x)>Mathf.Abs(up.y)?(Mathf.Abs(up.x)>Mathf.Abs(up.z)?0:2):(Mathf.Abs(up.y)>Mathf.Abs(up.z)?1:2);int a=(vertical+1)%3,c=(vertical+2)%3;
  var old=shell.GetComponents<BoxCollider>();var material=old.Length>0?old[0].sharedMaterial:null;foreach(var box in old)UnityEngine.Object.DestroyImmediate(box);
  float t=Mathf.Min(.018f,Mathf.Min(b.size[a],b.size[c])*.12f);
  void Box(Vector3 lo,Vector3 hi){var collider=shell.gameObject.AddComponent<BoxCollider>();collider.center=(lo+hi)*.5f;collider.size=hi-lo;collider.sharedMaterial=material;collider.contactOffset=.002f;}
  if(up[vertical]>0){var hi=b.max;hi[vertical]=b.min[vertical]+t;Box(b.min,hi);}else{var lo=b.min;lo[vertical]=b.max[vertical]-t;Box(lo,b.max);}
  foreach(int axis in new[]{a,c}){var hi=b.max;hi[axis]=b.min[axis]+t;Box(b.min,hi);var lo=b.min;lo[axis]=b.max[axis]-t;Box(lo,b.max);}
 }
 static void OpenHandle(MeshFilter handle)
 {
  var b=handle.sharedMesh.bounds;var up=handle.transform.InverseTransformDirection(Vector3.up);int vertical=Mathf.Abs(up.x)>Mathf.Abs(up.y)?(Mathf.Abs(up.x)>Mathf.Abs(up.z)?0:2):(Mathf.Abs(up.y)>Mathf.Abs(up.z)?1:2);int a=(vertical+1)%3,c=(vertical+2)%3;int width=b.size[a]>b.size[c]?a:c;
  var old=handle.GetComponents<BoxCollider>();var material=old.Length>0?old[0].sharedMaterial:null;foreach(var box in old)UnityEngine.Object.DestroyImmediate(box);
  float thickness=Mathf.Min(.024f,b.size[width]*.10f);
  void Box(Vector3 lo,Vector3 hi){var box=handle.gameObject.AddComponent<BoxCollider>();box.center=(lo+hi)*.5f;box.size=Vector3.Max(hi-lo,Vector3.one*.008f);box.sharedMaterial=material;box.contactOffset=.002f;}
  var lower=b.max;lower[width]=b.min[width]+thickness;Box(b.min,lower);var upper=b.min;upper[width]=b.max[width]-thickness;Box(upper,b.max);
  if(up[vertical]>0){var lo=b.min;lo[vertical]=b.max[vertical]-thickness;Box(lo,b.max);}else{var hi=b.max;hi[vertical]=b.min[vertical]+thickness;Box(b.min,hi);}
 }
 static GameObject ExtractPanel(MeshFilter source,int sub,int id)
 {
  var mesh=source.sharedMesh;var selected=mesh.GetTriangles(sub);var p=mesh.vertices;var uv=mesh.uv;var normal=mesh.normals;
  var v=new List<Vector3>();var n=new List<Vector3>();var u=new List<Vector2>();var triangles=new List<int>();
  // A closed thin triangular prism per LCD triangle preserves its authored UVs and real thickness.
  for(int i=0;i<selected.Length;i+=3)
  {
   int a=selected[i],b=selected[i+1],c=selected[i+2];var direction=Vector3.Cross(p[b]-p[a],p[c]-p[a]).normalized;
   void Tri(Vector3 x,Vector3 y,Vector3 z,Vector2 ux,Vector2 uy,Vector2 uz)
   {int k=v.Count;v.AddRange(new[]{x,y,z});var nn=Vector3.Cross(y-x,z-x).normalized;n.AddRange(new[]{nn,nn,nn});u.AddRange(new[]{ux,uy,uz});triangles.AddRange(new[]{k,k+1,k+2});}
   var aa=p[a]+direction*.005f;var bb=p[b]+direction*.005f;var cc=p[c]+direction*.005f;var ab=aa-direction*.008f;var bc=bb-direction*.008f;var ca=cc-direction*.008f;
   Tri(aa,bb,cc,uv[a],uv[b],uv[c]);Tri(ab,ca,bc,uv[a],uv[c],uv[b]);
   Tri(aa,ab,bb,uv[a],uv[a],uv[b]);Tri(bb,ab,bc,uv[b],uv[a],uv[b]);Tri(bb,bc,cc,uv[b],uv[b],uv[c]);Tri(cc,bc,ca,uv[c],uv[b],uv[c]);Tri(cc,ca,aa,uv[c],uv[c],uv[a]);Tri(aa,ca,ab,uv[a],uv[c],uv[a]);
  }
  var panel=new Mesh();panel.SetVertices(v);panel.SetNormals(n);panel.SetUVs(0,u);panel.SetTriangles(triangles,0);panel.RecalculateBounds();
  var go=new GameObject("LCD_glazing");go.transform.SetParent(source.transform,false);panel=MansionArchitecture.SaveMesh(panel,"LCD_"+id);go.AddComponent<MeshFilter>().sharedMesh=panel;go.AddComponent<MeshRenderer>().sharedMaterial=source.GetComponent<Renderer>().sharedMaterials[sub];var box=go.AddComponent<BoxCollider>();box.center=panel.bounds.center;box.size=Vector3.Max(panel.bounds.size,Vector3.one*.008f);
  var full=mesh.bounds;var opening=panel.bounds;int axis=opening.size.x<opening.size.y?(opening.size.x<opening.size.z?0:2):(opening.size.y<opening.size.z?1:2);int uAxis=(axis+1)%3,vAxis=(axis+2)%3;
  var old=source.GetComponents<BoxCollider>();var material=old.Length>0?old[0].sharedMaterial:null;
  foreach(var shape in old)UnityEngine.Object.DestroyImmediate(shape);
  void Box(Vector3 min,Vector3 max){var size=max-min;if(size.x<=.001f||size.y<=.001f||size.z<=.001f)return;var collider=source.gameObject.AddComponent<BoxCollider>();collider.center=(min+max)*.5f;collider.size=size;collider.sharedMaterial=material;collider.contactOffset=.002f;}
  var lo=full.min;var hi=full.max;var facing=Vector3.zero;for(int i=0;i<selected.Length;i+=3)facing+=Vector3.Cross(p[selected[i+1]]-p[selected[i]],p[selected[i+2]]-p[selected[i]]);
  if(facing[axis]>=0){var backHi=hi;backHi[axis]=opening.min[axis]-.005f;Box(lo,backHi);}else{var backLo=lo;backLo[axis]=opening.max[axis]+.005f;Box(backLo,hi);}
  foreach(int edge in new[]{uAxis,vAxis})
  {
   var lowerMax=hi;lowerMax[edge]=opening.min[edge];Box(lo,lowerMax);
   var upperMin=lo;upperMin[edge]=opening.max[edge];Box(upperMin,hi);
  }
  var backing=UnityEngine.Object.Instantiate(mesh);backing.SetTriangles(new int[0],sub);source.sharedMesh=MansionArchitecture.SaveMesh(backing,"LCD_casing_"+id);return go;
 }
}
