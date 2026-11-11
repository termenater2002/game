using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Import the authored Blender low-poly mesh; keep the old Rider assets intact.</summary>
public static class ImportRiderV2
{
    const string Folder = "Assets/Game/Characters/RiderV2";
    const string Model = Folder + "/RiderV2.fbx";
    const string Prefab = Folder + "/RiderV2.prefab";
    public static object Import()
    {
        if (Application.isPlaying) throw new Exception("Import in Edit Mode.");
        AssetDatabase.ImportAsset(Model, ImportAssetOptions.ForceSynchronousImport);
        var importer = (ModelImporter)AssetImporter.GetAtPath(Model);
        importer.globalScale = 1.75f;
        importer.importAnimation = false;
        importer.importCameras = false; importer.importLights = false;
        importer.importNormals = ModelImporterNormals.Import;
        importer.isReadable = true;
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
        var boneNames = source.GetComponentsInChildren<Transform>().Select(t => t.name).ToHashSet();
        var human = HumanTrait.BoneName.Where(n => boneNames.Contains("MD_" + n.Replace(" ", "")))
            .Select(n => new HumanBone { humanName = n, boneName = "MD_" + n.Replace(" ", ""), limit = new HumanLimit { useDefaultValues = true } }).ToArray();
        if (human.Length != 22) throw new Exception("Expected 22 mapped Humanoid bones; got " + human.Length);
        var description = importer.humanDescription; description.human = human;
        description.skeleton = source.GetComponentsInChildren<Transform>().Select(t => new SkeletonBone { name = t.name, position = t.localPosition, rotation = t.localRotation, scale = t.localScale }).ToArray();
        description.upperArmTwist = .5f; description.lowerArmTwist = .5f;
        description.upperLegTwist = .5f; description.lowerLegTwist = .5f;
        description.armStretch = .02f; description.legStretch = .02f;
        importer.humanDescription = description; importer.SaveAndReimport();
        source = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
        var avatar = AssetDatabase.LoadAllAssetsAtPath(Model).OfType<Avatar>().Single();
        if (!avatar.isValid || !avatar.isHuman) throw new Exception("Invalid imported Humanoid avatar.");
        var go = UnityEngine.Object.Instantiate(source); go.name = "MajorDamage Rider V2";
        try
        {
            foreach (var key in importer.GetExternalObjectMap().Keys.ToArray())
                if (key.type == typeof(Material) && !key.name.StartsWith("R2 • ")) importer.RemoveRemap(key);
            var animator = go.GetComponent<Animator>(); animator.avatar = avatar;
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Game/Animations/PlayerMoveAnimation.controller");
            animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            go.AddComponent<PlayerAnimationIKRelay>();
            var skin = go.GetComponentInChildren<SkinnedMeshRenderer>();
            var materials = skin.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                var original = materials[i];
                string name = original.name.Replace("R2 • ", "");
                string path = Folder + "/" + name + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
                material.name = name;
                // Blender FBX stores the constant base colours; no generated textures or weave.
                material.SetColor("_BaseColor", original.HasProperty("_BaseColor") ? original.GetColor("_BaseColor") : original.color);
                material.SetFloat("_Metallic", name.Contains("alloy") ? .35f : 0f);
                material.SetFloat("_Smoothness", name.Contains("visor") ? .35f : name.Contains("helmet") ? .18f : .06f);
                EditorUtility.SetDirty(material); materials[i] = material;
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "R2 • " + name), material);
            }
            skin.sharedMaterials = materials; skin.shadowCastingMode = ShadowCastingMode.On; skin.receiveShadows = true;
            skin.updateWhenOffscreen = true;
            var bounds = skin.localBounds; bounds.Expand(bounds.size * .15f); skin.localBounds = bounds;
            PrefabUtility.SaveAsPrefabAsset(go, Prefab); AssetDatabase.WriteImportSettingsIfDirty(Model); AssetDatabase.SaveAssets();
            return Newtonsoft.Json.JsonConvert.SerializeObject(new { avatar.isValid, avatar.isHuman, animator.humanScale,
                vertices = skin.sharedMesh.vertexCount, triangles = skin.sharedMesh.triangles.Length / 3,
                bones = skin.bones.Length, materials = materials.Length, meshBounds = skin.sharedMesh.bounds.ToString(),
                worldBounds = skin.bounds.ToString(), hips = animator.GetBoneTransform(HumanBodyBones.Hips).position.ToString(), prefab = Prefab });
        }
        finally { UnityEngine.Object.DestroyImmediate(go); }
    }

    public static object ApplyToMainScene()
    {
        if (Application.isPlaying || SceneManager.GetActiveScene().name != "SampleScene") throw new Exception("Open SampleScene in Edit Mode.");
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
        var candidate = model != null ? model.GetComponent<Animator>() : null;
        if (candidate == null || candidate.avatar == null || !candidate.avatar.isHuman || !candidate.avatar.isValid)
            throw new Exception("Import a valid Humanoid before replacing the scene visual.");
        var player = UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();
        var oldAnimator = player.GetComponent<Animator>(); float originalScale = oldAnimator.humanScale;
        var previous = player.transform.Find("RiderVisual");
        if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
        foreach (var renderer in player.GetComponentsInChildren<Renderer>(true)) { renderer.enabled = false; PrefabUtility.RecordPrefabInstancePropertyModifications(renderer); }
        oldAnimator.enabled = false; PrefabUtility.RecordPrefabInstancePropertyModifications(oldAnimator);
        var rider = (GameObject)PrefabUtility.InstantiatePrefab(model, player.transform);
        rider.name = "RiderVisual"; rider.transform.localPosition = Vector3.zero; rider.transform.localRotation = Quaternion.identity; rider.transform.localScale = Vector3.one;
        var animator = rider.GetComponent<Animator>();
        if (!animator.isHuman || !animator.avatar.isValid) throw new Exception("Cannot attach an invalid Humanoid.");
        var driver = player.GetComponent<PlayerAnimationDriver>(); driver.characterAnimator = animator;
        float ratio = Mathf.Clamp(animator.humanScale / Mathf.Max(.01f, originalScale), .7f, 1.8f);
        driver.walkReferenceSpeed = .16f * ratio; driver.runReferenceSpeed = .4f * ratio;
        driver.blendSpaceScale = 1f / ratio; driver.footSoleFraction = .05f; driver.holdingReachFraction = .21f;
        player.GetComponent<FirstPersonBodyVisibility>().bodyRenderers = rider.GetComponentsInChildren<Renderer>();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        return Newtonsoft.Json.JsonConvert.SerializeObject(new { animator.isHuman, animator.humanScale, ratio, driver.walkReferenceSpeed, driver.runReferenceSpeed });
    }
}
