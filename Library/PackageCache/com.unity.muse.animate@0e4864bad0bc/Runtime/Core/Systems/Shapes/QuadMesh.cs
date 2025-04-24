using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Unity.Muse.Animate
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    class QuadMesh : MonoBehaviour
    {
        public Vector3[] Corners
        {
            get => m_Corners;
            private set
            {
                QueueUpdate(UpdateFlags.Mesh);
                if (m_Corners.Equals(value))
                    return;
                m_Corners = value;
            }
        }

        public Color Color
        {
            get => m_Color;
            set
            {
                QueueUpdate(UpdateFlags.Material);
                if (m_Color.Equals(value))
                    return;
                m_Color = value;
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
        [FormerlySerializedAs("m_ColorFill")]
        [SerializeField]
        Color m_Color;
        [SerializeField]
        Material m_Material;

        MaterialPropertyBlock m_MaterialPropertyBlock;
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
            Corners = new[] { Vector3.zero, Vector3.up, Vector3.up + Vector3.right, Vector3.right };

            m_MeshFilter = GetComponent<MeshFilter>();
            m_MeshRenderer = GetComponent<MeshRenderer>();

            // Add the materials to the list of materials
            m_Material = HandlesUtils.GetNewMeshMaterial();

            // Assign the materials to the mesh renderer
            m_MeshRenderer.SetMaterials(new List<Material> { m_Material });
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

            if (m_MeshFilter == null)
            {
                FindComponents();
            }

            // Construct the vertices
            var vertices = new Vector3[4];

            for (var i = 0; i < 4; i++)
            {
                vertices[i] = Corners[i];
            }

            // Construct the triangles
            var triangles = new int[6];
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;

            HandlesUtils.RotateVertices(RotationOffset, ref vertices);

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
            // Set the materials colors
            m_MeshRenderer.sharedMaterials[0].SetColor("_BaseColor", m_Color);
        }

        static void TransformVertices(Matrix4x4 transformMatrix, ref Vector3[] vertices)
        {
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] = transformMatrix.MultiplyPoint3x4(vertices[i]);
            }
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
        Vector3[] m_Corners = new[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

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

        public void SetQuadVertex(int i, Vector3 point)
        {
            m_Corners[i] = point;
            QueueUpdate(UpdateFlags.Mesh);
        }
    }
}
