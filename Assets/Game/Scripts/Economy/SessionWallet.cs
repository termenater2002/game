using UnityEngine;

[DefaultExecutionOrder(-100)]
public class SessionWallet : MonoBehaviour
{
    public static SessionWallet Current { get; private set; }
    public SessionEarnings Earnings { get; } = new SessionEarnings();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { Current = null; }
    private void Awake()
    {
        if (Current != null && Current != this)
        {
            Debug.LogError("Only one SessionWallet should be active in the demo.", this);
            enabled = false;
            return;
        }
        Current = this;
        Earnings.Reset();
    }
    private void OnDestroy() { if (Current == this) Current = null; }
}
