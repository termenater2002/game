using UnityEngine;

public class ObjectGrabber : MonoBehaviour
{
    public float grabDistance = 3f;
    public float grabForce = 100f;
    public LayerMask grabbableMask;
    [Min(0f)] public float maxAcceleration = 30f;
    [Min(0f)] public float breakDistance = 3f;
    [Min(0f)] public float rotationDamping = 2f;
    [Min(1f)] public float maximumGrabMass = 160f;
    [Min(.1f)] public float maximumLiftMass = 12f;
    [Min(1f)] public float maximumForce = 280f;
    [Min(.1f)] public float throwImpulse = 18f;
    [Min(.1f)] public float maximumThrowSpeed = 14f;
    private readonly RaycastHit[] obstructionHits = new RaycastHit[16];
    private Rigidbody heldBody;
    private Vector3 localPoint;
    private Vector3 previousTarget;
    private float heldDistance;
    private FirstPersonCamera look;
    private Rigidbody playerBody;
    public Rigidbody HeldBody => heldBody;
    public bool IsHolding => heldBody != null;
    public float HeldDistance => heldDistance;
    private void Awake()
    {
        look = GetComponent<FirstPersonCamera>();
        playerBody = GetComponentInParent<Rigidbody>();
    }
    private void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked || !Application.isFocused) { Release(); return; }
        if (look != null && look.CaptureFrame == Time.frameCount) return;
        if (Input.GetMouseButtonDown(0))
        {
            if (IsHolding) Release(); else TryGrab();
        }
    }
    public bool TryGetTarget(out Rigidbody target, out Vector3 point)
    {
        target = null;
        point = Vector3.zero;
        if (!Physics.Raycast(transform.position, transform.forward, out var hit, Mathf.Max(0f, grabDistance), grabbableMask, QueryTriggerInteraction.Ignore)) return false;
        var body = hit.rigidbody;
        if (body == null || body == playerBody || body.mass > maximumGrabMass) return false;
        if (body.isKinematic)
        {
            var item = body.GetComponent<IndependentPart>();
            var cloth = FlexibleFabric.ForBody(body);
            if ((item == null || !item.contained) && (cloth == null || !cloth.IsFrozen)) return false;
        }
        target = body;
        point = hit.point;
        return true;
    }
    public bool TryGrab()
    {
        if (IsHolding || !TryGetTarget(out var target, out var point)) return false;
        if (target.isKinematic)
        {
            var item = target.GetComponent<IndependentPart>();
            if (item != null && item.contained) item.Release(); else FlexibleFabric.ForBody(target).Wake();
        }
        heldBody = target;
        localPoint = heldBody.transform.InverseTransformPoint(point);
        heldDistance = Mathf.Clamp(Vector3.Distance(transform.position, point), .35f, Mathf.Max(.35f, grabDistance));
        previousTarget = transform.position + transform.forward * heldDistance;
        heldBody.WakeUp();
        return true;
    }
    public void Release() { heldBody = null; localPoint = Vector3.zero; heldDistance = 0f; }
    public bool Throw(float impulse)
    {
        if (heldBody == null) return false;
        var thrown = heldBody;
        var point = thrown.transform.TransformPoint(localPoint);
        Release();
        thrown.WakeUp();
        float availableSpeed = Mathf.Max(0, maximumThrowSpeed - Vector3.Dot(thrown.linearVelocity, transform.forward));
        float strength = Mathf.Clamp(impulse, 0f, availableSpeed * thrown.mass);
        thrown.AddForceAtPosition(transform.forward * strength, point, ForceMode.Impulse);
        return true;
    }
    private void FixedUpdate()
    {
        if (heldBody == null) { Release(); return; }
        if (heldBody.isKinematic || !heldBody.gameObject.activeInHierarchy) { Release(); return; }
        Vector3 point = heldBody.transform.TransformPoint(localPoint);
        Vector3 target = transform.position + transform.forward * heldDistance;
        // The ray stops the spring target at walls; the body itself retains all of its collisions.
        Vector3 toward = target - transform.position; float allowed = toward.magnitude;
        int count = Physics.RaycastNonAlloc(transform.position, toward.normalized, obstructionHits, allowed, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            var hit = obstructionHits[i];
            if (hit.rigidbody == heldBody || playerBody != null && hit.rigidbody == playerBody || hit.transform.IsChildOf(heldBody.transform)) continue;
            allowed = Mathf.Min(allowed, Mathf.Max(.15f, hit.distance - .08f));
        }
        target = transform.position + toward.normalized * allowed;
        if (heldBody.mass > maximumLiftMass) target.y = Mathf.Min(target.y, point.y + .1f);
        if (Vector3.Distance(point, target) > Mathf.Max(breakDistance, grabDistance)) { Release(); return; }
        Vector3 targetVelocity = Vector3.ClampMagnitude((target - previousTarget) / Time.fixedDeltaTime, 3f);
        previousTarget = target;
        float spring = Mathf.Max(0f, grabForce) / Mathf.Sqrt(Mathf.Max(1f, heldBody.mass / 8f));
        float damping = 2f * Mathf.Sqrt(spring * heldBody.mass);
        Vector3 force = (target - point) * spring + (targetVelocity - heldBody.GetPointVelocity(point)) * damping;
        if (heldBody.mass <= maximumLiftMass && heldBody.useGravity) force -= Physics.gravity * heldBody.mass;
        force = Vector3.ClampMagnitude(force, Mathf.Min(maximumForce, Mathf.Max(0f, maxAcceleration) * heldBody.mass));
        force.y = Mathf.Min(force.y, Mathf.Max(0, -Physics.gravity.y) * maximumLiftMass);
        heldBody.AddForceAtPosition(force, point, ForceMode.Force);
        heldBody.AddTorque(-heldBody.angularVelocity * Mathf.Max(0f, rotationDamping), ForceMode.Acceleration);
    }
    private void OnDisable() { Release(); }
    private void OnDestroy() { Release(); }
}
