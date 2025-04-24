using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [ExecuteAlways]
    [RequireComponent(typeof(Renderer))]
    class LitMaterialGenerator : MonoBehaviour
    {
        static string GetShaderName()
        {
            if (RenderPipelineUtils.IsUsingUrp())
            {
                return "Universal Render Pipeline/Complex Lit";
            }
            if (RenderPipelineUtils.IsUsingHdrp())
            {
                return "HDRP/Lit";
            }

            return "Standard";
        }

        Material m_Material;

        [SerializeField]
        Texture2D m_MainTexture;

        [SerializeField]
        Color m_Color = Color.white;

        [SerializeField]
        [Range(0, 1)]
        float m_Metallic;

        [SerializeField]
        [Range(0, 1)]
        float m_Smoothness;

        static readonly int k_Metallic = Shader.PropertyToID("_Metallic");
        static readonly int k_Smoothness = Shader.PropertyToID("_Smoothness");

        static int GetBaseMapPropertyID()
        {
            if (RenderPipelineUtils.IsUsingUrp())
            {
                return Shader.PropertyToID("_BaseMap");
            }
            if (RenderPipelineUtils.IsUsingHdrp())
            {
                return Shader.PropertyToID("_BaseColorMap");
            }

            return Shader.PropertyToID("_MainTex");
        }

        static int GetColorMapPropertyID()
        {
            if (RenderPipelineUtils.IsUsingUrp())
            {
                return Shader.PropertyToID("_BaseColor");
            }
            if (RenderPipelineUtils.IsUsingHdrp())
            {
                return Shader.PropertyToID("_BaseColor");
            }
            return Shader.PropertyToID("_Color");
        }

        void OnEnable()
        {
            if (!TryGetComponent(out Renderer renderer))
                return;

            if (m_Material == null)
            {
                m_Material = new Material(Shader.Find(GetShaderName()));
                m_Material.name = $"{gameObject.name}_lit";
                m_Material.hideFlags = HideFlags.HideAndDontSave;
            }

            if (m_MainTexture == null)
            {
                m_MainTexture = new Texture2D(1, 1);
                m_MainTexture.SetPixel(0, 0, Color.white);
                m_MainTexture.Apply();
            }

            m_Material.SetTexture(GetBaseMapPropertyID(), m_MainTexture);

            renderer.material = m_Material;
        }

        void Update()
        {
            m_Material.SetColor(GetColorMapPropertyID(), m_Color);
            m_Material.SetFloat(k_Metallic, m_Metallic);
            m_Material.SetFloat(k_Smoothness, m_Smoothness);
        }
    }
}
