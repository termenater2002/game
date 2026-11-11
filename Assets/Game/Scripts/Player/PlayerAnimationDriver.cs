using UnityEngine;

/// <summary>Presentation only: samples physics, never writes the Rigidbody or the player root.</summary>
[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerAnimationDriver : MonoBehaviour
{
    public Animator characterAnimator;
    public ObjectGrabber grabber;
    public float walkReferenceSpeed = .16f;
    public float runReferenceSpeed = .4f;
    [Min(.01f)] public float blendSpaceScale = 1f;
    [Range(.01f, .3f)] public float damping = .08f;
    [Range(1f, 4f)] public float maximumPlaybackRate = 2.5f;
    public LayerMask groundMask = ~0;
    [Range(.01f, .12f)] public float footSoleFraction = .025f;
    [Range(.1f, .4f)] public float holdingReachFraction = .33f;
    private Animator animator;
    private Rigidbody body;
    private CapsuleCollider capsule;
    private float airborneTime;
    private bool wasGrounded;
    private float holdingWeight;
    private float previousYaw;
    private bool ready;
    public bool Grounded { get; private set; }
    public Vector3 LocalVelocity { get; private set; }
    public float HoldingWeight => holdingWeight;
    public int LandingCount { get; private set; }
    public Animator CharacterAnimator => animator != null ? animator : characterAnimator;
    public const string LocomotionState = "Base Layer.Locomotion";
    static readonly int X = Animator.StringToHash("MoveX"), Z = Animator.StringToHash("MoveZ"),
        Speed = Animator.StringToHash("Speed"), Vertical = Animator.StringToHash("VerticalSpeed"),
        Ground = Animator.StringToHash("Grounded"), Holding = Animator.StringToHash("Holding"),
        Rate = Animator.StringToHash("PlaybackRate"), Turn = Animator.StringToHash("TurnRate");
    private readonly RaycastHit[] groundHits = new RaycastHit[16];

    private void Awake()
    {
        animator = characterAnimator != null ? characterAnimator : GetComponent<Animator>(); body = GetComponent<Rigidbody>(); capsule = GetComponent<CapsuleCollider>();
        if (grabber == null) grabber = GetComponentInChildren<ObjectGrabber>();
        previousYaw = transform.eulerAngles.y;
        if (animator == null) { Debug.LogError("Assign the visual Humanoid Animator.", this); enabled = false; return; }
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        ready = animator.avatar != null && animator.avatar.isValid && animator.isHuman && animator.runtimeAnimatorController != null;
        if (!ready) { Debug.LogError("Player animation requires a valid Humanoid avatar and controller.", this); enabled = false; }
    }
    private bool ProbeGround()
    {
        if (body.linearVelocity.y > .2f) return false;
        var bounds = capsule.bounds;
        float skin = Mathf.Max(.006f, bounds.size.y * .08f);
        Vector3 start = new Vector3(bounds.center.x, bounds.min.y + skin, bounds.center.z);
        int count = Physics.RaycastNonAlloc(start, Vector3.down, groundHits, skin * 2f, groundMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
            if (!groundHits[i].transform.IsChildOf(transform) && groundHits[i].normal.y > .5f) return true;
        return false;
    }
    private void Update()
    {
        if (!ready) return;
        LocalVelocity = transform.InverseTransformDirection(body.linearVelocity);
        float speed = new Vector2(LocalVelocity.x, LocalVelocity.z).magnitude;
        Grounded = ProbeGround();
        if (!Grounded) airborneTime += Time.deltaTime;
        else
        {
            if (!wasGrounded && airborneTime > .12f) LandingCount++;
            airborneTime = 0f;
        }
        wasGrounded = Grounded;
        var direction = Vector2.ClampMagnitude(new Vector2(LocalVelocity.x, LocalVelocity.z), runReferenceSpeed) * blendSpaceScale;
        animator.SetFloat(X, direction.x, damping, Time.deltaTime); animator.SetFloat(Z, direction.y, damping, Time.deltaTime);
        animator.SetFloat(Speed, speed, damping, Time.deltaTime);
        animator.SetFloat(Vertical, body.linearVelocity.y); animator.SetBool(Ground, Grounded);
        bool holding = grabber != null && grabber.IsHolding;
        animator.SetBool(Holding, holding);
        holdingWeight = Mathf.MoveTowards(holdingWeight, holding ? 1f : 0f, Time.deltaTime * 6f);
        // Backwards has only a walk clip. Match its cadence, without unbounded toy-scale strides.
        float reference = LocalVelocity.z < -.03f ? walkReferenceSpeed
            : Mathf.Clamp(speed, walkReferenceSpeed, Mathf.Max(walkReferenceSpeed, runReferenceSpeed));
        float rate = speed < .03f ? 1f : Mathf.Clamp(speed / Mathf.Max(.01f, reference), .4f, maximumPlaybackRate);
        animator.SetFloat(Rate, rate, damping, Time.deltaTime);
        float yaw = transform.eulerAngles.y;
        animator.SetFloat(Turn, Mathf.DeltaAngle(previousYaw, yaw) / Mathf.Max(.001f, Time.deltaTime), damping, Time.deltaTime); previousYaw = yaw;
    }
    private void OnAnimatorIK(int layer) { if (animator != null && animator.gameObject == gameObject) ApplyIK(layer); }
    public void ApplyIK(int layer)
    {
        if (!ready || layer != 0) return;
        float height = capsule.bounds.size.y;
        if (Grounded) { PlantFoot(AvatarIKGoal.LeftFoot, HumanBodyBones.LeftFoot, height); PlantFoot(AvatarIKGoal.RightFoot, HumanBodyBones.RightFoot, height); }
        else { animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0); animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0); }
        // The force grab holds objects beyond human reach. Hands indicate carrying; they never pull physics.
        Hand(AvatarIKGoal.LeftHand, -.105f, height);
        Hand(AvatarIKGoal.RightHand, .105f, height);
    }
    private void Hand(AvatarIKGoal goal, float side, float height)
    {
        var point = transform.position + transform.up * (height * .67f) + transform.forward * (height * holdingReachFraction) + transform.right * (height * side);
        animator.SetIKPositionWeight(goal, holdingWeight * .85f); animator.SetIKPosition(goal, point);
        animator.SetIKRotationWeight(goal, holdingWeight * .35f); animator.SetIKRotation(goal, transform.rotation * Quaternion.Euler(0, 0, goal == AvatarIKGoal.LeftHand ? 80 : -80));
    }
    private void PlantFoot(AvatarIKGoal goal, HumanBodyBones bone, float height)
    {
        var foot = animator.GetBoneTransform(bone); if (foot == null) return;
        float distance = height * .3f;
        if (Physics.Raycast(foot.position + Vector3.up * distance, Vector3.down, out var hit, distance * 1.5f, groundMask, QueryTriggerInteraction.Ignore)
            && !hit.transform.IsChildOf(transform) && hit.normal.y > .5f)
        {
            float lift = Mathf.Max(0, foot.position.y - hit.point.y - height * footSoleFraction);
            float weight = 1f - Mathf.Clamp01(lift / Mathf.Max(.01f, height * .1f));
            animator.SetIKPositionWeight(goal, weight * .65f); animator.SetIKPosition(goal, hit.point + Vector3.up * height * footSoleFraction);
        }
        else animator.SetIKPositionWeight(goal, 0);
    }
}
