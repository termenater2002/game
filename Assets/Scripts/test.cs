using UnityEngine;

public class DamageOnImpact : MonoBehaviour
{
    public float maxHP = 100f;
    private float currentHP;

    public float impactThreshold = 5f; // Минимальная сила удара, чтобы начать получать урон

    private Renderer objRenderer;
    private Color originalColor;
    private Color damagedColor = Color.black; // Цвет при максимальных повреждениях

    private void Start()
    {
        currentHP = maxHP;

        // Находим Renderer объекта
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
            // Уменьшаем HP в зависимости от силы удара
            float damage = impactForce * 5f; // Можешь настроить множитель
            TakeDamage(damage);
        }
    }

    private void TakeDamage(float amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0f);

        UpdateColor();

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

    private void BreakObject()
    {
        Debug.Log(gameObject.name + " сломан!");

        // Можно заменить Destroy на что-то красивее — например, спавнить сломанные куски
        Destroy(gameObject);
    }
}
