using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Unity.Muse.Animate
{
    readonly struct LightingSettings
    {
        public readonly Color FogColor;
        public readonly float FogDensity;
        public readonly Color SkyColor;
        public readonly Color EquatorColor;
        public readonly Color GroundColor;

        public LightingSettings(Color fogColor, float fogDensity, Color skyColor, Color equatorColor, Color groundColor)
        {
            FogColor = fogColor;
            FogDensity = fogDensity;
            SkyColor = skyColor;
            EquatorColor = equatorColor;
            GroundColor = groundColor;
        }
    }

    struct LightingSettingsOverride : IDisposable
    {
        public LightingSettingsOverride(in LightingSettings settings, Scene scene)
        {
#if UNITY_EDITOR
            if (RenderPipelineUtils.IsUsingHdrp())
                return;

            if (!UnityEditor.Unsupported.SetOverrideLightingSettings(scene))
            {
                DevLogger.LogWarning("Failed to set override lighting settings");
                return;
            }

            RenderSettings.fog = true;
            RenderSettings.fogColor = settings.FogColor;
            RenderSettings.fogDensity = settings.FogDensity;
            RenderSettings.skybox = null;
            RenderSettings.sun = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = settings.SkyColor;
            RenderSettings.ambientEquatorColor = settings.EquatorColor;
            RenderSettings.ambientGroundColor = settings.GroundColor;

            LightmapSettings.lightmaps = null;
            LightmapSettings.lightProbes = null;
#endif
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            if (!RenderPipelineUtils.IsUsingHdrp())
            {
                UnityEditor.Unsupported.RestoreOverrideLightingSettings();
            }
#endif
        }
    }
}
