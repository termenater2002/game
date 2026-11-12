using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class MansionSession : MonoBehaviour
{
    public Rigidbody player;
    public Vector3 spawnPosition;
    public float spawnYaw;
    private void Update()
    {
        if (player != null && player.position.y < -8f) Respawn();
        if (Cursor.lockState == CursorLockMode.Locked && Application.isFocused && Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(gameObject.scene.path);
    }
    public void Respawn()
    {
        if (player == null) return;
        var grabber = player.GetComponentInChildren<ObjectGrabber>(); if (grabber != null) grabber.Release();
        player.linearVelocity = Vector3.zero; player.angularVelocity = Vector3.zero;
        player.position = spawnPosition; player.rotation = Quaternion.Euler(0, spawnYaw, 0);
        var camera = player.GetComponentInChildren<FirstPersonCamera>(); if (camera != null) camera.SetLookAngles(0, spawnYaw);
    }
}
