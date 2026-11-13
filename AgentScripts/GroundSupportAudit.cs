using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
public static class GroundSupportAudit
{
 public static object Current()
 {
  var scene=SceneManager.GetActiveScene();var motor=Object.FindAnyObjectByType<FunnyMovement>();
  var driver=motor!=null?motor.GetComponent<PlayerAnimationDriver>():null;
  var look=motor!=null?motor.GetComponentInChildren<FirstPersonCamera>():null;
  var result=new{scene=scene.path,playing=Application.isPlaying,dirty=scene.isDirty,controlsEnabled=motor!=null&&motor.enabled&&look!=null&&look.enabled,animationEnabled=driver!=null&&driver.enabled,supportGraceSeconds=driver!=null?driver.supportGraceSeconds:0,position=motor!=null?motor.transform.position.ToString("F3"):null};
  Directory.CreateDirectory("Documentation/GroundSupport");File.WriteAllText("Documentation/GroundSupport/editor_final_state.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
 }
}
