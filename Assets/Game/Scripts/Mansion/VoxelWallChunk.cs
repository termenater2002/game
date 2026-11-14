using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public sealed class VoxelWallChunk : MonoBehaviour
{
    public VoxelWall wall;
    public int firstColumn, lastColumn;
    public Collider[] ignoreOnDamage = new Collider[0];
    private Mesh ownedMesh;
    private Rigidbody lastBody;
    private float lastImpactTime;
    private bool[] visited;
    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<Vector3> normals = new List<Vector3>();
    private readonly List<Vector2> uv = new List<Vector2>();
    private readonly List<int>[] triangles = { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
    private void Vertex(Vector3 p, Vector3 normal)
    {
        vertices.Add(p); normals.Add(normal);
        uv.Add(Mathf.Abs(normal.z) > .5f ? new Vector2(p.x + wall.transform.position.x + wall.transform.position.z, p.y) : Mathf.Abs(normal.x) > .5f ? new Vector2(p.z, p.y) : new Vector2(p.x, p.z));
    }
    private bool[] VisitBuffer(int count)
    {
        if (visited == null || visited.Length != count) visited = new bool[count];
        else System.Array.Clear(visited, 0, visited.Length);
        return visited;
    }
    private void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, int material)
    {
        if (Vector3.Dot(Vector3.Cross(b - a, c - a), normal) < 0) { var temp = b; b = d; d = temp; }
        int start = vertices.Count;
        Vertex(a, normal); Vertex(b, normal); Vertex(c, normal); Vertex(d, normal);
        var list = triangles[material]; list.Add(start); list.Add(start + 1); list.Add(start + 2); list.Add(start); list.Add(start + 2); list.Add(start + 3);
    }
    public void Rebuild(bool damaged)
    {
        vertices.Clear(); normals.Clear(); uv.Clear(); foreach (var list in triangles) list.Clear();
        int rows = wall.Rows, cols = lastColumn - firstColumn;
        foreach (bool front in new[] { false, true })
        {
            var used = VisitBuffer(rows * cols); float sign = front ? 1 : -1;
            for (int y = 0; y < rows; y++) for (int x = firstColumn; x < lastColumn; x++)
            {
                int index = y * cols + x - firstColumn; if (used[index] || !wall.Solid(x, y)) continue;
                float depth = wall.Depth(x, y, front); int material = depth > 0 ? 3 : wall.SurfaceMaterial(y);
                int ex = x + 1;
                bool Match(int xx, int yy) => wall.Solid(xx, yy) && !used[yy * cols + xx - firstColumn] && Mathf.Abs(wall.Depth(xx, yy, front) - depth) < .00001f && (depth > 0 || wall.SurfaceMaterial(yy) == material);
                while (ex < lastColumn && Match(ex, y)) ex++;
                int ey = y + 1; bool extend = true;
                while (ey < rows && extend) { for (int xx = x; xx < ex; xx++) if (!Match(xx, ey)) { extend = false; break; } if (extend) ey++; }
                for (int yy = y; yy < ey; yy++) for (int xx = x; xx < ex; xx++) used[yy * cols + xx - firstColumn] = true;
                float z = sign * (wall.thickness * .5f - depth);
                Quad(new Vector3(wall.X(x), wall.Y(y), z), new Vector3(wall.X(ex), wall.Y(y), z), new Vector3(wall.X(ex), wall.Y(ey), z), new Vector3(wall.X(x), wall.Y(ey), z), Vector3.forward * sign, material);
            }
            // Riser faces between neighbouring voxel-depth levels. No per-voxel GameObjects.
            for (int y = 0; y < rows; y++) for (int x = firstColumn; x < lastColumn; x++)
            {
                if (!wall.Solid(x, y)) continue; float deep = wall.Depth(x, y, front); if (deep <= 0) continue;
                for (int edge = 0; edge < 4; edge++)
                {
                    int xx = x + (edge == 0 ? -1 : edge == 1 ? 1 : 0), yy = y + (edge == 2 ? -1 : edge == 3 ? 1 : 0);
                    if (!wall.Solid(xx, yy)) continue; float shallow = wall.Depth(xx, yy, front); if (shallow >= deep) continue;
                    float z0 = sign * (wall.thickness * .5f - deep), z1 = sign * (wall.thickness * .5f - shallow);
                    if (edge < 2) { float px = wall.X(edge == 0 ? x : x + 1); Quad(new Vector3(px, wall.Y(y), z0), new Vector3(px, wall.Y(y + 1), z0), new Vector3(px, wall.Y(y + 1), z1), new Vector3(px, wall.Y(y), z1), Vector3.right * (edge == 0 ? 1 : -1), 3); }
                    else { float py = wall.Y(edge == 2 ? y : y + 1); Quad(new Vector3(wall.X(x), py, z0), new Vector3(wall.X(x + 1), py, z0), new Vector3(wall.X(x + 1), py, z1), new Vector3(wall.X(x), py, z1), Vector3.up * (edge == 2 ? 1 : -1), 3); }
                }
            }
        }
        // Ends, door/window reveals and top/bottom faces. Adjacent solid chunks emit no internal faces.
        for (int y = 0; y < rows; y++) for (int x = firstColumn; x < lastColumn; x++)
        {
            if (!wall.Solid(x, y)) continue;
            float z0 = -wall.thickness * .5f + wall.Depth(x, y, false), z1 = wall.thickness * .5f - wall.Depth(x, y, true);
            if (!wall.Solid(x - 1, y) && (x > 0 || wall.capEnds)) Quad(new Vector3(wall.X(x), wall.Y(y), z0), new Vector3(wall.X(x), wall.Y(y + 1), z0), new Vector3(wall.X(x), wall.Y(y + 1), z1), new Vector3(wall.X(x), wall.Y(y), z1), Vector3.left, 0);
            if (!wall.Solid(x + 1, y) && (x + 1 < wall.Columns || wall.capEnds)) Quad(new Vector3(wall.X(x + 1), wall.Y(y), z0), new Vector3(wall.X(x + 1), wall.Y(y + 1), z0), new Vector3(wall.X(x + 1), wall.Y(y + 1), z1), new Vector3(wall.X(x + 1), wall.Y(y), z1), Vector3.right, 0);
            if (!wall.Solid(x, y - 1)) Quad(new Vector3(wall.X(x), wall.Y(y), z0), new Vector3(wall.X(x + 1), wall.Y(y), z0), new Vector3(wall.X(x + 1), wall.Y(y), z1), new Vector3(wall.X(x), wall.Y(y), z1), Vector3.down, 0);
            if (!wall.Solid(x, y + 1)) Quad(new Vector3(wall.X(x), wall.Y(y + 1), z0), new Vector3(wall.X(x + 1), wall.Y(y + 1), z0), new Vector3(wall.X(x + 1), wall.Y(y + 1), z1), new Vector3(wall.X(x), wall.Y(y + 1), z1), Vector3.up, 0);
        }
        var filter = GetComponent<MeshFilter>();
        if (ownedMesh == null) { ownedMesh = new Mesh { name = name + " geometry", indexFormat = IndexFormat.UInt32 }; ownedMesh.MarkDynamic(); }
        ownedMesh.Clear(); ownedMesh.SetVertices(vertices); ownedMesh.SetNormals(normals); ownedMesh.SetUVs(0, uv); ownedMesh.subMeshCount = 4;
        for (int i = 0; i < 4; i++) ownedMesh.SetTriangles(triangles[i], i); ownedMesh.RecalculateBounds(); filter.sharedMesh = ownedMesh;
        if (!damaged)
        {
            // Intact geometry uses a few exact boxes, including the rectangles around doorways.
            var used = VisitBuffer(rows * cols);
            for (int y = 0; y < rows; y++) for (int x = firstColumn; x < lastColumn; x++)
            {
                if (!wall.Solid(x, y) || used[y * cols + x - firstColumn]) continue;
                int ex = x + 1; while (ex < lastColumn && wall.Solid(ex, y) && !used[y * cols + ex - firstColumn]) ex++;
                int ey = y + 1; bool extend = true;
                while (ey < rows && extend) { for (int xx = x; xx < ex; xx++) if (!wall.Solid(xx, ey) || used[ey * cols + xx - firstColumn]) { extend = false; break; } if (extend) ey++; }
                for (int yy = y; yy < ey; yy++) for (int xx = x; xx < ex; xx++) used[yy * cols + xx - firstColumn] = true;
                var box = gameObject.AddComponent<BoxCollider>(); box.center = new Vector3((wall.X(x) + wall.X(ex)) * .5f, (wall.Y(y) + wall.Y(ey)) * .5f, 0); box.size = new Vector3(wall.X(ex) - wall.X(x), wall.Y(ey) - wall.Y(y), wall.thickness); box.contactOffset = .003f;
            }
            return;
        }
        foreach (var box in GetComponents<BoxCollider>()) box.enabled = false;
        // Only a damaged chunk needs a static, non-convex mesh collider following its recesses.
        var collider = GetComponent<MeshCollider>();
        if (collider == null) collider = gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = null; collider.sharedMesh = ownedMesh; collider.contactOffset = .003f;
        foreach (var mounted in ignoreOnDamage) if (mounted != null) Physics.IgnoreCollision(collider, mounted, true);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (wall == null || collision.rigidbody == null || collision.rigidbody.GetComponentInParent<FunnyMovement>() != null) return;
        if (lastBody == collision.rigidbody && Time.time - lastImpactTime < .15f) return;
        lastBody = collision.rigidbody; lastImpactTime = Time.time;
        if (collision.contactCount == 0) return; var contact = collision.GetContact(0);
        var normal = contact.normal;
        if (Vector3.Dot(normal, collision.rigidbody.worldCenterOfMass - contact.point) < 0) normal = -normal;
        var damage = collision.rigidbody.GetComponent<DamageOnImpact>();
        float hardness = damage != null && damage.materialProfile != null ? damage.materialProfile.wallImpactMultiplier : 1;
        var fragment = collision.rigidbody.GetComponent<MansionFragment>(); if (fragment != null) hardness = fragment.wallImpactMultiplier;
        float normalSpeed = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, normal));
        wall.DamageAt(contact.point, normal, .5f * collision.rigidbody.mass * normalSpeed * normalSpeed * hardness);
    }
    public void CompleteEditorBake() { ownedMesh = null; }
    private void OnDestroy()
    {
        if (ownedMesh == null) return;
        if (Application.isPlaying) Destroy(ownedMesh); else DestroyImmediate(ownedMesh);
    }
}
