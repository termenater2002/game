using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FunnyMovement : MonoBehaviour
{
    public float moveForce = 10f;
    public float jumpForce = 5f;
    public float maxSpeed = 5f;
    public Transform groundCheck;
    public float groundRayLength = 0.6f;
    public LayerMask groundMask;
    [Min(0f)] public float jumpBufferSeconds = 0.12f;
    private Rigidbody body;
    private Vector2 movement;
    private readonly JumpRequest jump = new JumpRequest();
    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
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
            movement = Vector2.zero;
            jump.Clear();
            return;
        }
        movement = Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);
        if (Input.GetButtonDown("Jump")) jump.Queue(Time.time, jumpBufferSeconds);
    }
    private void FixedUpdate()
    {
        Vector3 origin = groundCheck != null ? groundCheck.position : transform.position + Vector3.up * 0.1f;
        IsGrounded = Physics.Raycast(origin, Vector3.down, groundRayLength, groundMask, QueryTriggerInteraction.Ignore);
        Vector3 direction = transform.right * movement.x + transform.forward * movement.y;
        if (direction.sqrMagnitude > 0f)
        {
            Vector3 horizontal = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up);
            Vector3 next = Vector3.ClampMagnitude(horizontal + direction * Mathf.Max(0f, moveForce) * Time.fixedDeltaTime, Mathf.Max(0f, maxSpeed));
            body.AddForce((next - horizontal) / Time.fixedDeltaTime, ForceMode.Acceleration);
        }
        if (jump.TryConsume(IsGrounded && body.linearVelocity.y <= 0.1f, Time.time))
            body.AddForce(Vector3.up * Mathf.Max(0f, jumpForce), ForceMode.Impulse);
    }
    private void OnDisable() { movement = Vector2.zero; jump.Clear(); }
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundRayLength);
    }
}
