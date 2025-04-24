using UnityEngine;

namespace Unity.Muse.Animate
{
    class CloudTextToMotionComponent : MonoBehaviour
    {
        public ArmatureToArmatureMapping CharacterToMotionArmatureMapping => m_CharacterToMotionArmatureMapping;
        public string CloudCharacterID => m_CloudCharacterID;

        [SerializeField]
        string m_CloudCharacterID;

        [SerializeField]
        ArmatureToArmatureMapping m_CharacterToMotionArmatureMapping;
    }
}
