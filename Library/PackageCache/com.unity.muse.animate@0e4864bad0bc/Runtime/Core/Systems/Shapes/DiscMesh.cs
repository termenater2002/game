using System;
using System.Collections.Generic;
using Unity.DeepPose.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.Muse.Animate
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    class DiscMesh : MonoBehaviour
    {
        public const int k_MaxResolution = 128;
        public const int k_MinResolution = 3;
        MaterialPropertyBlock m_MaterialPropertyBlockA;
        MaterialPropertyBlock m_MaterialPropertyBlockB;
        public int Resolution
        {
            get => m_Resolution;
            set
            {
                var valid = Mathf.Clamp(value, k_MinResolution, k_MaxResolution);
                if (m_Resolution.Equals(valid))
                    return;
                m_Resolution = valid;
                QueueUpdate(UpdateFlags.Mesh);
            }
        }

        public float Radius
        {
            get => m_Radius;
            set
            {
                var valid = Mathf.Max(0, value);
                if (m_Radius.Equals(valid))
                    return;
                m_Radius = valid;
                QueueUpdate(UpdateFlags.Mesh);
            }
        }

        public float BorderSideRatio
        {
            get => m_BorderSideRatio;
            set
            {
                var valid = Mathf.Clamp(value, 0f, 1f);
                if (m_BorderSideRatio.Equals(valid))
                    return;
                m_BorderSideRatio = valid;
                QueueUpdate(UpdateFlags.Mesh);
            }
        }

        public float BorderSpacing
        {
            get => m_BorderSpacing;
            set
            {
                var valid = Mathf.Max(0, value);
                if (m_BorderSpacing.Equals(valid))
                    return;
                m_BorderSpacing = valid;
                QueueUpdate(UpdateFlags.Mesh);
            }
        }

        public bool Fill
        {
            get => m_Fill;
            set
            {
                if (m_Fill.Equals(value))
                    return;
                m_Fill = value;
                QueueUpdate(UpdateFlags.Mesh);
            }
        }

        public float BorderSize
        {
            get => m_BorderSize;
            set
            {
                var valid = Mathf.Max(0, value);
                if (m_BorderSize.Equals(valid))
                    return;
                m_BorderSize = valid;
                QueueUpdate(UpdateFlags.Mesh);
            }
        }

        public Color ColorBorder
        {
            get => m_ColorBorder;
            set
            {
                QueueUpdate(UpdateFlags.Material);
                if (m_ColorBorder.Equals(value))
                    return;
                
                m_ColorBorder = value;
                QueueUpdate(UpdateFlags.Material);
            }
        }

        public Color ColorFill
        {
            get => m_ColorFill;
            set
            {
                QueueUpdate(UpdateFlags.Material);
                if (m_ColorFill.Equals(value))
                    return;
                m_ColorFill = value;
            }
        }

        public Vector3 RotationOffset
        {
            get => m_RotationOffset;
            set
            {
                if (m_RotationOffset.Equals(value))
                    return;
                m_RotationOffset = value;
                QueueUpdate(UpdateFlags.Mesh);
            }
        }

        [SerializeField]
        Vector3 m_RotationOffset = new(0, 0, 0);
        [SerializeField]
        int m_Resolution = 24;
        [SerializeField]
        float m_Radius = 1f;
        [SerializeField]
        float m_BorderSpacing = 0f;
        [SerializeField]
        float m_BorderSize = 0.2f;
        [SerializeField]
        float m_BorderSideRatio = 0f;
        [SerializeField]
        bool m_Fill = true;
        [SerializeField]
        Color m_ColorBorder;
        [SerializeField]
        Color m_ColorFill;

        [SerializeField]
        List<Material> m_materials;
        
        MeshFilter m_MeshFilter;
        MeshRenderer m_MeshRenderer;

        void Start()
        {
            if (m_MeshFilter == null)
                FindComponents();
        }

        void OnEnable()
        {
            if (m_MeshFilter == null)
                FindComponents();

            m_MeshRenderer.enabled = true;
        }

        void OnDisable()
        {
            m_MeshRenderer.enabled = false;
        }

        void FindComponents()
        {
            m_MeshFilter = GetComponent<MeshFilter>();
            m_MeshRenderer = GetComponent<MeshRenderer>();

            // Add the materials to the list of materials
            m_materials = new List<Material>
            {
                HandlesUtils.GetNewMeshMaterial(),
                HandlesUtils.GetNewMeshMaterial()
            };

            // Assign the materials to the mesh renderer
            m_MeshRenderer.SetMaterials(m_materials);
        }

        public void Update()
        {
            if (IsUpdateQueued(UpdateFlags.Mesh))
                UpdateMesh(m_Resolution, m_Radius, m_Fill, m_BorderSize, m_BorderSpacing, m_BorderSideRatio);
            
            if (IsUpdateQueued(UpdateFlags.Material))
                UpdateMaterial();
        }

        void UpdateMesh(int resolution, float radius, bool fill, float borderSize, float borderSpacing, float borderSideRatio)
        {
            ResetUpdate(UpdateFlags.Mesh);

            if (m_MeshFilter == null)
            {
                FindComponents();
            }

            var nbVertices = 0;
            var nbTriangles = 0;
            var borderVerticesStartIndex = 0;
            var borderTrianglesStartIndex = 0;

            // Determine the amount of vertices & triangles needed
            if (fill)
            {
                nbVertices += resolution + 1;
                nbTriangles += resolution * 3;
                borderVerticesStartIndex += nbVertices;
                borderTrianglesStartIndex += nbTriangles;
            }

            if (borderSize > 0f)
            {
                nbVertices += resolution * 2;
                nbTriangles += (resolution * 2) * 3;
            }

            var borderTotal = borderSize + borderSpacing;

            // Construct the vertices
            var vertices = new Vector3[nbVertices];

            if (fill)
            {
                // Fill Circle Vertices
                for (var i = 0; i < resolution; i++)
                {
                    var pad = borderTotal * borderSideRatio;
                    var angle = 2f * Mathf.PI * i / resolution;
                    var dx = Mathf.Cos(angle) * (radius - pad);
                    var dz = Mathf.Sin(angle) * (radius - pad);
                    vertices[i] = new Vector3(dx, 0f, dz);
                }

                // Fill Center Vertex
                vertices[resolution] = new Vector3(0f, 0f, 0f);
            }

            if (borderSize > 0f)
            {
                // Border Outer vertices
                for (var i = 0; i < resolution; i++)
                {
                    var pad = borderTotal * (1f - borderSideRatio);
                    var angle = 2f * Mathf.PI * i / resolution;
                    var dx = Mathf.Cos(angle) * (radius + pad);
                    var dz = Mathf.Sin(angle) * (radius + pad);
                    vertices[borderVerticesStartIndex + i] = new Vector3(dx, 0f, dz);
                }

                // Border Inner vertices
                for (var i = 0; i < resolution; i++)
                {
                    var pad = borderTotal * borderSideRatio;
                    var angle = 2f * Mathf.PI * i / resolution;
                    var dx = Mathf.Cos(angle) * (radius + borderSpacing - pad);
                    var dz = Mathf.Sin(angle) * (radius + borderSpacing - pad);
                    vertices[borderVerticesStartIndex + resolution + i] = new Vector3(dx, 0f, dz);
                }
            }

            // Construct the triangles
            var triangles = new int[nbTriangles];

            if (fill)
            {
                for (var i = 0; i < resolution; i++)
                {
                    var triangleAIndex = (i * 3);
                    triangles[triangleAIndex] = resolution;
                    triangles[triangleAIndex + 1] = HandlesUtils.GetCircleVertexIndex(i + 1, resolution);
                    triangles[triangleAIndex + 2] = HandlesUtils.GetCircleVertexIndex(i, resolution);
                }
            }

            if (borderSize > 0)
            {
                // Construct the triangles
                for (var i = 0; i < resolution; i++)
                {
                    var tiA = borderTrianglesStartIndex + (i * 2) * 3;
                    var tiB = borderTrianglesStartIndex + ((i * 2) + 1) * 3;

                    // Top left triangle
                    triangles[tiA] = borderVerticesStartIndex + HandlesUtils.GetCircleVertexIndex(i, resolution) + resolution;
                    triangles[tiA + 1] = borderVerticesStartIndex + HandlesUtils.GetCircleVertexIndex(i + 1, resolution);
                    triangles[tiA + 2] = borderVerticesStartIndex + HandlesUtils.GetCircleVertexIndex(i, resolution);

                    // Bottom right triangle
                    triangles[tiB] = borderVerticesStartIndex + HandlesUtils.GetCircleVertexIndex(i + 1, resolution) + resolution;
                    triangles[tiB + 1] = borderVerticesStartIndex + HandlesUtils.GetCircleVertexIndex(i + 1, resolution);
                    triangles[tiB + 2] = borderVerticesStartIndex + HandlesUtils.GetCircleVertexIndex(i, resolution) + resolution;
                }
            }

            HandlesUtils.RotateVertices(RotationOffset, ref vertices);

            // Create the mesh
            Mesh mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.subMeshCount = 2;

            if (!fill)
            {
                mesh.SetSubMesh(0, new SubMeshDescriptor(0, 0));
            }
            else
            {
                mesh.SetSubMesh(0, new SubMeshDescriptor(0, borderTrianglesStartIndex));
            }

            if (borderSize <= 0)
            {
                mesh.SetSubMesh(1, new SubMeshDescriptor(borderTrianglesStartIndex, 0));
            }
            else
            {
                mesh.SetSubMesh(1, new SubMeshDescriptor(borderTrianglesStartIndex, resolution * 2 * 3));
            }

            mesh.RecalculateNormals();

            // Assign the mesh to the MeshFilter
            m_MeshFilter.mesh = mesh;
            m_MeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            QueueUpdate(UpdateFlags.Material);
        }

        void UpdateMaterial()
        {
            /*
            m_MeshRenderer.GetPropertyBlock(m_MaterialPropertyBlockA, 0);
            m_MaterialPropertyBlockA.SetColor("_BaseColor", m_ColorFill);
            m_MeshRenderer.SetPropertyBlock(m_MaterialPropertyBlockA, 0);
            
            m_MeshRenderer.GetPropertyBlock(m_MaterialPropertyBlockB, 1);
            m_MaterialPropertyBlockB.SetColor("_BaseColor", m_ColorBorder);
            m_MeshRenderer.SetPropertyBlock(m_MaterialPropertyBlockB, 1);
            */
            //m_MeshRenderer.SetMaterials(m_materials);
            //m_MaterialPropertyBlockB.SetColor("_BaseColor", m_ColorBorder);
            //m_MeshRenderer.SetPropertyBlock(m_MaterialPropertyBlockB, 1);
            /*
            Debug.Log($"Updating Material {m_ColorFill}");
            ResetUpdate(UpdateFlags.Material);
            m_materials[0].color = m_ColorFill;
            m_materials[1].color = m_ColorBorder;
            /*
            m_materials[0].SetColor("_BaseColor", m_ColorFill);
            m_materials[1].SetColor("_BaseColor", m_ColorBorder);
            */
            // m_MeshRenderer.SetMaterials(m_materials);
            
            // Set the materials colors
            m_MeshRenderer.sharedMaterials[0].SetColor("_BaseColor", m_ColorFill);
            m_MeshRenderer.sharedMaterials[1].SetColor("_BaseColor", m_ColorBorder);
        }
        
        void OnDrawGizmosSelected()
        {
            var vertices = new Vector3[m_Resolution];

            // Construct the vertices
            // Border Vertices
            for (var i = 0; i < m_Resolution; i++)
            {
                var angle = 2f * Mathf.PI * i / m_Resolution;
                var dx = Mathf.Cos(angle) * (m_Radius);
                var dz = Mathf.Sin(angle) * (m_Radius);
                vertices[i] = new Vector3(dx, 0.001f, dz);
            }

            HandlesUtils.RotateVertices(RotationOffset, ref vertices);

            TransformVertices(gameObject.transform.localToWorldMatrix, ref vertices);

            for (var i = 0; i < m_Resolution; i++)
            {
                var from = vertices[i];
                var to = vertices[i + 1 <= (m_Resolution - 1) ? i + 1 : 0];
                GizmoUtils.DrawLine(from, to, Color.yellow, 3f);
            }
        }

        static void TransformVertices(Matrix4x4 transformMatrix, ref Vector3[] vertices)
        {
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] = transformMatrix.MultiplyPoint3x4(vertices[i]);
            }
        }
        
        // [Section] Update Flags
        public event Action<UpdateFlags,bool> OnUpdateFlagsChanged;
        
        [Flags]
        public enum UpdateFlags
        {
            Mesh = 1,
            Material = 2
        }

        UpdateFlags m_UpdateFlags;

        bool IsUpdateQueued(UpdateFlags updateFlag) => m_UpdateFlags.HasFlag(updateFlag);
        void QueueUpdate(UpdateFlags updateFlag) => SetUpdateFlag(updateFlag, true);
        void ResetUpdate(UpdateFlags updateFlag) => SetUpdateFlag(updateFlag, false);
        void SetUpdateFlag(UpdateFlags flag, bool value)
        {
            if (value == m_UpdateFlags.HasFlag(flag))
                return;

            if (value)
            {
                m_UpdateFlags |= flag;
            }
            else
            {
                m_UpdateFlags &= ~flag;
            }

            OnUpdateFlagsChanged?.Invoke(flag, value);
        }
        
        
    }
}
