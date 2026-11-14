using System.Collections.Generic;
using UnityEngine;

/// <summary>Strict active-body budget with O(1) removal, including expired groups.</summary>
public sealed class MansionDebris : MonoBehaviour
{
    public const int BodyBudget = 192;
    private static readonly LinkedList<MansionDebris> recent = new LinkedList<MansionDebris>();
    private static int liveBodies;
    private LinkedListNode<MansionDebris> node;
    private Rigidbody[] pieces;
    public static int LiveBodyCount => liveBodies;
    public static int LiveGroupCount => recent.Count;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetBudget() { recent.Clear(); liveBodies = 0; }
    private void OnEnable()
    {
        pieces = GetComponentsInChildren<Rigidbody>();
        if (pieces.Length > BodyBudget)
        {
            for (int i = BodyBudget; i < pieces.Length; i++) pieces[i].gameObject.SetActive(false);
            var first = new Rigidbody[BodyBudget]; System.Array.Copy(pieces, first, BodyBudget); pieces = first;
        }
        liveBodies += pieces.Length; node = recent.AddLast(this);
        while (liveBodies > BodyBudget && recent.First != node)
        {
            var oldest = recent.First.Value; oldest.Unregister(); oldest.gameObject.SetActive(false); Destroy(oldest.gameObject);
        }
    }
    private void Unregister()
    {
        if (node == null) return; recent.Remove(node); node = null; liveBodies -= pieces.Length;
    }
    private void FixedUpdate()
    {
        foreach (var body in pieces)
        {
            if (body == null || body.IsSleeping() || body.isKinematic) continue;
            body.linearVelocity = Vector3.ClampMagnitude(body.linearVelocity, 16);
            body.angularVelocity = Vector3.ClampMagnitude(body.angularVelocity, 14);
        }
    }
    private void OnDisable() { Unregister(); }
    private void OnDestroy() { Unregister(); }
}
