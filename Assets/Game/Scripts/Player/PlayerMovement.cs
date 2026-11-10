using UnityEngine;

/// <summary>Optional CharacterController alternative. Disabled in the demo.</summary>
public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 6f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;
    private Vector3 velocity;
    private Animator animator;
    private bool hasSpeedParameter;
    private void Start()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        var physical = GetComponent<FunnyMovement>();
        if (controller == null || (physical != null && physical.enabled))
        {
            Debug.LogError("PlayerMovement needs CharacterController and cannot run with FunnyMovement.", this);
            enabled = false;
            return;
        }
        animator = GetComponentInChildren<Animator>();
        if (animator != null)
            foreach (var parameter in animator.parameters)
                if (parameter.name == "Speed" && parameter.type == AnimatorControllerParameterType.Float) hasSpeedParameter = true;
    }
    private void Update()
    {
        bool captured = Cursor.lockState == CursorLockMode.Locked && Application.isFocused;
        bool grounded = groundCheck != null
            ? Physics.CheckSphere(groundCheck.position, groundDistance, groundMask, QueryTriggerInteraction.Ignore)
            : controller.isGrounded;
        if (grounded && velocity.y < 0f) velocity.y = -2f;
        Vector2 input = captured ? Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f) : Vector2.zero;
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        if (hasSpeedParameter) animator.SetFloat("Speed", move.magnitude);
        if (captured && grounded && Input.GetButtonDown("Jump")) velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        velocity.y += gravity * Time.deltaTime;
        controller.Move((move * speed + velocity) * Time.deltaTime);
    }
}
