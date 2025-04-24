using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if DEEPPOSE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
#if DEEPPOSE_URP
using UnityEngine.Rendering.Universal;
#endif

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Utility class for managing layers used by Muse Animate.
    /// </summary>
    /// <remarks>
    /// Muse Animate uses layers to isolate its rendering and physics from the rest of the project. It does this by
    /// assigning application GameObjects to unused layers.
    /// TODO: This may not be a sustainable solution. See https://jira.unity3d.com/browse/MUSEANIM-332
    /// </remarks>
    class ApplicationLayers
    {
        enum LayerName
        {
            Handles = 8,
            Posing = 9,
            Baking = 10,
            Thumbnail = 11,
            Environment = 12
        }
        
        /// <summary>
        /// Caution: To be used for UI only. Do not use these as layer masks.
        /// </summary>
        [System.Flags]
        public enum LayerMaskValue
        {
            Handles = 1 << LayerName.Handles,
            Posing = 1 << LayerName.Posing,
            Baking = 1 << LayerName.Baking,
            Thumbnail = 1 << LayerName.Thumbnail,
            Environment = 1 << LayerName.Environment
        }
        
        public static int LayerPosing => Instance.m_LayerPosing;
        public static LayerMask LayerMaskPosing => Instance.m_LayerMaskPosing;
        
        public static int LayerBaking => Instance.m_LayerBaking;
        public static LayerMask LayerMaskBaking => Instance.m_LayerMaskBaking;

        public static int LayerHandles => Instance.m_LayerHandles;
        public static LayerMask LayerMaskHandles => Instance.m_LayerMaskHandles;

        public static int LayerThumbnail => Instance.m_LayerThumbnail;
        public static LayerMask LayerMaskThumbnail => Instance.m_LayerMaskThumbnail;
        
        public static int LayerEnvironment => Instance.m_LayerEnvironment;
        public static LayerMask LayerMaskEnvironment => Instance.m_LayerMaskEnvironment;
        
        public static LayerMask LayerMaskAll => LayerMaskPosing | LayerMaskBaking | LayerMaskHandles | LayerMaskEnvironment;

        int m_LayerPosing;
        LayerMask m_LayerMaskPosing;

        int m_LayerBaking;
        LayerMask m_LayerMaskBaking;
        
        int m_LayerHandles;
        LayerMask m_LayerMaskHandles;

        int m_LayerThumbnail;
        LayerMask m_LayerMaskThumbnail;
        
        int m_LayerEnvironment;
        LayerMask m_LayerMaskEnvironment;

        static ApplicationLayers s_Instance;
        
        // Layer names to create
        const string k_LayerNamePosing = "Muse Animate Posing";
        const string k_LayerNameBaking = "Muse Animate Baking";
        const string k_LayerNameHandles = "Muse Animate Handles";
        const string k_LayerNameThumbnail = "Muse Animate Thumbnail";
        const string k_LayerNameEnvironment = "Muse Animate Environment";

        // Layers as defined in the original Application.prefab
        static readonly Dictionary<LayerName, string> k_OriginalLayerNames = new()
        {
            { LayerName.Handles, k_LayerNameHandles },
            { LayerName.Posing, k_LayerNamePosing },
            { LayerName.Baking, k_LayerNameBaking },
            { LayerName.Thumbnail, k_LayerNameThumbnail },
            { LayerName.Environment, k_LayerNameEnvironment }
        };

        public static ApplicationLayers Instance
        {
            get
            {
                if (s_Instance != null)
                    return s_Instance;

                s_Instance = new ApplicationLayers();
                return s_Instance;
            }
        }

        ApplicationLayers()
        {
            SolveLayer(k_LayerNamePosing, out m_LayerPosing, out m_LayerMaskPosing);
            SolveLayer(k_LayerNameBaking, out m_LayerBaking, out m_LayerMaskBaking);
            SolveLayer(k_LayerNameHandles, out m_LayerHandles, out m_LayerMaskHandles);
            SolveLayer(k_LayerNameThumbnail, out m_LayerThumbnail, out m_LayerMaskThumbnail);
            SolveLayer(k_LayerNameEnvironment, out m_LayerEnvironment, out m_LayerMaskEnvironment);
        }

        static void SolveLayer(string layerName, out int layer, out LayerMask mask)
        {
            layer = LayerMask.NameToLayer(layerName);
            if (layer < 0)
            {
                // Create the layer if it doesn't exist. Find the first empty layer.
                for (var i = 0; i < 32; i++)
                {
                    if (string.IsNullOrEmpty(LayerMask.LayerToName(i)))
                    {
                        layer = i;
                        break;
                    }
                }
                
                if (layer < 0)
                {
                    Debug.LogError($"Failed to create layer {layerName}. No empty layers available.");
                }
#if UNITY_EDITOR
                else
                {
                    // Set the layer name.
                    var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                    var layerProps = tagManager.FindProperty("layers");
                    layerProps.GetArrayElementAtIndex(layer).stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                }
#endif
            }

            mask = LayerMask.GetMask(layerName);
        }
        
        /// <summary>
        /// Reassigns the layers of the given root and all its children to the new layers.
        /// </summary>
        /// <param name="root"></param>
        public static void AssignLayers(Transform root)
        {
            root.gameObject.layer = RemapLayer(root.gameObject.layer);
            if (root.TryGetComponent(out Light light))
            {
                light.cullingMask = RemapLayerMask(light.cullingMask);
            }
            if (root.TryGetComponent(out CustomPhysicsRaycaster raycaster))
            {
                raycaster.eventMask = RemapLayerMask(raycaster.eventMask);
            }
            if (root.TryGetComponent(out Camera camera))
            {
                AssignCameraLayers(camera);
            }
            
            // Recurse into children.
            foreach (Transform child in root)
            {
                AssignLayers(child);
            }
        }
        
        static void AssignCameraLayers(Camera camera)
        {
#if DEEPPOSE_HDRP
            if (RenderPipelineUtils.IsUsingHdrp())
            {
                if (camera.TryGetComponent(out HDAdditionalCameraData hdData))
                {
                    hdData.volumeLayerMask = RemapLayerMask(hdData.volumeLayerMask);
                }
                else
                {
                    DevLogger.LogError("Failed to find HDAdditionalCameraData on camera.");
                }
            }
#endif
#if DEEPPOSE_URP
            if (RenderPipelineUtils.IsUsingUrp())
            {
                if (camera.TryGetComponent(out UniversalAdditionalCameraData urpData))
                {
                    urpData.volumeLayerMask = RemapLayerMask(urpData.volumeLayerMask);
                }
                else
                {
                    DevLogger.LogError("Failed to find UniversalAdditionalCameraData on camera.");
                }
            }
#endif
        }
        
        static int RemapLayer(int layer)
        {
            if (k_OriginalLayerNames.TryGetValue((LayerName)layer, out var layerName))
            {
                SolveLayer(layerName, out var newLayer, out _);
                return newLayer;
            }

            return layer;
        }

        public static int RemapLayerMask(int layerMask)
        {
            var newMask = 0;
            for (var i = 0; i < 32; i++)
            {
                if ((layerMask & (1 << i)) != 0)
                {
                    newMask |= 1 << RemapLayer(i);
                }
            }

            return newMask;
        }
    }
}
