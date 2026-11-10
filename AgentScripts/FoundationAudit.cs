using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FoundationAudit
{
    private static GameObject[] SceneObjects() => SceneManager.GetActiveScene().GetRootGameObjects()
        .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject).ToArray();

    public static object Inspect()
    {
        var scene = SceneManager.GetActiveScene();
        var objects = SceneObjects();
        var missing = objects.Where(go => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) > 0).Select(go => go.name).ToArray();
        var player = UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();
        var look = UnityEngine.Object.FindAnyObjectByType<FirstPersonCamera>();
        var grabber = UnityEngine.Object.FindAnyObjectByType<ObjectGrabber>();
        var damage = UnityEngine.Object.FindObjectsByType<DamageOnImpact>(FindObjectsSortMode.None);
        if (missing.Length != 0) throw new Exception("Missing scripts: " + string.Join(", ", missing));
        if (player == null || !player.enabled || player.GetComponent<PlayerMovement>().enabled) throw new Exception("Movement selection changed.");
        if (look == null || look.playerBody != player.transform || grabber == null) throw new Exception("Camera / grab references broken.");
        int listeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(l => l.enabled && l.gameObject.activeInHierarchy);
        if (listeners != 1) throw new Exception("Expected one active AudioListener, got " + listeners);
        if (damage.Length != 4) throw new Exception("Expected four original damage components.");
        var wallet = UnityEngine.Object.FindAnyObjectByType<SessionWallet>();
        var hud = UnityEngine.Object.FindAnyObjectByType<DemoHud>();
        if (wallet == null || hud == null || hud.wallet != wallet || hud.look != look || hud.grabber != grabber) throw new Exception("HUD / wallet references broken.");
        var scripts = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets/Game/Scripts" }).Select(g => AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
        // Plain helper classes need not resolve to UnityEngine.Object via MonoScript.GetClass().
        return new {
            scene = scene.path, sceneDirty = scene.isDirty, objectCount = objects.Length,
            missingScripts = missing, activeListeners = listeners,
            player = player.name, mass = player.GetComponent<Rigidbody>().mass,
            movement = new { player.moveForce, player.jumpForce, player.maxSpeed },
            grab = new { grabber.grabDistance, grabber.grabForce },
            targets = damage.Select(d => new { d.name, d.maxHP, d.impactThreshold, d.objectValue, brokenTemplate = d.brokenPrefab != null ? d.brokenPrefab.name : null }).ToArray(),
            prefabCount = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }).Length,
            runtime = Application.isPlaying, frame = Time.frameCount, version = Application.unityVersion
        };
    }

    public static object RuntimeEvidence()
    {
        if (!Application.isPlaying) throw new Exception("Enter Play Mode first.");
        var wallet = SessionWallet.Current;
        var hud = UnityEngine.Object.FindAnyObjectByType<DemoHud>();
        if (wallet == null || hud == null) throw new Exception("Missing runtime wallet/HUD.");
        var target = UnityEngine.Object.FindObjectsByType<DamageOnImpact>(FindObjectsSortMode.None).FirstOrDefault(d => d.objectValue == 100f);
        if (target == null) throw new Exception("No original 100-value target.");
        decimal before = wallet.Earnings.Balance;
        var hit = target.ApplyDamage(10f);
        decimal after = wallet.Earnings.Balance;
        if (after - before != hit.Reward || hit.AppliedDamage <= 0f) throw new Exception("Runtime damage/reward failed.");
        if (!hud.DisplayedBalance.Contains(after.ToString("N2", System.Globalization.CultureInfo.InvariantCulture))) throw new Exception("HUD did not update.");
        var breakable = UnityEngine.Object.FindObjectsByType<DamageOnImpact>(FindObjectsSortMode.None).First(d => d.objectValue == 10000f);
        var destruction = breakable.ApplyDamage(breakable.State.MaxHealth * 2f);
        var debris = GameObject.Find("Impact Debris");
        if (debris == null || !debris.activeInHierarchy || debris.GetComponent<Rigidbody>() == null) throw new Exception("Debris creation failed.");
        return new { target = target.name, damage = hit.AppliedDamage, reward = hit.Reward, before, after,
            destructionReward = destruction.Reward, debris = debris.name, debrisActive = debris.activeInHierarchy,
            finalBalance = wallet.Earnings.Balance, hud = hud.DisplayedBalance, frame = Time.frameCount, time = Time.time };
    }

    public static object PlayerSnapshot()
    {
        var player = UnityEngine.Object.FindAnyObjectByType<FunnyMovement>();
        var p = player.transform.position; var v = player.GetComponent<Rigidbody>().linearVelocity;
        var grab = UnityEngine.Object.FindAnyObjectByType<ObjectGrabber>();
        return new { position = new[] { p.x, p.y, p.z }, velocity = new[] { v.x, v.y, v.z }, heldBody = grab.HeldBody != null ? grab.HeldBody.name : null,
            grounded = player.IsGrounded, frame = Time.frameCount, time = Time.time, focused = Application.isFocused,
            cursor = Cursor.lockState.ToString() };
    }
}
