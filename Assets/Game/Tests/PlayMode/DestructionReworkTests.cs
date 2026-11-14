#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

public class DestructionReworkTests
{
    readonly List<GameObject> created=new List<GameObject>();
    HashSet<MansionDebris> originalDebris;
    static readonly Vector3 Origin=new Vector3(1400,0,1400);
    [SetUp] public void Remember(){originalDebris=new HashSet<MansionDebris>(Object.FindObjectsByType<MansionDebris>(FindObjectsSortMode.None));}
    GameObject New(string name,Vector3 position){var go=new GameObject(name);go.transform.position=position;created.Add(go);return go;}
    GameObject Prop(int id,Vector3 position)
    {
        var path=Directory.GetFiles("Assets/Game/Locations/Mansion/Props","MD_"+id+"_*.prefab").Single();
        var go=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),position,Quaternion.identity);created.Add(go);return go;
    }
    void Floor(){var floor=New("Test floor",Origin+Vector3.down*.1f);floor.AddComponent<BoxCollider>().size=new Vector3(20,.2f,20);}
    VoxelWall Wall()
    {
        var wall=New("Test continuous wall",Origin).AddComponent<VoxelWall>();wall.width=6;wall.height=3.2f;wall.openings=new[]{new Rect(-.55f,0,1.1f,2.36f)};
        var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Locations/Mansion/Materials/MD_001_ivory_plaster.mat");wall.plaster=wall.panelling=wall.trim=wall.interior=material;
        wall.BuildInitialGeometry();Physics.SyncTransforms();return wall;
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        foreach(var go in created)if(go!=null)Object.Destroy(go);created.Clear();yield return null;
        foreach(var debris in Object.FindObjectsByType<MansionDebris>(FindObjectsSortMode.None))if(!originalDebris.Contains(debris))Object.Destroy(debris.gameObject);
        yield return null;
    }
    [UnityTest] public IEnumerator WallRecessesAreLocalAccumulativeAndProtectOpenings()
    {
        var wall=Wall();var point=Origin+new Vector3(1.6f,1.6f,.14f);
        Assert.That(wall.GetComponentsInChildren<Rigidbody>().Length,Is.Zero);
        Assert.That(wall.GetComponentsInChildren<MeshCollider>().Length,Is.Zero,"Intact walls should use simple boxes.");
        Assert.That(wall.DamageAt(point,Vector3.forward,1),Is.False);
        Assert.That(wall.DamageAt(Origin+new Vector3(0,1,.14f),Vector3.forward,22),Is.False);
        Assert.That(wall.DamageAt(point,Vector3.forward,22),Is.True);wall.FlushDamage();float first=wall.RemovedVolume;
        Assert.That(first,Is.GreaterThan(0));Assert.That(wall.LastRebuiltChunks,Is.InRange(1,2));
        wall.DamageAt(point,Vector3.forward,22);wall.FlushDamage();Assert.That(wall.RemovedVolume,Is.GreaterThan(first));
        Assert.That(wall.DamageAt(Origin+new Vector3(-1.6f,1.8f,.14f),Vector3.forward,22),Is.True);wall.FlushDamage();
        Assert.That(wall.DeepestDamage,Is.LessThanOrEqualTo(.121f));Assert.That(wall.DamagedCells,Is.GreaterThan(5));
        Assert.That(wall.GetComponentsInChildren<Rigidbody>().Length,Is.Zero);
        Physics.SyncTransforms();
        var hits=Physics.RaycastAll(point+Vector3.forward,Vector3.back,2).Where(h=>h.collider.GetComponentInParent<VoxelWall>()==wall).ToArray();
        Assert.That(hits.Length,Is.GreaterThan(0));Assert.That(hits.Min(h=>h.distance),Is.GreaterThan(1.02f),"The collider must follow a real recess.");
        int revision=wall.DamageRevision;for(int i=0;i<20;i++)wall.DamageAt(point,Vector3.forward,22);
        wall.FlushDamage();float saturated=wall.RemovedVolume;wall.DamageAt(point,Vector3.forward,22);Assert.That(wall.RemovedVolume,Is.EqualTo(saturated));
        yield return null;
    }
    [UnityTest] public IEnumerator ActualToasterCollisionCarvesTheContactPosition()
    {
        var wall=Wall();var toaster=Prop(58,Origin+new Vector3(1.6f,2.1f,1.7f));var body=toaster.GetComponent<Rigidbody>();body.linearVelocity=Vector3.back*6;
        float until=Time.time+2;while(wall.DamageRevision==0&&Time.time<until)yield return new WaitForFixedUpdate();
        yield return null;Assert.That(wall.DamageRevision,Is.GreaterThan(0));Assert.That(wall.RemovedVolume,Is.GreaterThan(0));
        var contact=wall.transform.InverseTransformPoint(wall.LastDamagePoint);Assert.That(contact.x,Is.EqualTo(1.6f).Within(.35f));Assert.That(contact.y,Is.InRange(1,2.4f));
        Assert.That(wall.DeepestDamage,Is.InRange(.01f,.121f));
    }
    [UnityTest] public IEnumerator TelevisionScreenBreaksWithoutDeletingHousingOrStand()
    {
        var tv=Prop(27,Origin+Vector3.up*2);tv.GetComponent<Rigidbody>().isKinematic=true;
        var root=tv.GetComponent<DamageOnImpact>();var screen=tv.GetComponentsInChildren<IndependentPart>().Single(p=>p.GetComponent<DamageOnImpact>().materialProfile.substance==DestructionSubstance.Glass);
        var stand=tv.GetComponentsInChildren<IndependentPart>().Single(p=>p!=screen);
        screen.GetComponent<DamageOnImpact>().ApplyDamageAt(34,screen.transform.position,Vector3.back);yield return null;
        Assert.That(screen==null,Is.True);Assert.That(tv!=null&&stand!=null,Is.True);Assert.That(root.State.CurrentHealth,Is.EqualTo(root.State.MaxHealth));
        Assert.That(tv.GetComponentsInChildren<MeshFilter>().Any(f=>f.name.Contains("housing")),Is.True);
    }
    [UnityTest] public IEnumerator BooksSurviveTheirShelfAsIndependentFallingBodies()
    {
        Floor();var cabinet=Prop(29,Origin+Vector3.up*2);var books=cabinet.GetComponentsInChildren<IndependentPart>();Assert.That(books.Length,Is.EqualTo(32));
        var initial=books.Select(b=>b.transform.position.y).ToArray();cabinet.GetComponent<DamageOnImpact>().ApplyDamage(10000);yield return null;
        Assert.That(cabinet==null,Is.True);Assert.That(books.All(b=>b!=null&&b.Released&&!b.GetComponent<Rigidbody>().isKinematic),Is.True);
        yield return new WaitForSeconds(.65f);Assert.That(books.Where((b,i)=>b!=null&&b.transform.position.y<initial[i]-.1f).Count(),Is.GreaterThan(20));
        Assert.That(books.All(b=>b.GetComponent<Collider>().enabled),Is.True);
    }
    [UnityTest] public IEnumerator MetalDentsAndStaysPhysicalInsteadOfExplodingIntoCubes()
    {
        var toaster=Prop(58,Origin+Vector3.up*2);toaster.GetComponent<Rigidbody>().isKinematic=true;
        var surface=toaster.GetComponentsInChildren<MeshFilter>().Single(f=>f.name.Contains("rounded_toaster_body"));var original=surface.sharedMesh;var before=original.vertices;
        var bounds=surface.GetComponent<Renderer>().bounds;var point=bounds.center+Vector3.back*bounds.extents.z;
        var damage=toaster.GetComponent<DamageOnImpact>();damage.ApplyDamageAt(34,point,Vector3.back);
        Assert.That(surface.sharedMesh,Is.Not.SameAs(original));Assert.That(surface.sharedMesh.vertices.Where((v,i)=>(v-before[i]).sqrMagnitude>.000001f).Count(),Is.GreaterThan(3));
        damage.ApplyDamageAt(10000,point,Vector3.back);yield return null;
        Assert.That(toaster!=null,Is.True);Assert.That(toaster.GetComponent<MansionMaterialResponse>().Failed,Is.True);Assert.That(damage.brokenPrefab,Is.Null);
    }
    [UnityTest] public IEnumerator UpholsteryCompressesAndLosesARealPatchWhenTorn()
    {
        var sofa=Prop(21,Origin+Vector3.up*2);sofa.GetComponent<Rigidbody>().isKinematic=true;
        var cushion=sofa.GetComponentsInChildren<MeshFilter>().First(f=>f.name.Contains("seat_cushion"));var before=cushion.sharedMesh.vertices;int triangles=cushion.sharedMesh.triangles.Length;
        var b=cushion.GetComponent<Renderer>().bounds;var point=b.center+Vector3.up*b.extents.y;var damage=cushion.GetComponent<DamageOnImpact>();
        damage.ApplyDamageAt(34,point,Vector3.up);Assert.That(cushion.sharedMesh.vertices.Where((v,i)=>(v-before[i]).sqrMagnitude>.000001f).Count(),Is.GreaterThan(3));
        damage.ApplyDamageAt(10000,point,Vector3.up);yield return null;
        Assert.That(sofa!=null,Is.True);Assert.That(cushion.sharedMesh.triangles.Length,Is.LessThan(triangles));Assert.That(damage.brokenPrefab,Is.Null);
    }
    [UnityTest] public IEnumerator RugEdgeLiftsAndBendsWhileTheOtherEndRemainsLower()
    {
        Floor();var rug=Prop(26,Origin);var cloth=rug.GetComponent<FlexibleFabric>();cloth.sleepDistance=10000;cloth.Wake();
        yield return new WaitForSeconds(.3f);
        var hand=New("Rug test hand",Origin+new Vector3(0,1.1f,-1.45f));hand.transform.LookAt(Origin+new Vector3(0,.025f,-.94f));var grab=hand.AddComponent<ObjectGrabber>();grab.enabled=false;grab.grabbableMask=~0;grab.grabForce=650;
        Physics.SyncTransforms();Assert.That(grab.TryGrab(),Is.True);hand.transform.position+=Vector3.up*.65f;
        for(int i=0;i<50;i++){grab.SendMessage("FixedUpdate");yield return new WaitForFixedUpdate();}
        var heights=cloth.segments.Select(b=>b.worldCenterOfMass.y).ToArray();var angles=cloth.segments.Select(b=>Quaternion.Angle(b.rotation,Quaternion.identity)).ToArray();
        Assert.That(heights.Max()-heights.Min(),Is.GreaterThan(.12f));Assert.That(angles.Max()-angles.Min(),Is.GreaterThan(8));
        grab.Release();yield return new WaitForSeconds(1.5f);Assert.That(cloth.segments.Max(b=>b.linearVelocity.magnitude),Is.LessThan(3));
    }
    [UnityTest] public IEnumerator ScaledRugKeepsPhysicalSeamsTogetherDuringBending()
    {
        Floor();var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Directory.GetFiles("Assets/Game/Locations/Mansion/Props","MD_26_*.prefab").Single());
        // Instantiate inactive so the final scene scale is in place before strip detachment.
        var staging=New("Inactive scaled rug staging",Origin);staging.SetActive(false);
        var rug=Object.Instantiate(prefab,staging.transform,false);rug.transform.localScale=new Vector3(.78f,1,2.7f);staging.SetActive(true);
        var cloth=rug.GetComponent<FlexibleFabric>();cloth.sleepDistance=10000;cloth.Wake();yield return new WaitForSeconds(.3f);
        var hand=New("Scaled rug hand",Origin+new Vector3(0,1.3f,-3.25f));hand.transform.LookAt(Origin+new Vector3(0,.025f,-2.55f));var grab=hand.AddComponent<ObjectGrabber>();grab.enabled=false;grab.grabbableMask=~0;grab.grabForce=650;
        Physics.SyncTransforms();Assert.That(grab.TryGrab(),Is.True);hand.transform.position+=Vector3.up*.65f;
        for(int i=0;i<55;i++){grab.SendMessage("FixedUpdate");yield return new WaitForFixedUpdate();}
        Assert.That(cloth.segments.Max(b=>b.worldCenterOfMass.y)-cloth.segments.Min(b=>b.worldCenterOfMass.y),Is.GreaterThan(.12f));
        foreach(var b in cloth.segments.Skip(1))
        {
            var j=b.GetComponent<HingeJoint>();float gap=Vector3.Distance(j.transform.TransformPoint(j.anchor),j.connectedBody.transform.TransformPoint(j.connectedAnchor));
            Assert.That(gap,Is.LessThan(.035f),"Hinge endpoints must remain joined under the room's nonuniform rug scale");
            Assert.That(b.transform.IsChildOf(rug.transform),Is.False,"A strip must not inherit a rotating parent's nonuniform scale");
        }
        grab.Release();
    }
    [UnityTest] public IEnumerator RugDrapesOverARaisedSupport()
    {
        Floor();var support=New("Raised cloth support",Origin+Vector3.up*.25f);support.AddComponent<BoxCollider>().size=new Vector3(4,.5f,.65f);
        var rug=Prop(26,Origin+Vector3.up*1.2f);var cloth=rug.GetComponent<FlexibleFabric>();cloth.sleepDistance=10000;cloth.Wake();
        yield return new WaitForSeconds(2.5f);var heights=cloth.segments.Select(b=>b.worldCenterOfMass.y).ToArray();
        Assert.That(heights[3],Is.GreaterThan(.4f));Assert.That(heights.Max()-heights.Min(),Is.GreaterThan(.2f));
        Assert.That(Mathf.Min(heights[0],heights[7]),Is.LessThan(heights[3]-.15f));
    }
    [UnityTest] public IEnumerator DistantSettledRugSleepsAndWakesWhenActuallyGrabbed()
    {
        Floor();var observer=New("Distant fabric observer",Origin+new Vector3(100,2,100));observer.AddComponent<Rigidbody>().isKinematic=true;var motor=observer.AddComponent<FunnyMovement>();motor.enabled=false;
        var rug=Prop(26,Origin);var cloth=rug.GetComponent<FlexibleFabric>();cloth.sleepDistance=1;
        yield return new WaitForSeconds(1.5f);Assert.That(cloth.IsFrozen,Is.True);Assert.That(cloth.segments.All(b=>b.isKinematic),Is.True);
        var hand=New("Sleeping cloth hand",Origin+new Vector3(0,1.1f,-1.45f));hand.transform.LookAt(Origin+new Vector3(0,.025f,-.94f));var grab=hand.AddComponent<ObjectGrabber>();grab.enabled=false;grab.grabbableMask=~0;
        Physics.SyncTransforms();Assert.That(grab.TryGetTarget(out var target,out var point),Is.True);Assert.That(cloth.IsFrozen,Is.True,"A HUD query must not wake the simulation.");
        Assert.That(grab.TryGrab(),Is.True);Assert.That(cloth.IsFrozen,Is.False);Assert.That(cloth.segments.All(b=>!b.isKinematic),Is.True);
    }
    [UnityTest] public IEnumerator EqualThrowImpulseHasAMuchSmallerEffectOnHeavyObjects()
    {
        var hand=New("Mass throw test",Origin+Vector3.up*3);var grab=hand.AddComponent<ObjectGrabber>();grab.enabled=false;grab.grabbableMask=~0;
        float[] speeds=new float[2];int i=0;
        foreach(float mass in new[]{2f,40f})
        {
            var go=New("Throw parcel",hand.transform.position+Vector3.forward*1.5f);go.AddComponent<BoxCollider>();var body=go.AddComponent<Rigidbody>();body.mass=mass;body.useGravity=false;
            Physics.SyncTransforms();Assert.That(grab.TryGrab(),Is.True);Assert.That(grab.Throw(18),Is.True);yield return new WaitForFixedUpdate();speeds[i++]=body.linearVelocity.z;go.SetActive(false);
        }
        Assert.That(speeds[0],Is.GreaterThan(speeds[1]*10));Assert.That(speeds[1],Is.EqualTo(18f/40).Within(.02f));
    }
    [UnityTest] public IEnumerator FurnitureAboveLiftCapacityStaysOnTheFloor()
    {
        Floor();var hand=New("Heavy lifting test",Origin+new Vector3(0,.5f,-2));var grab=hand.AddComponent<ObjectGrabber>();grab.enabled=false;grab.grabbableMask=~0;grab.grabForce=650;
        var box=New("80 kg cabinet proxy",Origin+Vector3.up*.5f);box.AddComponent<BoxCollider>();var body=box.AddComponent<Rigidbody>();body.mass=80;
        Physics.SyncTransforms();Assert.That(grab.TryGrab(),Is.True);hand.transform.position+=Vector3.up;
        for(int i=0;i<40;i++){grab.SendMessage("FixedUpdate");yield return new WaitForFixedUpdate();}
        Assert.That(box.GetComponent<Collider>().bounds.min.y,Is.LessThan(.08f));Assert.That(body.useGravity,Is.True);
        grab.Release();body.mass=200;body.position=Origin+Vector3.up*.5f;hand.transform.position=Origin+new Vector3(0,.5f,-2);Physics.SyncTransforms();Assert.That(grab.TryGrab(),Is.False);
    }
    [UnityTest] public IEnumerator GentleSettlementDoesNotDamageTheCookingPot()
    {
        Floor();var pot=Prop(63,Origin+Vector3.up*.04f);var damage=pot.GetComponent<DamageOnImpact>();
        yield return new WaitForSeconds(.8f);Assert.That(damage.State.CurrentHealth,Is.EqualTo(damage.State.MaxHealth));
    }
    [UnityTest] public IEnumerator RealHeavyFurnitureCanBeDraggedWithoutBeingLifted()
    {
        Floor();var stand=Prop(28,Origin);var body=stand.GetComponent<Rigidbody>();Assert.That(body.mass,Is.GreaterThan(20));
        Physics.SyncTransforms();float bottom=stand.GetComponentsInChildren<Collider>().Min(c=>c.bounds.min.y);body.position+=Vector3.up*(.01f-bottom);Physics.SyncTransforms();
        yield return new WaitForSeconds(.3f);var target=stand.GetComponentsInChildren<Collider>().Where(c=>c.attachedRigidbody==body).OrderByDescending(c=>c.bounds.size.sqrMagnitude).First().bounds.center;
        var hand=New("Furniture drag hand",target-Vector3.forward*1.8f);hand.transform.LookAt(target);var grab=hand.AddComponent<ObjectGrabber>();grab.enabled=false;grab.grabbableMask=~0;grab.grabForce=650;
        Physics.SyncTransforms();Assert.That(grab.TryGrab(),Is.True);Assert.That(grab.HeldBody,Is.SameAs(body));
        hand.transform.position+=new Vector3(1,1,0);
        for(int i=0;i<55;i++){grab.SendMessage("FixedUpdate");yield return new WaitForFixedUpdate();}
        Assert.That(body.position.x-Origin.x,Is.GreaterThan(.15f),$"body={body.position} velocity={body.linearVelocity} held={grab.IsHolding} kinematic={body.isKinematic} hand={hand.transform.position} target={target} bottom={bottom} constraints={body.constraints}");Assert.That(stand.GetComponentsInChildren<Collider>().Min(c=>c.bounds.min.y),Is.LessThan(.12f));
    }
    [UnityTest] public IEnumerator DebrisBudgetAndExpiredGroupBookkeepingStayBounded()
    {
        for(int i=0;i<30;i++)
        {
            var group=New("Budget test debris",Origin+Vector3.up*10);group.SetActive(false);
            for(int b=0;b<10;b++){var go=new GameObject("Budget body");go.transform.SetParent(group.transform,false);go.AddComponent<Rigidbody>().isKinematic=true;}
            group.AddComponent<MansionDebris>();group.SetActive(true);
        }
        yield return null;Assert.That(MansionDebris.LiveBodyCount,Is.LessThanOrEqualTo(MansionDebris.BodyBudget));Assert.That(MansionDebris.LiveGroupCount,Is.LessThanOrEqualTo(192));
        foreach(var go in created)if(go!=null)Object.Destroy(go);yield return null;Assert.That(MansionDebris.LiveGroupCount,Is.EqualTo(originalDebris.Count));
    }
}
#endif
