using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>Offline planar clipping with interpolated UVs and closed cut surfaces. No runtime mesh cutting.</summary>
public static class MansionFracture
{
    public struct Vertex
    {
        public Vector3 p, n; public Vector2 uv;
        public static Vertex Mix(Vertex a, Vertex b, float t) => new Vertex {p=Vector3.LerpUnclamped(a.p,b.p,t),n=Vector3.LerpUnclamped(a.n,b.n,t).normalized,uv=Vector2.LerpUnclamped(a.uv,b.uv,t)};
    }
    public struct Triangle { public Vertex a,b,c; public int m,surface; }
    public sealed class Group
    {
        public string sourceName=""; public Matrix4x4 inverse=Matrix4x4.identity;
        public List<Triangle> faces=new List<Triangle>();
    }
    public static List<Triangle> Clip(List<Triangle> source, Vector3 normal, float offset, bool keepPositive, int interior)
    {
        float sign=keepPositive?1:-1;var result=new List<Triangle>();var cuts=new Dictionary<int,List<Vector3>>();
        foreach(var t in source)
        {
            var input=new[]{t.a,t.b,t.c};var polygon=new List<Vertex>();
            for(int j=0;j<3;j++)
            {
                var a=input[j];var b=input[(j+1)%3];float da=(Vector3.Dot(normal,a.p)-offset)*sign, db=(Vector3.Dot(normal,b.p)-offset)*sign;bool ia=da>=-.000001f,ib=db>=-.000001f;
                if(ia)polygon.Add(a);
                if(ia!=ib){var v=Vertex.Mix(a,b,da/(da-db));polygon.Add(v);if(!cuts.ContainsKey(t.surface))cuts[t.surface]=new List<Vector3>();cuts[t.surface].Add(v.p);}
            }
            for(int j=1;j+1<polygon.Count;j++)
                if(Vector3.Cross(polygon[j].p-polygon[0].p,polygon[j+1].p-polygon[0].p).sqrMagnitude>1e-12f)result.Add(new Triangle{a=polygon[0],b=polygon[j],c=polygon[j+1],m=t.m,surface=t.surface});
        }
        foreach(var boundary in cuts)
        {
        var unique=new List<Vector3>();foreach(var v in boundary.Value)if(!unique.Any(p=>(p-v).sqrMagnitude<1e-9f))unique.Add(v);
        if(unique.Count>=3)
        {
            Vector3 center=unique.Aggregate(Vector3.zero,(a,b)=>a+b)/unique.Count;Vector3 outward=-normal*sign;Vector3 u=Vector3.Cross(outward,Mathf.Abs(outward.y)<.9f?Vector3.up:Vector3.right).normalized,v=Vector3.Cross(outward,u);
            unique.Sort((a,b)=>Mathf.Atan2(Vector3.Dot(a-center,v),Vector3.Dot(a-center,u)).CompareTo(Mathf.Atan2(Vector3.Dot(b-center,v),Vector3.Dot(b-center,u))));
            Vertex V(Vector3 p)=>new Vertex{p=p,n=outward,uv=new Vector2(Vector3.Dot(p,u),Vector3.Dot(p,v))};
            for(int i=0;i<unique.Count;i++){var a=V(center);var b=V(unique[i]);var c=V(unique[(i+1)%unique.Count]);if(Vector3.Cross(b.p-a.p,c.p-a.p).sqrMagnitude>1e-12f)result.Add(new Triangle{a=a,b=b,c=c,m=interior,surface=boundary.Key});}
        }
        }
        return result;
    }
    public static Mesh ToMesh(List<Triangle> faces,int materialCount,out Vector3 center)
    {
        var bounds=new Bounds(faces[0].a.p,Vector3.zero);foreach(var t in faces){bounds.Encapsulate(t.a.p);bounds.Encapsulate(t.b.p);bounds.Encapsulate(t.c.p);}center=bounds.center;
        var points=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var indices=Enumerable.Range(0,materialCount).Select(_=>new List<int>()).ToArray();
        foreach(var t in faces){foreach(var v in new[]{t.a,t.b,t.c}){indices[t.m].Add(points.Count);points.Add(v.p-center);normals.Add(v.n);uv.Add(v.uv);}}
        var mesh=new Mesh{name="Textured cut fragment"};mesh.SetVertices(points);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.subMeshCount=materialCount;for(int i=0;i<materialCount;i++)mesh.SetTriangles(indices[i],i);mesh.RecalculateBounds();return mesh;
    }
    public static GameObject Build(GameObject source,string path,float mass,Material interior,string[] movingParts,int assetId,string substance="wood")
    {
        var filters=source.GetComponentsInChildren<MeshFilter>().Where(f=>{var part=f.GetComponentInParent<IndependentPart>();return part==null||part.gameObject==source;}).ToArray();var materials=filters.SelectMany(f=>f.GetComponent<MeshRenderer>().sharedMaterials).Distinct().ToList();materials.Add(interior);int cutMaterial=materials.Count-1;
        var groups=new Dictionary<string,Group>();groups[""]=new Group();
        foreach(var f in filters)
        {
            var transform=f.transform;Transform moving=null;
            for(var parent=transform;parent!=null&&parent!=source.transform;parent=parent.parent)if(movingParts.Contains(parent.name)){moving=parent;break;}
            string key=moving!=null?moving.name:f.name;if(!groups.ContainsKey(key))groups[key]=new Group{sourceName=moving!=null?moving.name:"",inverse=moving!=null?(source.transform.worldToLocalMatrix*moving.localToWorldMatrix).inverse:Matrix4x4.identity};
            var g=groups[key];var mesh=f.sharedMesh;var coords=mesh.vertices;var normals=mesh.normals;var uv=mesh.uv;var matrix=source.transform.worldToLocalMatrix*f.transform.localToWorldMatrix;var mats=f.GetComponent<MeshRenderer>().sharedMaterials;
            Vertex At(int i)=>new Vertex{p=matrix.MultiplyPoint3x4(coords[i]),n=matrix.MultiplyVector(normals.Length==coords.Length?normals[i]:Vector3.up).normalized,uv=uv.Length==coords.Length?uv[i]:Vector2.zero};
            for(int m=0;m<mesh.subMeshCount;m++){int material=materials.IndexOf(mats[Mathf.Min(m,mats.Length-1)]);var tris=mesh.GetTriangles(m);for(int i=0;i<tris.Length;i+=3)g.faces.Add(new Triangle{a=At(tris[i]),b=At(tris[i+1]),c=At(tris[i+2]),m=material,surface=Array.IndexOf(filters,f)});}
        }
        var random=new System.Random(assetId*7919+17);
        var tuning=MansionPropFactory.Profile(substance);float maxFragment=assetId==20?.65f:tuning.maximumFragmentSize;
        var root=new GameObject("Debris "+source.name);root.SetActive(false);root.AddComponent<MansionDebris>();var pieces=new List<Rigidbody>();int index=0;
        foreach(var g in groups.Values)
        {
            if(g.faces.Count==0)continue;var bb=new Bounds(g.faces[0].a.p,Vector3.zero);foreach(var t in g.faces){bb.Encapsulate(t.a.p);bb.Encapsulate(t.b.p);bb.Encapsulate(t.c.p);}
            var chunks=new List<List<Triangle>>{g.faces};int planes=groups.Count>4?1:substance=="wood"?1:3;
            Vector3 RandomPlane(int axis){var n=new Vector3((float)random.NextDouble()*.9f-.45f,(float)random.NextDouble()*.9f-.45f,(float)random.NextDouble()*.9f-.45f);n[axis]=1;return n.normalized;}
            var normals=new[]{RandomPlane(0),RandomPlane(1),RandomPlane(2)};
            for(int plane=0;plane<planes;plane++)
            {
                var next=new List<List<Triangle>>();float offset=Vector3.Dot(normals[plane],bb.center)+bb.extents[plane]*((float)random.NextDouble()-.5f)*.4f;
                foreach(var chunk in chunks)foreach(bool side in new[]{false,true}){var clipped=Clip(chunk,normals[plane],offset,side,cutMaterial);if(clipped.Count>=4)next.Add(clipped);}chunks=next;
            }
            // Split oversized components further, instead of retaining door-sized triangular sails.
            for(int pass=0;pass<6;pass++)
            {
                bool changed=false;var refined=new List<List<Triangle>>();
                foreach(var chunk in chunks)
                {
                    var cb=new Bounds(chunk[0].a.p,Vector3.zero);foreach(var t in chunk){cb.Encapsulate(t.a.p);cb.Encapsulate(t.b.p);cb.Encapsulate(t.c.p);}
                    int axis=cb.size.x>cb.size.y?(cb.size.x>cb.size.z?0:2):(cb.size.y>cb.size.z?1:2);
                    if(cb.size[axis]>maxFragment && refined.Count+chunks.Count<Mathf.Clamp(tuning.targetFragments*4,16,96))
                    {
                        var normal=RandomPlane(axis);float split=Vector3.Dot(normal,cb.center)+cb.extents[axis]*((float)random.NextDouble()-.5f)*.36f;
                        var left=Clip(chunk,normal,split,false,cutMaterial);var right=Clip(chunk,normal,split,true,cutMaterial);
                        if(left.Count>=4&&right.Count>=4){refined.Add(left);refined.Add(right);changed=true;}else refined.Add(chunk);
                    }
                    else refined.Add(chunk);
                }
                chunks=refined;if(!changed)break;
            }
            foreach(var chunk in chunks)
            {
                var mesh=ToMesh(chunk,materials.Count,out var center);if(mesh.bounds.size.sqrMagnitude<.00002f){UnityEngine.Object.DestroyImmediate(mesh);continue;}
                string meshPath=path+"_"+(index++).ToString("D2")+".asset";mesh.name=System.IO.Path.GetFileNameWithoutExtension(meshPath);var existing=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);if(existing==null)AssetDatabase.CreateAsset(mesh,meshPath);else{EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;}
                var piece=new GameObject("Shard "+source.name+" "+index);piece.transform.SetParent(root.transform,false);piece.transform.localPosition=center;piece.AddComponent<MeshFilter>().sharedMesh=mesh;piece.AddComponent<MeshRenderer>().sharedMaterials=materials.ToArray();var box=piece.AddComponent<BoxCollider>();box.size=Vector3.Max(mesh.bounds.size,Vector3.one*.014f);box.center=mesh.bounds.center;box.contactOffset=.002f;
                var body=piece.AddComponent<Rigidbody>();body.isKinematic=true;body.mass=Mathf.Max(.04f,mass/Mathf.Max(4,groups.Count*4));body.maxDepenetrationVelocity=2;body.maxAngularVelocity=14;body.angularDamping=substance=="glass"?.18f:.65f;body.linearDamping=substance=="glass"?.03f:.08f;body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;pieces.Add(body);
                var meta=piece.AddComponent<MansionFragment>();meta.wallImpactMultiplier=substance=="glass"?.2f:substance=="ceramic"?.6f:.5f;meta.sourceName=g.sourceName;meta.sourceRestInverse=g.inverse;meta.restCenter=center;
            }
        }
        foreach(var body in pieces)body.mass=Mathf.Max(.025f,mass/Mathf.Max(1,pieces.Count));return root;
    }
}
