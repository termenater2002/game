using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Two bounded voxel-depth surfaces over a solid wall. Only intersected chunks are rebuilt.</summary>
public sealed class VoxelWall : MonoBehaviour
{
    public float width = 8, height = 3.2f, thickness = .28f;
    [Range(.035f, .15f)] public float voxelSize = .06f;
    public float minimumImpactEnergy = 3, maximumRadius = .38f, maximumDepth = .12f;
    public float radiusPerSqrtJoule = .014f, depthPerJoule = .0014f;
    public int chunkColumns = 48;
    public float repairCostPerCubicMetre = 2500;
    public bool capEnds = true;
    public Rect[] openings = new Rect[0];
    public Rect[] protectedAreas = new Rect[0];
    public Material plaster, panelling, trim, interior;
    [SerializeField] private VoxelWallChunk[] chunks;
    private float[] xs, ys;
    private byte[] front, back;
    private bool[] solid;
    private int nx, ny;
    private readonly HashSet<int> dirty = new HashSet<int>();
    public int DamageRevision { get; private set; }
    public int LastRebuiltChunks { get; private set; }
    public int TotalChunkRebuilds { get; private set; }
    public int DamagedCells { get; private set; }
    public int ChunkCount => chunks == null ? 0 : chunks.Length;
    public float DeepestDamage { get; private set; }
    public float RemovedVolume { get; private set; }
    public Vector3 LastDamagePoint { get; private set; }
    public Vector3 LastDamageNormal { get; private set; }
    internal int Columns => nx;
    internal int Rows => ny;
    internal float X(int i) => xs[i];
    internal float Y(int i) => ys[i];
    internal bool Solid(int x, int y) => x >= 0 && x < nx && y >= 0 && y < ny && solid[y * nx + x];
    internal float Depth(int x, int y, bool positive)
    {
        if (!Solid(x, y)) return 0;
        return Mathf.Min((positive ? front : back)[y * nx + x] * voxelSize, Mathf.Min(maximumDepth, (thickness - .035f) * .5f));
    }
    internal int SurfaceMaterial(int y) => ys[y] >= 1.2f ? 0 : ys[y] >= 1.12f ? 2 : 1;
    private void EnsureData()
    {
        if (front != null) return;
        xs = Axis(-width * .5f, width * .5f, true); ys = Axis(0, height, false);
        nx = xs.Length - 1; ny = ys.Length - 1;
        front = new byte[nx * ny]; back = new byte[front.Length]; solid = new bool[front.Length];
        for (int y = 0; y < ny; y++) for (int x = 0; x < nx; x++)
        {
            var p = new Vector2((xs[x] + xs[x + 1]) * .5f, (ys[y] + ys[y + 1]) * .5f);
            bool exists = true; foreach (var hole in openings) if (hole.Contains(p)) { exists = false; break; }
            solid[y * nx + x] = exists;
        }
    }
    private float[] Axis(float lo, float hi, bool horizontal)
    {
        var points = new List<float> { lo, hi };
        int count = Mathf.CeilToInt((hi - lo) / Mathf.Max(.035f, voxelSize));
        for (int i = 1; i < count; i++) points.Add(Mathf.Lerp(lo, hi, i / (float)count));
        foreach (var r in openings) { points.Add(Mathf.Clamp(horizontal ? r.xMin : r.yMin, lo, hi)); points.Add(Mathf.Clamp(horizontal ? r.xMax : r.yMax, lo, hi)); }
        if (!horizontal) { if (height > 1.12f) points.Add(1.12f); if (height > 1.2f) points.Add(1.2f); }
        points.Sort(); var result = new List<float>();
        foreach (float value in points) if (result.Count == 0 || value - result[result.Count - 1] > .0001f) result.Add(value);
        return result.ToArray();
    }
    public void BuildInitialGeometry()
    {
        EnsureData(); var list = new List<VoxelWallChunk>();
        for (int start = 0; start < nx; start += Mathf.Max(8, chunkColumns))
        {
            var child = new GameObject("Wall chunk " + list.Count); child.transform.SetParent(transform, false);
            var chunk = child.AddComponent<VoxelWallChunk>(); chunk.wall = this; chunk.firstColumn = start; chunk.lastColumn = Mathf.Min(nx, start + Mathf.Max(8, chunkColumns));
            child.GetComponent<MeshRenderer>().sharedMaterials = new[] { plaster, panelling, trim, interior };
            chunk.Rebuild(false); list.Add(chunk);
        }
        chunks = list.ToArray();
    }
    public bool DamageAt(Vector3 point, Vector3 normal, float energy)
    {
        if (!isActiveAndEnabled || float.IsNaN(energy) || float.IsInfinity(energy) || energy < minimumImpactEnergy) return false;
        EnsureData(); Vector3 local = transform.InverseTransformPoint(point); Vector3 n = transform.InverseTransformDirection(normal);
        if (Mathf.Abs(n.z) < .55f || Mathf.Abs(local.x) > width * .5f + .02f || local.y < 0 || local.y > height) return false;
        bool positive = n.z > 0;
        float radius = Mathf.Clamp(.055f + Mathf.Sqrt(energy - minimumImpactEnergy) * radiusPerSqrtJoule, voxelSize * 1.4f, maximumRadius);
        var data = positive ? front : back; bool changed = false; float removed = 0;
        for (int y = 0; y < ny; y++)
        {
            float cy = (ys[y] + ys[y + 1]) * .5f; if (Mathf.Abs(cy - local.y) > radius) continue;
            for (int x = 0; x < nx; x++)
            {
                if (!Solid(x, y)) continue; float cx = (xs[x] + xs[x + 1]) * .5f;
                if (Mathf.Abs(cx) > width * .5f - .07f || cy < .06f || cy > height - .06f) continue;
                var uv = new Vector2(cx, cy); bool guarded = false;
                foreach (var area in protectedAreas) if (area.Contains(uv)) { guarded = true; break; }
                foreach (var hole in openings)
                { var guard = hole; guard.xMin -= .045f; guard.xMax += .045f; guard.yMin -= .045f; guard.yMax += .045f; if (guard.Contains(uv)) { guarded = true; break; } }
                if (guarded) continue;
                float distance = Vector2.Distance(uv, new Vector2(local.x, local.y));
                uint hash = unchecked((uint)(x * 73856093 ^ y * 19349663)); float rough = .83f + (hash % 100) / 300f;
                float falloff = 1 - distance / (radius * rough); if (falloff <= 0) continue;
                int index = y * nx + x; int old = data[index];
                float maxDepth = Mathf.Min(maximumDepth, (thickness - .035f) * .5f);
                int amount = Mathf.Max(1, Mathf.CeilToInt((energy - minimumImpactEnergy) * depthPerJoule * falloff / voxelSize));
                int next = Mathf.Min(Mathf.CeilToInt(maxDepth / voxelSize), old + amount);
                if (next == old) continue;
                float previousDepth = Depth(x, y, positive);
                data[index] = (byte)next; if (old == 0 && (positive ? back : front)[index] == 0) DamagedCells++;
                removed += (Depth(x, y, positive) - previousDepth) * (xs[x + 1] - xs[x]) * (ys[y + 1] - ys[y]);
                DeepestDamage = Mathf.Max(DeepestDamage, Depth(x, y, positive)); changed = true;
                for (int i = 0; chunks != null && i < chunks.Length; i++) if (x >= chunks[i].firstColumn - 1 && x <= chunks[i].lastColumn) dirty.Add(i);
            }
        }
        if (changed)
        {
            DamageRevision++; RemovedVolume += removed; LastDamagePoint = point; LastDamageNormal = normal;
            if (Application.isPlaying && SessionWallet.Current != null) SessionWallet.Current.Earnings.Add((decimal)(removed * Mathf.Max(0, repairCostPerCubicMetre)));
            MansionImpactFx.Emit(point, normal, radius);
        }
        return changed;
    }
    private void LateUpdate() { FlushDamage(); }
    public void FlushDamage()
    {
        if (dirty.Count == 0) return;
        LastRebuiltChunks = dirty.Count;
        TotalChunkRebuilds += dirty.Count;
        foreach (int index in dirty) chunks[index].Rebuild(true);
        dirty.Clear();
    }
}
