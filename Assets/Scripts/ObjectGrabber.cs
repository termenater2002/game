using UnityEngine;

public class ObjectGrabber : MonoBehaviour
{
    public float grabDistance = 3f;
    public float grabForce = 100f;
    public LayerMask grabbableMask;

    private Rigidbody grabbedObject;
    private Transform grabPoint;

    void Start()
    {
        grabPoint = new GameObject("GrabPoint").transform;
        grabPoint.SetParent(transform); // прикрепляем к камере
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // ЛКМ — хватать/отпускать
        {
            if (grabbedObject == null)
                TryGrab();
            else
                Release();
        }

        if (grabbedObject != null)
        {
            MoveObject();
        }
    }

    void TryGrab()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, grabbableMask))
        {
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
            {
                grabbedObject = rb;
                grabPoint.position = hit.point;
            }
        }
    }

    void Release()
    {
        grabbedObject = null;
    }

    void MoveObject()
    {
        Vector3 targetPos = transform.position + transform.forward * grabDistance;
        Vector3 direction = (targetPos - grabbedObject.position);

        float distance = direction.magnitude;
        direction.Normalize();

        // Применяем силу к объекту, чтобы тянуть его к руке
        grabbedObject.AddForce(direction * grabForce * distance, ForceMode.Force);

        // Ограничим максимальную скорость, чтобы не разгонялся бесконечно
        grabbedObject.linearVelocity = Vector3.ClampMagnitude(grabbedObject.linearVelocity, 10f);
    }


}
