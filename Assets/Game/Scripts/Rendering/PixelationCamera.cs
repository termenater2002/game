using UnityEngine;

/// <summary>Opt a game camera into the world-only pixel filter. Overlay HUD stays at native resolution.</summary>
[DisallowMultipleComponent, RequireComponent(typeof(Camera))]
public sealed class PixelationCamera : MonoBehaviour
{
    [Tooltip("Approximate vertical resolution. Lower values produce larger square pixels.")]
    [Range(60, 720)] public int verticalPixels = 180;

    private MaterialPropertyBlock parameters;
    internal MaterialPropertyBlock Parameters => parameters ?? (parameters = new MaterialPropertyBlock());
}
