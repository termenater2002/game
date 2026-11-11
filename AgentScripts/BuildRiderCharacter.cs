using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Authored low-poly geometry and Humanoid rig. Coordinates below are the editable design source.</summary>
public static class BuildRiderCharacter
{
    const string Folder = "Assets/Game/Characters/Rider";
    const float Scale = 1.75f; // 1.96 design units -> 3.43 local units, fitting the unchanged 3.5-unit capsule.
    static readonly string[] MaterialNames = { "Storm Teal Jacket", "Charcoal Denim", "Graphite Trim", "Ivory Shell", "Smoked Blue Visor", "Amber Details", "Brushed Hardware", "Sage Panels" };
    static readonly string[] Colors = { "426B68", "282C39", "151F29", "D9D2BA", "172E41", "DEA653", "899D9D", "829B8B" };
    static readonly List<Transform> bones = new List<Transform>();
    static readonly Dictionary<string, int> boneIDs = new Dictionary<string, int>();
    static readonly List<HumanBone> humans = new List<HumanBone>();
    static Transform root;
    static Geometry mesh;
    static Vector3 V(float x, float y, float z = 0) => new Vector3(x, y, z);
    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/'); EnsureFolder(path.Substring(0, slash)); AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
    }
    static Transform Bone(string human, string parent, Vector3 position)
    {
        var b = new GameObject("MD_" + human).transform; b.SetParent(parent == null ? root : bones[boneIDs[parent]], false);
        b.position = root.TransformPoint(position * Scale); b.rotation = root.rotation;
        boneIDs[human] = bones.Count; bones.Add(b);
        humans.Add(new HumanBone { humanName = human, boneName = b.name, limit = new HumanLimit { useDefaultValues = true } }); return b;
    }
    static BoneWeight W(string a, string b = null, float weight = 1f)
        => new BoneWeight { boneIndex0 = boneIDs[a], weight0 = b == null ? 1 : weight, boneIndex1 = b == null ? 0 : boneIDs[b], weight1 = b == null ? 0 : 1 - weight };
    sealed class Geometry
    {
        public readonly List<Vector3> vertices = new List<Vector3>(), normals = new List<Vector3>();
        public readonly List<Vector2> uvs = new List<Vector2>();
        public readonly List<BoneWeight> weights = new List<BoneWeight>();
        public readonly List<int>[] triangles = Enumerable.Range(0, MaterialNames.Length).Select(_ => new List<int>()).ToArray();
        public void Face(Vector3[] points, BoneWeight[] skin, int material)
        {
            int start = vertices.Count; var normal = Vector3.Cross(points[1] - points[0], points[2] - points[0]).normalized;
            for (int i = 0; i < points.Length; i++) { vertices.Add(points[i] * Scale); normals.Add(normal); weights.Add(skin[i]); uvs.Add(new Vector2(points[i].x, points[i].y)); }
            for (int i = 1; i < points.Length - 1; i++) triangles[material].AddRange(new[] { start, start + i, start + i + 1 });
        }
        public void Face(Vector3[] points, BoneWeight skin, int material) => Face(points, Enumerable.Repeat(skin, points.Length).ToArray(), material);
    }
    struct Section
    {
        public Vector3 p; public float a, b; public BoneWeight w;
        public Section(Vector3 point, float radiusA, float radiusB, BoneWeight weight) { p = point; a = radiusA; b = radiusB; w = weight; }
    }
    static void Surface(Section[] sections, Vector3 u, Vector3 v, int sides, int material, bool jacketPanels = false)
    {
        var points = new Vector3[sections.Length, sides];
        for (int j = 0; j < sections.Length; j++) for (int i = 0; i < sides; i++) {
            float theta = i * Mathf.PI * 2 / sides;
            points[j, i] = sections[j].p + u * Mathf.Sin(theta) * sections[j].a + v * Mathf.Cos(theta) * sections[j].b;
        }
        for (int j = 0; j < sections.Length - 1; j++) for (int i = 0; i < sides; i++) {
            int next = (i + 1) % sides;
            int mat = jacketPanels && Mathf.Abs(Mathf.Sin((i + .5f) * Mathf.PI * 2 / sides)) > .86f ? 2 : material;
            mesh.Face(new[] { points[j, i], points[j, next], points[j + 1, next], points[j + 1, i] }, new[] { sections[j].w, sections[j].w, sections[j + 1].w, sections[j + 1].w }, mat);
        }
        mesh.Face(Enumerable.Range(0, sides).Reverse().Select(i => points[0, i]).ToArray(), sections[0].w, material);
        mesh.Face(Enumerable.Range(0, sides).Select(i => points[sections.Length - 1, i]).ToArray(), sections[sections.Length - 1].w, material);
    }
    static void Rings(Section[] sections, int sides, int material, bool panels = false) => Surface(sections, Vector3.right, Vector3.forward, sides, material, panels);
    static void Sleeve(Section[] sections, int sides, int material)
    {
        // Surface uses sin*u + cos*v; u x v must point opposite the increasing section axis.
        var axis = (sections[sections.Length - 1].p - sections[0].p).normalized;
        var v = Vector3.up; var u = Vector3.Cross(axis, v).normalized;
        Surface(sections, u, v, sides, material);
    }
    static void Box(Vector3 center, Vector3 size, int material, BoneWeight skin)
    {
        Vector3 s = size / 2;
        Vector3[] p = { center+V(-s.x,-s.y,-s.z),center+V(s.x,-s.y,-s.z),center+V(s.x,s.y,-s.z),center+V(-s.x,s.y,-s.z), center+V(-s.x,-s.y,s.z),center+V(s.x,-s.y,s.z),center+V(s.x,s.y,s.z),center+V(-s.x,s.y,s.z) };
        foreach (var face in new[] {new[]{0,3,2,1},new[]{4,5,6,7},new[]{0,1,5,4},new[]{1,2,6,5},new[]{2,3,7,6},new[]{3,0,4,7}}) mesh.Face(face.Select(i=>p[i]).ToArray(),skin,material);
    }
    static void Skeleton()
    {
        Bone("Hips", null, V(0, 1.0f)); Bone("Spine", "Hips", V(0,1.12f)); Bone("Chest", "Spine", V(0,1.32f));
        Bone("UpperChest", "Chest", V(0,1.44f)); Bone("Neck", "UpperChest", V(0,1.54f)); Bone("Head", "Neck", V(0,1.67f));
        foreach (var pair in new[] { new {side=-1f, name="Left"}, new {side=1f,name="Right"} }) {
            float s=pair.side; string n=pair.name;
            Bone(n+"Shoulder","UpperChest",V(s*.11f,1.46f)); Bone(n+"UpperArm",n+"Shoulder",V(s*.195f,1.46f));
            Bone(n+"LowerArm",n+"UpperArm",V(s*.435f,1.46f)); Bone(n+"Hand",n+"LowerArm",V(s*.64f,1.46f));
            Bone(n+"UpperLeg","Hips",V(s*.085f,.99f)); Bone(n+"LowerLeg",n+"UpperLeg",V(s*.09f,.55f,.01f));
            Bone(n+"Foot",n+"LowerLeg",V(s*.095f,.105f,.005f)); Bone(n+"Toes",n+"Foot",V(s*.095f,.065f,.15f));
        }
    }
    static BoneWeight TorsoSkin(float y) => y < 1.12f ? W("Hips","Spine",.6f) : y < 1.3f ? W("Spine","Chest",.65f) : y < 1.44f ? W("Chest","UpperChest",.7f) : W("UpperChest","Neck",.9f);
    static float Front(float x, float y)
    {
        float width = y < 1.2f ? .12f : y < 1.4f ? .17f : .193f;
        float depth = y < 1.2f ? .085f : .097f;
        return depth * Mathf.Sqrt(Mathf.Clamp01(1 - x*x/(width*width))) + .004f;
    }
    static void ChestPatch(float x0, float x1, float y0, float y1, int material)
    {
        var p = new[] {V(x0,y0,Front(x0,y0)),V(x1,y0,Front(x1,y0)),V(x1,y1,Front(x1,y1)),V(x0,y1,Front(x0,y1))};
        mesh.Face(p,new[]{TorsoSkin(y0),TorsoSkin(y0),TorsoSkin(y1),TorsoSkin(y1)},material);
    }
    static void Torso()
    {
        Rings(new[]{new Section(V(0,.92f),.146f,.09f,W("Hips")),new Section(V(0,1.035f),.139f,.093f,W("Hips")),new Section(V(0,1.075f),.122f,.084f,W("Hips"))},12,1);
        var levels = new[]{ new {y=1.045f,x=.126f,z=.087f},new{y=1.13f,x=.113f,z=.082f},new{y=1.25f,x=.137f,z=.09f},new{y=1.36f,x=.177f,z=.099f},new{y=1.46f,x=.193f,z=.091f},new{y=1.49f,x=.171f,z=.077f},new{y=1.53f,x=.066f,z=.056f} };
        Rings(levels.Select(l=>new Section(V(0,l.y),l.x,l.z,TorsoSkin(l.y))).ToArray(),16,0,true);
        Rings(new[]{new Section(V(0,1.04f),.127f,.09f,W("Hips")),new Section(V(0,1.07f),.125f,.089f,W("Hips"))},12,2);
        Rings(new[]{new Section(V(0,1.49f),.088f,.072f,W("UpperChest")),new Section(V(0,1.565f),.064f,.057f,W("Neck"))},12,2);
        Rings(new[]{new Section(V(0,1.53f),.052f,.049f,W("Neck")),new Section(V(0,1.66f),.054f,.051f,W("Neck","Head",.5f))},10,2);
        foreach(var range in new[]{new[]{1.08f,1.2f},new[]{1.2f,1.33f},new[]{1.33f,1.46f}}) ChestPatch(-.007f,.007f,range[0],range[1],6);
        ChestPatch(-.135f,-.037f,1.30f,1.355f,7); ChestPatch(-.135f,-.037f,1.35f,1.366f,2);
        ChestPatch(.045f,.13f,1.36f,1.38f,5);
        Box(V(0,1.40f,.104f),V(.017f,.028f,.009f),6,W("Chest"));
        foreach(float s in new[]{-1f,1f}) {
            ChestPatch(s<0?-.166f:.08f,s<0?-.08f:.166f,1.415f,1.457f,7);
            ChestPatch(s<0?-.105f:.09f,s<0?-.09f:.105f,1.12f,1.2f,5);
        }
        Box(V(0,1.035f,.096f),V(.04f,.024f,.01f),6,W("Hips"));
        Box(V(.1f,1.0f,.085f),V(.055f,.073f,.025f),2,W("Hips"));
        Box(V(.1f,1.024f,.101f),V(.048f,.018f,.008f),5,W("Hips"));
        // Reflective seam on the back; no lettering or insignia.
        Box(V(0,1.36f,-.103f),V(.012f,.083f,.006f),7,W("Chest"));
    }
    static void Limbs()
    {
        foreach(var pair in new[]{new{side=-1f,name="Left"},new{side=1f,name="Right"}}) {
            float s=pair.side; string n=pair.name;
            var leg = new[]{ new Section(V(s*.095f,.09f,.008f),.106f,.091f,W(n+"LowerLeg")),new Section(V(s*.095f,.22f,.008f),.102f,.09f,W(n+"LowerLeg")),new Section(V(s*.093f,.41f,.005f),.08f,.074f,W(n+"LowerLeg")),new Section(V(s*.09f,.55f,.01f),.067f,.065f,W(n+"LowerLeg",n+"UpperLeg",.55f)),new Section(V(s*.087f,.7f),.081f,.076f,W(n+"UpperLeg")),new Section(V(s*.085f,.88f),.091f,.082f,W(n+"UpperLeg")),new Section(V(s*.085f,1.0f),.086f,.078f,W(n+"UpperLeg","Hips",.65f)) };
            Rings(leg,10,1);
            Rings(new[]{new Section(V(s*.095f,.085f,.008f),.107f,.092f,W(n+"LowerLeg")),new Section(V(s*.095f,.113f,.008f),.108f,.093f,W(n+"LowerLeg"))},10,2);
            Box(V(s*.17f,.80f,.008f),V(.027f,.143f,.1f),1,W(n+"UpperLeg"));
            Box(V(s*.185f,.857f,.008f),V(.009f,.032f,.108f),7,W(n+"UpperLeg"));
            Box(V(s*.19f,.794f,.04f),V(.008f,.037f,.012f),5,W(n+"UpperLeg"));
            Box(V(s*.09f,.553f,.074f),V(.098f,.112f,.012f),2,W(n+"LowerLeg",n+"UpperLeg",.55f));
            Box(V(s*.09f,.57f,.082f),V(.076f,.009f,.005f),7,W(n+"LowerLeg",n+"UpperLeg",.55f));
            var arm = new[]{ new Section(V(s*.17f,1.46f),.063f,.073f,W(n+"UpperArm","UpperChest",.8f)),new Section(V(s*.26f,1.46f),.058f,.065f,W(n+"UpperArm")),new Section(V(s*.39f,1.46f),.05f,.053f,W(n+"UpperArm",n+"LowerArm",.8f)),new Section(V(s*.435f,1.46f),.054f,.057f,W(n+"UpperArm",n+"LowerArm",.45f)),new Section(V(s*.51f,1.46f),.046f,.05f,W(n+"LowerArm")),new Section(V(s*.635f,1.46f),.038f,.041f,W(n+"LowerArm",n+"Hand",.8f)) };
            Sleeve(arm,10,0);
            Sleeve(new[]{new Section(V(s*.61f,1.46f),.041f,.043f,W(n+"LowerArm")),new Section(V(s*.64f,1.46f),.04f,.042f,W(n+"Hand",n+"LowerArm",.5f))},10,2);
            Sleeve(new[]{new Section(V(s*.55f,1.46f),.047f,.048f,W(n+"LowerArm")),new Section(V(s*.564f,1.46f),.046f,.047f,W(n+"LowerArm"))},10,3);
            Box(V(s*.30f,1.507f,.017f),V(.083f,.017f,.075f),7,W(n+"UpperArm"));
            Box(V(s*.435f,1.415f,-.024f),V(.09f,.021f,.062f),2,W(n+"LowerArm",n+"UpperArm",.5f));
            Gloves(s,n); Boots(s,n);
        }
    }
    static void Gloves(float s,string n)
    {
        var w=W(n+"Hand");
        Sleeve(new[]{new Section(V(s*.634f,1.46f),.029f,.028f,w),new Section(V(s*.681f,1.46f),.038f,.024f,w),new Section(V(s*.713f,1.456f),.035f,.02f,w)},8,2);
        Box(V(s*.687f,1.483f,0),V(.043f,.012f,.062f),6,w);
        for(int j=0;j<4;j++) {
            float z=(j-1.5f)*.018f;float end=.757f-(j==0||j==3?.012f:0);
            Sleeve(new[]{new Section(V(s*.71f,1.455f,z),.009f,.009f,w),new Section(V(s*(end-.014f),1.449f,z),.008f,.008f,w),new Section(V(s*end,1.435f,z),.006f,.007f,w)},6,2);
        }
        Sleeve(new[]{new Section(V(s*.665f,1.455f,.035f),.014f,.013f,w),new Section(V(s*.704f,1.435f,.056f),.012f,.011f,w),new Section(V(s*.72f,1.422f,.054f),.009f,.009f,w)},6,2);
    }
    static void Boots(float s,string n)
    {
        float x=s*.095f; var w=W(n+"Foot");
        Rings(new[]{new Section(V(x,.012f,.055f),.071f,.139f,w),new Section(V(x,.04f,.055f),.073f,.142f,w)},10,3);
        Rings(new[]{new Section(V(x,.04f,.055f),.074f,.14f,w),new Section(V(x,.095f,.045f),.067f,.124f,w),new Section(V(x,.17f,.004f),.06f,.066f,W(n+"Foot",n+"LowerLeg",.75f))},10,2);
        Box(V(x,.117f,.085f),V(.085f,.013f,.067f),1,w);
        for(int i=0;i<3;i++) Box(V(x,.126f,.065f+i*.019f),V(.073f,.005f,.007f),3,w);
        Box(V(x,.061f,.188f),V(.098f,.018f,.006f),6,w);
    }
    static void Helmet()
    {
        var w=W("Head");
        Rings(new[]{new Section(V(0,1.59f,.006f),.086f,.098f,w),new Section(V(0,1.65f,.008f),.125f,.143f,w),new Section(V(0,1.74f,.006f),.146f,.159f,w),new Section(V(0,1.84f),.147f,.145f,w),new Section(V(0,1.925f,-.003f),.105f,.111f,w),new Section(V(0,1.965f,-.002f),.025f,.03f,w)},16,3);
        for(int i=0;i<10;i++) {
            float a=Mathf.Lerp(-1.12f,1.12f,i/10f),b=Mathf.Lerp(-1.12f,1.12f,(i+1)/10f);
            Func<float,float,Vector3> p=(theta,y)=>V(.151f*Mathf.Sin(theta),y,.163f*Mathf.Cos(theta)+.007f);
            mesh.Face(new[]{p(a,1.731f),p(b,1.731f),p(b,1.845f-.018f*Mathf.Abs(Mathf.Sin(b))),p(a,1.845f-.018f*Mathf.Abs(Mathf.Sin(a)))},w,4);
            mesh.Face(new[]{p(a,1.845f-.018f*Mathf.Abs(Mathf.Sin(a))),p(b,1.845f-.018f*Mathf.Abs(Mathf.Sin(b))),p(b,1.859f-.018f*Mathf.Abs(Mathf.Sin(b))),p(a,1.859f-.018f*Mathf.Abs(Mathf.Sin(a)))},w,2);
        }
        mesh.Face(new[]{V(-.075f,1.748f,.173f),V(-.028f,1.748f,.174f),V(.026f,1.819f,.171f),V(-.021f,1.819f,.174f)},w,6);
        Vector3[] chin={V(-.061f,1.635f,.16f),V(.061f,1.635f,.16f),V(.085f,1.725f,.168f),V(-.085f,1.725f,.168f)};
        mesh.Face(chin,w,3);
        mesh.Face(new[]{chin[3],V(-.12f,1.72f,.09f),V(-.105f,1.64f,.09f),chin[0]},w,3);
        mesh.Face(new[]{chin[1],V(.105f,1.64f,.09f),V(.12f,1.72f,.09f),chin[2]},w,3);
        for(int i=0;i<3;i++) Box(V(0,1.663f+i*.014f,.171f),V(.067f,.007f,.006f),2,w);
        foreach(float s in new[]{-1f,1f}) {
            Sleeve(new[]{new Section(V(s*.145f,1.784f,.01f),.021f,.021f,w),new Section(V(s*.155f,1.784f,.01f),.021f,.021f,w)},10,2);
            Box(V(s*.162f,1.784f,.01f),V(.006f,.018f,.018f),5,w);
            Box(V(s*.061f,1.909f,.101f),V(.03f,.011f,.02f),2,w);
            Box(V(s*.111f,1.661f,.106f),V(.016f,.042f,.011f),2,w);
        }
        mesh.Face(new[]{V(-.014f,1.902f,.122f),V(.014f,1.902f,.122f),V(.014f,1.971f,.005f),V(-.014f,1.971f,.005f)},w,5);
        mesh.Face(new[]{V(-.014f,1.971f,.005f),V(.014f,1.971f,.005f),V(.014f,1.912f,-.121f),V(-.014f,1.912f,-.121f)},w,5);
    }
    static T Save<T>(T fresh,string path) where T:UnityEngine.Object
    {
        var existing=AssetDatabase.LoadAssetAtPath<T>(path);
        if(existing==null) { AssetDatabase.CreateAsset(fresh,path);return fresh; }
        if(fresh is Mesh fromMesh && existing is Mesh toMesh) {
            // CopySerialized alone can leave stale GPU index/skin buffers on a mesh used by an open scene.
            toMesh.Clear(false);toMesh.indexFormat=fromMesh.indexFormat;toMesh.vertices=fromMesh.vertices;toMesh.normals=fromMesh.normals;toMesh.uv=fromMesh.uv;
            toMesh.boneWeights=fromMesh.boneWeights;toMesh.bindposes=fromMesh.bindposes;toMesh.subMeshCount=fromMesh.subMeshCount;
            for(int i=0;i<fromMesh.subMeshCount;i++) toMesh.SetTriangles(fromMesh.GetTriangles(i),i);
            toMesh.RecalculateBounds();toMesh.UploadMeshData(false);toMesh.name=fromMesh.name;
        } else if(fresh is Material fromMaterial && existing is Material toMaterial) {
            toMaterial.shader=fromMaterial.shader;toMaterial.CopyPropertiesFromMaterial(fromMaterial);toMaterial.name=fromMaterial.name;
        } else EditorUtility.CopySerialized(fresh,existing);
        UnityEngine.Object.DestroyImmediate(fresh);EditorUtility.SetDirty(existing);return existing;
    }
    public static object Build()
    {
        if(Application.isPlaying) throw new Exception("Build in Edit Mode."); EnsureFolder(Folder);
        bones.Clear();boneIDs.Clear();humans.Clear();mesh=new Geometry();
        var go=new GameObject("MajorDamage Rider");root=go.transform;
        try {
            Skeleton();Torso();Limbs();Helmet();
            var description=new HumanDescription { human=humans.ToArray(),skeleton=go.GetComponentsInChildren<Transform>().Select(t=>new SkeletonBone{name=t.name,position=t.localPosition,rotation=t.localRotation,scale=t.localScale}).ToArray(), upperArmTwist=.5f,lowerArmTwist=.5f,upperLegTwist=.5f,lowerLegTwist=.5f,armStretch=.02f,legStretch=.02f,feetSpacing=0,hasTranslationDoF=false };
            var avatar=AvatarBuilder.BuildHumanAvatar(go,description);avatar.name="Rider Humanoid Avatar";
            if(!avatar.isValid||!avatar.isHuman) throw new Exception("Generated rider avatar is not a valid Humanoid.");
            avatar=Save(avatar,Folder+"/RiderAvatar.asset");
            var data=new Mesh{name="Rider Faceted Skinned Geometry",indexFormat=IndexFormat.UInt32};
            data.SetVertices(mesh.vertices);data.SetNormals(mesh.normals);data.SetUVs(0,mesh.uvs);data.boneWeights=mesh.weights.ToArray();
            data.bindposes=bones.Select(b=>b.worldToLocalMatrix*root.localToWorldMatrix).ToArray();data.subMeshCount=MaterialNames.Length;
            for(int i=0;i<MaterialNames.Length;i++) data.SetTriangles(mesh.triangles[i],i);
            data.RecalculateBounds();data=Save(data,Folder+"/RiderMesh.asset");
            var materials=new Material[MaterialNames.Length];
            for(int i=0;i<materials.Length;i++) {
                var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=MaterialNames[i]};ColorUtility.TryParseHtmlString("#"+Colors[i],out var color);
                m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",0);m.SetFloat("_Smoothness",i==4?.42f:.09f);
                materials[i]=Save(m,Folder+"/"+MaterialNames[i]+".mat");
            }
            var rendererObject=new GameObject("Rider Skinned Geometry");rendererObject.transform.SetParent(root,false);
            var renderer=rendererObject.AddComponent<SkinnedMeshRenderer>();renderer.sharedMesh=data;renderer.bones=bones.ToArray();renderer.rootBone=bones[boneIDs["Hips"]];renderer.sharedMaterials=materials;renderer.updateWhenOffscreen=true;renderer.shadowCastingMode=ShadowCastingMode.On;
            renderer.localBounds=new Bounds(V(0,1,0)*Scale,V(2.1f,2.4f,1.2f)*Scale);
            var animator=go.AddComponent<Animator>();animator.avatar=avatar;animator.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Game/Animations/PlayerMoveAnimation.controller");animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            go.AddComponent<PlayerAnimationIKRelay>();
            PrefabUtility.SaveAsPrefabAsset(go,Folder+"/Rider.prefab");AssetDatabase.SaveAssets();
            return Newtonsoft.Json.JsonConvert.SerializeObject(new {avatar.isValid,avatar.isHuman,bones=bones.Count,vertices=data.vertexCount,triangles=mesh.triangles.Sum(t=>t.Count)/3,height=data.bounds.size.y,humanScale=animator.humanScale,prefab=Folder+"/Rider.prefab"});
        } finally {UnityEngine.Object.DestroyImmediate(go);}
    }
    public static object ApplyToMainScene()
    {
        if(Application.isPlaying||SceneManager.GetActiveScene().name!="SampleScene") throw new Exception("Open SampleScene in Edit Mode.");
        var player=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();var oldAnimator=player.GetComponent<Animator>();
        float oldScale=oldAnimator.humanScale;
        var previous=player.transform.Find("RiderVisual");if(previous!=null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
        foreach(var r in player.GetComponentsInChildren<Renderer>(true)) {r.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
        oldAnimator.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(oldAnimator);
        var rider=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/Rider.prefab"),player.transform);
        rider.name="RiderVisual";rider.transform.localPosition=Vector3.zero;rider.transform.localRotation=Quaternion.identity;rider.transform.localScale=Vector3.one;
        var animator=rider.GetComponent<Animator>();var driver=player.GetComponent<PlayerAnimationDriver>();driver.characterAnimator=animator;
        float ratio=Mathf.Clamp(animator.humanScale/Mathf.Max(.01f,oldScale),.7f,1.8f);
        driver.walkReferenceSpeed=.16f*ratio;driver.runReferenceSpeed=.4f*ratio;driver.blendSpaceScale=1f/ratio;driver.footSoleFraction=.05f;driver.holdingReachFraction=.21f;
        var visibility=player.GetComponent<FirstPersonBodyVisibility>();visibility.bodyRenderers=rider.GetComponentsInChildren<Renderer>();
        var side=player.GetComponent<SideViewTestCamera>()??player.gameObject.AddComponent<SideViewTestCamera>();side.firstPersonCamera=player.GetComponentInChildren<FirstPersonCamera>().GetComponent<Camera>();
        if(side.sideCamera==null) {
            var view=new GameObject("Side Test Camera (F6)");view.transform.SetParent(player.transform,false);side.sideCamera=view.AddComponent<Camera>();side.sideCamera.CopyFrom(side.firstPersonCamera);side.sideCamera.enabled=false;side.sideCamera.tag="Untagged";
            var source=side.firstPersonCamera.GetComponents<MonoBehaviour>().FirstOrDefault(c=>c!=null&&c.GetType().Name=="UniversalAdditionalCameraData");if(source!=null) EditorUtility.CopySerialized(source,view.AddComponent(source.GetType()));
        }
        var bounds=player.GetComponent<CapsuleCollider>().bounds;float height=bounds.size.y;
        side.sideCamera.transform.position=bounds.center+player.transform.right*height*3f+Vector3.up*height*.12f;side.sideCamera.transform.LookAt(bounds.center);
        side.sideCamera.orthographic=true;side.sideCamera.orthographicSize=height*.72f;side.sideCamera.nearClipPlane=.003f;
        side.firstPersonCamera.enabled=true;side.sideCamera.enabled=false;side.sideCamera.tag="Untagged";
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        return Newtonsoft.Json.JsonConvert.SerializeObject(new{avatar=animator.avatar.name,animator.isHuman,ratio,driver.walkReferenceSpeed,driver.runReferenceSpeed,sideCamera=side.sideCamera.name,renderers=visibility.bodyRenderers.Length});
    }
}
