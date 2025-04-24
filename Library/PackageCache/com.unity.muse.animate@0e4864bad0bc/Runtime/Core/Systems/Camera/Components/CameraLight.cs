using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.Muse.Animate
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    class CameraLight : MonoBehaviour
    {
        public Transform LightA;
        public Transform LightB;

        public Quaternion OffsetA;
        public Quaternion OffsetB;
        
        Camera m_Camera;

        void OnEnable()
        {
            m_Camera = GetComponent<Camera>();
            DeactivateLights();

            if (RenderPipelineUtils.IsUsingHdrp() || RenderPipelineUtils.IsUsingUrp())
            {
                RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
                RenderPipelineManager.endContextRendering += OnEndContextRendering;
            }
        }

        void OnDisable()
        {
            if (RenderPipelineUtils.IsUsingHdrp() || RenderPipelineUtils.IsUsingUrp())
            {
                RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
                RenderPipelineManager.endContextRendering -= OnEndContextRendering;
            }
        }

        void OnPreCull()
        {
            // Activate lights before rendering (BiRP)
            ActivateLights();
        }
        
        void OnPostRender()
        {
            // Deactivate lights after rendering (BiRP), so we don't interfere with other cameras
            DeactivateLights();
        }

        void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            // Activate lights before rendering (URP and HDRP)
            if (cameras.Contains(m_Camera))
            {
                ActivateLights();
            }
        }
        
        void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            // Deactivate lights after rendering (URP and HDRP), so we don't interfere with other cameras
            if (cameras.Contains(m_Camera))
            {
                DeactivateLights();
            }
        }

        void ActivateLights()
        {
            if (LightA != null)
            {
                LightA.gameObject.SetActive(true);
            }
        
            if (LightB != null)
            {
                LightB.gameObject.SetActive(true);
            }
        }
        
        void DeactivateLights()
        {
            if (LightA != null)
            {
                LightA.gameObject.SetActive(false);
            }
        
            if (LightB != null)
            {
                LightB.gameObject.SetActive(false);
            }
        }
    }
}