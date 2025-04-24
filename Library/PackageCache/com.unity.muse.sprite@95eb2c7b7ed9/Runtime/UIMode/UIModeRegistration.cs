using Unity.Muse.Common;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Muse.Sprite.UIMode
{
    static class UIModeRegistration
    {
#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        [Preserve]
        public static void RegisterUIMode()
        {
            UIModeFactory.RegisterUIMode<UIMode>(UIMode.modeKey);
        }
    }
}