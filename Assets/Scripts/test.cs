using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    // Этот метод вызывается, когда объект сталкивается с чем-то
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Столкнулся с объектом: " + collision.gameObject.name);
        
        // Пример действия: уничтожить другой объект при столкновении
        // Destroy(collision.gameObject);
    }

    // Этот метод вызывается, если объект "входит" в триггер
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Вошёл в триггер: " + other.gameObject.name);
    }
}