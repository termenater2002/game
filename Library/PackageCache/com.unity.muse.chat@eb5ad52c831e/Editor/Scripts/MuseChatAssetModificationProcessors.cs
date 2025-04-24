using System;
using UnityEditor;

namespace Unity.Muse.Chat
{
    static class MuseChatAssetsModificationProcessors
    {
        public static event Action<string[]> AssetDeletes;

        public static void NotifyDeletes(string[] paths)
        {
            AssetDeletes?.Invoke(paths);
        }
    }

    class TrackSelectedAssetsProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (deletedAssets is { Length: > 0 })
            {
                MuseChatAssetsModificationProcessors.NotifyDeletes(deletedAssets);
            }
        }
    }
}
