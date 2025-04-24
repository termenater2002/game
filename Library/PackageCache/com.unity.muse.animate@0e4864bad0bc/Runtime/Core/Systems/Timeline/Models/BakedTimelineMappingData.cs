using System;
using System.Collections.Generic;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct BakedTimelineMappingData
    {
        [Serializable]
        public struct MappingEntry
        {
            public int TimelineIndex;
            public int StartBakedFrameIndex;
            public int EndBakedFrameIndex;

            public bool IsValid => StartBakedFrameIndex <= EndBakedFrameIndex;

            public bool Contains(int bakedFrameIndex)
            {
                return bakedFrameIndex >= StartBakedFrameIndex && bakedFrameIndex <= EndBakedFrameIndex;
            }
        }

        // TODO: use more efficient data structure for look-ups?
        public List<MappingEntry> Keys;
        public List<MappingEntry> Transitions;
    }

    static class BakedTimelineMappingDataUtils
    {
        public static bool TryGetEntryAtBakedIndex(this List<BakedTimelineMappingData.MappingEntry> entries, int bakedFrameIndex, out BakedTimelineMappingData.MappingEntry foundEntry)
        {
            foreach (var entry in entries)
            {
                if (entry.Contains(bakedFrameIndex))
                {
                    foundEntry = entry;
                    return true;
                }
            }

            foundEntry = default;
            return false;
        }

        public static bool TryGetEntryAtTimelineIndex(this List<BakedTimelineMappingData.MappingEntry> entries, int timelineIndex, out BakedTimelineMappingData.MappingEntry foundEntry)
        {
            foreach (var entry in entries)
            {
                if (entry.TimelineIndex == timelineIndex)
                {
                    foundEntry = entry;
                    return true;
                }
            }

            foundEntry = default;
            return false;
        }
    }
}
