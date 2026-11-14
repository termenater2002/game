using UnityEngine;

[RequireComponent(typeof(ObjectGrabber))]
public sealed class MansionInteraction : MonoBehaviour
{
    public float strikeDistance = 2.6f;
    public float strikeDamage = 34f;
    public float strikeImpulse = 5f;
    public float cooldown = .28f;
    private float readyAt;
    private ObjectGrabber grabber;
    private FirstPersonCamera look;
    public DamageOnImpact LastStruck { get; private set; }
    private void Awake() { grabber = GetComponent<ObjectGrabber>(); look = GetComponent<FirstPersonCamera>(); }
    private void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked || !Application.isFocused || (look != null && look.CaptureFrame == Time.frameCount)) return;
        if (Input.GetMouseButtonDown(1))
        {
            if (grabber.IsHolding) ThrowHeld();
            else Strike();
        }
        if (Input.GetKeyDown(KeyCode.E)) Use();
    }
    public bool ThrowHeld()
    {
        if (!grabber.IsHolding) return false;
        return grabber.Throw(grabber.throwImpulse);
    }
    public bool Strike()
    {
        if (Time.time < readyAt) return false;
        if (!Physics.Raycast(transform.position, transform.forward, out var hit, strikeDistance, ~0, QueryTriggerInteraction.Ignore)) return false;
        var wall = hit.collider.GetComponentInParent<VoxelWall>();
        if (wall != null)
        {
            readyAt = Time.time + cooldown;
            return wall.DamageAt(hit.point, hit.normal, 22f);
        }
        var damage = hit.collider.GetComponentInParent<DamageOnImpact>();
        if (damage == null && hit.rigidbody != null)
        { var fabric = FlexibleFabric.ForBody(hit.rigidbody); if (fabric != null) damage = fabric.GetComponent<DamageOnImpact>(); }
        if (damage == null) return false;
        readyAt = Time.time + cooldown; LastStruck = damage;
        if (hit.rigidbody != null && !hit.rigidbody.isKinematic)
        { hit.rigidbody.WakeUp(); hit.rigidbody.AddForceAtPosition(transform.forward * strikeImpulse, hit.point, ForceMode.Impulse); }
        damage.ApplyDamageAt(strikeDamage / Mathf.Sqrt(Mathf.Max(1, damage.materialToughnessScale)), hit.point, hit.normal);
        return true;
    }
    public bool Use()
    {
        if (!Physics.Raycast(transform.position, transform.forward, out var hit, strikeDistance, ~0, QueryTriggerInteraction.Ignore)) return false;
        var mechanism = hit.collider.GetComponentInParent<MansionMechanism>();
        if (mechanism == null) return false;
        mechanism.Toggle(); return true;
    }
}
