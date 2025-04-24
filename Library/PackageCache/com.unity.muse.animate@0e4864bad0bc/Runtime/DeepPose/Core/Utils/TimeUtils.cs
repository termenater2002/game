using System;

namespace Unity.DeepPose.Core
{
    static class TimeUtils
    {
        public static string NiceDurationString(float seconds)
        {
            if (seconds < 60f)
                return $"{seconds} sec.";

            if (seconds < 3600f)
                return $"{seconds / 60f} min.";

            return $"{seconds / 3600f} hours";
        }
    }
}
