using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class PlayerAnimationAssetTests
{
    const string Root = "Assets/Game/Animations/";
    [Test] public void LocomotionHasEightValidMotionsAndAirStates()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Root + "PlayerMoveAnimation.controller");
        Assert.That(controller, Is.Not.Null);
        var sm = controller.layers[0].stateMachine;
        Assert.That(controller.layers[0].iKPass, Is.True);
        CollectionAssert.AreEquivalent(new[] { "Locomotion", "Jump", "Fall", "Landing" }, sm.states.Select(s => s.state.name));
        var tree = sm.states.First(s => s.state.name == "Locomotion").state.motion as BlendTree;
        Assert.That(tree, Is.Not.Null); Assert.That(tree.blendType, Is.EqualTo(BlendTreeType.FreeformCartesian2D));
        Assert.That(tree.children.Length, Is.EqualTo(8));
        foreach (var child in tree.children) { Assert.That(child.motion, Is.Not.Null); Assert.That(((AnimationClip)child.motion).isHumanMotion, Is.True); Assert.That(((AnimationClip)child.motion).isLooping, Is.True); }
        Assert.That(sm.anyStateTransitions.All(t => t.conditions.Any(c => c.parameter == "Grounded" && c.mode == AnimatorConditionMode.IfNot)), Is.True);
    }
    [Test] public void PackKeepsOriginalAvatarAndExtractsLocomotionRootTranslation()
    {
        foreach (var path in AssetDatabase.FindAssets("t:Model", new[] { Root + "Pack" }).Select(AssetDatabase.GUIDToAssetPath))
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            Assert.That(importer.animationType, Is.EqualTo(ModelImporterAnimationType.Human), path);
            Assert.That(importer.sourceAvatar.isValid && importer.sourceAvatar.isHuman, Is.True, path);
            Assert.That(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(importer.sourceAvatar)), Is.EqualTo("2d226e59912d0014fbf2646ed5c4b916"), path);
            var clip = importer.clipAnimations.Single();
            Assert.That(clip.lockRootRotation && clip.lockRootHeightY, Is.True, path);
            Assert.That(clip.lockRootPositionXZ, Is.EqualTo(path.EndsWith("idle.fbx")), path);
            Assert.That(clip.keepOriginalPositionXZ, Is.False, path);
            Assert.That(clip.loopTime, Is.EqualTo(!path.EndsWith("jump.fbx")), path);
        }
    }
    [Test] public void AuthoredMaterialsAreMatteAndDoNotPointToErrorShader()
    {
        var materials = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Game/Art/LowPoly" });
        Assert.That(materials.Length, Is.GreaterThanOrEqualTo(9));
        foreach (var guid in materials)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            Assert.That(m.shader.name, Is.EqualTo("Universal Render Pipeline/Lit"), m.name);
            Assert.That(m.GetFloat("_Metallic"), Is.Zero, m.name); Assert.That(m.GetFloat("_Smoothness"), Is.LessThanOrEqualTo(.1f), m.name);
        }
    }
    [TestCase("RiderV2", "RiderV2")]
    [TestCase("RiderV2TShirt", "RiderV2TShirt")]
    [TestCase("StyleV3/RiderV3", "RiderV3")]
    [TestCase("StyleV3/RiderTShirtV3", "RiderTShirtV3")]
    public void RiderHasAValidWeightedHumanoidAndLongLeggedSlimProportions(string folder, string outfit)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Characters/"+folder+"/"+outfit+".prefab");
        Assert.That(prefab,Is.Not.Null);var animator=prefab.GetComponent<Animator>();
        Assert.That(animator.avatar.isValid&&animator.avatar.isHuman,Is.True);Assert.That(animator.applyRootMotion,Is.False);
        var skin=prefab.GetComponentInChildren<SkinnedMeshRenderer>();var mesh=skin.sharedMesh;
        Assert.That(mesh.triangles.Length/3,Is.InRange(2000,10000));Assert.That(skin.bones.Length,Is.GreaterThanOrEqualTo(22));
        Assert.That(mesh.boneWeights.All(w=>Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1)<.0001f),Is.True);
        var points=mesh.vertices.Select(v=>prefab.transform.InverseTransformPoint(skin.transform.TransformPoint(v))).ToArray();
        var bones=prefab.GetComponentsInChildren<Transform>();float height=points.Max(p=>p.y)-points.Min(p=>p.y);
        Assert.That(animator.humanScale/height,Is.InRange(.45f,.65f),"FBX unit normalization must not inflate Humanoid retargeting scale.");
        float hipHeight=bones.Single(t=>t.name=="MD_Hips").position.y;
        float shoulderWidth=Vector3.Distance(bones.Single(t=>t.name=="MD_LeftUpperArm").position,bones.Single(t=>t.name=="MD_RightUpperArm").position);
        Assert.That(hipHeight/height,Is.InRange(.49f,.55f));Assert.That(shoulderWidth/height,Is.InRange(.17f,.22f));
        Assert.That(skin.sharedMaterials.All(m=>m!=null&&m.shader.name=="Universal Render Pipeline/Lit"),Is.True);
        Assert.That(bones.Single(t=>t.name=="MD_LeftUpperArm").position.x,Is.LessThan(0),"Humanoid left must match the forward-facing mesh.");
        Assert.That(bones.Single(t=>t.name=="MD_LeftToes").position.z,Is.GreaterThan(bones.Single(t=>t.name=="MD_LeftFoot").position.z));
        // A wide hem that crosses the centre line must not attach to the opposite leg.
        var triangles=mesh.triangles;var weights=mesh.boneWeights;
        for(int i=0;i<triangles.Length;i+=3)
        {
            var indices=triangles.Skip(i).Take(3).ToArray();
            if(!indices.All(k=>points[k].y<height*.2f))continue;
            var names=indices.Select(k=>skin.bones[weights[k].boneIndex0].name).ToArray();
            Assert.That(names.Any(n=>n.Contains("Left"))&&names.Any(n=>n.Contains("Right")),Is.False,"A lower trouser / shoe triangle spans both leg weights.");
        }
    }
    [Test] public void LilacSkinAndSleevesShareWeightsAtTheirTextureSeams()
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Characters/StyleV3/LilacV3/LilacV3.prefab");
        var skin=prefab.GetComponentsInChildren<SkinnedMeshRenderer>().Single(s=>s.name=="LilacV3_Body");
        var mesh=skin.sharedMesh;var points=mesh.vertices;var uv=mesh.uv;var weights=mesh.boneWeights;int shared=0;
        float Influence(BoneWeight w,int bone) => (w.boneIndex0==bone?w.weight0:0)+(w.boneIndex1==bone?w.weight1:0)
            +(w.boneIndex2==bone?w.weight2:0)+(w.boneIndex3==bone?w.weight3:0);
        // FBX duplicates vertices at UV boundaries. Their positions and deformation must agree.
        var seams=Enumerable.Range(0,points.Length).GroupBy(i=>(Mathf.RoundToInt(points[i].x*100000),Mathf.RoundToInt(points[i].y*100000),Mathf.RoundToInt(points[i].z*100000)));
        foreach(var seam in seams)
        {
            var bare=seam.Where(i=>Vector2.Distance(uv[i],new Vector2(.53f,.47f))<.0001f).ToArray();
            var cloth=seam.Where(i=>uv[i].x>.499f&&(uv[i].y>.499f||uv[i].y<.251f)).ToArray();
            if(bare.Length==0||cloth.Length==0)continue;
            shared++;
            foreach(var b in bare)foreach(var c in cloth)
                for(int bone=0;bone<skin.bones.Length;bone++)
                    Assert.That(Influence(weights[b],bone),Is.EqualTo(Influence(weights[c],bone)).Within(.00001f),"Skin / fabric seam deforms independently: "+skin.bones[bone].name);
        }
        Assert.That(shared,Is.GreaterThan(24),"The shoulder and sleeve need actual shared UV boundaries.");
    }

}
