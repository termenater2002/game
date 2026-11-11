using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Import the original V3 atlas characters; preserve previous assets and physics.</summary>
public static class BuildCharacterStyleV3
{
    public static readonly string[] Names={"LilacV3","RiderV3","RiderTShirtV3","FamilyFatherV3","FamilyMotherV3"};
    const string Root="Assets/Game/Characters/StyleV3/";
    static string Folder(string name)=>Root+name+"/";
    static string Prefab(string name)=>Folder(name)+name+".prefab";
    static Avatar AvatarFor(string name)=>AssetDatabase.LoadAllAssetsAtPath(Folder(name)+name+".fbx").OfType<Avatar>().Single();
    static object Import(string name)
    {
        string folder=Folder(name),path=folder+name+".fbx",texturePath=folder+name+"_atlas.png";
        AssetDatabase.ImportAsset(texturePath,ImportAssetOptions.ForceSynchronousImport);
        var ti=(TextureImporter)AssetImporter.GetAtPath(texturePath);ti.textureType=TextureImporterType.Default;
        ti.sRGBTexture=true;ti.filterMode=FilterMode.Point;ti.mipmapEnabled=false;ti.wrapMode=TextureWrapMode.Clamp;
        ti.maxTextureSize=512;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.SaveAndReimport();
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(ModelImporter)AssetImporter.GetAtPath(path);
        importer.globalScale=1.75f;importer.importAnimation=false;importer.importBlendShapes=true;
        importer.importCameras=false;importer.importLights=false;importer.importNormals=ModelImporterNormals.Import;
        importer.isReadable=true;importer.animationType=ModelImporterAnimationType.Human;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
        // Re-read the normalized FBX rest pose on the second pass (metres, not centimetres).
        for(int pass=0;pass<2;pass++)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);var transforms=source.GetComponentsInChildren<Transform>();
            var names=transforms.Select(t=>t.name).ToHashSet();var desc=importer.humanDescription;
            desc.human=HumanTrait.BoneName.Where(n=>names.Contains("MD_"+n.Replace(" ","")))
                .Select(n=>new HumanBone{humanName=n,boneName="MD_"+n.Replace(" ",""),limit=new HumanLimit{useDefaultValues=true}}).ToArray();
            if(desc.human.Length!=22)throw new Exception(name+": expected 22 human bones.");
            desc.skeleton=transforms.Select(t=>new SkeletonBone{name=t.name,position=t.localPosition,rotation=t.localRotation,scale=t.localScale}).ToArray();
            desc.upperArmTwist=.5f;desc.lowerArmTwist=.5f;desc.upperLegTwist=.5f;desc.lowerLegTwist=.5f;desc.armStretch=.02f;desc.legStretch=.02f;
            importer.humanDescription=desc;importer.SaveAndReimport();
        }
        var avatar=AvatarFor(name);if(!avatar.isValid||!avatar.isHuman)throw new Exception(name+": invalid Avatar.");
        string materialPath=folder+"Painted Atlas.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,materialPath);}
        material.SetColor("_BaseColor",Color.white);material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
        material.SetFloat("_Metallic",0);material.SetFloat("_Smoothness",.04f);material.SetFloat("_Cull",0);material.doubleSidedGI=true;
        material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",new Color(.06f,.06f,.06f));material.SetTexture("_EmissionMap",material.GetTexture("_BaseMap"));EditorUtility.SetDirty(material);
        var go=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));go.name=name;
        try
        {
            var animator=go.GetComponent<Animator>();animator.avatar=avatar;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            animator.runtimeAnimatorController=name.StartsWith("Rider")?AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Game/Animations/PlayerMoveAnimation.controller"):null;
            if(name.StartsWith("Rider"))go.AddComponent<PlayerAnimationIKRelay>();
            foreach(var skin in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                foreach(var original in skin.sharedMaterials)importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),original.name),material);
                skin.sharedMaterials=skin.sharedMaterials.Select(m=>material).ToArray();skin.updateWhenOffscreen=true;skin.shadowCastingMode=ShadowCastingMode.On;skin.receiveShadows=true;
                var bounds=skin.localBounds;bounds.Expand(bounds.size*.18f);skin.localBounds=bounds;
                for(int i=0;i<skin.sharedMesh.blendShapeCount;i++)skin.SetBlendShapeWeight(i,0);
            }
            if(animator.humanScale<1||animator.humanScale>3)throw new Exception(name+": unit normalization failed.");
            if(!name.StartsWith("Rider"))Relax(go);
            PrefabUtility.SaveAsPrefabAsset(go,Prefab(name));AssetDatabase.WriteImportSettingsIfDirty(path);AssetDatabase.SaveAssets();
            return Report(go,name);
        }
        finally{UnityEngine.Object.DestroyImmediate(go);}
    }
    static void Relax(GameObject go)
    {
        var transforms=go.GetComponentsInChildren<Transform>();
        foreach(var pair in new[]{("Left",-1f),("Right",1f)})
        {
            var upper=transforms.Single(t=>t.name=="MD_"+pair.Item1+"UpperArm");var lower=transforms.Single(t=>t.name=="MD_"+pair.Item1+"LowerArm");
            var target=new Vector3(pair.Item2*Mathf.Cos(65*Mathf.Deg2Rad),-Mathf.Sin(65*Mathf.Deg2Rad),0);
            upper.rotation=Quaternion.FromToRotation((lower.position-upper.position).normalized,target)*upper.rotation;
        }
    }
    static object Report(GameObject go,string name)
    {
        var animator=go.GetComponent<Animator>();var skins=go.GetComponentsInChildren<SkinnedMeshRenderer>();
        return new{name,animator.isHuman,animator.avatar.isValid,animator.humanScale,triangles=skins.Sum(s=>s.sharedMesh.triangles.Length/3),
            skinnedMeshes=skins.Length,bones=go.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("MD_")),
            morphs=skins.SelectMany(s=>Enumerable.Range(0,s.sharedMesh.blendShapeCount).Select(i=>s.sharedMesh.GetBlendShapeName(i))).ToArray()};
    }
    public static object ImportAll()
    {
        if(Application.isPlaying)throw new Exception("Use Edit Mode.");
        return Names.Select(Import).ToArray();
    }
    public static object ImportCorrections()
    {
        if(Application.isPlaying)throw new Exception("Use Edit Mode.");
        return new[]{"LilacV3","RiderV3","RiderTShirtV3","FamilyMotherV3"}.Select(Import).ToArray();
    }
    public static object ImportSalute()
    {
        string path=Folder("LilacV3")+"LilacV3_Salute.fbx";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.globalScale=1.75f;importer.importCameras=false;importer.importLights=false;
        importer.animationType=ModelImporterAnimationType.Human;importer.avatarSetup=ModelImporterAvatarSetup.CopyFromOther;importer.sourceAvatar=AvatarFor("LilacV3");
        importer.importAnimation=true;importer.importBlendShapes=true;importer.SaveAndReimport();
        var clips=importer.defaultClipAnimations;if(clips.Length!=1)throw new Exception("Expected one salute take.");
        clips[0].name="LilacV3 Salute";clips[0].firstFrame=1;clips[0].lastFrame=72;clips[0].loopTime=true;clips[0].lockRootRotation=true;clips[0].lockRootPositionXZ=true;
        importer.clipAnimations=clips;importer.SaveAndReimport();
        var clip=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().First(c=>!c.name.StartsWith("__preview__"));
        // Keep an editable Humanoid clip and clamp only eyelid curves: FBX resampling can
        // otherwise introduce small negative weights between zero-valued blink keys.
        string clipPath=Folder("LilacV3")+"LilacV3 Salute.anim";
        var editable=AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if(editable==null){editable=UnityEngine.Object.Instantiate(clip);AssetDatabase.CreateAsset(editable,clipPath);}
        else EditorUtility.CopySerialized(clip,editable);
        editable.name="LilacV3 Salute";
        foreach(var binding in AnimationUtility.GetCurveBindings(editable).Where(b=>b.propertyName.StartsWith("blendShape.")))
        {
            var curve=AnimationUtility.GetEditorCurve(editable,binding);var keys=curve.keys;
            for(int i=0;i<keys.Length;i++)keys[i].value=Mathf.Clamp(keys[i].value,0,100);
            curve.keys=keys;
            for(int i=0;i<curve.length;i++){AnimationUtility.SetKeyLeftTangentMode(curve,i,AnimationUtility.TangentMode.Linear);AnimationUtility.SetKeyRightTangentMode(curve,i,AnimationUtility.TangentMode.Linear);}
            AnimationUtility.SetEditorCurve(editable,binding,curve);
        }
        EditorUtility.SetDirty(editable);clip=editable;
        string controllerPath=Folder("LilacV3")+"LilacV3 Salute.controller";
        var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath)??AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        var machine=controller.layers[0].stateMachine;var state=machine.states.Length==0?machine.AddState("Salute"):machine.states[0].state;state.motion=clip;machine.defaultState=state;
        var go=PrefabUtility.LoadPrefabContents(Prefab("LilacV3"));go.GetComponent<Animator>().runtimeAnimatorController=controller;
        PrefabUtility.SaveAsPrefabAsset(go,Prefab("LilacV3"));PrefabUtility.UnloadPrefabContents(go);AssetDatabase.SaveAssets();
        return new{clip.name,clip.length,clip.frameRate,clip.isHumanMotion,clip.isLooping,curves=AnimationUtility.GetCurveBindings(clip).Length};
    }
    public static object Verify()
    {
        foreach(string name in Names)
        {
            var go=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(name));var animator=go.GetComponent<Animator>();
            if(!animator.isHuman||!animator.avatar.isValid)throw new Exception("Invalid Avatar "+name);
            foreach(var skin in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var mesh=skin.sharedMesh;
                if(mesh.uv.Length!=mesh.vertexCount)throw new Exception("Missing UVs "+skin.name);
                if(!mesh.boneWeights.All(w=>Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1)<.0001f))throw new Exception("Unweighted vertex "+skin.name);
                if(!skin.sharedMaterials.All(m=>m!=null&&m.GetTexture("_BaseMap")!=null&&m.shader.name=="Universal Render Pipeline/Lit"))throw new Exception("Atlas/material missing "+skin.name);
                var atlas=(Texture2D)skin.sharedMaterials[0].GetTexture("_BaseMap");
                if(atlas.width!=512||atlas.height!=512||atlas.filterMode!=FilterMode.Point||atlas.mipmapCount!=1)throw new Exception("Atlas import settings incorrect "+name);
            }
            if(!name.StartsWith("Rider")&&!go.GetComponentsInChildren<SkinnedMeshRenderer>().Any(s=>s.sharedMesh.blendShapeCount==2))throw new Exception("Missing facial morphs "+name);
        }
        return Names.Select(n=>Report(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(n)),n)).ToArray();
    }
    public static object ApplyHero()
    {
        if(Application.isPlaying)throw new Exception("Use Edit Mode.");
        if(SceneManager.GetActiveScene().isDirty)throw new Exception("Preserve unsaved scene edits before applying.");
        EditorSceneManager.OpenScene("Assets/Game/Scenes/SampleScene.unity");
        var player=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();var oldAnimator=player.GetComponent<Animator>();float originalScale=oldAnimator.humanScale;
        var previous=player.transform.Find("RiderVisual");if(previous!=null)UnityEngine.Object.DestroyImmediate(previous.gameObject);
        foreach(var r in player.GetComponentsInChildren<Renderer>(true)){r.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
        oldAnimator.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(oldAnimator);
        var visual=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab("RiderV3")),player.transform);
        visual.name="RiderVisual";visual.transform.localPosition=Vector3.zero;visual.transform.localRotation=Quaternion.identity;visual.transform.localScale=Vector3.one;
        var animator=visual.GetComponent<Animator>();var driver=player.GetComponent<PlayerAnimationDriver>();driver.characterAnimator=animator;
        float ratio=Mathf.Clamp(animator.humanScale/Mathf.Max(.01f,originalScale),.7f,1.8f);
        driver.walkReferenceSpeed=.16f*ratio;driver.runReferenceSpeed=.4f*ratio;driver.blendSpaceScale=1/ratio;driver.footSoleFraction=.05f;driver.holdingReachFraction=.21f;
        player.GetComponent<FirstPersonBodyVisibility>().bodyRenderers=visual.GetComponentsInChildren<Renderer>();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        return new{model="RiderV3",ratio,animator.humanScale,collider=player.GetComponent<CapsuleCollider>().height};
    }
    public static object Gallery()=>BuildGallery(Names,"Assets/Game/Scenes/CharacterStyleV3.unity");
    public static object ParentsGallery()=>BuildGallery(new[]{"FamilyFatherV3","FamilyMotherV3"},"Assets/Game/Scenes/FamilyPreview.unity");
    static object BuildGallery(string[] names,string path)
    {
        if(Application.isPlaying||SceneManager.GetActiveScene().isDirty)throw new Exception("Use a saved scene in Edit Mode.");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
        for(int i=0;i<names.Length;i++)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(names[i])),scene);
            go.name=names[i];go.transform.localScale=Vector3.one*.5f;go.transform.position=new Vector3((i-(names.Length-1)/2f)*1.35f,.012f,0);
            if(names[i].StartsWith("Rider")){go.GetComponent<Animator>().runtimeAnimatorController=null;Relax(go);}
        }
        var floor=GameObject.CreatePrimitive(PrimitiveType.Plane);floor.name="Neutral preview floor";floor.transform.localScale=Vector3.one*3;
        floor.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Art/LowPoly/Ground Basalt.mat");
        var camera=new GameObject("Character Style Camera").AddComponent<Camera>();camera.gameObject.AddComponent<AudioListener>();camera.tag="MainCamera";
        camera.transform.position=new Vector3(.15f,1.55f,7);camera.transform.LookAt(new Vector3(0,.95f,0));camera.orthographic=true;camera.orthographicSize=names.Length>2?1.9f:1.3f;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.41f,.44f,.48f);camera.nearClipPlane=.03f;camera.farClipPlane=100;
        var light=new GameObject("Soft directional key").AddComponent<Light>();light.type=LightType.Directional;light.transform.rotation=Quaternion.Euler(35,155,0);light.intensity=1.3f;light.shadows=LightShadows.Soft;
        RenderSettings.skybox=null;RenderSettings.fog=false;RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.65f,.67f,.7f);
        EditorSceneManager.SaveScene(scene,path);EditorSceneManager.OpenScene(path);return path;
    }
    public static async Task<object> CaptureLoop()
    {
        if(!Application.isPlaying||SceneManager.GetActiveScene().name!="CharacterStyleV3")throw new Exception("Play CharacterStyleV3 first.");
        string dir="Documentation/StyleV3/AnimationFrames";Directory.CreateDirectory(dir);
        var camera=Camera.main;var target=camera.targetTexture;var active=RenderTexture.active;float aspect=camera.aspect;
        var rt=RenderTexture.GetTemporary(1280,720,24,RenderTextureFormat.ARGB32);var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);
        var samples=new List<object>();var animator=GameObject.Find("LilacV3").GetComponent<Animator>();
        animator.Play("Salute",0,0);animator.Update(0);
        int start=Time.frameCount;float startTime=Time.time;
        try
        {
            camera.aspect=1280f/720;
            for(int i=0;i<40;i++)
            {
                await Task.Delay(70);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
                texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();
                File.WriteAllBytes(dir+"/frame"+i.ToString("D3")+".png",texture.EncodeToPNG());
                var head=animator.GetComponentsInChildren<SkinnedMeshRenderer>().First(s=>s.sharedMesh.blendShapeCount==2);
                samples.Add(new{index=i,frame=Time.frameCount,time=Time.time-startTime,rightHand=animator.GetBoneTransform(HumanBodyBones.RightHand).position.ToString("F4"),blink=head.GetBlendShapeWeight(0)});
            }
        }
        finally{camera.targetTexture=target;camera.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(texture);}
        File.WriteAllText("Documentation/StyleV3/AnimationTiming.json",Newtonsoft.Json.JsonConvert.SerializeObject(samples,Newtonsoft.Json.Formatting.Indented));
        if(Time.frameCount<=start)throw new Exception("Frozen player loop.");
        return new{startFrame=start,endFrame=Time.frameCount,samples=samples.Count};
    }
    public static async Task<object> MotionProbe()
    {
        if(!Application.isPlaying)throw new Exception("Run in CharacterStyleV3 Play Mode.");
        int start=Time.frameCount;var reports=new List<object>();
        foreach(string name in Names)
        {
            var go=GameObject.Find(name);var animator=go.GetComponent<Animator>();
            foreach(var prop in go.GetComponentsInChildren<SkinnedMeshRenderer>().Where(s=>s.name.EndsWith("_Prop")))prop.enabled=false;
            if(name!="LilacV3")
            {
                animator.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Game/Animations/PlayerMoveAnimation.controller");
                animator.Rebind();animator.SetBool("Grounded",true);animator.SetFloat("PlaybackRate",1);animator.SetFloat("MoveZ",.16f);animator.SetFloat("Speed",.16f);animator.Play("Locomotion",0,0);animator.Update(0);
            }
            var joint=animator.GetBoneTransform(name=="LilacV3"?HumanBodyBones.RightHand:HumanBodyBones.LeftFoot);var initial=joint.rotation;float maximum=0;
            for(int i=0;i<14;i++){await Task.Delay(140);maximum=Mathf.Max(maximum,Quaternion.Angle(initial,joint.rotation));}
            if(maximum<2)throw new Exception(name+": no real animation observed.");
            reports.Add(new{name,maxJointRotation=maximum,frame=Time.frameCount,animator.applyRootMotion});
        }
        if(Time.frameCount<=start)throw new Exception("Player loop did not advance.");
        return new{startFrame=start,endFrame=Time.frameCount,actors=reports};
    }
}
