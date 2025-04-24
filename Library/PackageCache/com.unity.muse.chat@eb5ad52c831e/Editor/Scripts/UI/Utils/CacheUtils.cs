namespace Unity.Muse.Chat.UI.Utils
{
    static class CacheUtils
    {
        static readonly char[] k_PathTrimChars = {' ', '\t', '\n', '/'};

        public static string GetCachePath(string basePath, string subPath, string resourceFolderName)
        {
            if (string.IsNullOrEmpty(basePath))
            {
                basePath = MuseChatConstants.UIEditorPath;
            }

            string result = basePath + resourceFolderName;

            if (!string.IsNullOrEmpty(subPath))
            {
                result = result + subPath.Trim(k_PathTrimChars) + MuseChatConstants.UnityPathSeparator;
            }

            return result;
        }

        public static string GetCacheKey(string basePath, string subPath)
        {
            return string.Concat(basePath, "_", subPath ?? string.Empty);
        }
    }
}
