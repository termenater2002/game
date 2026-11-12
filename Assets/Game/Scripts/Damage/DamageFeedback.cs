using UnityEngine;

/// <summary>Presentation only; legacy serialized settings stay on DamageOnImpact.</summary>
public class DamageFeedback : MonoBehaviour
{
    private DamageOnImpact owner;
    private Renderer target;
    private MaterialPropertyBlock properties;
    private Color originalColor;
    private int colorProperty;
    public void Initialize(DamageOnImpact source)
    {
        if (owner != null) owner.Damaged -= OnDamaged;
        owner = source;
        target = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        if (target != null && target.sharedMaterial != null)
        {
            var material = target.sharedMaterial;
            colorProperty = Shader.PropertyToID(material.HasProperty("_BaseColor") ? "_BaseColor" : "_Color");
            originalColor = material.HasProperty(colorProperty) ? material.GetColor(colorProperty) : Color.white;
            properties = new MaterialPropertyBlock();
        }
        owner.Damaged += OnDamaged;
    }
    private void OnDamaged(DamageReceipt receipt)
    {
        if (target != null && properties != null)
        {
            target.GetPropertyBlock(properties);
            properties.SetColor(colorProperty, Color.Lerp(Color.black, originalColor, receipt.HealthFraction));
            target.SetPropertyBlock(properties);
        }
        if (!receipt.Destroyed || owner.brokenPrefab == null) return;
        // Prevent the newly spawned pieces colliding with the intact body before Destroy is flushed.
        foreach (var shape in GetComponentsInChildren<Collider>()) shape.enabled = false;
        var debris = Instantiate(owner.brokenPrefab, transform.position, transform.rotation);
        debris.name = "Impact Debris";
        var mansionProp = GetComponent<MansionProp>();
        if (mansionProp != null)
        {
            debris.transform.localScale = transform.lossyScale;
            mansionProp.PrepareFracture(debris);
        }
        debris.SetActive(true);
        var velocity = GetComponent<Rigidbody>();
        foreach (var piece in debris.GetComponentsInChildren<Rigidbody>())
        {
            piece.isKinematic = false;
            if (velocity != null) piece.linearVelocity = velocity.linearVelocity;
            piece.AddExplosionForce(owner.explosionForce, transform.position, owner.explosionRadius);
        }
        Destroy(debris, Mathf.Max(0f, owner.debrisLifetime));
    }
    private void OnDestroy() { if (owner != null) owner.Damaged -= OnDamaged; }
}
