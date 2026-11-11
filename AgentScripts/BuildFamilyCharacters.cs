using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public static class BuildFamilyCharacters
{
    const string Root = "Assets/Game/Characters/Family";
    const string ScenePath = "Assets/Game/Scenes/FamilyPreview.unity";
    static readonly Dictionary<string,string> TextureNames = new Dictionary<string,string> {
        {"father drawn face","father_face"},{"mother drawn face","mother_face"},
        {"cashmere painted","cashmere"},{"coat facing painted","cashmere"},
        {"red floral brocade","red_brocade"},{"burgundy facing painted","red_wool"},
        {"black pinstripe painted","black_wool"},{"red pinstripe painted","red_wool"},
        {"ivory silk painted","ivory_silk"},{"plum embroidered ascot","plum_embroidery"},
        {"bob painted strands","painted_hair"}
    };
    static string Folder(string name) => Root + (name == "FamilyFather" ? "/Father" : "/Mother");
    static string ModelPath(string name) => Folder(name) + "/" + name + ".fbx";
    static string PrefabPath(string name) => Folder(name) + "/" + name + ".prefab";
    static void ConfigureTextures()
    {
        foreach(var path in Directory.GetFiles(Root+"/Textures","*.png"))
        {
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;
            importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=256;
            importer.wrapMode=Path.GetFileName(path).Contains("face")?TextureWrapMode.Clamp:TextureWrapMode.Repeat;
            importer.SaveAndReimport();
        }
    }
    static object Import(string name)
    {
        string path=ModelPath(name);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(ModelImporter)AssetImporter.GetAtPath(path);
        importer.globalScale=1.75f;importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;
        importer.importNormals=ModelImporterNormals.Import;importer.isReadable=true;
        importer.animationType=ModelImporterAnimationType.Human;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
        for(int pass=0;pass<2;pass++)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var transforms=source.GetComponentsInChildren<Transform>();var names=transforms.Select(t=>t.name).ToHashSet();
            var desc=importer.humanDescription;
            desc.human=HumanTrait.BoneName.Where(n=>names.Contains("MD_"+n.Replace(" ","")))
                .Select(n=>new HumanBone {humanName=n,boneName="MD_"+n.Replace(" ",""),limit=new HumanLimit {useDefaultValues=true}}).ToArray();
            if(desc.human.Length!=22)throw new Exception(name+": expected 22 mapped bones.");
            desc.skeleton=transforms.Select(t=>new SkeletonBone {name=t.name,position=t.localPosition,rotation=t.localRotation,scale=t.localScale}).ToArray();
            desc.upperArmTwist=.5f;desc.lowerArmTwist=.5f;desc.upperLegTwist=.5f;desc.lowerLegTwist=.5f;
            desc.armStretch=.02f;desc.legStretch=.02f;importer.humanDescription=desc;importer.SaveAndReimport();
        }
        var avatar=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().Single();
        if(!avatar.isValid||!avatar.isHuman)throw new Exception(name+": invalid Humanoid.");
        var go=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));go.name=name;
        try
        {
            var animator=go.GetComponent<Animator>();animator.avatar=avatar;animator.runtimeAnimatorController=null;
            animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            var savedMaterials=new Dictionary<string,Material>();
            foreach(var skin in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var materials=skin.sharedMaterials;
                for(int i=0;i<materials.Length;i++)
                {
                    string originalName=materials[i].name;string materialName=originalName.Replace("FAM • ","");
                    if(!savedMaterials.TryGetValue(materialName,out var material))
                    {
                        string materialPath=Folder(name)+"/"+materialName+".mat";
                        material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                        if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,materialPath);}
                        material.name=materialName;
                        if(TextureNames.TryGetValue(materialName,out var texture))
                        {
                            var image=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/"+texture+".png");
                            if(image==null)throw new Exception("Missing authored texture "+texture);
                            material.SetTexture("_BaseMap",image);material.SetColor("_BaseColor",Color.white);
                        }
                        else material.SetColor("_BaseColor",materials[i].HasProperty("_BaseColor")?materials[i].GetColor("_BaseColor"):materials[i].color);
                        material.SetFloat("_Metallic",materialName.Contains("gold")?.35f:0);
                        material.SetFloat("_Smoothness",materialName.Contains("gold")?.25f:.04f);
                        EditorUtility.SetDirty(material);savedMaterials[materialName]=material;
                        importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),"FAM • "+materialName),material);
                    }
                    materials[i]=material;
                }
                skin.sharedMaterials=materials;skin.updateWhenOffscreen=true;skin.shadowCastingMode=ShadowCastingMode.On;skin.receiveShadows=true;
                var bounds=skin.localBounds;bounds.Expand(bounds.size*.15f);skin.localBounds=bounds;
            }
            if(animator.humanScale<1||animator.humanScale>3)throw new Exception(name+": FBX unit normalization failed: "+animator.humanScale);
            Relax(go);
            PrefabUtility.SaveAsPrefabAsset(go,PrefabPath(name));AssetDatabase.WriteImportSettingsIfDirty(path);AssetDatabase.SaveAssets();
            return new {name,avatar.isValid,avatar.isHuman,animator.humanScale,triangles=go.GetComponentsInChildren<SkinnedMeshRenderer>().Sum(s=>s.sharedMesh.triangles.Length/3),
                skinnedMeshes=go.GetComponentsInChildren<SkinnedMeshRenderer>().Length,texturedMaterials=savedMaterials.Values.Count(m=>m.GetTexture("_BaseMap")!=null)};
        }
        finally {UnityEngine.Object.DestroyImmediate(go);}
    }
    static void Relax(GameObject go)
    {
        var transforms=go.GetComponentsInChildren<Transform>();
        foreach(var pair in new[]{("Left",-1f),("Right",1f)})
        {
            var upper=transforms.Single(t=>t.name=="MD_"+pair.Item1+"UpperArm");
            var lower=transforms.Single(t=>t.name=="MD_"+pair.Item1+"LowerArm");
            var target=new Vector3(pair.Item2*Mathf.Cos(68*Mathf.Deg2Rad),-Mathf.Sin(68*Mathf.Deg2Rad),0);
            upper.rotation=Quaternion.FromToRotation((lower.position-upper.position).normalized,target)*upper.rotation;
        }
    }
    public static object ImportAll()
    {
        if(Application.isPlaying)throw new Exception("Import in Edit Mode.");
        ConfigureTextures();return new[]{Import("FamilyFather"),Import("FamilyMother")};
    }
    public static object Gallery()
    {
        if(Application.isPlaying)throw new Exception("Build in Edit Mode.");
        var current=SceneManager.GetActiveScene();
        if(current.isDirty)throw new Exception("Current scene has unsaved edits; gallery creation leaves it intact.");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
        foreach(var name in new[]{"FamilyFather","FamilyMother"})
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(name)),scene);
            go.name=name;go.transform.localScale=Vector3.one*.5f;go.transform.position=new Vector3(name=="FamilyFather"?-.7f:.7f,.012f,0);
        }
        var floor=GameObject.CreatePrimitive(PrimitiveType.Plane);floor.name="Family preview floor";floor.transform.localScale=Vector3.one*2;
        floor.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Art/LowPoly/Ground Basalt.mat");
        var camera=new GameObject("Family Preview Camera").AddComponent<Camera>();camera.gameObject.AddComponent<AudioListener>();camera.tag="MainCamera";
        camera.transform.position=new Vector3(.6f,2.2f,6);camera.transform.LookAt(new Vector3(0,.98f,0));camera.orthographic=true;camera.orthographicSize=1.25f;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.45f,.49f,.52f);camera.nearClipPlane=.03f;camera.farClipPlane=100;
        var light=new GameObject("Family soft key").AddComponent<Light>();light.type=LightType.Directional;light.transform.rotation=Quaternion.Euler(35,-25,0);light.intensity=1.5f;light.shadows=LightShadows.Soft;
        RenderSettings.skybox=null;RenderSettings.fog=false;RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.7f,.72f,.75f);
        EditorSceneManager.SaveScene(scene,ScenePath);EditorSceneManager.OpenScene(ScenePath);
        return "Saved FamilyPreview with both separately rigged parents and pixel-painted materials.";
    }
    public static object Verify()
    {
        var reports=new List<object>();
        foreach(var name in new[]{"FamilyFather","FamilyMother"})
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(name));var animator=prefab.GetComponent<Animator>();
            if(!animator.avatar.isValid||!animator.isHuman)throw new Exception("Invalid Avatar "+name);
            var skins=prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach(var skin in skins)
            {
                if(!skin.sharedMesh.boneWeights.All(w=>Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1)<.0001f))throw new Exception("Unweighted vertex "+skin.name);
                if(!skin.sharedMaterials.All(m=>m!=null&&m.shader.name=="Universal Render Pipeline/Lit"))throw new Exception("Broken material "+skin.name);
                if(skin.sharedMaterials.Any(m=>m.GetTexture("_BaseMap")!=null)&&skin.sharedMesh.uv.Length!=skin.sharedMesh.vertexCount)throw new Exception("Missing UVs "+skin.name);
            }
            var textures=skins.SelectMany(s=>s.sharedMaterials).Select(m=>m.GetTexture("_BaseMap")).Where(t=>t!=null).Distinct().ToArray();
            foreach(var texture in textures)
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
                if(importer.filterMode!=FilterMode.Point||importer.mipmapEnabled)throw new Exception("Pixel filtering lost "+texture.name);
            }
            reports.Add(new{name,animator.avatar.isValid,animator.isHuman,animator.humanScale,triangles=skins.Sum(s=>s.sharedMesh.triangles.Length/3),textures=textures.Select(t=>t.name).ToArray(),bones=prefab.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("MD_"))});
        }
        return reports;
    }
    public static async Task<object> MotionProbe()
    {
        if(!Application.isPlaying)throw new Exception("Run in FamilyPreview Play Mode.");
        var reports=new List<object>();int startFrame=Time.frameCount;
        foreach(var name in new[]{"FamilyFather","FamilyMother"})
        {
            var go=GameObject.Find(name);var animator=go.GetComponent<Animator>();
            foreach(var prop in go.GetComponentsInChildren<SkinnedMeshRenderer>().Where(s=>s.name.EndsWith("_Prop")))prop.enabled=false;
            animator.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Game/Animations/PlayerMoveAnimation.controller");
            animator.applyRootMotion=false;animator.Rebind();animator.SetBool("Grounded",true);animator.SetFloat("PlaybackRate",1);animator.SetFloat("MoveZ",.16f);animator.SetFloat("Speed",.16f);animator.Play("Locomotion",0,0);animator.Update(0);
            var foot=animator.GetBoneTransform(HumanBodyBones.LeftFoot);var initial=foot.localRotation;float maxAngle=0;
            for(int i=0;i<8;i++){await Task.Delay(160);maxAngle=Mathf.Max(maxAngle,Quaternion.Angle(initial,foot.localRotation));}
            if(maxAngle<2)throw new Exception(name+": Humanoid did not animate.");
            reports.Add(new{name,maximumFootRotation=maxAngle,frame=Time.frameCount,animator.isHuman,animator.applyRootMotion});
        }
        if(Time.frameCount<=startFrame)throw new Exception("Game was frozen.");
        return new{startFrame,endFrame=Time.frameCount,actors=reports};
    }
}
