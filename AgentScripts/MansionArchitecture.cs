using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>Continuous wall runs with exact opening edges and world-aligned UVs. Reusable for other floor plans.</summary>
public static class MansionArchitecture
{
    const string Folder="Assets/Game/Locations/Mansion/Architecture";
    public static Mesh SaveMesh(Mesh mesh,string name)
    {
        Directory.CreateDirectory(Folder);var path=Folder+"/"+name+".asset";mesh.name=name;
        var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(saved==null){AssetDatabase.CreateAsset(mesh,path);return mesh;}
        EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);return saved;
    }
    public static Mesh BoxMesh(Vector3 size,float tile=1)
    {
        var v=new List<Vector3>();var n=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();
        void Face(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector3 normal)
        {
            if(Vector3.Dot(Vector3.Cross(b-a,c-a),normal)<0){var temp=b;b=d;d=temp;}
            int start=v.Count;foreach(var p in new[]{a,b,c,d}){v.Add(p);n.Add(normal);uv.Add(Mathf.Abs(normal.y)>.5f?new Vector2(p.x,p.z)/tile:Mathf.Abs(normal.z)>.5f?new Vector2(p.x,p.y)/tile:new Vector2(p.z,p.y)/tile);}
            t.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});
        }
        var q=size*.5f;
        Face(new Vector3(-q.x,-q.y,q.z),new Vector3(q.x,-q.y,q.z),new Vector3(q.x,q.y,q.z),new Vector3(-q.x,q.y,q.z),Vector3.forward);
        Face(new Vector3(-q.x,-q.y,-q.z),new Vector3(q.x,-q.y,-q.z),new Vector3(q.x,q.y,-q.z),new Vector3(-q.x,q.y,-q.z),Vector3.back);
        Face(new Vector3(-q.x,-q.y,-q.z),new Vector3(-q.x,-q.y,q.z),new Vector3(-q.x,q.y,q.z),new Vector3(-q.x,q.y,-q.z),Vector3.left);
        Face(new Vector3(q.x,-q.y,-q.z),new Vector3(q.x,-q.y,q.z),new Vector3(q.x,q.y,q.z),new Vector3(q.x,q.y,-q.z),Vector3.right);
        Face(new Vector3(-q.x,q.y,-q.z),new Vector3(q.x,q.y,-q.z),new Vector3(q.x,q.y,q.z),new Vector3(-q.x,q.y,q.z),Vector3.up);
        Face(new Vector3(-q.x,-q.y,-q.z),new Vector3(q.x,-q.y,-q.z),new Vector3(q.x,-q.y,q.z),new Vector3(-q.x,-q.y,q.z),Vector3.down);
        var m=new Mesh();m.SetVertices(v);m.SetNormals(n);m.SetUVs(0,uv);m.SetTriangles(t,0);m.RecalculateBounds();return m;
    }
    public static void Build(Transform architecture,Transform ceilingRoot,Dictionary<string,Material> mats,Action<int,string,Vector3,float,bool> place)
    {
        Directory.CreateDirectory(Folder);AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        void Slab(string name,Transform parent,float y,Material material)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=Vector3.up*y;
            go.AddComponent<MeshFilter>().sharedMesh=SaveMesh(BoxMesh(new Vector3(24.28f,.18f,24.28f),3),name);
            go.AddComponent<MeshRenderer>().sharedMaterial=material;go.AddComponent<BoxCollider>().size=new Vector3(24.28f,.18f,24.28f);
        }
        Slab("MD_6_ParquetFloor_continuous",architecture,-.09f,mats["MD_008_herringbone_parquet"]);
        Slab("Continuous ceiling",ceilingRoot,3.29f,mats["MD_001_ivory_plaster"]);
        var cutPath="Assets/Game/Locations/Mansion/Materials/Cut_plaster.mat";var cut=AssetDatabase.LoadAssetAtPath<Material>(cutPath);
        if(cut==null){cut=new Material(Shader.Find("Universal Render Pipeline/Lit"));cut.SetColor("_BaseColor",new Color(.43f,.32f,.22f));cut.SetFloat("_Smoothness",.02f);AssetDatabase.CreateAsset(cut,cutPath);}
        int sequence=0;
        VoxelWall Wall(string name,Vector3 a,Vector3 b,Rect[] openings,bool capEnds=true,Rect[] protectedAreas=null)
        {
            var go=new GameObject("Continuous wall • "+name);go.transform.SetParent(architecture,false);go.transform.position=(a+b)*.5f;var tangent=(b-a).normalized;go.transform.rotation=Quaternion.LookRotation(Vector3.Cross(tangent,Vector3.up),Vector3.up);
            var wall=go.AddComponent<VoxelWall>();wall.width=Vector3.Distance(a,b);wall.height=3.2f;wall.thickness=.28f;wall.voxelSize=.06f;wall.capEnds=capEnds;wall.openings=openings;wall.protectedAreas=protectedAreas??new Rect[0];wall.plaster=mats["MD_001_ivory_plaster"];wall.panelling=mats["MD_019_wooden_panels"];wall.trim=mats["MD_027_gilded_metal"];wall.interior=cut;
            wall.BuildInitialGeometry();int i=0;foreach(var chunk in go.GetComponentsInChildren<VoxelWallChunk>())
            {var filter=chunk.GetComponent<MeshFilter>();filter.sharedMesh=SaveMesh(filter.sharedMesh,"Wall_"+sequence+"_"+(i++));chunk.CompleteEditorBake();}sequence++;return wall;
        }
        Rect Window(float center)=>new Rect(center-.8f,1.15f,1.6f,1.30f);
        foreach(float x in new[]{-12f,12f})
        {
            var holes=new[]{-10.5f,-4.5f,1.5f,7.5f}.Select(z=>Window(z)).ToArray();
            Wall(x<0?"West exterior":"East exterior",new Vector3(x,0,-11.86f),new Vector3(x,0,11.86f),holes,false,new[]{new Rect(-4.2f,0,.4f,3.2f),new Rect(3.8f,0,.4f,3.2f)});
            foreach(float z in new[]{-10.5f,-4.5f,1.5f,7.5f})place(11,"Window",new Vector3(x,1.08f,z),x<0?90:-90,true);
        }
        foreach(float z in new[]{-12f,12f})
        {
            var holes=new List<Rect>{new Rect(-.95f,0,1.90f,2.64f)};holes.AddRange(new[]{-10.5f,-4.5f,7.5f}.Select(Window));
            Wall(z<0?"Front exterior":"Rear exterior",new Vector3(-12.14f,0,z),new Vector3(12.14f,0,z),holes.ToArray(),true,new[]{new Rect(-2.2f,0,.4f,3.2f),new Rect(1.8f,0,.4f,3.2f)});
            foreach(float x in new[]{-10.5f,-4.5f,7.5f})place(11,"Window",new Vector3(x,1.08f,z),z<0?0:180,true);
            place(9,"Entrance",new Vector3(0,0,z),z<0?0:180,true);
        }
        foreach(float x in new[]{-2f,2f})
        {
            Wall(x<0?"West hall":"East hall",new Vector3(x,0,-11.86f),new Vector3(x,0,11.86f),new[]{-8f,0,8}.Select(z=>new Rect(z-.55f,0,1.10f,2.36f)).ToArray(),false,new[]{new Rect(-4.2f,0,.4f,3.2f),new Rect(3.8f,0,.4f,3.2f)});
            foreach(float z in new[]{-8f,0,8})place(8,"Hall door",new Vector3(x,0,z),x<0?90:-90,true);
        }
        foreach(float z in new[]{-4f,4})
        {
            Wall("West partition "+z,new Vector3(-11.86f,0,z),new Vector3(-2.14f,0,z),new Rect[0],false);
            Wall("East partition "+z,new Vector3(2.14f,0,z),new Vector3(11.86f,0,z),new Rect[0],false);
        }
        var fx=new GameObject("Shared plaster impact pool").AddComponent<MansionImpactFx>();fx.transform.SetParent(architecture,false);fx.dustMaterial=cut;fx.chipMesh=SaveMesh(BoxMesh(Vector3.one),"PlasterChip");
        var audio=new GameObject("Pooled plaster audio").AddComponent<AudioSource>();audio.transform.SetParent(fx.transform,false);audio.playOnAwake=false;audio.spatialBlend=1;audio.maxDistance=18;audio.clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Game/Locations/Mansion/Art/Audio/ceramic_hit.wav");fx.impactAudio=audio;
    }
}
