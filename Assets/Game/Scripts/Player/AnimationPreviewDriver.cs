using UnityEngine;

/// <summary>Only attached in AnimationPreview: a physical treadmill showcase, not a gameplay controller.</summary>
public class AnimationPreviewDriver : MonoBehaviour
{
    public Camera previewCamera;
    public bool autoCycle = true;
    [Range(0, 7)] public int mode;
    private Rigidbody body;
    private PlayerAnimationDriver animationDriver;
    private float clock;
    private int previousMode = -1;
    private Vector3 origin;
    public int Cycle { get; private set; }
    private void Awake() { body = GetComponent<Rigidbody>(); animationDriver = GetComponent<PlayerAnimationDriver>(); origin = transform.position; }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) autoCycle = !autoCycle;
        for (int i = 0; i < 8; i++) if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i))) { autoCycle = false; mode = i; }
        if (autoCycle)
        {
            clock += Time.deltaTime;
            int next = Mathf.FloorToInt(clock / 1.6f);
            if (next >= 8) { clock = 0; next = 0; Cycle++; body.position = origin; body.linearVelocity = Vector3.zero; }
            mode = next;
        }
        if (mode != previousMode && mode == 6 && animationDriver.Grounded) Jump();
        if (mode != previousMode && mode == 7) { body.position += Vector3.up * 1.4f; body.linearVelocity = Vector3.zero; }
        previousMode = mode;
    }
    private void FixedUpdate()
    {
        float walk = animationDriver.walkReferenceSpeed, run = animationDriver.runReferenceSpeed;
        var desired = mode == 1 ? transform.forward * walk : mode == 2 ? transform.forward * run : mode == 3 ? -transform.forward * walk : mode == 4 ? -transform.right * walk : mode == 5 ? transform.right * walk : Vector3.zero;
        var horizontal = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up);
        body.AddForce(Vector3.ClampMagnitude(desired - horizontal, 10 * Time.fixedDeltaTime) / Time.fixedDeltaTime, ForceMode.Acceleration);
    }
    [ContextMenu("Restart automatic preview")]
    public void Restart()
    {
        autoCycle = true; clock = 0; mode = 0; previousMode = -1;
        if (body != null) { body.position = origin; body.linearVelocity = Vector3.zero; }
    }
    [ContextMenu("Jump preview")] public void Jump() { if (body != null) body.AddForce(Vector3.up * 4.2f, ForceMode.Impulse); }
    private void LateUpdate()
    {
        if (previewCamera == null) return;
        var pivot = transform.position + Vector3.up * .85f;
        previewCamera.transform.position = pivot + new Vector3(2.8f, 1.1f, 3.7f); previewCamera.transform.LookAt(pivot);
    }
}
