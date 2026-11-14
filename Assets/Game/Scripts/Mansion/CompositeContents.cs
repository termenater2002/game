using UnityEngine;

/// <summary>Exclude child bodies from their owner's self-collision while mounted or packed.</summary>
public sealed class CompositeContents : MonoBehaviour
{
    private void Awake()
    {
        var parts = GetComponentsInChildren<IndependentPart>(); var shapes = GetComponentsInChildren<Collider>();
        foreach (var part in parts) foreach (var a in part.GetComponentsInChildren<Collider>()) foreach (var b in shapes)
            if (a != b) Physics.IgnoreCollision(a, b, true);
    }
}
