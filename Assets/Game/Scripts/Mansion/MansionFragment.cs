using UnityEngine;
public sealed class MansionFragment : MonoBehaviour
{
    public float wallImpactMultiplier = .5f;
    public string sourceName;
    public Matrix4x4 sourceRestInverse = Matrix4x4.identity;
    public Vector3 restCenter;
}
