using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
#if UNITY_MUSE_DEV
    [CreateAssetMenu(fileName = "ApplicationConfiguration", menuName = "Muse Animate Dev/Configuration/Application Configuration")]
#endif
    class ApplicationConfiguration : ScriptableObject
    {
        [Header("UI")]
        
        [SerializeField]
        public UIDocument UIDocumentPrefab;
        
        [SerializeField]
        public UITemplatesRegistry UITemplatesRegistry;

        [Header("Scene Controls")]
        
        [SerializeField]
        public HandlesConfiguration HandlesConfiguration;
        
        [Header("Stage Entities")]
        
        [SerializeField]
        public ActorRegistry ActorRegistry;

        [SerializeField]
        public PropRegistry PropRegistry;

        [Header("Stage Prefabs")]
        
        [SerializeField]
        public Camera CameraPrefabURP;
        
        [SerializeField]
        public Camera CameraPrefabHDRP;
        
        [SerializeField]
        public Camera CameraPrefab;

        [SerializeField]
        public Transform EnvironmentPrefabURP;
        
        [SerializeField]
        public Transform EnvironmentPrefabHDRP;
        
        [SerializeField]
        public Transform EnvironmentPrefab;

        [Header("Materials")]
        
        [SerializeField]
        public Material OutlineMaterial;
        
        [SerializeField]
        public Material LoopGhostMaterial;
    }
}
