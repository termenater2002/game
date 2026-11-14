using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

public class VoxelArchitectureTests
{
    GameObject root;
    VoxelWall wall;
    [SetUp] public void Create()
    {
        root=new GameObject("Voxel architecture test");root.transform.position=new Vector3(5000,0,5000);wall=root.AddComponent<VoxelWall>();wall.width=6;wall.height=3.2f;wall.openings=new[]{new Rect(-.6f,0,1.2f,2.4f)};
        var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Locations/Mansion/Materials/MD_001_ivory_plaster.mat");wall.plaster=wall.panelling=wall.trim=wall.interior=mat;wall.BuildInitialGeometry();Physics.SyncTransforms();
    }
    [TearDown] public void Cleanup(){if(root!=null)Object.DestroyImmediate(root);}
    [Test] public void IntactSurfaceUsesFewChunksAndBoxesWithoutVoxelRigidbodies()
    {
        Assert.That(wall.ChunkCount,Is.InRange(2,4));Assert.That(root.GetComponentsInChildren<Rigidbody>().Length,Is.Zero);Assert.That(root.GetComponentsInChildren<MeshCollider>().Length,Is.Zero);
        Assert.That(root.GetComponentsInChildren<BoxCollider>().Length,Is.LessThan(15));Assert.That(root.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.vertexCount),Is.LessThan(10000));
    }
    [Test] public void DamagingOneEndLeavesTheOtherChunkGeometryUnchanged()
    {
        var far=root.GetComponentsInChildren<VoxelWallChunk>().Last();var filter=far.GetComponent<MeshFilter>();var before=filter.sharedMesh.vertices;
        var point=root.transform.TransformPoint(new Vector3(-1.8f,1.6f,.14f));Assert.That(wall.DamageAt(point,root.transform.forward,22),Is.True);wall.FlushDamage();
        CollectionAssert.AreEqual(before,filter.sharedMesh.vertices);Assert.That(wall.LastRebuiltChunks,Is.LessThan(wall.ChunkCount));
        Assert.That(root.GetComponentsInChildren<MeshFilter>().SelectMany(f=>f.sharedMesh.vertices).Any(v=>Mathf.Abs(v.z)<.13f&&Mathf.Abs(v.z)>.01f),Is.True);
    }
    [Test] public void RepeatedNearbyDamageKeepsTheDoorApertureClearAndMemoryBounded()
    {
        for(int i=0;i<150;i++)
        {
            var local=new Vector3(.7f+(i%8)*.18f,.4f+(i%11)*.2f,.14f);
            wall.DamageAt(root.transform.TransformPoint(local),root.transform.forward,32);
        }
        wall.FlushDamage();Physics.SyncTransforms();
        foreach(float x in new[]{-.55f,0,.55f})
        {
            var from=root.transform.TransformPoint(new Vector3(x,1.4f,1));var hits=Physics.RaycastAll(from,-root.transform.forward,2);
            Assert.That(hits.Any(h=>h.collider.GetComponentInParent<VoxelWall>()==wall),Is.False,"The opening must not acquire hidden collision geometry.");
        }
        Assert.That(root.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.vertexCount),Is.LessThan(150000));Assert.That(wall.DeepestDamage,Is.LessThanOrEqualTo(.121f));
    }
    [Test] public void ContinuousParquetAssetHasOneUnbrokenTopQuad()
    {
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Game/Locations/Mansion/Architecture/MD_6_ParquetFloor_continuous.asset");Assert.That(mesh,Is.Not.Null);
        Assert.That(mesh.bounds.size.x,Is.EqualTo(24.28f).Within(.001f));Assert.That(mesh.bounds.size.z,Is.EqualTo(24.28f).Within(.001f));
        Assert.That(mesh.normals.Count(n=>n.y>.99f),Is.EqualTo(4));Assert.That(mesh.vertexCount,Is.EqualTo(24));
    }
}
