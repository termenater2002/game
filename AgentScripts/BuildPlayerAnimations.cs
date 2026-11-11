using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildPlayerAnimations
{
    const string Root = "Assets/Game/Animations/";
    const string ControllerPath = Root + "PlayerMoveAnimation.controller";
    static AnimationClip Clip(string file, string name = null) => AssetDatabase.LoadAllAssetsAtPath(Root + "Pack/" + file + ".fbx").OfType<AnimationClip>().First(c => !c.name.StartsWith("__preview") && (name == null || c.name == name));
    static void ConfigureImports()
    {
        foreach (var p in Directory.GetFiles(Root + "Pack", "*.fbx"))
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(p);
            var original = importer.defaultClipAnimations[0];
            // Keep existing take name and local IDs; only override playback/root settings.
            original.loopTime = Path.GetFileName(p) != "jump.fbx"; original.loopPose = original.loopTime;
            original.lockRootRotation = true; original.keepOriginalOrientation = true;
            original.lockRootPositionXZ = true; original.keepOriginalPositionXZ = true;
            original.lockRootHeightY = true; original.keepOriginalPositionY = true;
            importer.clipAnimations = new[] { original }; importer.SaveAndReimport();
        }
    }
    static AnimationClip Derived(string name, AnimationClip source, float from, float to, float duration, bool loop, bool reverse = false)
    {
        string p = Root + name + ".anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(p);
        if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, p); }
        clip.ClearCurves(); clip.name = name; clip.frameRate = 30;
        int steps = Mathf.Max(2, Mathf.CeilToInt(duration * 30));
        foreach (var binding in AnimationUtility.GetCurveBindings(source))
        {
            var original = AnimationUtility.GetEditorCurve(source, binding);
            var keys = new Keyframe[steps + 1];
            for (int i = 0; i <= steps; i++) {
                float u = (float)i / steps; float t = Mathf.Lerp(from, to, reverse ? 1 - u : u);
                keys[i] = new Keyframe(u * duration, original.Evaluate(t));
            }
            if (keys.Max(k => k.value) - keys.Min(k => k.value) < .00001f)
                keys = new[] { new Keyframe(0, keys[0].value), new Keyframe(duration, keys[0].value) };
            var curve = new AnimationCurve(keys);
            for (int i = 0; i < keys.Length; i++) {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            }
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }
        var settings = AnimationUtility.GetAnimationClipSettings(source);
        settings.startTime = 0; settings.stopTime = duration; settings.loopTime = loop; settings.loopBlend = loop;
        settings.loopBlendOrientation = true; settings.loopBlendPositionXZ = true; settings.loopBlendPositionY = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings); EditorUtility.SetDirty(clip); return clip;
    }
    static AnimatorState State(AnimatorStateMachine sm, string name, Motion motion, float x, float y)
    {
        var s = sm.AddState(name, new Vector3(x, y)); s.motion = motion; s.writeDefaultValues = false; s.iKOnFeet = true; return s;
    }
    static AnimatorStateTransition Transition(AnimatorState from, AnimatorState to, float blend = .1f)
    {
        var t = from.AddTransition(to); t.hasExitTime = false; t.hasFixedDuration = true; t.duration = blend; t.canTransitionToSelf = false; return t;
    }
    static void Air(AnimatorStateMachine sm, AnimatorState target, AnimatorConditionMode velocity, float threshold)
    {
        var t = sm.AddAnyStateTransition(target); t.hasExitTime = false; t.duration = .08f; t.canTransitionToSelf = false;
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded"); t.AddCondition(velocity, threshold, "VerticalSpeed");
    }
    public static object Apply()
    {
        if (Application.isPlaying || SceneManager.GetActiveScene().name != "SampleScene") throw new Exception("Open SampleScene in Edit Mode.");
        ConfigureImports();
        var idle = Clip("idle"); var walk = Clip("walking"); var run = Clip("running"); var jump = Clip("jump");
        if (!new[] { idle, walk, run, jump }.All(c => c.isHumanMotion)) throw new Exception("Expected existing Humanoid clips.");
        var backward = Derived("Backward Walk", walk, 0, walk.length, walk.length, true, true);
        var takeoff = Derived("Jump Takeoff", jump, .45f, .95f, .38f, false);
        var falling = Derived("Fall Pose", jump, .95f, .95f, .6f, true);
        var landing = Derived("Landing", jump, 1.35f, jump.length, .24f, false);
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null) throw new Exception("Keep the existing controller and GUID.");
        // These are generated controller subassets only. Original FBXs, GUIDs and unrelated resources survive.
        controller.layers = new AnimatorControllerLayer[0]; controller.parameters = new AnimatorControllerParameter[0];
        foreach (var sub in AssetDatabase.LoadAllAssetsAtPath(ControllerPath)) if (sub != null && sub != controller) UnityEngine.Object.DestroyImmediate(sub, true);
        foreach (string n in new[] { "MoveX", "MoveZ", "Speed", "VerticalSpeed", "PlaybackRate", "TurnRate" }) controller.AddParameter(n, AnimatorControllerParameterType.Float);
        controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool); controller.AddParameter("Holding", AnimatorControllerParameterType.Bool);
        var sm = new AnimatorStateMachine { name = "Base Layer", hideFlags = HideFlags.HideInHierarchy }; AssetDatabase.AddObjectToAsset(sm, controller);
        controller.AddLayer(new AnimatorControllerLayer { name = "Base Layer", stateMachine = sm, defaultWeight = 1f, iKPass = true });
        var tree = new BlendTree { name = "Directional Locomotion", blendType = BlendTreeType.FreeformCartesian2D, blendParameter = "MoveX", blendParameterY = "MoveZ", useAutomaticThresholds = false, hideFlags = HideFlags.HideInHierarchy };
        AssetDatabase.AddObjectToAsset(tree, controller);
        float w = .16f, r = .4f;
        tree.AddChild(idle, Vector2.zero); tree.AddChild(walk, new Vector2(0, w)); tree.AddChild(run, new Vector2(0, r));
        tree.AddChild(backward, new Vector2(0, -w));
        tree.AddChild(Clip("left strafe walking"), new Vector2(-w, 0)); tree.AddChild(Clip("right strafe walking"), new Vector2(w, 0));
        tree.AddChild(Clip("left strafe"), new Vector2(-r, 0)); tree.AddChild(Clip("right strafe"), new Vector2(r, 0));
        var locomotion = State(sm, "Locomotion", tree, 240, 70); locomotion.speedParameter = "PlaybackRate"; locomotion.speedParameterActive = true;
        var rising = State(sm, "Jump", takeoff, 500, -60); var fall = State(sm, "Fall", falling, 750, -60); var land = State(sm, "Landing", landing, 500, 160);
        sm.defaultState = locomotion;
        Air(sm, rising, AnimatorConditionMode.Greater, .15f); Air(sm, fall, AnimatorConditionMode.Less, .05f);
        Transition(rising, fall).AddCondition(AnimatorConditionMode.Less, .05f, "VerticalSpeed");
        Transition(rising, land).AddCondition(AnimatorConditionMode.If, 0, "Grounded"); Transition(fall, land).AddCondition(AnimatorConditionMode.If, 0, "Grounded");
        var recover = Transition(land, locomotion, .1f); recover.hasExitTime = true; recover.exitTime = .85f;
        var player = UnityEngine.Object.FindAnyObjectByType<FunnyMovement>(); var animator = player.GetComponent<Animator>();
        if (!animator.isHuman || !animator.avatar.isValid) throw new Exception("Invalid player avatar.");
        animator.runtimeAnimatorController = controller; animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        var driver = player.GetComponent<PlayerAnimationDriver>() ?? player.gameObject.AddComponent<PlayerAnimationDriver>(); driver.grabber = player.GetComponentInChildren<ObjectGrabber>();
        driver.walkReferenceSpeed = w; driver.runReferenceSpeed = r;
        var visibility = player.GetComponent<FirstPersonBodyVisibility>() ?? player.gameObject.AddComponent<FirstPersonBodyVisibility>();
        visibility.firstPersonCamera = player.GetComponentInChildren<Camera>(); visibility.bodyRenderers = player.GetComponentsInChildren<Renderer>();
        PrefabUtility.RecordPrefabInstancePropertyModifications(animator); EditorUtility.SetDirty(controller); AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        return "Rebuilt existing controller: directional locomotion, jump/fall/landing, Rigidbody driver, hand IK and per-camera body visibility.";
    }
    public static object PreviewScene()
    {
        if (Application.isPlaying || SceneManager.GetActiveScene().name != "SampleScene") throw new Exception("Open SampleScene in Edit Mode.");
        var original = UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();
        var clone = UnityEngine.Object.Instantiate(original.gameObject); clone.name = "Player Animation Preview";
        if (PrefabUtility.IsPartOfPrefabInstance(clone)) PrefabUtility.UnpackPrefabInstance(clone, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        var sky = RenderSettings.ambientSkyColor; var equator = RenderSettings.ambientEquatorColor; var ground = RenderSettings.ambientGroundColor;
        var preview = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive); SceneManager.MoveGameObjectToScene(clone, preview); SceneManager.SetActiveScene(preview);
        clone.transform.SetPositionAndRotation(new Vector3(0, .03f, 0), Quaternion.identity); clone.transform.localScale = Vector3.one * .5f;
        clone.GetComponent<FunnyMovement>().enabled = false; clone.GetComponent<PlayerMovement>().enabled = false;
        foreach (var cam in clone.GetComponentsInChildren<Camera>()) cam.enabled = false;
        foreach (var listener in clone.GetComponentsInChildren<AudioListener>()) listener.enabled = false;
        clone.GetComponentInChildren<FirstPersonCamera>().enabled = false; clone.GetComponentInChildren<ObjectGrabber>().enabled = false;
        var animation = clone.GetComponent<PlayerAnimationDriver>(); animation.walkReferenceSpeed = .8f; animation.runReferenceSpeed = 2f; animation.blendSpaceScale = .2f;
        // Keep the same blend space when demonstrating the original toy-scale model at 5x size.
        var show = clone.AddComponent<AnimationPreviewDriver>();
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane); floor.name = "Preview Floor"; floor.transform.localScale = Vector3.one * 3f;
        // The floor is neutral; a palette UV atlas must not be stretched across a primitive.
        floor.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Art/LowPoly/Ground Basalt.mat");
        var cameraObject = new GameObject("Preview Camera"); var camera = cameraObject.AddComponent<Camera>(); cameraObject.AddComponent<AudioListener>(); camera.tag = "MainCamera";
        camera.transform.position = new Vector3(2.8f, 2f, 3.7f); camera.transform.LookAt(Vector3.up * .85f); camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.6353f, .7333f, .7647f); camera.nearClipPlane = .03f; camera.fieldOfView = 36; show.previewCamera = camera;
        var key = new GameObject("Preview Key").AddComponent<Light>(); key.type = LightType.Directional; key.transform.rotation = Quaternion.Euler(45, -35, 0); key.intensity = 1.65f; key.shadows = LightShadows.Soft; key.shadowStrength = .65f;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight; RenderSettings.ambientSkyColor = sky; RenderSettings.ambientEquatorColor = equator; RenderSettings.ambientGroundColor = ground;
        RenderSettings.skybox = null; RenderSettings.fog = false;
        EditorSceneManager.MarkSceneDirty(preview); EditorSceneManager.SaveScene(preview, "Assets/Game/Scenes/AnimationPreview.unity");
        EditorSceneManager.OpenScene("Assets/Game/Scenes/SampleScene.unity"); return "Saved AnimationPreview: same avatar/controller, physical movement, 1–8 select modes, P automatic cycle.";
    }
}
