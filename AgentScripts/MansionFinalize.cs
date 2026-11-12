using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
public static class MansionFinalize
{
 public static object Restore()
 {
  if(EditorApplication.isPlaying)throw new Exception("Stop the agent's test Play session first");
  var scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/MansionPrototype.unity",OpenSceneMode.Single);
  var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
  var player=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();var look=player.GetComponentInChildren<FirstPersonCamera>();
  var result=new{scene=scene.path,playing=Application.isPlaying,props=UnityEngine.Object.FindObjectsByType<MansionProp>(FindObjectsSortMode.None).Length,controlsEnabled=player.enabled&&look.enabled&&look.GetComponent<ObjectGrabber>().enabled&&look.GetComponent<MansionInteraction>().enabled,captureOnStart=look.captureOnStart,mainCameraEnabled=look.GetComponent<Camera>().enabled,sideDistance=player.GetComponent<SideViewTestCamera>().distanceInHeights,missingScripts=all.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)),player=player.transform.position.ToString("F3"),camera=look.transform.position.ToString("F3"),sceneDirty=scene.isDirty};
  File.WriteAllText("Documentation/Mansion/editor_final_state.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
 }
}
