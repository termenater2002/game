using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static class RiderCameraAudit
{
    const string DirectoryPath = "Documentation/RiderUpdate/";
    static Texture2D Render(Camera camera, string name)
    {
        Directory.CreateDirectory(DirectoryPath);
        var target = camera.targetTexture; var active = RenderTexture.active; float aspect = camera.aspect;
        var rt = RenderTexture.GetTemporary(1200, 900, 24, RenderTextureFormat.ARGB32);
        var image = new Texture2D(1200, 900, TextureFormat.RGB24, false);
        try {
            camera.targetTexture = rt; camera.aspect = 4f/3f; camera.Render(); RenderTexture.active = rt;
            image.ReadPixels(new Rect(0,0,1200,900),0,0); image.Apply(); File.WriteAllBytes(DirectoryPath+name+".png",image.EncodeToPNG());
        } finally {camera.targetTexture=target;camera.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);}
        return image;
    }
    public static object ShadowEvidence()
    {
        if(!Application.isPlaying) throw new Exception("Enter Play Mode first.");
        var driver=UnityEngine.Object.FindAnyObjectByType<PlayerAnimationDriver>();var view=driver.GetComponent<SideViewTestCamera>();view.SetSideView(false);
        var visibility=driver.GetComponent<FirstPersonBodyVisibility>();var camera=visibility.firstPersonCamera;
        var light=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).First(l=>l.type==LightType.Directional);
        var direction=Vector3.ProjectOnPlane(light.transform.forward,Vector3.up).normalized;
        var oldRotation=camera.transform.rotation; var oldEnabled=visibility.bodyRenderers.Select(r=>r.enabled).ToArray();
        Texture2D on=null,off=null;bool originalBodyVisible=visibility.showFirstPersonBody;
        try {
            visibility.showFirstPersonBody=false;
            driver.GetComponentInChildren<FirstPersonCamera>().ReleaseCursor();
            float height=driver.GetComponent<CapsuleCollider>().bounds.size.y;
            camera.transform.LookAt(driver.transform.position+direction*height*.65f);
            on=Render(camera,"FPS-Shadow-On");
            foreach(var renderer in visibility.bodyRenderers) renderer.enabled=false;
            off=Render(camera,"FPS-Shadow-Off-Reference");
            var a=on.GetPixels();var b=off.GetPixels();int darkened=0;double difference=0;
            for(int i=0;i<a.Length;i++){float delta=b[i].grayscale-a[i].grayscale;if(delta>.025f){darkened++;difference+=delta;}}
            if(darkened<100) throw new Exception("No measurable body shadow in FPS camera: "+darkened+" pixels.");
            return Newtonsoft.Json.JsonConvert.SerializeObject(new {darkenedPixels=darkened,averageDarkening=difference/Math.Max(1,darkened),width=on.width,height=on.height,
                firstPersonOnly=true, bodyInvisibleButCasts=true, frame=Time.frameCount,forceRenderingOff=visibility.bodyRenderers.Select(r=>r.forceRenderingOff).ToArray()});
        } finally {
            visibility.showFirstPersonBody=originalBodyVisible;
            for(int i=0;i<oldEnabled.Length;i++) visibility.bodyRenderers[i].enabled=oldEnabled[i];
            camera.transform.rotation=oldRotation;if(on!=null) UnityEngine.Object.DestroyImmediate(on);if(off!=null) UnityEngine.Object.DestroyImmediate(off);
        }
    }
    public static object SideEvidence()
    {
        var driver=UnityEngine.Object.FindAnyObjectByType<PlayerAnimationDriver>();var view=driver.GetComponent<SideViewTestCamera>();
        if(!Application.isPlaying||!view.SideViewActive) throw new Exception("Enable the real side camera (F6) first.");
        var picture=Render(view.sideCamera,"Side-Camera");UnityEngine.Object.DestroyImmediate(picture);
        return Inspect();
    }
    public static object Portrait()
    {
        var driver=UnityEngine.Object.FindAnyObjectByType<PlayerAnimationDriver>();
        var go=new GameObject("Rider portrait camera (temporary)");var c=go.AddComponent<Camera>();c.enabled=false;
        var b=driver.GetComponent<CapsuleCollider>().bounds;float h=b.size.y;
        c.transform.position=b.center+driver.transform.forward*h*2+driver.transform.right*h*.8f+Vector3.up*h*.14f;c.transform.LookAt(b.center);
        c.orthographic=true;c.orthographicSize=h*.59f;c.nearClipPlane=.003f;c.farClipPlane=100;c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.39f,.49f,.53f);
        try{var image=Render(c,"Rider-Front");UnityEngine.Object.DestroyImmediate(image);
            c.transform.position=b.center-driver.transform.forward*h*2+driver.transform.right*h*.75f+Vector3.up*h*.14f;c.transform.LookAt(b.center);
            image=Render(c,"Rider-Back");UnityEngine.Object.DestroyImmediate(image);
        }finally{UnityEngine.Object.DestroyImmediate(go);}
        return Inspect();
    }
    public static object Inspect()
    {
        var d=UnityEngine.Object.FindAnyObjectByType<PlayerAnimationDriver>();var a=d.CharacterAnimator;
        var v=d.GetComponent<SideViewTestCamera>();var r=d.GetComponent<FirstPersonBodyVisibility>();var body=d.GetComponent<Rigidbody>();
        return Newtonsoft.Json.JsonConvert.SerializeObject(new {
            playing=Application.isPlaying,model=a.name,avatar=a.avatar.name,a.avatar.isHuman,a.avatar.isValid,a.applyRootMotion,
            mesh=a.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh.name,
            sideActive=v.SideViewActive,firstPersonEnabled=v.firstPersonCamera.enabled,sideEnabled=v.sideCamera.enabled,
            shadows=r.bodyRenderers.Select(x=>new{x.name,mode=x.shadowCastingMode.ToString(),x.forceRenderingOff,x.enabled}).ToArray(),
            body.mass,body.useGravity,position=body.position.ToString("F4"),velocity=body.linearVelocity.ToString("F4"),
            frame=Time.frameCount,time=Time.time,focused=Application.isFocused,cursor=Cursor.lockState.ToString()
        });
    }
}
