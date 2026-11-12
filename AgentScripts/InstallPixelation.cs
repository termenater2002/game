using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using Newtonsoft.Json;
public static class InstallPixelation
{
 public static object Apply()
 {
  if(Application.isPlaying)throw new Exception("Install in Edit Mode");
  var shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Game/Shaders/Pixelation.shader");if(shader==null||!shader.isSupported)throw new Exception("Pixelation shader is missing or unsupported");
  foreach(string name in new[]{"PC_Renderer","Mobile_Renderer"})
  {
   var path="Assets/Game/Settings/"+name+".asset";var data=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
   var feature=data.rendererFeatures.OfType<PixelationRendererFeature>().FirstOrDefault();
   if(feature==null){feature=ScriptableObject.CreateInstance<PixelationRendererFeature>();feature.name="MAJOR DAMAGE Pixelation";AssetDatabase.AddObjectToAsset(feature,data);data.rendererFeatures.Add(feature);}
   feature.pixelationShader=shader;feature.SetActive(true);feature.Create();EditorUtility.SetDirty(feature);data.SetDirty();EditorUtility.SetDirty(data);
  }
  var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
  if(scene.path!="Assets/Game/Scenes/MansionPrototype.unity"||scene.isDirty)throw new Exception("Open the clean saved MansionPrototype before installing camera settings");
  var cameras=UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Where(c=>c.name=="Main Camera"||c.name=="Side Test Camera (F6)").ToArray();
  foreach(var camera in cameras)
  {
   var settings=camera.GetComponent<PixelationCamera>()??camera.gameObject.AddComponent<PixelationCamera>();settings.verticalPixels=180;settings.enabled=true;
   camera.GetUniversalAdditionalCameraData().antialiasing=AntialiasingMode.None;EditorUtility.SetDirty(camera);EditorUtility.SetDirty(settings);
  }
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  return new{cameras=cameras.Select(c=>c.name).ToArray(),verticalPixels=180,shader=shader.name};
 }
 public static object PrepareCapture()
 {
  if(Application.isPlaying)throw new Exception("Prepare in Edit Mode");
  var player=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();player.enabled=false;
  var look=player.GetComponentInChildren<FirstPersonCamera>();look.enabled=false;look.captureOnStart=false;
  look.GetComponent<ObjectGrabber>().enabled=false;look.GetComponent<MansionInteraction>().enabled=false;
  EditorApplication.isPlaying=true;return new{prepared=true};
 }
 public static object Toggle(bool enabled)
 {
  var camera=GameObject.Find("Main Camera").GetComponent<Camera>();camera.GetComponent<PixelationCamera>().enabled=enabled;
  return new{enabled,verticalPixels=camera.GetComponent<PixelationCamera>().verticalPixels};
 }
}
