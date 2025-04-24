using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    class Outliner : MonoBehaviour
    {
        static readonly int k_OutlineColor = Shader.PropertyToID("_Outline_Color");
        static readonly int k_OutlineThickness = Shader.PropertyToID("_Outline_Thickness");
        
        public Color OutlineColor
        {
            get => m_OutlineColor;
            set
            {
                m_OutlineColor = value;
                m_NeedToUpdate = true;
            }
        }

        public float OutlineWidth
        {
            get => m_OutlineWidth;
            set
            {
                m_OutlineWidth = value;
                m_NeedToUpdate = true;
            }
        }
        private static HashSet<Mesh> s_RegisteredMeshes = new HashSet<Mesh>();

        [Serializable]
        class ListVector3
        {
            public List<Vector3> Data;
        }

        [SerializeField]
        Color m_OutlineColor = Color.white;

        [SerializeField, Range(0f, 0.01f)]
        float m_OutlineWidth = 0.002f;

        [Header("Optional")]
        [SerializeField, HideInInspector]
        List<Mesh> m_BakedKeys = new List<Mesh>();
        
        [SerializeField, HideInInspector]
        List<ListVector3> m_BakedKeyValues = new List<ListVector3>();

        Renderer[] m_Renderers;
        Material m_OutlineMaterial;
        bool m_NeedToUpdate;

        public void Awake()
        {
            // Cache renderers
            m_Renderers = GetComponentsInChildren<Renderer>();

            // Instantiate outline materials
            m_OutlineMaterial = HandlesUtils.GetNewOutlinerMaterial();
            m_OutlineMaterial.name = "Outline";

            // Retrieve or generate smooth normals
            LoadSmoothNormals();

            // Apply material properties immediately
            m_NeedToUpdate = true;
        }

        void OnEnable()
        {
            foreach (var renderer in m_Renderers)
            {
                // Append outline shaders
                var materials = renderer.sharedMaterials.ToList();
                materials.Add(m_OutlineMaterial);
                renderer.materials = materials.ToArray();
            }
        }

        void OnValidate()
        {
            // Update material properties
            m_NeedToUpdate = true;

            // Clear cache when baking is disabled or corrupted
            if (m_BakedKeys.Count != m_BakedKeyValues.Count)
            {
                m_BakedKeys.Clear();
                m_BakedKeyValues.Clear();
            }

            // Generate smooth normals when baking is enabled
            if (m_BakedKeys.Count == 0)
            {
                Bake();
            }
        }

        void Update()
        {
            if (m_NeedToUpdate)
            {
                m_NeedToUpdate = false;
                UpdateMaterialProperties();
            }
        }

        void OnDisable()
        {
            foreach (var entry in m_Renderers)
            {
                // Remove outline shaders
                var materials = entry.sharedMaterials.ToList();
                materials.Remove(m_OutlineMaterial);
                entry.materials = materials.ToArray();
            }
        }

        void OnDestroy()
        {
            // Destroy material instances
            GameObjectUtils.Destroy(m_OutlineMaterial);
        }

        void Bake()
        {
            // Generate smooth normals for each mesh
            var bakedMeshes = new HashSet<Mesh>();

            foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
            {
                // Skip duplicates
                if (!bakedMeshes.Add(meshFilter.sharedMesh))
                {
                    continue;
                }

                // Serialize smooth normals
                var smoothNormals = SmoothNormals(meshFilter.sharedMesh);

                m_BakedKeys.Add(meshFilter.sharedMesh);
                m_BakedKeyValues.Add(new ListVector3() { Data = smoothNormals });
            }
        }

        void LoadSmoothNormals()
        {
            // Retrieve or generate smooth normals
            foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
            {
                // Skip if smooth normals have already been adopted
                if (!s_RegisteredMeshes.Add(meshFilter.sharedMesh))
                {
                    continue;
                }

                // Retrieve or generate smooth normals
                var index = m_BakedKeys.IndexOf(meshFilter.sharedMesh);
                var smoothNormals = (index >= 0) ? m_BakedKeyValues[index].Data : SmoothNormals(meshFilter.sharedMesh);

                // Store smooth normals in UV3
                meshFilter.sharedMesh.SetUVs(3, smoothNormals);

                // Combine submeshes
                var meshRenderer = meshFilter.GetComponent<Renderer>();

                if (meshRenderer != null)
                {
                    CombineSubmeshes(meshFilter.sharedMesh, meshRenderer.sharedMaterials);
                }
            }

            // Clear UV3 on skinned mesh renderers
            foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var mesh = skinnedMeshRenderer.sharedMesh;
                
                // Skip if UV3 has already been reset
                if (!s_RegisteredMeshes.Add(mesh))
                {
                    continue;
                }

                // Clear UV3
                mesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];

                // Combine submeshes
                CombineSubmeshes(mesh, skinnedMeshRenderer.sharedMaterials);
            }
        }

        List<Vector3> SmoothNormals(Mesh mesh)
        {
            // Group vertices by location
            var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);

            // Copy normals to a new list
            var smoothNormals = new List<Vector3>(mesh.normals);

            // Average normals for grouped vertices
            foreach (var group in groups)
            {
                // Skip single vertices
                if (group.Count() == 1)
                {
                    continue;
                }

                // Calculate the average normal
                var smoothNormal = Vector3.zero;

                foreach (var pair in group)
                {
                    smoothNormal += smoothNormals[pair.Value];
                }

                smoothNormal.Normalize();

                // Assign smooth normal to each vertex
                foreach (var pair in group)
                {
                    smoothNormals[pair.Value] = smoothNormal;
                }
            }

            return smoothNormals;
        }

        void CombineSubmeshes(Mesh mesh, IReadOnlyCollection<Material> materials)
        {
            // Skip meshes with a single submesh
            if (mesh.subMeshCount == 1)
            {
                return;
            }

            // Skip if submesh count exceeds material count
            if (mesh.subMeshCount > materials.Count)
            {
                return;
            }

            // Append combined submesh
            mesh.subMeshCount++;
            mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
        }

        void UpdateMaterialProperties()
        {
            // Apply properties according to mode
            m_OutlineMaterial.SetColor(k_OutlineColor, m_OutlineColor);
            m_OutlineMaterial.SetFloat(k_OutlineThickness, m_OutlineWidth);
        }
    }
}
