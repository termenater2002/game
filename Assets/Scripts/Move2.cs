using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FunnyMovement : MonoBehaviour
{
    public float moveForce = 10f;       // Сила движения
    public float jumpForce = 5f;        // Сила прыжка
    public float maxSpeed = 5f;         // Ограничение скорости

    public Transform groundCheck;       // Пустышка под персонажем
    public float groundRayLength = 0.6f;// Длина луча до земли
    public LayerMask groundMask;        // Что считается землёй

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Чтобы не крутился от движения
    }

    void FixedUpdate()
    {
        // Проверка на землю через Raycast от точки groundCheck вниз
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundRayLength, groundMask);

        // Движение
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDir = (transform.right * horizontal + transform.forward * vertical).normalized;

        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(moveDir * moveForce, ForceMode.Acceleration);
        }

        // Прыжок
        if (isGrounded && Input.GetButton("Jump"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundRayLength);
    }
}
