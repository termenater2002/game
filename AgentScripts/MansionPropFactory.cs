using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>Authored substance profiles and reusable composite/soft prop construction.</summary>
public static class MansionPropFactory
{
    const string Folder="Assets/Game/Locations/Mansion";
    public static DestructionProfile Profile(string kind)
    {
        string path=Folder+"/DestructionProfiles/"+kind+".asset";
        var p=AssetDatabase.LoadAssetAtPath<DestructionProfile>(path);if(p!=null){var hit=AssetDatabase.LoadAssetAtPath<AudioClip>(Folder+"/Art/Audio/"+kind+"_hit.wav");var broken=AssetDatabase.LoadAssetAtPath<AudioClip>(Folder+"/Art/Audio/"+kind+"_break.wav");if(hit!=null)p.impactSound=hit;if(broken!=null)p.breakSound=broken;EditorUtility.SetDirty(p);return p;}
        Directory.CreateDirectory(Folder+"/DestructionProfiles");p=ScriptableObject.CreateInstance<DestructionProfile>();p.name=kind;
        p.substance=(DestructionSubstance)Enum.Parse(typeof(DestructionSubstance),kind,true);
        p.failure=kind=="metal"?DestructionFailure.DentAndDetach:kind=="fabric"?DestructionFailure.TearAndSag:DestructionFailure.Fracture;
        p.minimumEnergy=kind=="glass"?.15f:kind=="ceramic"?.25f:kind=="plastic"?2:4;
        p.fractureEnergy=kind=="glass"?1.2f:kind=="ceramic"?1.4f:kind=="plastic"?45:kind=="metal"?220:kind=="fabric"?150:130;
        p.maximumFragmentSize=kind=="glass"?.23f:kind=="ceramic"?.20f:kind=="plastic"?.42f:.65f;
        p.targetFragments=kind=="glass"?18:kind=="plastic"?9:14;p.deformationRadius=kind=="fabric"?.5f:.24f;p.maximumDent=kind=="fabric"?.14f:.09f;
        p.wallImpactMultiplier=kind=="fabric"?.04f:kind=="glass"?.25f:kind=="plastic"?.45f:kind=="metal"?1.2f:kind=="wood"?.7f:1;
        p.impactSound=AssetDatabase.LoadAssetAtPath<AudioClip>(Folder+"/Art/Audio/"+kind+"_hit.wav");p.breakSound=AssetDatabase.LoadAssetAtPath<AudioClip>(Folder+"/Art/Audio/"+kind+"_break.wav");
        if(p.impactSound==null)p.impactSound=AssetDatabase.LoadAssetAtPath<AudioClip>(Folder+"/Art/Audio/wood_hit.wav");if(p.breakSound==null)p.breakSound=AssetDatabase.LoadAssetAtPath<AudioClip>(Folder+"/Art/Audio/wood_break.wav");
        AssetDatabase.CreateAsset(p,path);return p;
    }
    static Mesh Subdivide(Mesh source)
    {
        var p=source.vertices;var normal=source.normals;var uv=source.uv;var v=new List<Vector3>();var n=new List<Vector3>();var u=new List<Vector2>();var groups=new List<int[]>();
        for(int sub=0;sub<source.subMeshCount;sub++)
        {
            var tris=source.GetTriangles(sub);var indices=new List<int>();
            void Triangle(Vector3 a,Vector3 b,Vector3 c,Vector3 na,Vector3 nb,Vector3 nc,Vector2 ua,Vector2 ub,Vector2 uc)
            {int start=v.Count;v.AddRange(new[]{a,b,c});n.AddRange(new[]{na,nb,nc});u.AddRange(new[]{ua,ub,uc});indices.AddRange(new[]{start,start+1,start+2});}
            for(int i=0;i<tris.Length;i+=3)
            {
                int ia=tris[i],ib=tris[i+1],ic=tris[i+2];var a=p[ia];var b=p[ib];var c=p[ic];var na=normal[ia];var nb=normal[ib];var nc=normal[ic];var ua=uv[ia];var ub=uv[ib];var uc=uv[ic];var ab=(a+b)*.5f;var bc=(b+c)*.5f;var ca=(c+a)*.5f;var nab=(na+nb).normalized;var nbc=(nb+nc).normalized;var nca=(nc+na).normalized;
                Triangle(a,ab,ca,na,nab,nca,ua,(ua+ub)*.5f,(uc+ua)*.5f);Triangle(ab,b,bc,nab,nb,nbc,(ua+ub)*.5f,ub,(ub+uc)*.5f);Triangle(ca,bc,c,nca,nbc,nc,(uc+ua)*.5f,(ub+uc)*.5f,uc);Triangle(ab,bc,ca,nab,nbc,nca,(ua+ub)*.5f,(ub+uc)*.5f,(uc+ua)*.5f);
            }
            groups.Add(indices.ToArray());
        }
        var mesh=new Mesh();mesh.SetVertices(v);mesh.SetNormals(n);mesh.SetUVs(0,u);mesh.subMeshCount=groups.Count;for(int i=0;i<groups.Count;i++)mesh.SetTriangles(groups[i],i);mesh.RecalculateBounds();return mesh;
    }
    public static void Response(GameObject go,DestructionProfile profile,MeshFilter[] surfaces,int id)
    {
        if(profile.failure==DestructionFailure.Fracture)return;
        var response=go.AddComponent<MansionMaterialResponse>();response.profile=profile;response.deformableSurfaces=surfaces;
        foreach(var f in surfaces)
        {
            var first=Subdivide(f.sharedMesh);var second=Subdivide(first);UnityEngine.Object.DestroyImmediate(first);
            f.sharedMesh=MansionArchitecture.SaveMesh(second,"Deform_"+id+"_"+f.name);
        }
    }
    public static void RugGeometry(GameObject go)
    {
        var material=go.GetComponentInChildren<Renderer>().sharedMaterial;
        foreach(Transform child in go.transform.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);
        for(int i=0;i<8;i++)
        {
            var child=new GameObject("Woven strip "+i);child.transform.SetParent(go.transform,false);float z=-.875f+i*.25f;child.transform.localPosition=new Vector3(0,.012f,z);
            var mesh=MansionArchitecture.BoxMesh(new Vector3(3,.018f,.25f));var vertices=mesh.vertices;var coords=mesh.uv;
            for(int j=0;j<coords.Length;j++)coords[j]=new Vector2((vertices[j].x+1.5f)/3,(vertices[j].z+z+1)/2);mesh.uv=coords;
            child.AddComponent<MeshFilter>().sharedMesh=MansionArchitecture.SaveMesh(mesh,"RugStrip_"+i);child.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
    }
    public static void Configure(GameObject go,int id,string kind)
    {
        var contact=AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(Folder+"/Materials/PropContact.physicMaterial");
        if(contact==null){contact=new PhysicsMaterial("PropContact"){staticFriction=.35f,dynamicFriction=.25f,frictionCombine=PhysicsMaterialCombine.Minimum,bounciness=.05f,bounceCombine=PhysicsMaterialCombine.Minimum};AssetDatabase.CreateAsset(contact,Folder+"/Materials/PropContact.physicMaterial");}
        foreach(var shape in go.GetComponentsInChildren<Collider>())shape.sharedMaterial=contact;
        var root=go.GetComponent<Rigidbody>();var damage=go.GetComponent<DamageOnImpact>();var profile=Profile(kind);damage.materialProfile=profile;float referenceMass=kind=="ceramic"||kind=="glass"?.5f:kind=="metal"?3:kind=="plastic"?2:10;damage.materialToughnessScale=Mathf.Max(1,Mathf.Pow(root.mass/referenceMass,kind=="ceramic"?1:.65f));damage.retainOnDestruction=profile.failure!=DestructionFailure.Fracture;
        var meta=go.GetComponent<MansionProp>();meta.hitSound=profile.impactSound;meta.breakSound=profile.breakSound;
        if(id==29)Books(go,damage);
        if(id==27||id==115)Television(go,damage,id);
        if(id==26)
        {
            var stripFilters=go.GetComponentsInChildren<MeshFilter>();var bodies=new List<Rigidbody>();root.mass=.4f;root.linearDamping=.2f;root.angularDamping=1;root.maxDepenetrationVelocity=.5f;root.sleepThreshold=.04f;
            bodies.Add(root);Rigidbody previous=root;
            for(int i=1;i<stripFilters.Length;i++)
            {
                var strip=stripFilters[i].gameObject;var body=strip.AddComponent<Rigidbody>();body.mass=.4f;body.angularDamping=1;body.linearDamping=.2f;body.sleepThreshold=.04f;body.maxDepenetrationVelocity=.5f;body.maxAngularVelocity=5;body.solverIterations=12;
                var joint=strip.AddComponent<HingeJoint>();joint.connectedBody=previous;joint.autoConfigureConnectedAnchor=false;joint.axis=Vector3.right;joint.anchor=new Vector3(0,0,-.125f);var anchor=strip.transform.TransformPoint(joint.anchor);joint.connectedAnchor=previous.transform.InverseTransformPoint(anchor);joint.useLimits=true;joint.limits=new JointLimits{min=-65,max=65};joint.enableCollision=false;joint.useSpring=true;joint.spring=new JointSpring{spring=.6f,damper=.18f,targetPosition=0};joint.breakForce=800;joint.breakTorque=120;
                bodies.Add(body);previous=body;
            }
            root.solverIterations=12;var cloth=go.AddComponent<FlexibleFabric>();cloth.segments=bodies.ToArray();
        }
    }
    static void Books(GameObject go,DamageOnImpact owner)
    {
        go.AddComponent<CompositeContents>();int i=0;
        foreach(var f in go.GetComponentsInChildren<MeshFilter>().Where(f=>f.name.Contains("individual_book")))
        {
            var rb=f.gameObject.AddComponent<Rigidbody>();rb.mass=.45f;rb.isKinematic=true;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;rb.maxAngularVelocity=10;rb.maxDepenetrationVelocity=1;
            var part=f.gameObject.AddComponent<IndependentPart>();part.owner=owner;part.contained=true;part.partMass=.45f;part.role="book";
            var damage=f.gameObject.AddComponent<DamageOnImpact>();damage.materialProfile=Profile("fabric");damage.maxHP=25;damage.objectValue=5;damage.retainOnDestruction=true;
            var response=f.gameObject.AddComponent<MansionMaterialResponse>();response.profile=damage.materialProfile;response.deformableSurfaces=new[]{f};i++;
        }
    }
    static void Television(GameObject go,DamageOnImpact owner,int id)
    {
        go.AddComponent<CompositeContents>();var root=go.GetComponent<Rigidbody>();owner.objectValue*=.7f;
        var screen=go.GetComponentsInChildren<MeshFilter>().First(f=>f.name.Contains("separate_display"));
        Part(screen.gameObject,Profile("glass"),.65f,24,120,true);
        var feet=go.GetComponentsInChildren<MeshFilter>().Where(f=>f.name.Contains("screen_foot")).ToArray();
        var stand=new GameObject("Independent metal stand");stand.transform.SetParent(go.transform,false);stand.transform.position=feet.Select(f=>f.transform.position).Aggregate(Vector3.zero,(a,b)=>a+b)/feet.Length;
        foreach(var f in feet)f.transform.SetParent(stand.transform,true);Part(stand,Profile("metal"),1.2f,65,45,false);
        void Part(GameObject child,DestructionProfile profile,float mass,float hp,float value,bool fracture)
        {
            var body=child.AddComponent<Rigidbody>();body.mass=mass;body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;body.maxDepenetrationVelocity=1.5f;
            var joint=child.AddComponent<FixedJoint>();joint.connectedBody=root;joint.autoConfigureConnectedAnchor=false;joint.anchor=Vector3.zero;joint.connectedAnchor=go.transform.InverseTransformPoint(child.transform.position);joint.enableCollision=false;joint.breakForce=700;joint.breakTorque=400;
            var independent=child.AddComponent<IndependentPart>();independent.owner=owner;independent.partMass=mass;independent.role=fracture?"display":"stand";
            var damage=child.AddComponent<DamageOnImpact>();damage.materialProfile=profile;damage.maxHP=hp;damage.objectValue=value;damage.retainOnDestruction=!fracture;damage.explosionForce=.15f;damage.explosionRadius=.5f;damage.debrisLifetime=70;
            if(fracture)
            {
                var debris=MansionFracture.Build(child,Folder+"/FragmentMeshes/TVScreen_"+id,mass,AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/Cut_glass.mat"),new string[0],id+1000,"glass");
                damage.brokenPrefab=PrefabUtility.SaveAsPrefabAsset(debris,Folder+"/Fragments/TVScreen_"+id+".prefab");UnityEngine.Object.DestroyImmediate(debris);
            }
            else Response(child,profile,child.GetComponentsInChildren<MeshFilter>(),id);
        }
    }
}
