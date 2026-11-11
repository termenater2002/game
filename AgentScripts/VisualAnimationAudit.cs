using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class VisualAnimationAudit
{
    static string PathOf(Transform t) => t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name;
    public static object RuntimeSnapshot()
    {
        var d = UnityEngine.Object.FindAnyObjectByType<PlayerAnimationDriver>(); var a = d.GetComponent<Animator>(); var b = d.GetComponent<Rigidbody>();
        var current = a.GetCurrentAnimatorStateInfo(0); var next = a.GetNextAnimatorStateInfo(0);
        var names = new[] { "Locomotion", "Jump", "Fall", "Landing" };
        return Newtonsoft.Json.JsonConvert.SerializeObject(new {
            state = names.FirstOrDefault(n => current.IsName(n)), next = names.FirstOrDefault(n => next.IsName(n)), transition = a.IsInTransition(0),
            position = new[] { b.position.x, b.position.y, b.position.z }, velocity = new[] { b.linearVelocity.x, b.linearVelocity.y, b.linearVelocity.z },
            d.Grounded, d.LandingCount, d.HoldingWeight, a.applyRootMotion, a.isHuman,
            parameters = a.parameters.ToDictionary(p => p.name, p => p.type == AnimatorControllerParameterType.Bool ? (object)a.GetBool(p.name) : a.GetFloat(p.name)),
            clips = a.GetCurrentAnimatorClipInfo(0).Select(c => new { c.clip.name, path = AssetDatabase.GetAssetPath(c.clip), c.weight, c.clip.isLooping }).ToArray(),
            bones = new[] { HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot, HumanBodyBones.LeftHand, HumanBodyBones.RightHand }.Select(bone => new { bone = bone.ToString(), position = a.GetBoneTransform(bone).position.ToString("F4") }).ToArray(),
            frame = Time.frameCount, time = Time.time, focused = Application.isFocused, cursor = Cursor.lockState.ToString()
        });
    }
    public static object PrepareGrab()
    {
        if (!Application.isPlaying) throw new Exception("Play Mode only; changes are discarded on stop.");
        var p = UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();
        var target = UnityEngine.Object.FindObjectsByType<DamageOnImpact>(FindObjectsSortMode.None).First(d => d.objectValue == 100f);
        var look = p.GetComponentInChildren<FirstPersonCamera>(); var grab = p.GetComponentInChildren<ObjectGrabber>();
        var body = p.GetComponent<Rigidbody>(); var center = target.GetComponent<Renderer>().bounds.center;
        var pos = center - Vector3.forward * .7f; pos.y = .1f; body.position = pos; body.rotation = Quaternion.identity; body.linearVelocity = Vector3.zero;
        p.transform.SetPositionAndRotation(pos, Quaternion.identity); Physics.SyncTransforms(); grab.transform.LookAt(center);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(FirstPersonCamera).GetField("pitch", flags).SetValue(look, Mathf.DeltaAngle(0, grab.transform.localEulerAngles.x));
        typeof(FirstPersonCamera).GetField("yaw", flags).SetValue(look, 0f); look.enabled = false;
        EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus(); look.CaptureCursor();
        return "Positioned player within the original bin's 1m ray, using real grabber/physics. Focus Game View for holding.";
    }
    public static object Grab()
    {
        var grab = UnityEngine.Object.FindAnyObjectByType<ObjectGrabber>(); bool success = grab.TryGrab();
        return Newtonsoft.Json.JsonConvert.SerializeObject(new { success, holding = grab.IsHolding, target = grab.HeldBody?.name, position = grab.HeldBody?.position.ToString("F4"), mass = grab.HeldBody?.mass, gravity = grab.HeldBody?.useGravity,
            rayOrigin = grab.transform.position.ToString(), rayDirection = grab.transform.forward.ToString(), mask = grab.grabbableMask.value });
    }
    public static object HeldSnapshot()
    {
        var grab = UnityEngine.Object.FindAnyObjectByType<ObjectGrabber>();
        return Newtonsoft.Json.JsonConvert.SerializeObject(new { holding = grab.IsHolding, target = grab.HeldBody?.name, position = grab.HeldBody?.position.ToString("F4"), mass = grab.HeldBody?.mass, gravity = grab.HeldBody?.useGravity });
    }
    public static object PlayerDetails()
    {
        var p = UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();
        var a = p.GetComponent<Animator>();
        return Newtonsoft.Json.JsonConvert.SerializeObject(new {
            player = p.transform.position.ToString(), scale = p.transform.lossyScale.ToString(), a.humanScale,
            bones = Enum.GetValues(typeof(HumanBodyBones)).Cast<HumanBodyBones>().Where(b => b != HumanBodyBones.LastBone)
                .Select(b => new { bone = b.ToString(), path = a.GetBoneTransform(b) == null ? null : PathOf(a.GetBoneTransform(b)), pos = a.GetBoneTransform(b)?.position.ToString() }).ToArray(),
            colliders = p.GetComponentsInChildren<Collider>(true).Select(c => new { c.name, type = c.GetType().Name, c.enabled, bounds = c.bounds.ToString(), serialized = EditorJsonUtility.ToJson(c) }).ToArray(),
            groundCheck = p.groundCheck.position.ToString(), p.groundRayLength, mask = p.groundMask.value,
            gravity = Physics.gravity.ToString(), materials = AssetDatabase.FindAssets("t:Material").Length
        });
    }
    public static object Inspect()
    {
        var objects = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToArray();
        return Newtonsoft.Json.JsonConvert.SerializeObject(new {
            scene = SceneManager.GetActiveScene().path,
            objects = objects.Select(t => new { path = PathOf(t), position = t.position.ToString("F3"), scale = t.lossyScale.ToString("F3"), active = t.gameObject.activeSelf }).ToArray(),
            renderers = objects.SelectMany(t => t.GetComponents<Renderer>()).Select(r => new {
                path = PathOf(r.transform), bounds = r.bounds.ToString("F3"), enabled = r.enabled,
                mesh = r is SkinnedMeshRenderer ? AssetDatabase.GetAssetPath(((SkinnedMeshRenderer)r).sharedMesh) : AssetDatabase.GetAssetPath(r.GetComponent<MeshFilter>()?.sharedMesh),
                triangles = r is SkinnedMeshRenderer ? ((SkinnedMeshRenderer)r).sharedMesh.triangles.Length / 3 : (r.GetComponent<MeshFilter>()?.sharedMesh?.triangles.Length ?? 0) / 3,
                materials = r.sharedMaterials.Select(m => m == null ? null : new { m.name, path = AssetDatabase.GetAssetPath(m), shader = m.shader.name,
                    color = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor").ToString() : m.color.ToString(), texture = AssetDatabase.GetAssetPath(m.mainTexture) }).ToArray()
            }).ToArray(),
            animators = objects.SelectMany(t => t.GetComponents<Animator>()).Select(a => new {
                path = PathOf(a.transform), a.enabled, a.isHuman, a.applyRootMotion,
                avatar = a.avatar == null ? null : new { a.avatar.name, a.avatar.isValid, a.avatar.isHuman },
                controller = AssetDatabase.GetAssetPath(a.runtimeAnimatorController)
            }).ToArray(),
            clips = AssetDatabase.FindAssets("t:Model", new[] { "Assets/Game/Animations" }).Select(AssetDatabase.GUIDToAssetPath).Select(p => new {
                path = p, avatar = AssetDatabase.LoadAllAssetsAtPath(p).OfType<Avatar>().Select(a => new { a.name, a.isValid, a.isHuman }).ToArray(),
                clips = AssetDatabase.LoadAllAssetsAtPath(p).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")).Select(c => new { c.name, c.length, c.isHumanMotion, c.isLooping, averageSpeed = c.averageSpeed.ToString("F3"), c.averageAngularSpeed }).ToArray()
            }).ToArray(),
            lights = objects.SelectMany(t => t.GetComponents<Light>()).Select(l => new { path = PathOf(l.transform), l.type, l.intensity, color = l.color.ToString(), l.shadows }).ToArray(),
            camera = Camera.main == null ? null : new { position = Camera.main.transform.position.ToString("F3"), rotation = Camera.main.transform.eulerAngles.ToString("F3"), Camera.main.cullingMask },
            ambient = RenderSettings.ambientLight.ToString(), RenderSettings.ambientMode
        });
    }
}
