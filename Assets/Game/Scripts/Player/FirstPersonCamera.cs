using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    [Range(1f, 89f)] public float pitchLimit = 85f;
    public bool captureOnStart = true;
    private float pitch;
    private float yaw;
    private Rigidbody body;
    public int CaptureFrame { get; private set; } = -1;

    private void Awake()
    {
        if (playerBody == null) playerBody = transform.parent;
        if (playerBody == null) { Debug.LogError("Assign the camera player body.", this); enabled = false; return; }
        body = playerBody.GetComponent<Rigidbody>();
        yaw = playerBody.eulerAngles.y;
        pitch = Mathf.DeltaAngle(0f, transform.localEulerAngles.x);
    }
    private void Start() { if (captureOnStart) CaptureCursor(); }
    public void CaptureCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        CaptureFrame = Time.frameCount;
    }
    public void ReleaseCursor() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { ReleaseCursor(); return; }
        if (!Application.isFocused) return;
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetMouseButtonDown(0)) CaptureCursor();
            return;
        }
        // Mouse axes contain frame deltas. Preserve the former sensitivity at 60 FPS.
        float sensitivity = Mathf.Max(0f, mouseSensitivity) / 60f;
        yaw += Input.GetAxisRaw("Mouse X") * sensitivity;
        pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * sensitivity, -pitchLimit, pitchLimit);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        if (body == null) playerBody.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
    private void FixedUpdate()
    {
        if (body != null && Cursor.lockState == CursorLockMode.Locked)
            body.MoveRotation(Quaternion.Euler(0f, yaw, 0f));
    }
    private void OnApplicationFocus(bool focused) { if (!focused) ReleaseCursor(); }
    private void OnDisable() { ReleaseCursor(); }
}
