namespace Unity.Muse.Animate.Editor
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(LibraryItemAsset))]
    public class LibraryItemAssetEditor: Editor
    {
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            var item = (LibraryItemAsset)target;

            if (item == null || item.Thumbnail.Texture == null)
                return null;

            return item.Thumbnail.Texture;
        }
    }
}
