using UnityEngine;

/// <summary>F6 observes the live player from the side; movement and interaction stay in their original controllers.</summary>
[DefaultExecutionOrder(100)]
public class SideViewTestCamera : MonoBehaviour
{
    public Camera firstPersonCamera;
    public Camera sideCamera;
    public bool SideViewActive { get; private set; }
    public bool viewFromLeft;
    private bool originalFirstPersonEnabled;
    private string originalSideTag = "Untagged";
    private bool initialized;
    private CapsuleCollider capsule;
    private void Awake()
    {
        capsule = GetComponent<CapsuleCollider>();
        if (firstPersonCamera == null || sideCamera == null) { enabled = false; return; }
        originalFirstPersonEnabled = firstPersonCamera.enabled; originalSideTag = sideCamera.tag;
        initialized = true;
        sideCamera.enabled = false;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6)) SetSideView(!SideViewActive);
        if (Input.GetKeyDown(KeyCode.F7)) viewFromLeft = !viewFromLeft;
    }
    public void SetSideView(bool active)
    {
        if (!Application.isPlaying || !initialized || firstPersonCamera == null || sideCamera == null) return;
        SideViewActive = active;
        firstPersonCamera.enabled = active ? false : originalFirstPersonEnabled;
        sideCamera.tag = active ? "MainCamera" : originalSideTag;
        sideCamera.enabled = active;
        UpdatePose();
    }
    private void LateUpdate() { if (SideViewActive) UpdatePose(); }
    private void UpdatePose()
    {
        if (sideCamera == null || capsule == null) return;
        float height = Mathf.Max(.1f, capsule.bounds.size.y);
        Vector3 center = capsule.bounds.center;
        sideCamera.transform.position = center + transform.right * height * (viewFromLeft ? -3f : 3f) + Vector3.up * height * .12f;
        sideCamera.transform.LookAt(center);
        sideCamera.orthographic = true; sideCamera.orthographicSize = height * .72f;
        sideCamera.nearClipPlane = Mathf.Max(.003f, height * .015f);
    }
    private void OnDisable()
    {
        if (!initialized) return;
        if (firstPersonCamera != null) firstPersonCamera.enabled = originalFirstPersonEnabled;
        if (sideCamera != null) { sideCamera.enabled = false; if (!string.IsNullOrEmpty(originalSideTag)) sideCamera.tag = originalSideTag; }
        SideViewActive = false;
    }
}
