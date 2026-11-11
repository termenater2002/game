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
    [Test] public void RiderHasAValidWeightedHumanoidAndLongLeggedSlimProportions()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Characters/Rider/Rider.prefab");
        Assert.That(prefab,Is.Not.Null);var animator=prefab.GetComponent<Animator>();
        Assert.That(animator.avatar.isValid&&animator.avatar.isHuman,Is.True);Assert.That(animator.applyRootMotion,Is.False);
        var skin=prefab.GetComponentInChildren<SkinnedMeshRenderer>();var mesh=skin.sharedMesh;
        Assert.That(mesh.triangles.Length/3,Is.GreaterThan(2000));Assert.That(skin.bones.Length,Is.GreaterThanOrEqualTo(22));
        Assert.That(mesh.boneWeights.All(w=>Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1)<.0001f),Is.True);
        var bones=prefab.GetComponentsInChildren<Transform>();float height=mesh.bounds.size.y;
        float hipHeight=bones.Single(t=>t.name=="MD_Hips").position.y;
        float shoulderWidth=Vector3.Distance(bones.Single(t=>t.name=="MD_LeftUpperArm").position,bones.Single(t=>t.name=="MD_RightUpperArm").position);
        Assert.That(hipHeight/height,Is.InRange(.49f,.55f));Assert.That(shoulderWidth/height,Is.InRange(.17f,.22f));
        Assert.That(skin.sharedMaterials.All(m=>m!=null&&m.shader.name=="Universal Render Pipeline/Lit"),Is.True);
    }
}
