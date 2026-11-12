using UnityEngine;

public class ObjectGrabber : MonoBehaviour
{
    public float grabDistance = 3f;
    public float grabForce = 100f;
    public LayerMask grabbableMask;
    [Min(0f)] public float maxAcceleration = 30f;
    [Min(0f)] public float breakDistance = 3f;
    [Min(0f)] public float rotationDamping = 2f;
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
        if (body == null || body == playerBody || body.isKinematic) return false;
        target = body;
        point = hit.point;
        return true;
    }
    public bool TryGrab()
    {
        if (IsHolding || !TryGetTarget(out var target, out var point)) return false;
        heldBody = target;
        localPoint = heldBody.transform.InverseTransformPoint(point);
        heldDistance = Mathf.Clamp(Vector3.Distance(transform.position, point), .35f, Mathf.Max(.35f, grabDistance));
        previousTarget = transform.position + transform.forward * heldDistance;
        heldBody.WakeUp();
        return true;
    }
    public void Release() { heldBody = null; localPoint = Vector3.zero; heldDistance = 0f; }
    public bool Throw(float speed)
    {
        if (heldBody == null) return false;
        var thrown = heldBody;
        Release();
        thrown.WakeUp();
        thrown.AddForce(transform.forward * Mathf.Clamp(speed, 0f, 20f), ForceMode.VelocityChange);
        return true;
    }
    private void FixedUpdate()
    {
        if (heldBody == null) { Release(); return; }
        if (heldBody.isKinematic || !heldBody.gameObject.activeInHierarchy) { Release(); return; }
        Vector3 point = heldBody.transform.TransformPoint(localPoint);
        Vector3 target = transform.position + transform.forward * heldDistance;
        if (Vector3.Distance(point, target) > Mathf.Max(breakDistance, grabDistance)) { Release(); return; }
        Vector3 targetVelocity = Vector3.ClampMagnitude((target - previousTarget) / Time.fixedDeltaTime, 10f);
        previousTarget = target;
        float spring = Mathf.Max(0f, grabForce);
        float damping = 2f * Mathf.Sqrt(spring * heldBody.mass);
        Vector3 force = (target - point) * spring + (targetVelocity - heldBody.GetPointVelocity(point)) * damping;
        force = Vector3.ClampMagnitude(force, Mathf.Max(0f, maxAcceleration) * heldBody.mass);
        heldBody.AddForceAtPosition(force, point, ForceMode.Force);
        heldBody.AddTorque(-heldBody.angularVelocity * Mathf.Max(0f, rotationDamping), ForceMode.Acceleration);
    }
    private void OnDisable() { Release(); }
    private void OnDestroy() { Release(); }
}
