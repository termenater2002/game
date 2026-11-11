using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class ConfigureFirstPersonWalk
{
    public static object Apply()
    {
        if(Application.isPlaying)throw new Exception("Use Edit Mode to persist the FPS setup.");
        var scene=SceneManager.GetActiveScene();
        if(scene.path!="Assets/Game/Scenes/SampleScene.unity" || scene.isDirty)throw new Exception("Open the saved SampleScene first.");
        var movement=UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();var look=movement.GetComponentInChildren<FirstPersonCamera>();
        var visibility=movement.GetComponent<FirstPersonBodyVisibility>();
        Undo.RecordObjects(new UnityEngine.Object[]{movement,look,look.transform,look.GetComponent<Camera>(),visibility},"Fit first-person body and gait");
        movement.matchAnimationSpeeds=true;movement.accelerationSeconds=.12f;movement.stoppingSeconds=.12f;
        movement.walkSpeed=movement.WalkSpeed;movement.sprintSpeed=movement.SprintSpeed;
        look.fitBodyView=true;look.gaitMotion=true;look.stepHeightFraction=.012f;look.stepSwayFraction=.004f;look.FitBodyView();
        visibility.showFirstPersonBody=true;
        foreach(var o in new UnityEngine.Object[]{movement,look,look.transform,look.GetComponent<Camera>(),visibility})
        {EditorUtility.SetDirty(o);PrefabUtility.RecordPrefabInstancePropertyModifications(o);}
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        return new{scene=scene.path,movement.WalkSpeed,movement.SprintSpeed,nearClip=look.GetComponent<Camera>().nearClipPlane,eye=look.transform.localPosition.ToString("F4")};
    }
}
