using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 6f;          // Скорость передвижения
    public float gravity = -9.81f;   // Сила гравитации
    public float jumpHeight = 1.5f;  // Высота прыжка

    public Transform groundCheck;    // Объект для проверки земли
    public float groundDistance = 0.2f; // Радиус проверки земли
    public LayerMask groundMask;     // Слой земли

    private Vector3 velocity;        // Текущая скорость персонажа
    private bool isGrounded;         // Находится ли персонаж на земле
    private Animator animator;       // Ссылка на Animator

    void Start()
    {
        // Получаем компонент Animator
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Проверка, находится ли персонаж на земле
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Сброс скорости падения
        }

        // Движение по осям X и Z (WASD/стрелки)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // Установка параметра Speed в зависимости от движения
        animator.SetFloat("Speed", move.magnitude);

        // Прыжок
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Применение гравитации
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}