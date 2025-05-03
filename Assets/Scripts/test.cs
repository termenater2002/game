using UnityEngine;

public class DamageOnImpact : MonoBehaviour
{
    [Header("Прочность объекта")]
    public float maxHP = 100f;
    private float currentHP;
    public float damageMultiplier = 1f; // Новый множитель урона

    [Header("Сила столкновения")]
    public float impactThreshold = 5f;

    [Header("Визуализация повреждений")]
    private Renderer objRenderer;
    private Color originalColor;
    private Color damagedColor = Color.black;

    [Header("Разрушение")]
    public GameObject brokenPrefab;
    public float explosionForce = 300f;
    public float explosionRadius = 2f;
    public float debrisLifetime = 5f;

    [Header("Экономика")]
    public float objectValue = 100f; // Цена объекта
    private float totalDamageDealt = 0f; // Накопленный урон

    private void Start()
    {
        currentHP = maxHP;

        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null)
        {
            originalColor = objRenderer.material.color;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce >= impactThreshold)
        {
            float baseDamage = impactForce * 5f;
            float finalDamage = baseDamage * damageMultiplier;
            TakeDamage(finalDamage);
        }
    }

    private void TakeDamage(float amount)
    {
        float previousHP = currentHP;

        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0f);

        float damageThisHit = previousHP - currentHP;
        totalDamageDealt += damageThisHit;

        UpdateColor();

        AwardMoneyForDamage(damageThisHit);

        if (currentHP <= 0f)
        {
            BreakObject();
        }
    }

    private void UpdateColor()
    {
        if (objRenderer != null)
        {
            float hpPercent = currentHP / maxHP;
            objRenderer.material.color = Color.Lerp(damagedColor, originalColor, hpPercent);
        }
    }

    private void AwardMoneyForDamage(float damage)
    {
        float percent = damage / maxHP;
        float reward = percent * objectValue;

        // Заглушка — в будущем здесь будет реальная система
        Debug.Log("💰 Получено денег за повреждение: " + Mathf.RoundToInt(reward));
    }

    private void BreakObject()
    {
        if (brokenPrefab != null)
        {
            GameObject broken = Instantiate(brokenPrefab, transform.position, transform.rotation);

            foreach (Rigidbody piece in broken.GetComponentsInChildren<Rigidbody>())
            {
                Vector3 explosionPos = transform.position;
                piece.AddExplosionForce(explosionForce, explosionPos, explosionRadius);
            }

            Destroy(broken, debrisLifetime);
        }

        Destroy(gameObject);
    }
}
