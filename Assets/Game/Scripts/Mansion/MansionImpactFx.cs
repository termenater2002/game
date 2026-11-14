using UnityEngine;

/// <summary>Bounded reusable low-poly plaster chips. No collider, Rigidbody, or Particle System dependency.</summary>
public sealed class MansionImpactFx : MonoBehaviour
{
    public Mesh chipMesh;
    public Material dustMaterial;
    public int capacity = 48;
    public AudioSource impactAudio;
    private static MansionImpactFx instance;
    private Transform[] chips;
    private Vector3[] velocity;
    private float[] age, size;
    private int cursor;
    private void OnEnable() { instance = this; }
    private void Awake()
    {
        int count = Mathf.Clamp(capacity, 8, 96); chips = new Transform[count]; velocity = new Vector3[count]; age = new float[count]; size = new float[count];
        for (int i = 0; i < count; i++)
        {
            var child = new GameObject("Pooled plaster chip"); child.transform.SetParent(transform, false);
            child.AddComponent<MeshFilter>().sharedMesh = chipMesh; child.AddComponent<MeshRenderer>().sharedMaterial = dustMaterial;
            chips[i] = child.transform; age[i] = 1; child.SetActive(false);
        }
    }
    private void OnDisable() { if (instance == this) instance = null; }
    public static void Emit(Vector3 point, Vector3 normal, float radius)
    {
        if (instance == null || instance.chips == null) return;
        if (instance.impactAudio != null && instance.impactAudio.clip != null)
        { instance.impactAudio.transform.position = point; instance.impactAudio.PlayOneShot(instance.impactAudio.clip, .35f); }
        int count = Mathf.Clamp(Mathf.RoundToInt(radius * 30), 3, 10);
        for (int n = 0; n < count; n++)
        {
            int i = instance.cursor++ % instance.chips.Length; var chip = instance.chips[i];
            chip.position = point + normal * .025f + Random.insideUnitSphere * radius * .25f; chip.rotation = Random.rotation;
            instance.velocity[i] = normal * Random.Range(.3f, .8f) + Vector3.up * .4f + Random.insideUnitSphere * .3f;
            instance.age[i] = 0; instance.size[i] = Random.Range(.012f, .035f); chip.localScale = Vector3.one * instance.size[i]; chip.gameObject.SetActive(true);
        }
    }
    private void LateUpdate()
    {
        for (int i = 0; chips != null && i < chips.Length; i++)
        {
            if (age[i] >= .55f) continue; age[i] += Time.deltaTime;
            if (age[i] >= .55f) { chips[i].gameObject.SetActive(false); continue; }
            velocity[i] += Vector3.down * (3 * Time.deltaTime); chips[i].position += velocity[i] * Time.deltaTime;
            chips[i].localScale = Vector3.one * (size[i] * (1 - age[i] / .55f));
        }
    }
}
