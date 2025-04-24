using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.Muse.Animate
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    class ConeMesh : MonoBehaviour
    {
        public const int k_MaxResolution = 128;
        public const int k_MinResolution = 3;

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

        public float Height
        {
            get => m_Height;
            set
            {
                var valid = Mathf.Max(0, value);
                if (m_Height.Equals(valid))
                    return;
                m_Height = valid;
                QueueUpdate(UpdateFlags.Mesh);
            }
        }

        public Color Color
        {
            get => m_Color;
            set
            {
                if (m_Color.Equals(value))
                    return;
                m_Color = value;
                QueueUpdate(UpdateFlags.Material);
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
        int m_Resolution = 12;
        [SerializeField]
        float m_Radius = 1f;
        [SerializeField]
        float m_Height = 2f;
        [SerializeField]
        Color m_Color = Color.white;
        [SerializeField]
        Vector3 m_RotationOffset = new(0, 0, 0);

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

            QueueUpdate(UpdateFlags.Mesh);
        }

        void FindComponents()
        {
            m_MeshFilter = GetComponent<MeshFilter>();
            m_MeshRenderer = GetComponent<MeshRenderer>();

            // Add the materials to the list of materials
            var materials = new List<Material>
            {
                HandlesUtils.GetNewMeshMaterial()
            };

            // Assign the materials to the mesh renderer
            m_MeshRenderer.SetMaterials(materials);
        }

        void OnDisable()
        {
            m_MeshRenderer.enabled = false;
        }

        public void Update()
        {
            if (IsUpdateQueued(UpdateFlags.Mesh))
                UpdateMesh();

            if (IsUpdateQueued(UpdateFlags.Material))
                UpdateMaterial();
        }

        void UpdateMesh()
        {
            ResetUpdate(UpdateFlags.Mesh);

            var vertices = new Vector3[m_Resolution * 2 + 2];
            var triangles = new int[m_Resolution * 2 * 3];

            // Edge of of the base
            for (var i = 0; i < m_Resolution; i++)
            {
                vertices[i] = HandlesUtils.GetCircleXZPoint(m_Radius, i, m_Resolution);
            }

            // Edge of the apex (Duplicated to allow hard edges on cone base)
            for (var i = 0; i < m_Resolution; i++)
            {
                vertices[m_Resolution + i] = HandlesUtils.GetCircleXZPoint(m_Radius, i, m_Resolution);
            }

            // Center of the base
            vertices[m_Resolution * 2] = Vector3.zero;

            // Tip of Cone
            vertices[m_Resolution * 2 + 1] = Vector3.up * m_Height;

            // Construct the triangles
            // Base triangles
            for (var i = 0; i < m_Resolution; i++)
            {
                triangles[i * 3] = m_Resolution * 2;
                triangles[i * 3 + 1] = HandlesUtils.GetCircleVertexIndex(i, m_Resolution);
                triangles[i * 3 + 2] = HandlesUtils.GetCircleVertexIndex(i + 1, m_Resolution);
            }

            // Apex triangles
            for (var i = 0; i < m_Resolution; i++)
            {
                triangles[(m_Resolution + i) * 3] = m_Resolution * 2 + 1;
                triangles[(m_Resolution + i) * 3 + 2] = m_Resolution + HandlesUtils.GetCircleVertexIndex(i, m_Resolution);
                triangles[(m_Resolution + i) * 3 + 1] = m_Resolution + HandlesUtils.GetCircleVertexIndex(i + 1, m_Resolution);
            }

            HandlesUtils.RotateVertices(m_RotationOffset, ref vertices);

            // Create the mesh
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            // Assign the mesh to the MeshFilter
            m_MeshFilter.mesh = mesh;
            m_MeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            
            QueueUpdate(UpdateFlags.Material);
        }

        void UpdateMaterial()
        {
            ResetUpdate(UpdateFlags.Material);

            // Set the materials colors
            m_MeshRenderer.sharedMaterials[0].SetColor("_BaseColor", m_Color);
        }

        // [Section] Update Flags

        public event Action<UpdateFlags, bool> OnUpdateFlagsChanged;

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
