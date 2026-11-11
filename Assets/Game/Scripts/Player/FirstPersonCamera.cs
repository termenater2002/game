using UnityEngine;

[DefaultExecutionOrder(-50)]
public class FirstPersonCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    [Range(1f, 89f)] public float pitchLimit = 85f;
    public bool captureOnStart = true;
    public bool fitBodyView = true;
    public bool gaitMotion = true;
    [Range(0f, .03f)] public float stepHeightFraction = .012f;
    [Range(0f, .02f)] public float stepSwayFraction = .004f;
    private float pitch;
    private float yaw;
    private Rigidbody body;
    private PlayerAnimationDriver animationDriver;
    private CapsuleCollider capsule;
    private Vector3 restPosition;
    private bool initialized;
    public Vector3 GaitOffset { get; private set; }
    public float Pitch => pitch;
    public float Yaw => yaw;
    public int CaptureFrame { get; private set; } = -1;

    private void Awake()
    {
        if (playerBody == null) playerBody = transform.parent;
        if (playerBody == null) { Debug.LogError("Assign the camera player body.", this); enabled = false; return; }
        body = playerBody.GetComponent<Rigidbody>();
        animationDriver = playerBody.GetComponent<PlayerAnimationDriver>(); capsule = playerBody.GetComponent<CapsuleCollider>();
        if (fitBodyView) FitBodyView();
        restPosition = transform.localPosition; initialized = true;
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
    public void SetLookAngles(float pitchDegrees, float yawDegrees)
    {
        pitch = Mathf.Clamp(pitchDegrees, -pitchLimit, pitchLimit); yaw = yawDegrees;
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        if (body != null) body.rotation = Quaternion.Euler(0f, yaw, 0f);
        else if (playerBody != null) playerBody.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
    public void FitBodyView()
    {
        if (playerBody == null) playerBody = transform.parent;
        if (playerBody == null) return;
        var shape = playerBody.GetComponent<CapsuleCollider>(); var view = GetComponent<Camera>();
        if (shape == null || view == null) return;
        Vector3 eye = shape.center + new Vector3(0f, shape.height * .42f, shape.radius * .6f);
        transform.position = playerBody.TransformPoint(eye);
        view.nearClipPlane = Mathf.Max(.001f, shape.height * Mathf.Abs(playerBody.lossyScale.y) * .015f);
        restPosition = transform.localPosition;
    }
    private void LateUpdate()
    {
        if (!initialized) return;
        Vector3 target = Vector3.zero;
        var animator = animationDriver != null ? animationDriver.CharacterAnimator : null;
        if (gaitMotion && body != null && capsule != null && animationDriver != null && animationDriver.Grounded && animator != null)
        {
            var state = animator.GetCurrentAnimatorStateInfo(0);
            float height = capsule.bounds.size.y;
            float speed = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude;
            float weight = Mathf.InverseLerp(height * .03f, height * .45f, speed);
            if (state.IsName(PlayerAnimationDriver.LocomotionState))
            {
                float phase = (state.normalizedTime % 1f) * Mathf.PI * 2f;
                Vector3 worldSize = new Vector3(Mathf.Sin(phase) * height * stepSwayFraction, -Mathf.Cos(phase * 2f) * height * stepHeightFraction, 0f) * weight;
                // Amplitudes are defined in metres, even for the existing 0.1-scale player.
                target = transform.parent.InverseTransformVector(playerBody.TransformDirection(worldSize));
            }
        }
        GaitOffset = Vector3.Lerp(GaitOffset, target, 1f - Mathf.Exp(-12f * Time.deltaTime));
        transform.localPosition = restPosition + GaitOffset;
    }
    private void OnApplicationFocus(bool focused) { if (!focused) ReleaseCursor(); }
    private void OnDisable() { ReleaseCursor(); if (initialized) transform.localPosition = restPosition; GaitOffset = Vector3.zero; }
}
