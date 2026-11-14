using UnityEngine;

/// <summary>Contact and ownership link for a physical fabric strip detached from its moving parent.</summary>
public sealed class FabricSegment : MonoBehaviour
{
    public FlexibleFabric fabric;
    private void OnCollisionEnter(Collision collision)
    {
        if (fabric == null) return;
        if (fabric.IsFrozen) fabric.Wake();
        var damage = fabric.GetComponent<DamageOnImpact>();
        if (damage != null) damage.ReceiveCollision(collision);
    }
}
