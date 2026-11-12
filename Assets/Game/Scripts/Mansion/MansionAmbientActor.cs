using UnityEngine;
public sealed class MansionAmbientActor : MonoBehaviour
{
    public string state = "Idle";
    [Range(0,1)] public float phase;
    private void Start()
    {
        var animator=GetComponent<Animator>();
        if(animator!=null && animator.runtimeAnimatorController!=null)
        { animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;animator.Play(state,0,phase); }
    }
}
