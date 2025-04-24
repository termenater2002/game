using Unity.Muse.Common;
using Unity.Muse.Common.Tools;
using Unity.Muse.Sprite.Operators;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Muse.Sprite.Tools
{
    static class ToolRegistration
    {
#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        [Preserve]
        public static void RegisterTools()
        {
            AvailableToolsFactory.RegisterTool<BrushTool<SpriteRefiningMaskOperator>>(UIMode.UIMode.modeKey);
            AvailableToolsFactory.RegisterTool<PanTool>(UIMode.UIMode.modeKey);
        }
    }
}