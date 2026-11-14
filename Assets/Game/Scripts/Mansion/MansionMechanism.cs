using UnityEngine;

/// <summary>A powered nudge for E; the leaf/drawer remains an actual jointed Rigidbody and can be grabbed.</summary>
public sealed class MansionMechanism : MonoBehaviour
{
    public HingeJoint hinge;
    public ConfigurableJoint drawer;
    public DamageOnImpact owner;
    public float openAngle = 100f;
    public float travel = .32f;
    public bool IsOpen { get; private set; }
    public void Toggle()
    {
        IsOpen = !IsOpen;
        if (hinge != null)
        {
            var spring = hinge.spring;
            spring.spring = 160f; spring.damper = 26f;
            spring.targetPosition = IsOpen ? openAngle : 0f;
            hinge.spring = spring; hinge.useSpring = true;
            hinge.GetComponent<Rigidbody>().WakeUp();
        }
        if (drawer != null)
        {
            drawer.targetPosition = new Vector3(IsOpen ? -travel : 0f, 0f, 0f);
            drawer.GetComponent<Rigidbody>().WakeUp();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (owner == null || owner.State == null || owner.State.IsDestroyed) return;
        owner.ReceiveCollision(collision);
    }
}
