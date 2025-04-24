using System;
using Unity.Muse.Common.Analytics;
#if ENABLE_UNITYENGINE_ANALITICS
using UnityEngine.Analytics;
#endif

namespace Unity.Muse.Sprite.Editor.Analytics
{
    [Serializable]
    class SaveSpriteData : IAnalytic.IData
    {
        public const string eventName = "muse_spriteTool_save";
        public const int version = 1;
#if !ENABLE_UNITYENGINE_ANALITICS
        public string EventName => eventName;
        public int Version => version;
#endif

        public SpriteSaveDestination drag_destination;
        public bool is_drag;
        public string material_hash;
    }

#if ENABLE_UNITYENGINE_ANALITICS
    [AnalyticInfo(eventName: SaveSpriteData.eventName, vendorKey: AnalyticsManager.vendorKey, SaveSpriteData.version, AnalyticsManager.maxEventsPerHour)]
#endif
    class SaveSpriteAnalytic : IAnalytic
    {
        readonly SpriteSaveDestination m_DragDestination;
        readonly bool m_IsDrag;
        readonly string m_MaterialHash;

        internal SaveSpriteAnalytic(SpriteSaveDestination dragDestination, bool isDrag, string materialHash)
        {
            m_DragDestination = dragDestination;
            m_IsDrag = isDrag;
            m_MaterialHash = materialHash;
        }

        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            var parameters = new SaveSpriteData
            {
                drag_destination = m_DragDestination,
                is_drag = m_IsDrag,
                material_hash = m_MaterialHash
            };
            data = parameters;
            return data != null;
        }
    }
}
