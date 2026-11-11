using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class VisualAnimationTools
{
    private static bool follow;
    [MenuItem("Tools/Major Damage/Toggle Side Test Camera (F6)")]
    public static void ToggleSideCamera()
    {
        if (!EditorApplication.isPlaying) return;
        var view = Object.FindAnyObjectByType<SideViewTestCamera>();
        if (view != null && view.enabled) view.SetSideView(!view.SideViewActive);
    }
    [MenuItem("Tools/Major Damage/Open Animation Preview")]
    public static void OpenPreview()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode before changing scenes."); return; }
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene("Assets/Game/Scenes/AnimationPreview.unity");
    }
    [MenuItem("Tools/Major Damage/Open Main Scene")]
    public static void OpenMain()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode before changing scenes."); return; }
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene("Assets/Game/Scenes/SampleScene.unity");
    }
    [MenuItem("Tools/Major Damage/Follow Player in Scene View")]
    public static void FollowPlayer()
    {
        follow = !follow; EditorApplication.update -= UpdateView;
        if (follow) EditorApplication.update += UpdateView;
        Menu.SetChecked("Tools/Major Damage/Follow Player in Scene View", follow); UpdateView();
    }
    private static void UpdateView()
    {
        if (SceneView.lastActiveSceneView == null) return;
        var player = Object.FindAnyObjectByType<PlayerAnimationDriver>(); if (player == null) return;
        float height = player.GetComponent<CapsuleCollider>().bounds.size.y;
        var rotation = player.transform.rotation * Quaternion.Euler(10, 150, 0);
        SceneView.lastActiveSceneView.LookAt(player.transform.position + Vector3.up * height * .5f, rotation, height * 1.6f, false, true);
    }
    [MenuItem("Tools/Major Damage/Frame Level")]
    public static void FrameLevel()
    {
        if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.LookAt(new Vector3(-.6f, .5f, 0), Quaternion.Euler(38, -45, 0), 9f, true);
    }
}
