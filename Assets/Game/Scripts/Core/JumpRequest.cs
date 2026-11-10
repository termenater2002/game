/// <summary>A single press survives Update/FixedUpdate and can be consumed only once.</summary>
public sealed class JumpRequest
{
    private bool pending;
    private float expiresAt;
    public void Queue(float now, float bufferSeconds) { pending = true; expiresAt = now + System.Math.Max(0f, bufferSeconds); }
    public bool TryConsume(bool grounded, float now)
    {
        if (!pending) return false;
        if (now > expiresAt) { Clear(); return false; }
        if (!grounded) return false;
        Clear();
        return true;
    }
    public void Clear() { pending = false; }
}
