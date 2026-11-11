using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Run in Edit Mode on SampleScene. Never edits vendor assets or physics settings.</summary>
public static class BuildLowPolyLook
{
    public const string Root = "Assets/Game/Art/LowPoly";
    const string Kay = "Assets/ThirdParty/KayKit_City_Builder_Bits_1.0_FREE/Assets/fbx (unity)/";
    public static Color Hex(string s) { ColorUtility.TryParseHtmlString("#" + s, out var c); return c; }
    static void Folder(string p) { if (AssetDatabase.IsValidFolder(p)) return; Folder(Path.GetDirectoryName(p).Replace('\\', '/')); AssetDatabase.CreateFolder(Path.GetDirectoryName(p).Replace('\\', '/'), Path.GetFileName(p)); }
    static Texture2D Palette(string source, string name)
    {
        var input = new Texture2D(2, 2); input.LoadImage(File.ReadAllBytes(source));
        var colors = input.GetPixels();
        for (int i = 0; i < colors.Length; i++)
        {
            var c = colors[i]; Color.RGBToHSV(c, out var h, out var s, out var v);
            // Four broad tones retain UV panel detail while removing smooth photometric gradients.
            v = Mathf.Lerp(0.09f, 0.94f, Mathf.Round(Mathf.InverseLerp(0.04f, 1f, v) * 4f) / 4f);
            s *= 0.68f;
            var n = Color.HSVToRGB(h, s, v);
            n = Color.Lerp(n, Hex("ADBCC1"), 0.055f); n.a = c.a; colors[i] = n;
        }
        var result = new Texture2D(input.width, input.height, TextureFormat.RGBA32, false); result.SetPixels(colors); result.Apply();
        string p = Root + "/" + name + ".png"; File.WriteAllBytes(p, result.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(input); UnityEngine.Object.DestroyImmediate(result);
        AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(p); importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = true; importer.filterMode = FilterMode.Bilinear; importer.wrapMode = TextureWrapMode.Clamp; importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(p);
    }
    public static Material Matte(string name, string color, Texture texture = null)
    {
        string p = Root + "/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(p);
        if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, p); }
        m.SetColor("_BaseColor", Hex(color)); m.SetTexture("_BaseMap", texture);
        m.SetFloat("_Smoothness", 0.08f); m.SetFloat("_Metallic", 0f); m.SetFloat("_SpecularHighlights", 0f); m.SetFloat("_EnvironmentReflections", 0f);
        m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF"); m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
        EditorUtility.SetDirty(m); return m;
    }
    static void Assign(Renderer r, Material m) { r.sharedMaterials = Enumerable.Repeat(m, r.sharedMaterials.Length).ToArray(); PrefabUtility.RecordPrefabInstancePropertyModifications(r); }
    static void Prop(Transform parent, string model, Vector3 position, float yaw, Material material, float scale = 1f)
    {
        if (parent.Find(model + " " + position.ToString()) != null) return;
        var g = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Kay + model + ".fbx"));
        g.name = model + " " + position.ToString(); g.transform.SetParent(parent); g.transform.position = position;
        g.transform.rotation = Quaternion.Euler(0, yaw, 0); g.transform.localScale = Vector3.one * scale;
        foreach (var r in g.GetComponentsInChildren<Renderer>()) Assign(r, material);
    }
    public static object Apply()
    {
        if (Application.isPlaying || SceneManager.GetActiveScene().name != "SampleScene") throw new Exception("Open SampleScene in Edit Mode.");
        Folder(Root);
        var city = Palette(Kay + "citybits_texture.png", "CityPalette");
        var character = Palette("Assets/ThirdParty/CharacterPack Lowpoly (FREE)/Textures/characters_map.png", "CharacterPalette");
        var street = Matte("Street Slate", "D7E2DD", city);
        var architecture = Matte("Architecture Warm", "FFF0D8", city);
        var furniture = Matte("Street Furniture", "BDDCD4", city);
        var target = Matte("Salvage Teal", "8BE6CD", city);
        var car = Matte("Vehicle Ivory", "E7E8D7", city);
        var carGold = Matte("Vehicle Gold", "F7D09A", city);
        var player = Matte("Player Matte", "E8F2E8", character);
        var ground = Matte("Ground Basalt", "414F59");
        var debris = Matte("Debris Graphite", "617B81");
        var scene = SceneManager.GetActiveScene();
        foreach (var r in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Renderer>(true)))
        {
            if (r.GetComponentInParent<FunnyMovement>() != null) Assign(r, player);
            else if (r.name == "Ground") Assign(r, ground);
            else if (r.name.Contains("Debris Template")) Assign(r, debris);
            else
            {
                var top = r.transform; while (top.parent != null && top.parent.name != "Demo Environment") top = top.parent;
                string n = top.name;
                if (n.StartsWith("building")) Assign(r, architecture);
                else if (n.Contains("Scrap Bin")) Assign(r, target);
                else if (n.Contains("High Value")) Assign(r, carGold);
                else if (n.Contains("Police Car")) Assign(r, car);
                else if (n.StartsWith("base") || n.StartsWith("road")) Assign(r, street);
            }
        }
        var dressing = GameObject.Find("Low Poly Dressing") ?? new GameObject("Low Poly Dressing");
        // Existing meshes add silhouette detail; decorative additions introduce no colliders.
        Prop(dressing.transform, "bench", new Vector3(3.7f, .1f, 2.8f), -90, furniture, .65f);
        Prop(dressing.transform, "bench", new Vector3(3.7f, .1f, -2.8f), -90, furniture, .65f);
        Prop(dressing.transform, "streetlight", new Vector3(1f, .1f, 3.3f), 90, furniture, .8f);
        Prop(dressing.transform, "streetlight", new Vector3(1f, .1f, -3.3f), 90, furniture, .8f);
        Prop(dressing.transform, "bush", new Vector3(4.1f, .1f, 3.9f), 25, furniture, .6f);
        Prop(dressing.transform, "bush", new Vector3(4.1f, .1f, -3.9f), -20, furniture, .6f);
        Prop(dressing.transform, "firehydrant", new Vector3(-1.65f, .1f, 2.8f), 20, architecture, .7f);
        var sun = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).First(l => l.type == LightType.Directional);
        sun.transform.rotation = Quaternion.Euler(48, -32, 0); sun.color = Hex("FFE4C0"); sun.intensity = 1.65f;
        sun.shadows = LightShadows.Soft; sun.shadowStrength = .65f; sun.shadowBias = .03f; sun.shadowNormalBias = .1f;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = Hex("95B5CA"); RenderSettings.ambientEquatorColor = Hex("798D99"); RenderSettings.ambientGroundColor = Hex("4D5D63");
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogColor = Hex("A2BBC3"); RenderSettings.fogDensity = .025f;
        RenderSettings.skybox = null;
        var camera = Camera.main; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = RenderSettings.fogColor;
        var motor = UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();
        motor.transform.rotation = Quaternion.Euler(0, -70, 0); PrefabUtility.RecordPrefabInstancePropertyModifications(motor.transform);
        EditorSceneManager.MarkSceneDirty(scene); AssetDatabase.SaveAssets(); EditorSceneManager.SaveScene(scene);
        return "Applied authored matte palette, directional soft shadows and seven vendor-mesh decorations; physics unchanged.";
    }
}
