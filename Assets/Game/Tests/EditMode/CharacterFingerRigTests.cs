using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class CharacterFingerRigTests
{
    [TestCase("LilacV3")]
    [TestCase("RiderV3")]
    [TestCase("RiderTShirtV3")]
    [TestCase("FamilyFatherV3")]
    [TestCase("FamilyMotherV3")]
    public void EveryFingerJointDeformsItsOwnMeshAndKeepsTheWristStill(string name)
    {
        string path="Assets/Game/Characters/StyleV3/"+name+"/"+name+".prefab";
        var actor=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        var before=new Mesh();var after=new Mesh();
        try
        {
            var animator=actor.GetComponent<Animator>();animator.enabled=false;
            Assert.That(animator.avatar.isValid&&animator.isHuman,Is.True);
            Assert.That(actor.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("MD_")),Is.EqualTo(52));
            var skin=actor.GetComponentsInChildren<SkinnedMeshRenderer>().Single(s=>s.name==name+"_Body");
            foreach(string side in new[]{"Left","Right"})
            {
                var wrist=animator.GetBoneTransform((HumanBodyBones)Enum.Parse(typeof(HumanBodyBones),side+"Hand"));
                foreach(string finger in new[]{"Thumb","Index","Middle","Ring","Little"})
                foreach(string joint in new[]{"Proximal","Intermediate","Distal"})
                {
                    string humanName=side+finger+joint;
                    var bone=animator.GetBoneTransform((HumanBodyBones)Enum.Parse(typeof(HumanBodyBones),humanName));
                    Assert.That(bone,Is.Not.Null,humanName);StringAssert.EndsWith(humanName,bone.name);
                    var saved=bone.localRotation;var wristPosition=wrist.position;
                    try
                    {
                        skin.BakeMesh(before);var a=before.vertices;
                        bone.localRotation=saved*Quaternion.AngleAxis(25,Vector3.forward);
                        skin.BakeMesh(after);var b=after.vertices;
                        float maximum=0;int moved=0;
                        for(int i=0;i<a.Length;i++){float distance=Vector3.Distance(a[i],b[i]);maximum=Mathf.Max(maximum,distance);if(distance>.0001f)moved++;}
                        Assert.That(maximum,Is.GreaterThan(.0005f),humanName+": no finger deformation.");
                        Assert.That(moved,Is.LessThan(a.Length*.15f),humanName+": moved unrelated body geometry.");
                        Assert.That(Vector3.Distance(wrist.position,wristPosition),Is.LessThan(.000001f),humanName+": wrist moved with the finger.");
                    }
                    finally{bone.localRotation=saved;}
                }
            }
        }
        finally{UnityEngine.Object.DestroyImmediate(actor);UnityEngine.Object.DestroyImmediate(before);UnityEngine.Object.DestroyImmediate(after);}
    }
}
