#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

public class MansionPhysicsTests
{
    readonly List<GameObject> created=new List<GameObject>();
    HashSet<MansionDebris> before;
    [SetUp] public void RememberDebris(){before=new HashSet<MansionDebris>(Object.FindObjectsByType<MansionDebris>(FindObjectsSortMode.None));}
    [UnityTearDown] public IEnumerator Cleanup()
    {
        foreach(var go in created)if(go!=null)Object.Destroy(go);created.Clear();
        foreach(var debris in Object.FindObjectsByType<MansionDebris>(FindObjectsSortMode.None))if(!before.Contains(debris))Object.Destroy(debris.gameObject);
        yield return null;
    }
    GameObject New(string name,Vector3 p){var go=new GameObject(name);go.transform.position=p;created.Add(go);return go;}
    [UnityTest] public IEnumerator PickupPreservesHitDistanceAndThrowReleasesBody()
    {
        var camera=New("Test hand",new Vector3(1000,10,1000));var grab=camera.AddComponent<ObjectGrabber>();grab.enabled=false;grab.grabbableMask=~0;grab.grabDistance=3;
        var target=New("Test parcel",camera.transform.position+Vector3.forward*1.5f);target.AddComponent<BoxCollider>();var rb=target.AddComponent<Rigidbody>();rb.useGravity=false;
        Physics.SyncTransforms();Assert.That(grab.TryGrab(),Is.True);Assert.That(grab.HeldDistance,Is.EqualTo(1).Within(.02f));Assert.That(grab.Throw(8),Is.True);Assert.That(grab.IsHolding,Is.False);
        yield return new WaitForFixedUpdate();Assert.That(rb.linearVelocity.z,Is.EqualTo(8).Within(.1f));
    }
    [UnityTest] public IEnumerator CupBreaksOnActualCollisionAndProducesSmallTexturedPieces()
    {
        var floor=New("Test stone",new Vector3(1000,0,1000));var box=floor.AddComponent<BoxCollider>();box.size=new Vector3(8,.2f,8);
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Locations/Mansion/Props/MD_60_Teacup.prefab");Assert.That(prefab,Is.Not.Null);
        var cup=Object.Instantiate(prefab,new Vector3(1000,2,1000),Quaternion.identity);created.Add(cup);var damage=cup.GetComponent<DamageOnImpact>();float deadline=Time.time+3;
        while(cup!=null&&Time.time<deadline)yield return new WaitForFixedUpdate();Assert.That(cup==null,Is.True,"The actual impact must destroy the cup");
        var pieces=Object.FindObjectsByType<MansionDebris>(FindObjectsSortMode.None).Where(d=>!before.Contains(d)).SelectMany(d=>d.GetComponentsInChildren<MansionFragment>()).ToArray();Assert.That(pieces.Length,Is.GreaterThan(3));
        foreach(var piece in pieces){Assert.That(piece.GetComponent<MeshFilter>().sharedMesh.bounds.size.magnitude,Is.LessThan(.25f));Assert.That(piece.GetComponent<Renderer>().sharedMaterials.All(m=>m!=null),Is.True);}
    }
    [Test] public void FurnitureFragmentsHaveNoDoorSizedSails()
    {
        var paths=System.IO.Directory.GetFiles("Assets/Game/Locations/Mansion/Fragments","*.prefab");Assert.That(paths.Length,Is.GreaterThan(50));
        foreach(var path in paths)foreach(var f in AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponentsInChildren<MansionFragment>(true))
        {
            var s=f.GetComponent<MeshFilter>().sharedMesh.bounds.size;Assert.That(Mathf.Max(s.x,s.y,s.z),Is.LessThan(.71f),path+" / "+f.name);
        }
    }
}
#endif
