using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FunnyMovement : MonoBehaviour
{
    public float moveForce = 10f;
    public float jumpForce = 5f;
    public float maxSpeed = 5f;
    [Min(.01f)] public float walkSpeed = 2f;
    [Min(.01f)] public float sprintSpeed = 5f;
    public bool matchAnimationSpeeds = true;
    [Range(.03f, .5f)] public float accelerationSeconds = .12f;
    [Range(.03f, .5f)] public float stoppingSeconds = .12f;
    public Transform groundCheck;
    public float groundRayLength = 0.6f;
    public LayerMask groundMask;
    [Min(0f)] public float jumpBufferSeconds = 0.12f;
    private Rigidbody body;
    private Vector2 movement;
    private bool sprinting;
    private PlayerAnimationDriver animationDriver;
    private CapsuleCollider capsule;
    private PhysicsMaterial originalMaterial;
    private PhysicsMaterial locomotionMaterial;
    private readonly RaycastHit[] groundHits = new RaycastHit[16];
    private readonly JumpRequest jump = new JumpRequest();
    public bool IsGrounded { get; private set; }
    private PlayerAnimationDriver AnimationDriver => animationDriver != null ? animationDriver : (animationDriver = GetComponent<PlayerAnimationDriver>());
    public float WalkSpeed => matchAnimationSpeeds && AnimationDriver != null ? AnimationDriver.walkReferenceSpeed : walkSpeed;
    public float SprintSpeed => matchAnimationSpeeds && AnimationDriver != null ? AnimationDriver.runReferenceSpeed : sprintSpeed;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        animationDriver = GetComponent<PlayerAnimationDriver>(); capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            originalMaterial = capsule.sharedMaterial;
            locomotionMaterial = originalMaterial != null ? Instantiate(originalMaterial) : new PhysicsMaterial("Player controlled ground movement");
            locomotionMaterial.hideFlags = HideFlags.DontSave;
            locomotionMaterial.staticFriction = 0f; locomotionMaterial.dynamicFriction = 0f;
            locomotionMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
            capsule.sharedMaterial = locomotionMaterial;
        }
        body.constraints |= RigidbodyConstraints.FreezeRotation;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        var alternative = GetComponent<PlayerMovement>();
        if (alternative != null && alternative.enabled)
        {
            Debug.LogError("Choose one movement component; FunnyMovement is being disabled.", this);
            enabled = false;
        }
    }
    private void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked || !Application.isFocused)
        {
            SetMoveInput(Vector2.zero);
            jump.Clear();
            return;
        }
        SetMoveInput(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        if (Input.GetButtonDown("Jump")) jump.Queue(Time.time, jumpBufferSeconds);
    }
    private void FixedUpdate()
    {
        IsGrounded = ProbeGround();
        Vector3 direction = transform.right * movement.x + transform.forward * movement.y;
        Vector3 horizontal = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up);
        float speed = Mathf.Min(Mathf.Max(.01f, maxSpeed), Mathf.Max(.01f, sprinting ? SprintSpeed : WalkSpeed));
        if (IsGrounded && body.linearVelocity.y <= .2f)
        {
            float acceleration = direction.sqrMagnitude > 0f
                ? Mathf.Min(Mathf.Max(0f, moveForce), speed / Mathf.Max(.03f, accelerationSeconds))
                : Mathf.Max(.01f, SprintSpeed) / Mathf.Max(.03f, stoppingSeconds);
            Vector3 next = Vector3.MoveTowards(horizontal, direction * speed, acceleration * Time.fixedDeltaTime);
            body.AddForce((next - horizontal) / Time.fixedDeltaTime, ForceMode.Acceleration);
        }
        else if (direction.sqrMagnitude > 0f)
        {
            // Input cannot boost a normal jump beyond the selected gait. Existing
            // impulses keep their momentum; vertical velocity is never rewritten.
            Vector3 axis = direction.normalized;
            float remaining = Mathf.Max(0f, speed * movement.magnitude - Vector3.Dot(horizontal, axis));
            float change = Mathf.Min(remaining, Mathf.Max(0f, moveForce) * movement.magnitude * Time.fixedDeltaTime);
            body.AddForce(axis * change / Time.fixedDeltaTime, ForceMode.Acceleration);
        }
        if (jump.TryConsume(IsGrounded && body.linearVelocity.y <= 0.1f, Time.time))
            body.AddForce(Vector3.up * Mathf.Max(0f, jumpForce), ForceMode.Impulse);
    }
    public void SetMoveInput(Vector2 input, bool sprint = false)
    {
        movement = Vector2.ClampMagnitude(input, 1f); sprinting = sprint;
    }
    private bool ProbeGround()
    {
        if (capsule == null)
        {
            Vector3 origin = groundCheck != null ? groundCheck.position : transform.position + Vector3.up * .1f;
            return Physics.Raycast(origin, Vector3.down, groundRayLength, groundMask, QueryTriggerInteraction.Ignore);
        }
        var bounds = capsule.bounds; float skin = Mathf.Max(.006f, bounds.size.y * .08f);
        var start = new Vector3(bounds.center.x, bounds.min.y + skin, bounds.center.z);
        int count = Physics.RaycastNonAlloc(start, Vector3.down, groundHits, skin * 2f, groundMask, QueryTriggerInteraction.Ignore);
        for (int i=0;i<count;i++) if (!groundHits[i].transform.IsChildOf(transform) && groundHits[i].normal.y>.5f) return true;
        return false;
    }
    private void OnDisable() { movement = Vector2.zero; jump.Clear(); }
    private void OnDestroy()
    {
        if (capsule != null && capsule.sharedMaterial == locomotionMaterial) capsule.sharedMaterial = originalMaterial;
        if (locomotionMaterial != null) { if (Application.isPlaying) Destroy(locomotionMaterial); else DestroyImmediate(locomotionMaterial); }
    }
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundRayLength);
    }
}
