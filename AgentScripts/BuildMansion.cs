using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public static class BuildMansion
{
    const string Folder="Assets/Game/Locations/Mansion";
    public const string ScenePath="Assets/Game/Scenes/MansionPrototype.unity";
    public class AssetSpec { public int id;public string name,label;public float[] dimensions;public string[] moving_parts; }
    public class MaterialSpec { public string name,texture;public float roughness,metallic,alpha;public bool tileable; }
    public class Profile {public string kind;public float mass,hp,threshold,value;public Profile(string k,float m,float h,float t,float v){kind=k;mass=m;hp=h;threshold=t;value=v;} }
    static Dictionary<int,AssetSpec> specs;
    static Dictionary<string,Material> materials;
    static Dictionary<int,GameObject> prefabs=new Dictionary<int,GameObject>();
    static List<object> placed=new List<object>();
    static Scene scene;static Transform architecture,contents,ceilings;
    static int instances;static bool reuseAssets;
    static Profile Substance(int id)
    {
        if(new[]{11,12,27,36,37,38,61,62,82,96,115}.Contains(id))return new Profile("glass",id==27?15:id==11?11:id==61?.18f:id==62?.45f:2,32,3.8f,450);
        if(new[]{34,35,39,40,59,60,63,69,91,93,94,101,104,105}.Contains(id))return new Profile("ceramic",id==91?115:id==93?36:id==34?7:id==35?8:id==59?.4f:id==60?.18f:3, id==91?210:id==93?120:id==59?19:id==60?17:42,3.8f,250);
        if(new[]{21,22,23,26,73,74,75,88,89,99,100,109}.Contains(id))return new Profile("fabric",id<=23?65:id==26?3:1.5f,id<=23?120:60,8,600);
        if(new[]{31,32,33,43,48,49,50,51,52,53,54,55,56,57,58,64,67,70,78,81,90,92,95,97,103,107,108,112,113,115,116,117,118,119,123,124,125,126,129,130}.Contains(id))return new Profile("metal",id==113?120:id==48?65:id==49||id==55?40:id==33?18:id==97?14:id==43?6:3.5f,id==113?280:id==48?180:110,8,850);
        return new Profile("wood",id==41?42:id==71?80:id==79?65:id==29?45:id==44?48:id==47?60:id==111?38:id==42?7:id==25?13:id<=10?18:5, id==41||id==71||id==79?130:id==42?62:88,7,500);
    }
    static Material Mat(string name)
    {
        if(materials.TryGetValue(name,out var material))return material;
        throw new Exception("Missing authored material: "+name);
    }
    public static object ImportLibrary()
    {
        foreach(var dir in new[]{"Materials","Props","Fragments","FragmentMeshes"})Directory.CreateDirectory(Folder+"/"+dir);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        specs=JsonConvert.DeserializeObject<List<AssetSpec>>(File.ReadAllText(Folder+"/Art/Data/assets.json")).ToDictionary(x=>x.id);
        materials=new Dictionary<string,Material>();
        foreach(var e in JsonConvert.DeserializeObject<List<MaterialSpec>>(File.ReadAllText(Folder+"/Art/Data/materials.json")))
        {
            string texPath=Folder+"/Art/Textures/"+e.texture;var ti=(TextureImporter)AssetImporter.GetAtPath(texPath);ti.filterMode=FilterMode.Point;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.mipmapEnabled=true;ti.wrapMode=e.tileable?TextureWrapMode.Repeat:TextureWrapMode.Clamp;ti.maxTextureSize=512;ti.SaveAndReimport();
            string matPath=Folder+"/Materials/"+e.name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,matPath);}
            mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(texPath));mat.SetColor("_BaseColor",new Color(1,1,1,e.alpha));mat.SetFloat("_Smoothness",1-e.roughness);mat.SetFloat("_Metallic",e.metallic);mat.SetFloat("_Cull",0);mat.doubleSidedGI=true;
            // Painted glazing stays opaque enough to read in the retro style; no sorting-dependent transparent shelves.
            if(e.alpha<1){mat.SetColor("_BaseColor",new Color(.65f,.83f,.89f,1));mat.SetFloat("_Smoothness",.55f);}
            materials[e.name]=mat;EditorUtility.SetDirty(mat);
        }
        foreach(var e in specs.Values)
        {
            string path=Folder+"/Art/Models/"+e.name+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.globalScale=1;importer.useFileUnits=true;importer.isReadable=true;importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.importNormals=ModelImporterNormals.Import;
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach(var mat in model.GetComponentsInChildren<Renderer>().SelectMany(r=>r.sharedMaterials).Distinct())
                if(mat!=null)importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),mat.name),Mat(mat.name));
            importer.SaveAndReimport();
        }
        AssetDatabase.SaveAssets();return new{models=specs.Count,materials=materials.Count};
    }
    static Bounds LocalBounds(GameObject go,IEnumerable<MeshFilter> selection=null)
    {
        bool first=true;var bounds=new Bounds();
        foreach(var mf in selection??go.GetComponentsInChildren<MeshFilter>())
        {
            var bb=mf.sharedMesh.bounds;var m=go.transform.worldToLocalMatrix*mf.transform.localToWorldMatrix;
            foreach(int x in new[]{-1,1})foreach(int y in new[]{-1,1})foreach(int z in new[]{-1,1})
            {var p=m.MultiplyPoint3x4(bb.center+Vector3.Scale(bb.extents,new Vector3(x,y,z)));if(first){bounds=new Bounds(p,Vector3.zero);first=false;}else bounds.Encapsulate(p);}
        }
        return bounds;
    }
    static void BoxShapes(GameObject root,Transform subtree=null)
    {
        foreach(var f in (subtree??root.transform).GetComponentsInChildren<MeshFilter>())
        {
            if(f.sharedMesh.bounds.size.sqrMagnitude<.00001f)continue;
            var box=f.gameObject.AddComponent<BoxCollider>();box.center=f.sharedMesh.bounds.center;box.size=Vector3.Max(f.sharedMesh.bounds.size,Vector3.one*.016f);box.contactOffset=.004f;
        }
    }
    static GameObject Visual(int id)
    {
        var spec=specs[id];var go=new GameObject(spec.name);var model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/Art/Models/"+spec.name+".fbx"),go.transform,false);model.name=spec.name+"_Visual";return go;
    }
    static Material Interior(string kind)
    {
        string path=Folder+"/Materials/Cut_"+kind+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m!=null)return m;
        m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.SetColor("_BaseColor",kind=="wood"?new Color(.37f,.24f,.14f):kind=="ceramic"?new Color(.65f,.61f,.49f):kind=="fabric"?new Color(.16f,.12f,.12f):new Color(.22f,.26f,.29f));m.SetFloat("_Smoothness",.05f);AssetDatabase.CreateAsset(m,path);return m;
    }
    static GameObject Prop(int id)
    {
        if(prefabs.TryGetValue(id,out var known))return known;
        var e=specs[id];if(reuseAssets){var stored=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/Props/"+e.name+".prefab");if(stored!=null){prefabs[id]=stored;return stored;}}var profile=Substance(id);var go=Visual(id);BoxShapes(go);var body=go.AddComponent<Rigidbody>();body.mass=profile.mass;if(id==27||id==115)body.centerOfMass=new Vector3(0,.16f,.15f);if(id==31||id==32)body.centerOfMass=new Vector3(0,.18f,0);body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;body.maxDepenetrationVelocity=2f;body.maxAngularVelocity=12;body.linearDamping=.10f;body.angularDamping=.55f;
        var damage=go.AddComponent<DamageOnImpact>();damage.maxHP=profile.hp;damage.impactThreshold=profile.threshold;damage.damageMultiplier=profile.kind=="glass"||profile.kind=="ceramic"?1.5f:.9f;damage.objectValue=profile.value;damage.explosionForce=2.5f;damage.explosionRadius=1.4f;damage.debrisLifetime=70;
        var meta=go.AddComponent<MansionProp>();meta.assetId=id;meta.displayName=e.label;meta.substance=profile.kind;meta.hitSound=AssetDatabase.LoadAssetAtPath<AudioClip>(Folder+"/Art/Audio/"+profile.kind+"_hit.wav");meta.breakSound=AssetDatabase.LoadAssetAtPath<AudioClip>(Folder+"/Art/Audio/"+profile.kind+"_break.wav");
        foreach(var name in e.moving_parts)
        {
            var moving=go.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name==name);if(moving==null)throw new Exception("Moving part missing: "+name);
            var rb=moving.gameObject.AddComponent<Rigidbody>();rb.mass=Mathf.Min(profile.mass*.18f,4);rb.mass=Mathf.Max(.15f,rb.mass);rb.interpolation=RigidbodyInterpolation.Interpolate;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;rb.maxDepenetrationVelocity=2;rb.angularDamping=.7f;
            var mechanism=moving.gameObject.AddComponent<MansionMechanism>();mechanism.owner=damage;
            if(name.Contains("drawer"))
            {
                var j=moving.gameObject.AddComponent<ConfigurableJoint>();j.connectedBody=body;j.autoConfigureConnectedAnchor=false;j.anchor=Vector3.zero;j.connectedAnchor=go.transform.InverseTransformPoint(moving.position);j.axis=moving.InverseTransformDirection(go.transform.forward);j.secondaryAxis=moving.InverseTransformDirection(go.transform.up);j.xMotion=ConfigurableJointMotion.Limited;j.yMotion=j.zMotion=ConfigurableJointMotion.Locked;j.angularXMotion=j.angularYMotion=j.angularZMotion=ConfigurableJointMotion.Locked;j.linearLimit=new SoftJointLimit{limit=.34f};j.xDrive=new JointDrive{positionSpring=1800,positionDamper=110,maximumForce=250};j.breakForce=1600;j.breakTorque=1600;j.enableCollision=false;mechanism.drawer=j;mechanism.travel=.28f;
            }
            else
            {
                var j=moving.gameObject.AddComponent<HingeJoint>();j.connectedBody=body;j.autoConfigureConnectedAnchor=false;j.anchor=Vector3.zero;j.connectedAnchor=go.transform.InverseTransformPoint(moving.position);bool horizontal=name.Contains("oven_door")||name.Contains("dishwasher")||name.Contains("toilet_lid")||name.Contains("bin_lid")||name.Contains("pot_lid");j.axis=moving.InverseTransformDirection(horizontal?go.transform.right:go.transform.up);j.useLimits=true;j.limits=new JointLimits{min=-105,max=105};j.breakForce=1600;j.breakTorque=1200;j.enableCollision=false;mechanism.hinge=j;mechanism.openAngle=name.EndsWith(".001")?-95:95;
            }
        }
        var broken=MansionFracture.Build(go,Folder+"/FragmentMeshes/"+id,profile.mass,Interior(profile.kind),e.moving_parts,id);damage.brokenPrefab=PrefabUtility.SaveAsPrefabAsset(broken,Folder+"/Fragments/"+e.name+"_Broken.prefab");UnityEngine.Object.DestroyImmediate(broken);
        go.SetActive(true);var result=PrefabUtility.SaveAsPrefabAsset(go,Folder+"/Props/"+e.name+".prefab");UnityEngine.Object.DestroyImmediate(go);prefabs[id]=result;return result;
    }
    static GameObject Place(int id,string room,Vector3 bottom,float yaw=0,bool mounted=false,Vector3? scale=null)
    {
        var go=(GameObject)PrefabUtility.InstantiatePrefab(Prop(id),scene);go.name=room+" • "+specs[id].name+" #"+(++instances);go.transform.SetParent(contents,false);go.transform.localScale=scale??Vector3.one;var bounds=LocalBounds(go);go.transform.SetPositionAndRotation(bottom-Vector3.up*bounds.min.y*go.transform.localScale.y,Quaternion.Euler(0,yaw,0));var meta=go.GetComponent<MansionProp>();meta.room=room;meta.wallMounted=mounted;
        if(mounted){var joint=go.AddComponent<FixedJoint>();joint.connectedBody=null;joint.autoConfigureConnectedAnchor=false;joint.anchor=Vector3.zero;joint.connectedAnchor=go.transform.position;joint.breakForce=Mathf.Max(220,go.GetComponent<Rigidbody>().mass*40);joint.breakTorque=joint.breakForce*.7f;joint.enableCollision=false;}
        placed.Add(new {name=go.name,id,room,position=go.transform.position.ToString("F3"), mounted});return go;
    }
    static GameObject Structure(int id,Vector3 bottom,float yaw=0,Vector3? scale=null,Transform parent=null)
    {
        var go=Visual(id);SceneManager.MoveGameObjectToScene(go,scene);go.transform.SetParent(parent??architecture,false);go.transform.localScale=scale??Vector3.one;var bb=LocalBounds(go);go.transform.SetPositionAndRotation(bottom-Vector3.up*bb.min.y*go.transform.localScale.y,Quaternion.Euler(0,yaw,0));BoxShapes(go);return go;
    }
    static void Wall(Vector3 a,Vector3 b,float yaw)
    {
        float length=Vector3.Distance(a,b);int count=Mathf.CeilToInt(length/3);float piece=length/count;
        for(int i=0;i<count;i++)Structure(1,Vector3.Lerp(a,b,(i+.5f)/count),yaw,new Vector3(piece/3,3.2f/3,1));
    }
    static void HallWall(float x,float yaw)
    {
        float start=-12;
        foreach(float doorZ in new[]{-8f,0,8})
        {
            Wall(new Vector3(x,0,start),new Vector3(x,0,doorZ-.62f),yaw);
            var lintel=GameObject.CreatePrimitive(PrimitiveType.Cube);lintel.name="Door lintel";lintel.transform.SetParent(architecture,false);lintel.transform.localScale=new Vector3(.20f,.76f,1.24f);lintel.transform.position=new Vector3(x,2.82f,doorZ);lintel.GetComponent<Renderer>().sharedMaterial=materials["MD_001_ivory_plaster"];
            Place(8,"Hall door",new Vector3(x,0,doorZ),yaw,true);start=doorZ+.62f;
        }
        Wall(new Vector3(x,0,start),new Vector3(x,0,12),yaw);
    }
    static void LightAt(string name,Vector3 p,Color color,float intensity,float range,bool shadows=false)
    {
        var go=new GameObject(name);SceneManager.MoveGameObjectToScene(go,scene);go.transform.position=p;var l=go.AddComponent<Light>();l.type=LightType.Point;l.color=color;l.intensity=intensity;l.range=range;l.shadows=shadows?LightShadows.Soft:LightShadows.None;
        if(shadows){var data=new SerializedObject(l.GetUniversalAdditionalLightData());data.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=0;data.ApplyModifiedPropertiesWithoutUndo();}
    }
    static void Furnish()
    {
        // Hall: clear central navigation spine and graspable decorations along its edges.
        Place(26,"Hall",new Vector3(0,.008f,-7.4f),0,false,new Vector3(.78f,1,2.7f));
        foreach(float z in new[]{-5.5f,3.5f,10.3f}){Place(31,"Hall",new Vector3(-1.55f,0,z));Place(34,"Hall",new Vector3(1.5f,0,z+.4f));}
        Place(30,"Hall",new Vector3(.8f,0,11.5f),180);Place(39,"Hall",new Vector3(.8f,1.16f,11.5f));Place(37,"Hall",new Vector3(-1.86f,1.45f,5),90,true);
        // Lounge, 10 x 8 metres. Its centre remains open for throwing furniture.
        Place(20,"Lounge",new Vector3(-7,0,-11.55f),0,true);Place(37,"Lounge",new Vector3(-7,1.52f,-11.8f),0,true);
        Place(21,"Lounge",new Vector3(-7,0,-6),180);Place(23,"Lounge",new Vector3(-10,0,-8.5f),90);Place(22,"Lounge",new Vector3(-3.5f,0,-9),-90);
        Place(26,"Lounge",new Vector3(-7,.008f,-8.4f),0,false,new Vector3(1.3f,1,1.3f));Place(25,"Lounge",new Vector3(-7,.04f,-8.5f));
        Place(27,"Lounge",new Vector3(-3.0f,.7f,-6),-90);Place(28,"Lounge",new Vector3(-3,0,-6),-90);Place(29,"Lounge",new Vector3(-10.6f,0,-4.4f),180);
        Place(31,"Lounge",new Vector3(-10.7f,0,-5.1f));Place(35,"Lounge",new Vector3(-11.2f,0,-10.8f));Place(33,"Lounge",new Vector3(-7,2.35f,-8.5f),0,true);
        Place(60,"Lounge",new Vector3(-7.35f,.60f,-8.4f));Place(61,"Lounge",new Vector3(-6.8f,.60f,-8.5f));Place(62,"Lounge",new Vector3(-7,.60f,-8.7f));Place(127,"Lounge",new Vector3(-7.35f,.60f,-8.8f));
        // Dining room.
        Place(41,"Dining",new Vector3(7,0,-8));
        foreach(float x in new[]{6.05f,7,7.95f}){Place(42,"Dining",new Vector3(x,0,-9.0f));Place(42,"Dining",new Vector3(x,0,-7.0f),180);}
        Place(30,"Dining",new Vector3(10.8f,0,-11.55f));Place(39,"Dining",new Vector3(10.35f,1.17f,-11.55f));Place(34,"Dining",new Vector3(3,0,-11));Place(35,"Dining",new Vector3(11,0,-5));Place(33,"Dining",new Vector3(7,2.35f,-8),0,true);
        foreach(float x in new[]{6.15f,7,7.85f})foreach(float z in new[]{-8.30f,-7.70f}){Place(59,"Dining",new Vector3(x,.855f,z));Place(61,"Dining",new Vector3(x+.20f,.855f,z));}
        Place(62,"Dining",new Vector3(7,.86f,-8));Place(69,"Dining",new Vector3(11.3f,1.17f,-11.55f));Place(37,"Dining",new Vector3(7,1.35f,-11.85f),0,true);
        // Study.
        Place(111,"Study",new Vector3(-7,0,1.2f));Place(112,"Study",new Vector3(-7,0,2.45f),180);
        foreach(float x in new[]{-10.5f,-8.5f,-6.5f})Place(29,"Study",new Vector3(x,0,3.5f),180);
        Place(113,"Study",new Vector3(-3.0f,0,3),180,true);Place(114,"Study",new Vector3(-11.5f,0,.7f),90);Place(24,"Study",new Vector3(-10,0,-2.4f));Place(25,"Study",new Vector3(-8.7f,0,-2.4f));Place(31,"Study",new Vector3(-11,0,-2.5f));
        Place(115,"Study",new Vector3(-7,.86f,1.2f));Place(116,"Study",new Vector3(-7,.86f,1.5f));Place(117,"Study",new Vector3(-6.6f,.86f,1.5f));Place(119,"Study",new Vector3(-7.6f,.86f,1.2f));Place(121,"Study",new Vector3(-6.5f,.86f,1.0f));Place(124,"Study",new Vector3(-8.7f,.56f,-2.4f));Place(38,"Study",new Vector3(-7,2.32f,3.85f),180,true);
        // Kitchen, counters deliberately leave a walkable central island aisle.
        foreach(float x in new[]{4f,6f,8f})Place(44,"Kitchen",new Vector3(x,0,3.4f),180,true);
        Place(48,"Kitchen",new Vector3(10.7f,0,3.35f),180);Place(49,"Kitchen",new Vector3(9.4f,0,3.35f),180);Place(52,"Kitchen",new Vector3(9.4f,1.8f,3.5f),180,true);
        foreach(float x in new[]{4f,6f,8f})Place(45,"Kitchen",new Vector3(x,1.75f,3.65f),180,true);
        Place(53,"Kitchen",new Vector3(6,1.10f,3.3f),180,true);Place(55,"Kitchen",new Vector3(2.65f,0,3.35f),180);
        Place(47,"Kitchen",new Vector3(7,0,-.5f));Place(43,"Kitchen",new Vector3(6.5f,0,-1.55f));Place(43,"Kitchen",new Vector3(7.4f,0,-1.55f));
        Place(56,"Kitchen",new Vector3(4,1.10f,3.3f),180);Place(57,"Kitchen",new Vector3(8,1.10f,3.3f));Place(63,"Kitchen",new Vector3(9.4f,.95f,3.3f));Place(64,"Kitchen",new Vector3(7.55f,1.10f,-.5f));
        foreach(var q in new[]{new Vector3(6.5f,1.10f,-.5f),new Vector3(7,1.10f,-.5f),new Vector3(6.6f,1.10f,-.25f)})Place(65+(int)(q.x*10)%4,"Kitchen",q);
        Place(70,"Kitchen",new Vector3(11,0,-3));Place(60,"Kitchen",new Vector3(4.35f,1.10f,3.3f));Place(58,"Kitchen",new Vector3(8.65f,1.10f,3.3f));
        // Bedroom.
        Place(71,"Bedroom",new Vector3(-7,0,10.5f),180);Place(76,"Bedroom",new Vector3(-8.45f,0,11.2f),180);Place(76,"Bedroom",new Vector3(-5.55f,0,11.2f),180);Place(32,"Bedroom",new Vector3(-8.45f,.72f,11.2f));Place(90,"Bedroom",new Vector3(-5.55f,.72f,11.2f));
        Place(79,"Bedroom",new Vector3(-11.1f,0,7),90);Place(80,"Bedroom",new Vector3(-3,0,10.8f),-90);Place(81,"Bedroom",new Vector3(-3,0,6),-90);Place(89,"Bedroom",new Vector3(-3.15f,.9f,6.1f),-90);Place(86,"Bedroom",new Vector3(-9,0,5.2f));Place(88,"Bedroom",new Vector3(-8.4f,0,5.2f));Place(26,"Bedroom",new Vector3(-7,.008f,7),0,false,new Vector3(1.3f,1,1.0f));
        Place(74,"Bedroom",new Vector3(-7.35f,.85f,11.1f));Place(74,"Bedroom",new Vector3(-6.65f,.85f,11.1f));
        // Bathroom and laundry occupy one generous testing room.
        Place(91,"Bath",new Vector3(5,0,10.4f),180);Place(92,"Bath",new Vector3(10.8f,0,10.8f),180,true);Place(93,"Bath",new Vector3(9.0f,0,11),180,true);Place(95,"Bath",new Vector3(3,0,6.2f),90,true);Place(97,"Bath",new Vector3(11.8f,.5f,6),-90,true);Place(99,"Bath",new Vector3(10.5f,0,5));Place(110,"Bath",new Vector3(9.5f,0,5));Place(100,"Bath",new Vector3(6,.04f,8));
        Place(101,"Bath",new Vector3(4.7f,.755f,10.35f));Place(105,"Bath",new Vector3(5.3f,.755f,10.35f));Place(104,"Bath",new Vector3(3,1.1f,6.2f));Place(109,"Bath",new Vector3(3,1.1f,6.5f));
    }
    static void CreatePlayer()
    {
        var source=EditorSceneManager.OpenScene("Assets/Game/Scenes/SampleScene.unity",OpenSceneMode.Additive);var original=source.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<FunnyMovement>()).Single();var player=UnityEngine.Object.Instantiate(original.gameObject);SceneManager.MoveGameObjectToScene(player,scene);player.name="Mansion Player";EditorSceneManager.CloseScene(source,true);SceneManager.SetActiveScene(scene);
        float factor=1.78f/(player.GetComponent<CapsuleCollider>().height*Mathf.Abs(player.transform.lossyScale.y));player.transform.localScale*=factor;player.transform.rotation=Quaternion.identity;player.transform.position=Vector3.zero;var cap=player.GetComponent<CapsuleCollider>();float bottomOffset=player.transform.TransformVector(cap.center).y-cap.height*Mathf.Abs(player.transform.lossyScale.y)*.5f;player.transform.position=new Vector3(0,.04f-bottomOffset,-10.4f);var body=player.GetComponent<Rigidbody>();body.mass=75;body.maxDepenetrationVelocity=3;
        var side=player.GetComponent<SideViewTestCamera>();if(side!=null)side.distanceInHeights=.9f;
        var move=player.GetComponent<FunnyMovement>();move.walkSpeed=1.1f;move.sprintSpeed=2.7f;move.speedMultiplier=2;move.maxSpeed=5.4f;move.moveForce=45;move.jumpForce=75*5;move.matchAnimationSpeeds=true;move.groundMask=~0;move.SetStoryWalking(false);
        var driver=player.GetComponent<PlayerAnimationDriver>();driver.walkReferenceSpeed=1.1f;driver.runReferenceSpeed=2.7f;driver.maximumPlaybackRate=1;driver.blendSpaceScale=.47873625f/2.7f;driver.groundMask=~0;
        var look=player.GetComponentInChildren<FirstPersonCamera>();look.captureOnStart=true;look.fitBodyView=true;look.FitBodyView();look.SetLookAngles(0,0);var camera=look.GetComponent<Camera>();camera.nearClipPlane=.026f;camera.farClipPlane=90;camera.fieldOfView=76;camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
        var grab=look.GetComponent<ObjectGrabber>();grab.grabDistance=2.4f;grab.grabForce=650;grab.maxAcceleration=35;grab.breakDistance=3.4f;grab.grabbableMask=~0;look.gameObject.AddComponent<MansionInteraction>();
        var system=new GameObject("Mansion Session");SceneManager.MoveGameObjectToScene(system,scene);var wallet=system.AddComponent<SessionWallet>();var hud=system.AddComponent<DemoHud>();hud.wallet=wallet;hud.look=look;hud.grabber=grab;hud.sessionTitle="MAJOR DAMAGE  /  MANSION";hud.mansionControls=true;hud.controlHints="WASD: run   SPACE: jump   MOUSE: look   ESC: cursor\nLMB: grab / drop   RMB: throw / hit   E: door / drawer\nR: reset mansion   F6: side camera";
        var session=system.AddComponent<MansionSession>();session.player=body;session.spawnPosition=player.transform.position;session.spawnYaw=0;
    }
    static void AmbientActors()
    {
        var ids=new[]{"M01_Butler","M02_Maid","M04_Chef","M07_Bodyguard"};
        var positions=new[]{new Vector3(-1.15f,0,-10.9f),new Vector3(-10.5f,0,-6.5f),new Vector3(5.0f,0,1.4f),new Vector3(-1.0f,0,9.5f)};
        for(int i=0;i<ids.Length;i++)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NPCFactoryValidation/Prefabs/"+ids[i]+".prefab");if(prefab==null)continue;
            var actor=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);actor.name="Mansion staff • "+ids[i];actor.transform.position=positions[i];actor.transform.rotation=Quaternion.Euler(0,i==2?0:180,0);
            var shape=actor.AddComponent<CapsuleCollider>();shape.height=1.75f;shape.center=new Vector3(0,.875f,0);shape.radius=i==3?.36f:.25f;
            var motion=actor.AddComponent<MansionAmbientActor>();motion.state=i==3?"LookAround":i==2?"Talk":"Idle";motion.phase=i*.17f;
        }
    }
    static void Lighting()
    {
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.30f,.39f,.50f);RenderSettings.ambientEquatorColor=new Color(.30f,.27f,.23f);RenderSettings.ambientGroundColor=new Color(.12f,.10f,.09f);RenderSettings.fog=false;
        var sun=new GameObject("Late afternoon through the windows").AddComponent<Light>();SceneManager.MoveGameObjectToScene(sun.gameObject,scene);sun.type=LightType.Directional;sun.color=new Color(1,.89f,.72f);sun.intensity=1.2f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(42,-35,0);
        foreach(float x in new[]{-7f,7f})foreach(float z in new[]{-8f,0,8}){LightAt("Warm room practical",new Vector3(x,2.75f,z),new Color(1,.79f,.56f),3.5f,7,z<4);LightAt("Cool window fill",new Vector3(x<0?-11:11,2.2f,z),new Color(.64f,.79f,1),2,6);}
        foreach(float z in new[]{-9f,-3,3,9})LightAt("Hall lantern",new Vector3(0,2.9f,z),new Color(1,.82f,.64f),2.5f,5,z==-9f);
        var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Folder+"/MansionAtmosphere.asset");if(profile==null){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,Folder+"/MansionAtmosphere.asset");}
        var tone=profile.components.OfType<Tonemapping>().FirstOrDefault()??profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.Neutral);var color=profile.components.OfType<ColorAdjustments>().FirstOrDefault()??profile.Add<ColorAdjustments>(true);color.contrast.Override(6);color.saturation.Override(5);var vignette=profile.components.OfType<Vignette>().FirstOrDefault()??profile.Add<Vignette>(true);vignette.intensity.Override(.16f);var bloom=profile.components.OfType<Bloom>().FirstOrDefault()??profile.Add<Bloom>(true);bloom.intensity.Override(.15f);bloom.threshold.Override(1.2f);EditorUtility.SetDirty(profile);
        foreach(var component in profile.components)if(!AssetDatabase.Contains(component))AssetDatabase.AddObjectToAsset(component,profile);var volume=new GameObject("Mansion atmosphere").AddComponent<Volume>();SceneManager.MoveGameObjectToScene(volume.gameObject,scene);volume.isGlobal=true;volume.sharedProfile=profile;
    }
    public static object All() { return Build(true); }
    public static object Rebuild() { return Build(false); }
    static void AuditInitialContacts()
    {
        Physics.SyncTransforms();var colliders=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Collider>()).ToArray();var found=new List<object>();
        for(int i=0;i<colliders.Length;i++)
        {
            var a=colliders[i];if(a.attachedRigidbody==null)continue;var ap=a.GetComponentInParent<MansionProp>();
            foreach(var b in Physics.OverlapBox(a.bounds.center,a.bounds.extents+Vector3.one*.002f))
            {
                int j=Array.IndexOf(colliders,b);if(j<0||j==i||(b.attachedRigidbody!=null&&j<i)||a.attachedRigidbody==b.attachedRigidbody)continue;
                var bp=b.GetComponentInParent<MansionProp>();if(ap!=null&&ap.mountSupports.Contains(b)||bp!=null&&bp.mountSupports.Contains(a))continue;
                var aj=a.attachedRigidbody.GetComponent<Joint>();var bj=b.attachedRigidbody!=null?b.attachedRigidbody.GetComponent<Joint>():null;
                if(aj!=null&&!aj.enableCollision&&aj.connectedBody!=null&&aj.connectedBody==b.attachedRigidbody||bj!=null&&!bj.enableCollision&&bj.connectedBody!=null&&bj.connectedBody==a.attachedRigidbody)continue;
                if(Physics.ComputePenetration(a,a.transform.position,a.transform.rotation,b,b.transform.position,b.transform.rotation,out var normal,out var distance)&&distance>.004f)
                    found.Add(new{a=ap!=null?ap.name:a.name,partA=a.name,b=bp!=null?bp.name:b.name,partB=b.name,depth=distance});
            }
        }
        File.WriteAllText("Documentation/Mansion/initial_penetrations.json",JsonConvert.SerializeObject(found,Formatting.Indented));
    }
    public static object RepairFinal()
    {
        if(Application.isPlaying)throw new Exception("Use Edit Mode");
        specs=JsonConvert.DeserializeObject<List<AssetSpec>>(File.ReadAllText(Folder+"/Art/Data/assets.json")).ToDictionary(x=>x.id);
        materials=JsonConvert.DeserializeObject<List<MaterialSpec>>(File.ReadAllText(Folder+"/Art/Data/materials.json")).ToDictionary(x=>x.name,x=>AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/"+x.name+".mat"));
        reuseAssets=false;prefabs.Clear();foreach(int id in new[]{21,26,27,29,31,32,71,79,115})Prop(id);
        reuseAssets=true;return PrepareTest();
    }
    public static object SceneOnly() { reuseAssets=true; return Build(false); }
    public static object SceneTest() { reuseAssets=true; return PrepareTest(); }
    public static object PrepareTest()
    {
        var result=Build(false);var player=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<FunnyMovement>()).Single();
        player.enabled=false;var look=player.GetComponentInChildren<FirstPersonCamera>();look.captureOnStart=false;look.enabled=false;look.GetComponent<ObjectGrabber>().enabled=false;look.GetComponent<MansionInteraction>().enabled=false;
        AuditInitialContacts();EditorApplication.isPlaying=true;return result;
    }
    static object Build(bool import)
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode before building the persistent mansion scene");if(import)ImportLibrary();else{specs=JsonConvert.DeserializeObject<List<AssetSpec>>(File.ReadAllText(Folder+"/Art/Data/assets.json")).ToDictionary(x=>x.id);materials=JsonConvert.DeserializeObject<List<MaterialSpec>>(File.ReadAllText(Folder+"/Art/Data/materials.json")).ToDictionary(x=>x.name,x=>AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/"+x.name+".mat"));}prefabs.Clear();placed.Clear();instances=0;
        foreach(var s in Enumerable.Range(0,SceneManager.sceneCount).Select(SceneManager.GetSceneAt).ToArray())if(s.isDirty)throw new Exception("Unsaved scene needs saving before level build: "+s.path);
        scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);scene.name="MansionPrototype";SceneManager.SetActiveScene(scene);
        architecture=new GameObject("Mansion shell — structural surfaces").transform;contents=new GameObject("Mansion contents — physical and breakable").transform;ceilings=new GameObject("Ceilings — hide in editor for plan view").transform;ceilings.SetParent(architecture);
        for(int x=0;x<8;x++)for(int z=0;z<8;z++){Structure(6,new Vector3(-10.5f+3*x,-.18f,-10.5f+3*z));Structure(7,new Vector3(-10.5f+3*x,3.2f,-10.5f+3*z),0,null,ceilings);}
        // Outer shell: real window openings, individually breakable glazed frames.
        for(int i=0;i<8;i++)
        {
            float p=-10.5f+i*3;foreach(float x in new[]{-12f,12}){float yaw=x<0?90:-90;Structure(i%2==0?3:1,new Vector3(x,0,p),yaw,new Vector3(1,3.2f/3,1));if(i%2==0)Place(11,"Window",new Vector3(x,1.08f,p),yaw,true);}
            foreach(float z in new[]{-12f,12}){float yaw=z<0?0:180;if(Mathf.Abs(p)<2)continue;Structure(i%2==0?3:1,new Vector3(p,0,z),yaw,new Vector3(1,3.2f/3,1));if(i%2==0)Place(11,"Window",new Vector3(p,1.08f,z),yaw,true);}
        }
        // Fill front/back central openings with a framed entrance and a rear garden exit.
        foreach(float z in new[]{-12f,12}){Place(9,"Entrance",new Vector3(0,0,z),z<0?0:180,true);Wall(new Vector3(-3,0,z),new Vector3(-1.08f,0,z),z<0?0:180);Wall(new Vector3(1.08f,0,z),new Vector3(3,0,z),z<0?0:180);}
        HallWall(-2,90);HallWall(2,-90);foreach(float z in new[]{-4f,4}){Wall(new Vector3(-12,0,z),new Vector3(-2,0,z),z<0?0:180);Wall(new Vector3(2,0,z),new Vector3(12,0,z),z<0?0:180);}
        // Exterior apron catches thrown objects; the structural boundary remains intact.
        var outside=GameObject.CreatePrimitive(PrimitiveType.Cube);outside.name="Stone courtyard apron";outside.transform.SetParent(architecture);outside.transform.localScale=new Vector3(34,.3f,34);outside.transform.position=new Vector3(0,-.36f,0);outside.GetComponent<Renderer>().sharedMaterial=materials["MD_012_cream_marble"];
        Furnish();CreatePlayer();Lighting();AmbientActors();
        Physics.SyncTransforms();
        var supports=architecture.GetComponentsInChildren<Collider>();
        foreach(var prop in contents.GetComponentsInChildren<MansionProp>().Where(p=>p.wallMounted))
        {
            var shapes=prop.GetComponentsInChildren<Collider>();var bounds=new Bounds(prop.transform.position,Vector3.zero);foreach(var shape in shapes)bounds.Encapsulate(shape.bounds);bounds.Expand(.08f);
            prop.mountSupports=supports.Where(c=>c.bounds.Intersects(bounds)&&(!c.name.StartsWith("MD_6_ParquetFloor")&&c.name!="Stone courtyard apron")).ToArray();
            PrefabUtility.RecordPrefabInstancePropertyModifications(prop);
        }
        var overview=new GameObject("Mansion Plan Camera").AddComponent<Camera>();overview.transform.position=new Vector3(21,30,-25);overview.transform.LookAt(Vector3.zero);overview.orthographic=true;overview.orthographicSize=20;overview.enabled=false;overview.farClipPlane=120;overview.clearFlags=CameraClearFlags.SolidColor;overview.backgroundColor=new Color(.12f,.15f,.18f);
        EditorSceneManager.SaveScene(scene,ScenePath);var buildScenes=EditorBuildSettings.scenes.ToList();if(!buildScenes.Any(s=>s.path==ScenePath)){buildScenes.Add(new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=buildScenes.ToArray();}AssetDatabase.SaveAssets();
        Directory.CreateDirectory("Documentation/Mansion");File.WriteAllText("Documentation/Mansion/placements.json",JsonConvert.SerializeObject(placed,Formatting.Indented));var result=new{scene=ScenePath,props=instances,uniquePropTypes=prefabs.Count,physicalBodies=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Rigidbody>()).Count(),mechanisms=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<MansionMechanism>()).Count()};File.WriteAllText("Documentation/Mansion/build.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
    }
}
