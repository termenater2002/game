using UnityEngine;

/// <summary>Animator callbacks live with the visual rig, while the physics driver stays on the player root.</summary>
public class PlayerAnimationIKRelay : MonoBehaviour
{
    private PlayerAnimationDriver driver;
    private void Awake() { driver = GetComponentInParent<PlayerAnimationDriver>(); }
    private void OnAnimatorIK(int layer) { if (driver != null && driver.enabled && driver.gameObject != gameObject) driver.ApplyIK(layer); }
}
