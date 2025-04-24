using UnityEngine;

namespace Unity.Muse.Animate
{
#if UNITY_MUSE_DEV
    [CreateAssetMenu(fileName = "HandlesConfiguration", menuName = "Muse Animate Dev/Configuration/Handles Configuration")]
#endif
    class HandlesConfiguration : ScriptableObject
    {
        [SerializeField]
        public Material HandlesMaterialURP;

        [SerializeField]
        public Material HandlesMaterialHDRP;

        [SerializeField]
        public Material HandlesMaterialBRP;

        [SerializeField]
        public Material OutlinerMaterialBRP;
        
        [SerializeField]
        public Material OutlinerMaterialHDRP;
        
        [SerializeField]
        public Material OutlinerMaterialURP;
    }
}
