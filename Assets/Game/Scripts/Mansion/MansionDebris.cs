using System.Collections.Generic;
using UnityEngine;

/// <summary>A body-count budget, independent from the size of the broken furniture.</summary>
public sealed class MansionDebris : MonoBehaviour
{
    private static readonly Queue<MansionDebris> recent = new Queue<MansionDebris>();
    private static int liveBodies;
    private Rigidbody[] pieces;
    private bool registered;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetBudget() { recent.Clear();liveBodies=0; }
    private void OnEnable()
    {
        pieces=GetComponentsInChildren<Rigidbody>();registered=true;liveBodies+=pieces.Length;recent.Enqueue(this);
        while(liveBodies>192&&recent.Count>1)
        {
            var oldest=recent.Dequeue();if(oldest==null||oldest==this)continue;
            if(oldest.registered){liveBodies-=oldest.pieces.Length;oldest.registered=false;}Destroy(oldest.gameObject);
        }
    }
    private void FixedUpdate()
    {
        foreach(var body in pieces)
        {
            if(body==null||body.IsSleeping()||body.isKinematic)continue;
            body.linearVelocity=Vector3.ClampMagnitude(body.linearVelocity,16f);
            body.angularVelocity=Vector3.ClampMagnitude(body.angularVelocity,14f);
        }
    }
    private void OnDestroy(){if(registered){liveBodies-=pieces.Length;registered=false;}}
}
