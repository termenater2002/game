using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Threading.Tasks;

public static class CaptureLowPoly
{
    public static object Overview() => Capture("Documentation/VisualAnimation/Scene-Overview.png", new Vector3(8.5f, 8f, -9f), new Vector3(-.6f, .4f, .1f), true, 7.4f);
    public static object Street() => Capture("Documentation/VisualAnimation/Street.png", new Vector3(4.35f, 1.5f, -3.7f), new Vector3(-2.4f, 1.05f, .65f), false, 45);
    public static async Task<object> MainSceneEvidence()
    {
        if (!Application.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "SampleScene") throw new Exception("Enter Play Mode in SampleScene.");
        await Task.Delay(2000); Overview(); Street(); Character(); return "Captured settled main scene, retaining the gameplay camera's URP rendering settings.";
    }
    public static object Character() {
        var p = UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();
        return Capture("Documentation/VisualAnimation/Character.png", p.transform.position + p.transform.forward * .7f + p.transform.right * .34f + Vector3.up * .26f, p.transform.position + Vector3.up * .18f, true, .24f);
    }
    public static object Holding() {
        var p = UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();
        return Capture("Documentation/VisualAnimation/Holding.png", p.transform.position + new Vector3(.55f, .33f, .65f), p.transform.position + Vector3.up * .2f, true, .26f);
    }
    public static async Task<object> PreviewEvidence()
    {
        var preview = UnityEngine.Object.FindAnyObjectByType<AnimationPreviewDriver>();
        if (!Application.isPlaying || preview == null) throw new Exception("Enter Play Mode in AnimationPreview first.");
        preview.Restart(); var start = Time.time;
        var seen = new System.Collections.Generic.HashSet<string>();
        var evidence = new System.Collections.Generic.List<object>();
        var animator = preview.GetComponent<Animator>();
        while (Time.time - start < 12.7f)
        {
            await Task.Delay(70);
            var current = animator.GetCurrentAnimatorStateInfo(0);
            string state = new[] { "Locomotion", "Jump", "Fall", "Landing" }.FirstOrDefault(n => current.IsName(n));
            string key = preview.mode + "-" + state;
            if (state == null || animator.IsInTransition(0) || seen.Contains(key)) continue;
            // Let the directional blend settle before documenting locomotion.
            if (state == "Locomotion" && (Time.time - start) % 1.6f < .5f) continue;
            seen.Add(key);
            var camera = preview.previewCamera;
            Capture("Documentation/VisualAnimation/Preview-" + key + ".png", camera.transform.position, preview.transform.position + Vector3.up * .85f, false, 36);
            evidence.Add(new { mode = preview.mode, state, time = Time.time, frame = Time.frameCount, velocity = preview.GetComponent<Rigidbody>().linearVelocity.ToString("F3"), clips = animator.GetCurrentAnimatorClipInfo(0).Select(c => new { path = AssetDatabase.GetAssetPath(c.clip), c.weight }).ToArray() });
        }
        return Newtonsoft.Json.JsonConvert.SerializeObject(evidence);
    }
    public static async Task<object> RecordPreview(string directory)
    {
        var preview = UnityEngine.Object.FindAnyObjectByType<AnimationPreviewDriver>();
        if (!Application.isPlaying || preview == null) throw new Exception("Enter Play Mode in AnimationPreview first.");
        Directory.CreateDirectory(directory); preview.Restart();
        var camera = preview.previewCamera; var originalTarget = camera.targetTexture; float aspect = camera.aspect;
        var rt = RenderTexture.GetTemporary(960, 600, 24, RenderTextureFormat.ARGB32);
        var texture = new Texture2D(960, 600, TextureFormat.RGB24, false);
        var samples = new System.Collections.Generic.List<object>(); var start = Time.time;
        try {
            camera.aspect = 1.6f;
            while (Time.time - start < 13.5f) {
                await Task.Delay(42);
                float time = Time.time - start; int frame = Time.frameCount;
                var previous = RenderTexture.active;
                try {
                    camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
                    texture.ReadPixels(new Rect(0, 0, 960, 600), 0, 0); texture.Apply();
                    File.WriteAllBytes(Path.Combine(directory, "frame" + samples.Count.ToString("D5") + ".png"), texture.EncodeToPNG());
                } finally { camera.targetTexture = originalTarget; RenderTexture.active = previous; }
                samples.Add(new { time, frame, mode = preview.mode });
            }
        } finally { camera.targetTexture = originalTarget; camera.aspect = aspect; RenderTexture.ReleaseTemporary(rt); UnityEngine.Object.DestroyImmediate(texture); }
        File.WriteAllText(Path.Combine(directory, "timing.json"), Newtonsoft.Json.JsonConvert.SerializeObject(samples));
        return new { directory, count = samples.Count, duration = Time.time - start, startFrame = samples.First(), endFrame = Time.frameCount };
    }
    public static object ProbeJump()
    {
        var p = UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();
        var clip = System.Linq.Enumerable.First(System.Linq.Enumerable.OfType<AnimationClip>(AssetDatabase.LoadAllAssetsAtPath("Assets/Game/Animations/Pack/jump.fbx")), c => !c.name.StartsWith("__preview"));
        AnimationMode.StartAnimationMode();
        try {
            for (int i = 0; i < 8; i++) {
                AnimationMode.BeginSampling(); AnimationMode.SampleAnimationClip(p.gameObject, clip, i * .3f); AnimationMode.EndSampling();
                Capture("Documentation/VisualAnimation/Probe-Jump-" + i + ".png", p.transform.position + p.transform.forward * .65f + p.transform.right * .3f + Vector3.up * .3f, p.transform.position + Vector3.up * .18f, true, .3f);
            }
        } finally { AnimationMode.StopAnimationMode(); }
        return "Captured eight source jump poses at 0.3s intervals without saving the scene.";
    }
    public static object Capture(string path, Vector3 position, Vector3 target, bool ortho, float size)
    {
        var go = new GameObject("Documentation camera (temporary)");
        var camera = go.AddComponent<Camera>(); camera.enabled = false; camera.transform.position = position; camera.transform.LookAt(target);
        // Match the gameplay camera's post-processing rather than silently dropping its URP settings.
        var main = Camera.main;
        var sourceData = main == null ? null : main.GetComponents<MonoBehaviour>().FirstOrDefault(c => c != null && c.GetType().Name == "UniversalAdditionalCameraData");
        if (sourceData != null) EditorUtility.CopySerialized(sourceData, go.AddComponent(sourceData.GetType()));
        camera.orthographic = ortho; camera.orthographicSize = size; camera.fieldOfView = size; camera.nearClipPlane = .005f; camera.farClipPlane = 100;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.6353f, 0.7333f, 0.7647f);
        var rt = RenderTexture.GetTemporary(1600, 1000, 24, RenderTextureFormat.ARGB32);
        var previous = RenderTexture.active; var tex = new Texture2D(1600, 1000, TextureFormat.RGB24, false);
        try {
            camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); tex.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllBytes(path, tex.EncodeToPNG());
        } finally { camera.targetTexture = null; RenderTexture.active = previous; RenderTexture.ReleaseTemporary(rt); UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(go); }
        return new FileInfo(path).FullName;
    }
}
